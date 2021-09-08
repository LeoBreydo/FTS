﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Binosoft.TraderLib.Indicators;
using BrokerFacadeIB;
using CoreTypes;
using CoreTypes.SignalServiceClasses;

namespace Driver
{
    public class MainObject
    {
        static MainObject()
        {
            IndicatorsServer.Init(GetIndicatorsFolder());
            DebugLog.SetLocation(@"Logs/DebugLog.txt");
            string  errors=IndicatorsServer.GetLastLoadErrorsReport();
            if (!string.IsNullOrEmpty(errors))
                DebugLog.AddMsg("The next errors were encountered while initializing indicator plugins: " + errors);
        }
        public IBBrokerFacade Facade { get; }
        public TradingConfiguration Configuration { get; }
        public TradingService TService { get; }
        public ClientCommunicationFacade Client {get;}
        public Scheduler Scheduler { get; }  
        public InfoLogger Logger { get; }
        public SignalService SignalService => TService.SignalService;
        

        private bool _stoppedByHost = false;
        
        public MainObject(IBCredentials credentials)
        {
            Facade = new IBBrokerFacade(credentials);
            
            // composition root:
            // 1) currencies/markets/indicators/strategies
            // 2) web controller communicator

            Configuration = ReadAndVerifyConfiguration("./cfg.xml");
            
            TService = new TradingService(Configuration, GetStrategiesFolder());
            Client = new ClientCommunicationFacade();
            Scheduler = new Scheduler(Configuration);
            Logger = new(15, "Logs/"); //"./");
        }

        private static string GetStrategiesFolder()
        {
            var path = Path.GetFullPath("Strategies");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        private static string GetIndicatorsFolder()
        {
            var path = Path.GetFullPath("Indicators");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private TradingConfiguration ReadAndVerifyConfiguration(string path)
        {
            return TradingConfiguration.Restore(path) ?? new();
        }

        private CancellationTokenSource _cts;
        private Task _workTask;
        public bool IsStarted => _cts != null;
        public bool IsStopping { get; private set; }
        public void StartWork()
        {
            if (IsStarted) return;
            _cts = new CancellationTokenSource();

            _workTask=Task.Factory.StartNew(_ =>
            {
                DebugLog.AddMsg("=========== START============");
                Thread.Sleep(1000);
                Console.WriteLine("Started");
                Console.WriteLine("Press ENTER to stop TradingServer");
                var ms = (int)Math.Floor(DateTime.UtcNow.TimeOfDay.TotalMilliseconds);
                try
                {
                    while (true)
                    {
                        if (_cts.Token.IsCancellationRequested)
                        {
                            PlaceStopRequest();
                            var elapsedSeconds = 0;
                            while (!IsReadyToBeStooped)
                            {
                                if (elapsedSeconds >= 120) break;
                                Thread.Sleep(5000);
                                elapsedSeconds += 5;
                            }
                            Facade.Stop();
                            Logger.Flush();
                            if (elapsedSeconds < 120)
                                throw new TaskCanceledException("System will be stopped properly.");
                            throw new TaskCanceledException("System can be stopped properly, it will be stopped anyway.");
                        }

                        var dt = DateTime.UtcNow;
                        DoWork(dt);
                        var cms = (int)Math.Floor(dt.TimeOfDay.TotalMilliseconds);
                        var lastWorkDuration = cms - ms;
                        if (lastWorkDuration > 1000)
                        {

                            ms = ++cms;
                            Thread.Sleep(1);
                        }
                        else
                        {
                            var sleepTime = 1000 - (cms % 1000);
                            ms += sleepTime;
                            Thread.Sleep(sleepTime);
                        }
                        DebugLog.Flush();
                    }
                }
                catch (TaskCanceledException e)
                {
                    Console.WriteLine(e.Message);
                    DebugLog.AddMsg("Exception " + e, true);
                }
                finally
                {
                    _cts.Dispose();
                    _cts = null;
                }
                DebugLog.AddMsg("============ DONE ===========", true);
            }
                , TaskCreationOptions.LongRunning
                , _cts.Token);
        }

        public void StopWork()
        {
            IsStopping = true;
            _cts?.Cancel();
            _workTask.Wait();
            IsStopping = false;
        }

        private void DoWork(DateTime dt)
        {
            var so = Facade.GetState(dt);
            if (so == null) return;

            var clientCmdList = Client.GetCommands();
            var schedulerCmdList = Scheduler.GetCommands(so.CurrentUtcTime);
            var icCommands = SignalService.GetCommands();
            var t = TService.ProcessCurrentState(so, clientCmdList, schedulerCmdList, icCommands);
            Facade.PlaceRequest(t.Subscriptions, t.Orders);
            Client.PushInfo(t.State);
            
            Logger.PostToLog(so.CurrentUtcTime, t.TicksInfo, t.BarsInfo, t.TradesInfo,
                so.OrderStateMessageList, so.TextMessageList, t.Errors);

            SignalService.ApplyNewMarketRestrictions(t.Commands);
        }

        private bool IsReadyToBeStooped => _stoppedByHost && TService.IsReadyToBeStopped;

        private void PlaceStopRequest()
        {
            _stoppedByHost = true;
        } 
    }
}
