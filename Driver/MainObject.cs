using System;
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
        
        public MainObject()
        {
            Facade = new IBBrokerFacade(new IBEngine());
            
            // composition root:
            // 1) currencies/markets/indicators/strategies
            // 2) web controller communicator

            Configuration = ReadAndVerifyConfiguration("./cfg.xml");
            TService = new TradingService(Configuration);
            Client = new ClientCommunicationFacade();
            Scheduler = new Scheduler();
            Logger = new(15, "./");
        }

        private TradingConfiguration ReadAndVerifyConfiguration(string path)
        {
            return new();
        }

        public void DoWork(DateTime dt)
        {
            var so = Facade.GetState(dt);
            if (so == null) return;

            var clientCmdList = Client.GetCommands();
            var schedulerCmdList = Scheduler.GetCommands();

            var (subscriptions, orders, state, ticksInfo, barsInfo, tradesInfo, errors) = TService.ProcessCurrentState(so, clientCmdList, schedulerCmdList);
            Facade.PlaceRequest(subscriptions, orders);
            Client.PushInfo(state);
            
            Logger.PostToLog(so.CurrentUtcTime, ticksInfo, barsInfo, tradesInfo,
                so.OrderStateMessageList, so.TextMessageList, errors);
        }

        public void Stop()
        {
            Facade.Stop();
            Logger.Flush();
        } 
    }
}
