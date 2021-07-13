using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CommonStructures
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarketState
    {
        HardStop = 0,
        HardStopLong = 1,
        HardStopShort = 2,
        SoftStop = 3,
        SoftStopLong = 4,
        SoftStopShort = 5,
        Warning = 6,
        TradingAllowed = 7,
    }

    public static class MarketStateX
    {
        public static bool NoRestrictions(this MarketState state)
        {
            return state == MarketState.TradingAllowed || state == MarketState.Warning;
        }
    }
}
