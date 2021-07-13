using System;

namespace BrokerInterfaces
{
    /// <summary>
    /// Generator for unique Client Order Identifiers
    /// </summary>
    public interface IClOrdIDGenerator
    {
        string GetNextID();
        string GetNextID(long strategyID);
        bool ExtractStrategyID(string clOrdId, out long strategyID);
        bool ExtractTime(string clOrdId, out DateTime time);
    }
}
