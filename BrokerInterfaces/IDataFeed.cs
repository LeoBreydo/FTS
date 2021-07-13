using ProductInterfaces;

namespace BrokerInterfaces
{
    /// <summary>
    /// Quotes provider interface. The part of broker service responsible for quotes support.
    /// </summary>
    /// <remarks>
    /// Accepts subscription requests from subscribers (method Subscribe), broadcasts quote streams using market data channel.
    /// To receive broadcasted messages subscribers has to be attached to the channel (method Register).
    /// Subscribers work independently of each other. Each subscriber can subscribe to multiple instruments.
    /// </remarks>
    public interface IDataFeed : IRegistry<IQtListener>
    {
        long BrokerID { get; }

        /// <summary>
        /// provide specified quotes stream (symbol/volume/mass or single quote) for subscriber
        /// </summary>
        /// <param name="subscriber">Subscriber enitity</param>
        /// <param name="symbol">currency pair to subscribe</param>
        void Subscribe(object subscriber, string symbol);

        #region to review/remove
        /// <summary>
        /// exclude subscriber from the specified quotes stream consumers
        /// todo исключить Unsubscribe (в контексте fix отписаться невозможно, в контексте нашего продукта отписка не требуется), упростить реализацию
        /// </summary>
        void Unsubscribe(object subscriber, string symbol);
        #endregion

    }
}
