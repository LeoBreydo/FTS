namespace BrokerInterfaces
{
    /// <summary>
    /// Manages centralized subscribers subsription to the broker (after data feed starts available or by another reason)
    /// </summary>
    public interface ISubsriptionsManager
    {
        void SubscribeQuotes(long brokerID);
        void UnsubscribeQuotes(long brokerID);
    }
}
