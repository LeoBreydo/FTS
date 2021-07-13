using System;

namespace Utilities
{
    public class UtcTimeBasedIncreasingIdGenerator
    {
        public long LastValue = 0;
        public static readonly long _base = new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        public long GetNextID()
        {
            long ticks = DateTime.UtcNow.Ticks - _base;
            if (ticks <= LastValue)
                ++LastValue;
            else
                LastValue = ticks;
            return LastValue;
        }
    }
}
