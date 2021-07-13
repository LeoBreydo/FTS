using System;

namespace BrokerInterfaces
{
    public interface ITickFilter
    {
        bool AcceptQuote(DateTime utcTime, double bid, double ask, string symbol); 
    }
}