using System;
using System.Collections.Generic;

namespace WebClientObjects
{
    public class ExchangeDetails
    {
        public int Id;
        public string Name;
        public string Currency;
        public bool IsConnected;
        public decimal UPL;
        public decimal RPL;
        public string Restrictions;
        public string Info;
        public List<Tuple<string, string>> MessagesToShow;
        public List<MarketOrStrategyDetails> MktOrStrategies = new();
    }
}