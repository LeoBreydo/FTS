using System;

namespace CommonStructures
{
    /// <summary>
    /// Class to support time moment occurs at specfic day of week.
    /// That's recurring time moment. 
    /// Implicitly assumed that time corresponds to specific time zone,
    /// which is defined in the calling context. But it is not obligatory.
    /// </summary>
    public class RecurringPattern
    {
        public DayOfWeek Day { get; private set; }
        public int Hour { get; private set; }
        public int Minute { get; private set; }
        public int Second { get; private set; }

        public RecurringPattern(DayOfWeek day, int hour, int minute, int second)
        {
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
        }

        public DateTime GetNextUtcDateTime(DateTime utcNow, TimeZoneInfo timeZone)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
            var today = now.Date;
            var daysUntilWeekday = ((int)Day - (int)today.DayOfWeek + 7) % 7;
            var nextWeekDay = today.AddDays(daysUntilWeekday);
            var nextTime = nextWeekDay.AddSeconds(Second).AddMinutes(Minute).AddHours(Hour);

            return nextTime < now
                ? TimeZoneInfo.ConvertTimeToUtc(nextTime.AddDays(7), timeZone)
                : TimeZoneInfo.ConvertTimeToUtc(nextTime, timeZone);
        }
    }
}
