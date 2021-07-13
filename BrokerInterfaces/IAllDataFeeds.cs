using CommonStructures;
using ProductInterfaces;

namespace BrokerInterfaces
{
    public interface IRepublishBrokerFacade
    {
        void Start();
        void Stop();
        void Call();
        bool IsConnectedToProvider(long providerID);

        void RegisterQuoteListener(IQtListener item);
        void UnregisterQuoteListener(IQtListener item);
        void Subscribe(object subscriber, long providerID, string symbol);

        void RegisterOrderReportListener(IMsgListener item);
        void UnregisterOrderReportListener(IMsgListener item);
        void SendOrder(Order order);
        void CancelOrder(Order order, string cancelRequestID);
        void ReplaceOrder(Order order, double newPrice, string newClOrdId);
    }

    /// <summary>
    /// Единая точка обращения ко всем поставщикам данных
    /// </summary>
    /// <remarks>
    /// Отделена от IAllTradingProviders 
    /// т.к. существует вероятность, что мы будем использовать дополнительных поставщиков данных, которые не проводят сделки
    /// </remarks>
    public interface IAllDataFeeds : IRegistry<IQtListener>
    {
        void Subscribe(object subscriber, long providerID,string symbol);
    }

    /// <summary>
    /// Единая точка обращения ко всем исполнителям ордеров
    /// </summary>
    public interface IAllTradingProviders : IRegistry<IMsgListener>
    {
        /// <summary>
        /// Send the new order
        /// </summary>
        void SendOrder(Order order);
        /// <summary>
        /// Cancel the order
        /// </summary>
        void CancelOrder(Order order, string cancelRequestID);
        /// <summary>
        /// Replace order (alter limit order price)
        /// </summary>
        void ReplaceOrder(Order order, double newPrice, string newClOrdId);
    }
}
