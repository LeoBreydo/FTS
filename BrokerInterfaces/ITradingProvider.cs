using CommonStructures;
using ProductInterfaces;

namespace BrokerInterfaces
{
    /// <summary>
    /// The part of broker facade responsible for orders execution.
    /// </summary>
    public interface ITradingProvider : IRegistry<IMsgListener>
    {
        long BrokerID { get; }
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
