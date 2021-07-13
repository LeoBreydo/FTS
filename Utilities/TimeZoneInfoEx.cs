using System;

namespace Utilities
{
    public static class TimeZoneInfoEx
    {
        /// <summary>
        /// convert time value from utc to the timezone local time
        /// </summary>
        public static DateTime UtcToLocalTime(this TimeZoneInfo tzi, DateTime utcTime)
        {
            return utcTime.Add(tzi.GetUtcOffset(utcTime));
        }
        /// <summary>
        /// convert time value from local timezone to utc
        /// </summary>
        public static DateTime LocalTimeToUtc(this TimeZoneInfo tzi, DateTime localTime)
        {
            return localTime.Add(-tzi.GetUtcOffset(localTime));
        }

        public static DateTime NextTimeOfWeek(this DateTime time, DayOfWeek dw, int hour, int minute)
        {
            DateTime nextTime = time.Date.AddDays(((int)dw) - (int)time.DayOfWeek).AddMinutes(hour * 60 + minute);
            if (nextTime < time)
                nextTime = nextTime.AddDays(7);
            return new DateTime(nextTime.Ticks, DateTimeKind.Local);
        }

    }
}
