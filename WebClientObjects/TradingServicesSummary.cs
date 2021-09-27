using System;
using System.Collections.Generic;

namespace WebClientObjects
{
    public class TradingServicesSummary
    {
        public int Id;
        public bool IsConnected;
        public string RestrictionDetails;
        public string CurrentRestriction;
        public int DayErrorNbr;
        public List<CurrencyGroupSummary> CGSummaries = new();
        public List<ExchangeSummary> ExSummaries = new();
        public List<Tuple<string, string>> MessagesToShow;
    }
}
