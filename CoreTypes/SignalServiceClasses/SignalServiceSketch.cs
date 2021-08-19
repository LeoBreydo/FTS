using System.Collections.Generic;

namespace CoreTypes.SignalServiceClasses
{
    // just a stub
    public class SignalServiceSketch
    {
        public Signal GetSignal(int strategyId)
        {
            return Signal.NO_SIGNAL;
        }

        public List<ICommand> GetCommands()
        {
            return new List<ICommand>();
            //throw new System.NotImplementedException();
        }

        public void ApplyNewMarketRestrictions(List<(string, TradingRestriction)> tCommands)
        {
            //throw new System.NotImplementedException();
        }
    }
}
