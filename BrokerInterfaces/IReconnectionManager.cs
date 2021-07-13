namespace BrokerInterfaces
{
    public enum EConnectionStatus
    {
        Connected,
        Disconnected,
        FatalError
    };

    /// <summary>
    /// the interface of the class responding for the stable connection to the broker
    /// </summary>
    public interface IReconnectionManager
    {
        /// <summary>
        /// needed to arrange broker reports reception
        /// </summary>
        IConnectionToBroker ConnectionToBroker { get; }
        /// <summary>
        /// start trying to connect to broker
        /// </summary>
        void ActivateWork();
        /// <summary>
        /// disconnect from broker
        /// </summary>
        void DeactivateWork();
        // process broker connection report
        void ProcessBrokerConnectionStatus(long brokerId, bool isQuoteSession, EConnectionStatus status);
        // process the secon pulse
        void SecondPulse();
    }
}
