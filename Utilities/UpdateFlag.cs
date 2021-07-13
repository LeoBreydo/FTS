using System.Threading;

namespace Utilities
{
    /// <summary>
    /// thread-safe uni-directional notification
    /// </summary>
    public class UpdateFlag
    {
        private int value;

        public void Set() { value = 1; }
        public void Reset() { value = 0; }
        public bool Get() { return value == 1; }
        public bool GetReset()
        {
            return Interlocked.Exchange(ref value, 0) == 1;
        }
    }

}
