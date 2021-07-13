using System;

namespace Utilities
{
    /// <summary>
    /// вспомогательный класс для работы с DateTime
    /// </summary>
    public static class DateTimeEx
    {
        public static DateTime RoundMinute(this DateTime t, bool bForward)
        {
            if (t == DateTime.MinValue || t == DateTime.MaxValue) return t;
            var res = new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0, t.Kind);
            if (!bForward) return res;
            if (res < t) res = res.AddMinutes(1);
            return res;
        }

        public static DateTime RoundSecond(this DateTime t, bool bForward)
        {
            if (t == DateTime.MinValue || t == DateTime.MaxValue) return t;
            var res = new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, t.Kind);
            if (!bForward) return res;
            if (res < t) res = res.AddSeconds(1);
            return res;
            
        }
    }
}
