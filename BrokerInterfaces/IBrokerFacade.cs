using ProductInterfaces;

namespace BrokerInterfaces
{
    public interface IConnectionToBroker
    {
        /// <summary>
        /// The brokerID
        /// </summary>
        long BrokerID { get; }
        /// <summary>
        /// connect to broker quotes session
        /// </summary>
        void ConnectQuotesSession();
        /// <summary>
        /// disconnect from broker quotes session
        /// </summary>
        void DisconnectQuotesSession();
        /// <summary>
        /// is connection to broker quotes session established or not
        /// </summary>
        bool IsQuotesSessionEstablished { get; }

        /// <summary>
        /// connect to the broker trade session
        /// </summary>
        void ConnectTradeSession();
        /// <summary>
        /// disconnect from broker trade session
        /// </summary>
        void DisconnectTradeSession();
        /// <summary>
        /// is connection to broker trade session established or not
        /// </summary>
        bool IsTradeSessionEstablished { get; }
        /// <summary>
        /// main reports channel listeners registration/unregistration
        /// </summary>
        /// <remarks>
        /// broker sends to the channel reports about connection/disconnection,
        /// also reports related to subscription requests of the DataFeed
        /// </remarks>
        IRegistry<IMsgListener> MainReportsChannel{ get; }

    }
    /// <summary>
    /// broker API interface
    /// </summary>
    /// <remarks>
    /// Provides the quotes and order execution subservices and the pilot channel to send to 
    /// notification messages responding to the connection state.
    /// The implementation must be based on a "challenge-response".
    /// The ensuring connection permanency, the keeping active orders and lifecycle etc.
    /// should be moved out to the other classes as far as 
    /// the implementation of such functionality can be different depending on the ultimate goals.
    /// </remarks>
    public interface IBrokerFacade : IConnectionToBroker
    {
        /// <summary>
        /// get quotes subservice
        /// </summary>
        IDataFeed DataFeed { get; }
        /// <summary>
        /// get orders management subservice
        /// </summary>
        ITradingProvider TradingProvider { get; }
    }

}
