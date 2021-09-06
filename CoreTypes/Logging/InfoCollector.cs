using System;
using System.Collections.Generic;
using System.Linq;
using static CoreTypes.MessageStringProducer;

namespace CoreTypes
{
    // aliases
    using LSSS = List<(string market, string exchange, string txt)>;
    using LSS = List<(string symbol, string exchange)>;
    using LS = List<string>;
    using LMOD = List<MarketOrderDescription>;
    using LC = List<ICommand>;

    /// <summary>
    /// Visitor for collecting all current information. 
    /// </summary>
    public class InfoCollector
    {
        public LSS Subscriptions=new();
        public LMOD Orders=new();
        public TradingServiceState State;
        public List<(string mktExch,PriceProviderInfo ppi)> TicksInfo=new();
        public List<Tuple<Bar, string, string>> BarsInfo = new();
        public LS TradesInfo=new();
        public LSSS Errors=new();
        public List<(string, int, double)> NewBpvMms=new();
        public List<(string, TradingRestriction)> Commands=new();

        public void Accept(List<(string mktExch, PriceProviderInfo ppi)> tickInfoList)
        {
            if (tickInfoList is { Count: > 0 })
                TicksInfo.AddRange(tickInfoList);
        }
        public void Accept(List<Tuple<Bar, string, string>> barInfoList)
        {
            if(barInfoList is {Count: > 0})
                BarsInfo.AddRange(barInfoList);
        }
        public void Accept((string, int b, double mm) bpvMinMoveInfo)
        {
            NewBpvMms.Add(bpvMinMoveInfo);
        }

        public void Accept(string mkt, string exchange)
        {
            Subscriptions.Add((mkt, exchange));
        }

        public void Accept(string mktExchange, int command)
        {
            Commands.Add((mktExchange, command < 0 ? TradingRestriction.NoRestrictions : TradingRestriction.HardStop));
        }

        public void Accept((string, string, string) errorInfo)
        {
            Errors.Add(errorInfo);
        }

        public void Accept(LS tradeInfos)
        {
            TradesInfo.AddRange(tradeInfos);
        }

        public void Accept(MarketOrderDescription order)
        {
            Orders.Add(order);
        }



        public LS TickInfoAsStrings(DateTime utcNow) =>
            TicksInfo.Select(ti => PriceProviderString(ti.ppi, utcNow, ti.mktExch)).ToList();
        public LS BarInfoAsStrings => BarsInfo.Select(t => 
            BarInfoString(t.Item1, t.Item2, t.Item3)).ToList();

        
    }
}
