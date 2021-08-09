using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CommonStructures
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarketState
    {
        TradingAllowed = 0,
        SoftStop=1,
        HardStop = 2
    }

    public static class MarketStateX
    {
        public static bool NoRestrictions(this MarketState state) => state == MarketState.TradingAllowed;
        public static bool ThereIsRestriction(this MarketState state) => !state.NoRestrictions();
    }
}
