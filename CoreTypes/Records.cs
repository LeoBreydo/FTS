using System;

namespace CoreTypes
{
    public record Execution(string ExecId, int OrderId, DateTime Time, decimal Price);
    public record MarketOrderDescription(int ClOrdId, string Symbol, string Exchange, int SignedContractsNbr);
    // Value can be of integer type as well (when it is a size)
    public record TickInfo(string SymbolExchange, int Tag, double Value);
    public record Bar5s(string SymbolExchange, string ContractCode, double Open, double High, double Low, double Close,
        DateTime BarOpenTime);
}
