using System;

namespace CoreTypes
{
    internal static class MessageStringProducer
    {
        public static string TradeString(Execution Open, Execution Close, int Size, string StrategyName) =>
            $"{StrategyName},{Open.ExecId},{Open.OrderId},{Open.Time:yyyyMMdd:HHmmss},{Open.Price}," +
            $"{Close.ExecId},{Close.OrderId},{Close.Time:yyyyMMdd:HHmmss},{Close.Price},{Size}";

        public static string TradeStringFormat =>
            "# StrategyName,OpenExecutionId,OpenOrderId,OpenTime:yyyyMMdd:HHmmss,OpenPrice," +
            "CloseExecutionId,CloseOrderId,CloseTime:yyyyMMdd:HHmmss,ClosePrice,Size";

        public static string MarketOrderDescriptionString(MarketOrderDescription mod) => 
            $"{mod.ClOrdId},{mod.Symbol},{mod.Exchange},{mod.SignedContractsNbr}";

        public static string MarketOrderDescriptionStringFormat => "# ClOrderId,Symbol,Exchange,Size";

        public static string BarInfoString(Bar b, string symbolExchange, bool isNewContract) =>
            $"{symbolExchange},{b.O},{b.H},{b.L},{b.C},{b.Start:yyyyMMdd:HHmmss},{b.End:yyyyMMdd:HHmmss},{isNewContract}";

        public static string BarInfoStringFormat => 
            "# SymbolExchange,Open,High,Low,Close," +
            "StartOfBar:yyyyMMdd:HHmmss,EndOfBar:yyyyMMdd:HHmmss,IsNewContract";

        public static string PriceProviderString(PriceProvider pp, DateTime utcNow, string symbolExchange) => 
            $"{symbolExchange},{utcNow:yyyyMMdd:HHmmss},{pp.Bid},{pp.Ask},{pp.LastPrice},{pp.BidSize},{pp.AskSize},{pp.LastSize}";

        public static string PriceProviderStringFormat => 
            "# SymbolExchange,Time:yyyyMMdd:HHmmss,Bid,Ask,Last,BidSize,AskSize,LastSize";
    }
}
