namespace BrokerInterfaces
{
    /// <summary>
    /// stores the identifiers of the recently finished orders
    /// </summary>
    public interface IRecentlyFinishedOrders
    {
        void Add(string clOrderID);
        bool Contains(string clOrderID);
    }
}
