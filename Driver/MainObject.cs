using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var msgToClient = so.TextMessageList.Where(t => t.Item1 == "CLIENT")
                .Select(t => t.Item2).ToList();
            Console.WriteLine(so.CurrentUtcTime.Second);
            if(so.BarUpdateList.Count != 0) Console.WriteLine("!");
            foreach (var ci in so.ContractInfoList) Console.WriteLine(ci.ToString());

            var (subscriptions, orders, state) = TService.ProcessCurrentState(so, clientCmdList, schedulerCmdList);
            Facade.PlaceRequest(subscriptions, orders);
            Client.PushInfo(msgToClient, state);
        }

        public void Stop() => Facade.Stop();
    }
}
