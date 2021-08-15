using System;

namespace CoreTypes
{
    public record Execution(string ExecId, int OrderId, DateTime Time, decimal Price)
    {
        public override string ToString()
        {
            return $"EId: {ExecId}, OId: {OrderId}, Time: {Time:yyyyMMdd:HHmmss}, Price: {Price}";
        }
    }
    public record Trade(Execution Open, Execution Close, int Size, string StrategyName)
    {
        public override string ToString()
        {
            return $"StrategyName: {StrategyName}, OpEx: {Open}, ClEx: {Close}, Size: {Size}";
        }
    }

    public record MarketOrderDescription(int ClOrdId, string Symbol, string Exchange, int SignedContractsNbr)
    {
        public override string ToString()
        {
            return $"COId: {ClOrdId}, SymbolExchange: {Symbol}{Exchange}, Size: {SignedContractsNbr}";
        }
    }
    
    // Value can be of integer type as well (when it is a size)
    public record TickInfo(string SymbolExchange, int Tag, double Value)
    {
        public override string ToString()
        {
            return $"{nameof(SymbolExchange)}: {SymbolExchange}, {nameof(Tag)}: {Tag}, {nameof(Value)}: {Value}";
        }
    }

    public record Bar5s(string SymbolExchange, string ContractCode, double Open, double High, double Low, double Close,
        DateTime BarOpenTime)
    {
        public override string ToString()
        {
            return $"SymbolExchange: {SymbolExchange}, ContractCode: {ContractCode}, O: {Open}, H: {High}, L: {Low}, C: {Close}, OpenTime: {BarOpenTime:yyyyMMdd:HHmmss}";
        }
    }

}
