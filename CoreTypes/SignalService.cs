using System.Collections.Generic;

namespace CoreTypes
{
    // just a stub
    public class SignalService
    {
        public Signal GetSignal(int strategyId)
        {
            return Signal.NO_SIGNAL;
        }

        public List<ICommand> GetCommands()
        {
            throw new System.NotImplementedException();
        }

        public void ApplyNewMarketRestrictions(List<(string, TradingRestriction)> tCommands)
        {
            throw new System.NotImplementedException();
        }
    }

    public enum Signal
    {
        NO_SIGNAL = -2,
        TO_SHORT = -1,
        TO_FLAT = 0,
        TO_LONG = 1
    }
}
