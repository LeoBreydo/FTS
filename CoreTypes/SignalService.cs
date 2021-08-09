using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreTypes
{
    // just a stub
    public class SignalService
    {
        public Signal GetSignal(int strategyId)
        {
            return Signal.NO_SIGNAL;
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
