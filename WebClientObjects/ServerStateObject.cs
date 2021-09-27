using System.Collections.Generic;

namespace WebClientObjects
{
    public class ServerStateObject
    {
        public TradingServicesSummary Summary;
        public Dictionary<int, ExchangeDetails> Details;
    }
}
