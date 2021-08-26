using System;
using System.IO;
using BrokerFacadeIB;
using CoreTypes;

namespace Driver
{
    public class MainObject
    {
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
            Scheduler = new Scheduler();
            Logger = new(15, "Logs/"); //"./");
        }

        private static string GetStrategiesFolder()
        {
            var path = Path.GetFullPath("Strategies");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private TradingConfiguration ReadAndVerifyConfiguration(string path)
        {
            var tcfg = TradingConfiguration.Restore(path);
            return tcfg ?? new();
        }

        public void DoWork(DateTime dt)
        {
            var so = Facade.GetState(dt);
            if (so == null) return;

            var clientCmdList = Client.GetCommands();
            var schedulerCmdList = Scheduler.GetCommands();
            var icCommands = SignalService.GetCommands();
            var t = TService.ProcessCurrentState(so, clientCmdList, schedulerCmdList, icCommands);
            Facade.PlaceRequest(t.Subscriptions, t.Orders);
            Client.PushInfo(t.State);
            
            Logger.PostToLog(so.CurrentUtcTime, t.TicksInfo, t.BarsInfo, t.TradesInfo,
                so.OrderStateMessageList, so.TextMessageList, t.Errors);

            SignalService.ApplyNewMarketRestrictions(t.Commands);
        }

        public bool IsReadyToBeStooped => _stoppedByHost && TService.IsReadyToBeStopped;

        public void PlaceStopRequest()
        {
            _stoppedByHost = true;
        } 
    }
}
