using System;

namespace BrokerInterfaces
{
    /// <summary>
    /// разрешение/запрет отправки ордеров брокеру ввиду расписания (или других условий, не связанных с дизаблингом брокера или отсутствием соединения)
    /// </summary>
    public interface IAllowSendOrders
    {
        long BrokerID { get; }
        void Update(DateTime utcNow);
        bool Allow { get; }
    }
    public class AlwaysAllowSendOrders : IAllowSendOrders
    {
        public AlwaysAllowSendOrders(long brokerID)
        {
            BrokerID = brokerID;
        }

        public long BrokerID { get; private set; }

        public void Update(DateTime utcNow)
        {
        }

        public bool Allow
        {
            get { return true; }
        }
    }

    /// <summary>
    /// разрешение/запрет отправки ордеров брокерам ввиду расписания (или других условий, не связанных с дизаблингом брокера или отсутствием соединения)
    /// </summary>
    public interface IAllowSendOrderToBrokers
    {
        void UpdateAll(DateTime utcNow);
        bool UpdateBroker(DateTime utcNow, long brokerID);
        bool AllowSendOrderToBroker(long brokerID); 
    }
    public class AlwaysAllowSendOrderToBrokers : IAllowSendOrderToBrokers
    {
        public void UpdateAll(DateTime utcNow) { }
        public bool UpdateBroker(DateTime utcNow, long brokerID){return true;}
        public bool AllowSendOrderToBroker(long brokerID) { return true; }
    }

}
