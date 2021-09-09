using System;

namespace CoreTypes
{
    public record Execution(string ExecId, int OrderId, DateTime Time, decimal Price);
    public record MarketOrderDescription(int ClOrdId, string Symbol, string Exchange, int SignedContractsNbr);
    // Value can be of integer type as well (when it is a size)
    public record TickInfo(string SymbolExchange, string ContractCode, int Tag, double Value);
}
