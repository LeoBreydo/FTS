using System;
using System.Xml.Serialization;

namespace Utilities
{
    /// <summary>
    /// Defines the DayOfWeek and TimeOfDay for specified TimeZone
    /// </summary>
    /// <remarks>
    /// like "monday 8:00 Sydney time"
    /// </remarks>
    public class TimeOfWeek
    {
        /// <summary>
        /// Represents timezone info
        /// </summary>
        [XmlIgnore]
        public TimeZoneInfo TimeZoneInfo { get; private set; }
        private string mTimeZoneId;
        private int mHour, mMinute, mSecond;

        /// <summary>
        /// timezone id (a member of windows timezones enumeration)
        /// </summary>
        public string TimeZoneId
        {
            get { return mTimeZoneId; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    TimeZoneInfo = TimeZoneInfo.Utc;
                    mTimeZoneId = TimeZoneInfo.Id;
                }
                else
                {
                    TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(value);
                    mTimeZoneId = value;
                }

            }
        }
        /// <summary>
        /// DayOfWeek
        /// </summary>
        public DayOfWeek DayOfWeek
        {
            get;
            set;
        }
        /// <summary>
        /// Hour
        /// </summary>
        public int Hour
        {
            get
            {
                return mHour;
            }
            set
            {
                if (value < 0 || value > 23)
                    throw new Exception(
                        string.Format(
                            "Invalid value ({0}) of the property TimeOfWeek.Hour; expected value must be in interval[0..23] ", value));
                mHour = value;
            }
        }
        /// <summary>
        /// Minute
        /// </summary>
        public int Minute
        {
            get
            {
                return mMinute;
            }
            set
            {
                if (value < 0 || value > 59)
                    throw new Exception(
                        string.Format(
                            "Invalid value ({0}) of the property TimeOfWeek.Minute; expected value must be in interval[0..59] ", value));
                mMinute = value;
            }
        }
        /// <summary>
        /// Second
        /// </summary>
        public int Second
        {
            get
            {
                return mSecond;
            }
            set
            {
                if (value < 0 || value > 59)
                    throw new Exception(
                        string.Format(
                            "Invalid value ({0}) of the property TimeOfWeek.mSecond; expected value must be in interval[0..59] ", value));
                mSecond = value;
            }
        }
        public TimeOfWeek() { }
        public TimeOfWeek(string timeZoneId, DayOfWeek dayOfWeek, int hour, int minute, int second=0)
        {
            TimeZoneId = timeZoneId;
            DayOfWeek = dayOfWeek;
            Hour = hour;
            Minute = minute;
            Second = second;
        }
        public TimeOfWeek(TimeOfWeek from)
        {
            TimeZoneId = from.TimeZoneId;
            DayOfWeek = from.DayOfWeek;
            Hour = from.Hour;
            Minute = from.Minute;
            Second = from.Second;
        }

        /// <summary>
        /// convert time value from utc to TimeOfWeek timezone 
        /// </summary>
        private DateTime UtcToLocalTime(DateTime utcTime)
        {
            //return utcTime.Add(TimeZoneInfo.GetUtcOffset(utcTime));
            return TimeZoneInfo.UtcToLocalTime(utcTime);
        }

        private int SecondOfTheDay()
        {
            return (Hour*60 + Minute)*60 + Second;
        }
        /// <summary>
        /// convert time value from TimeOfWeek timezone to utc
        /// </summary>
        private DateTime LocalTimeToUtc(DateTime localTime)
        {
            //return localTime.Add(-TimeZoneInfo.GetUtcOffset(localTime));
            return TimeZoneInfo.LocalTimeToUtc(localTime);
        }
        public DateTime GetLastTime(DateTime utcNow)
        {
            DateTime localTime = UtcToLocalTime(utcNow);
            DateTime localDate = localTime.Date;
            DateTime timepoint = localDate.AddDays(((int)DayOfWeek) - (int)localDate.DayOfWeek).AddSeconds(SecondOfTheDay());
            return LocalTimeToUtc((timepoint <= localTime) ? timepoint : timepoint.AddDays(-7));
        }
        public DateTime GetNextTime(DateTime utcNow)
        {
            DateTime localTime = UtcToLocalTime(utcNow);
            DateTime localDate = localTime.Date;
            DateTime timepoint = localDate.AddDays(((int)DayOfWeek) - (int)localDate.DayOfWeek).AddSeconds(SecondOfTheDay());
            return LocalTimeToUtc((timepoint <= localTime) ? timepoint.AddDays(7) : timepoint);
        }

        public Tuple<DateTime, DateTime> GetLastAndNextTime(DateTime utcNow)
        {
            DateTime localTime = UtcToLocalTime(utcNow);
            DateTime localDate = localTime.Date;
            DateTime timepoint = localDate.AddDays(((int)DayOfWeek) - (int)localDate.DayOfWeek).AddSeconds(SecondOfTheDay());
            return (timepoint <= localTime)
                       ? new Tuple<DateTime, DateTime>(LocalTimeToUtc(timepoint), LocalTimeToUtc(timepoint.AddDays(7)))
                       : new Tuple<DateTime, DateTime>(LocalTimeToUtc(timepoint.AddDays(-7)), LocalTimeToUtc(timepoint));
        }

        public bool IsValid()
        {
            return TimeZoneInfo != null && mHour >= 0 && mHour <= 23 && mMinute >= 0 && mMinute <= 59 && mSecond>=0 && mSecond<=59;
        }
        public TimeOfWeek AddSeconds(int seconds)
        {
            if (Math.Abs(seconds) >= 60 * 60 * 24 * 7)
                throw new Exception("Too much number of seconds " + seconds);

            if (seconds < 0)
                seconds += 60 * 60 * 24 * 7;

            DayOfWeek dw = DayOfWeek;
            int h = Hour;
            int m = Minute;
            int s = Second;

            if (seconds > 0)
            {
                s += seconds;
                if (s >= 60)
                {
                    m += s/60;
                    s %= 60;

                    if (m >= 60)
                    {
                        h += m/60;
                        m %= 60;
                        if (h >= 24)
                        {
                            int daysToAdd = h/24;
                            h %= 24;
                            dw = (DayOfWeek) (((int) dw + daysToAdd)%7);
                        }
                    }
                }

            }
            return new TimeOfWeek(TimeZoneId, dw, h, m, s);

        }

        public TimeOfWeek AddMinutes(int minutes)
        {
            return AddSeconds(minutes*60);
            //if (Math.Abs(minutes) >= 60*24*7)
            //throw new Exception("Too much number of minutes " + minutes);

            //if (minutes < 0)
            //    minutes += 60 * 24 * 7;

            //DayOfWeek dw = DayOfWeek;
            //int h = Hour;
            //int m=Minute;

            //if (minutes>0)
            //{
            //    m += minutes;
            //    if (m>=60)
            //    {
            //        h += (m/60);
            //        m %= 60;
            //        if (h>=24)
            //        {
            //            int daysToAdd = h / 24;
            //            h %= 24;
            //            dw = (DayOfWeek)(((int)dw + daysToAdd) % 7);
            //        }
            //    }
            //}
            //return new TimeOfWeek(TimeZoneId, dw, h, m);
          
        }
        public override string ToString()
        {
            if (Second==0)
                return string.Format("{0} {1}:{2}, {3}",
                                     DayOfWeek, Hour.ToString("00"), Minute.ToString("00"), TimeZoneId);
            return string.Format("{0} {1}:{2}:{3}, {4}",
                                 DayOfWeek, Hour.ToString("00"), Minute.ToString("00"), Second.ToString("00"),
                                 TimeZoneId);
        }

    }

    /// <summary>
    /// Specifies time of the day expressed for specified timezone
    /// </summary>
    public class TimeOfDay
    {
        /// <summary>
        /// Represents timezone info
        /// </summary>
        [XmlIgnore]
        public TimeZoneInfo TimeZoneInfo { get; private set; }
        private string mTimeZoneId;
        private int mHour, mMinute, mSecond;

        /// <summary>
        /// timezone id (a member of windows timezones enumeration)
        /// </summary>
        public string TimeZoneId
        {
            get { return mTimeZoneId; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    TimeZoneInfo = TimeZoneInfo.Utc;
                    mTimeZoneId = TimeZoneInfo.Id;
                }
                else
                {
                    TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(value);
                    mTimeZoneId = value;
                }

            }
        }
        /// <summary>
        /// Hour
        /// </summary>
        public int Hour
        {
            get
            {
                return mHour;
            }
            set
            {
                if (value < 0 || value > 23)
                    throw new Exception(
                        string.Format(
                            "Invalid value ({0}) of the property TimeOfDay.Hour; expected value must be in interval[0..23] ", value));
                mHour = value;
            }
        }
        /// <summary>
        /// Minute
        /// </summary>
        public int Minute
        {
            get
            {
                return mMinute;
            }
            set
            {
                if (value < 0 || value > 59)
                    throw new Exception(
                        string.Format(
                            "Invalid value ({0}) of the property TimeOfDay.Minute; expected value must be in interval[0..59] ", value));
                mMinute = value;
            }
        }
        /// <summary>
        /// Second
        /// </summary>
        public int Second
        {
            get
            {
                return mSecond;
            }
            set
            {
                if (value < 0 || value > 59)
                    throw new Exception(
                        string.Format(
                            "Invalid value ({0}) of the property TimeOfDay.mSecond; expected value must be in interval[0..59] ", value));
                mSecond = value;
            }
        }
        public TimeOfDay() { }
        public TimeOfDay(string timeZoneId, int hour, int minute, int second = 0)
        {
            TimeZoneId = timeZoneId;
            Hour = hour;
            Minute = minute;
            Second = second;
        }
        public TimeOfDay(TimeOfDay from)
        {
            TimeZoneId = from.TimeZoneId;
            Hour = from.Hour;
            Minute = from.Minute;
            Second = from.Second;
        }

        public bool IsValid()
        {
            return TimeZoneInfo != null && mHour >= 0 && mHour <= 23 && mMinute >= 0 && mMinute <= 59 && mSecond >= 0 && mSecond <= 59;
        }

        public DateTime GetNextTime(DateTime utcNow)
        {
            DateTime localTime = UtcToLocalTime(utcNow);
            DateTime localDate = localTime.Date;
            DateTime timepoint = localDate.AddSeconds(SecondOfTheDay());
            return LocalTimeToUtc(timepoint > localTime ? timepoint : timepoint.AddDays(1));
        }
        private DateTime UtcToLocalTime(DateTime utcTime)
        {
            //return utcTime.Add(TimeZoneInfo.GetUtcOffset(utcTime));
            return TimeZoneInfo.UtcToLocalTime(utcTime);
        }
        private DateTime LocalTimeToUtc(DateTime localTime)
        {
            //return localTime.Add(-TimeZoneInfo.GetUtcOffset(localTime));
            return TimeZoneInfo.LocalTimeToUtc(localTime);
        }
        private int SecondOfTheDay()
        {
            return (Hour * 60 + Minute) * 60 + Second;
        }
    }

    public class RestartScheduler
    {
        private readonly TimeOfDay _restartTime;
        private readonly int _restartDurationInSeconds;
        private DateTime _utcTimePoint;
        private bool _firstCall;
        private bool _isWorkingTime;

        public RestartScheduler(TimeOfDay restartTime, int restartDurationInSeconds)
        {
            _restartTime = restartTime;
            _restartDurationInSeconds = restartDurationInSeconds;
            _firstCall = true;
        }
        public bool IsWorkingTime(DateTime utcNow)
        {
            if (_firstCall)
            {
                _firstCall = false;
                _utcTimePoint = _restartTime.GetNextTime(utcNow);
                _isWorkingTime = true;
                return true;
            }

            if (utcNow < _utcTimePoint)
                return _isWorkingTime;

            // utcNow>=_utcNextTime
            if (_isWorkingTime)
            {
                // crosses the next close time
                _utcTimePoint = _utcTimePoint.AddSeconds(_restartDurationInSeconds); // get re-open time
                if (utcNow >= _utcTimePoint) // next re-open time is also reached
                    _utcTimePoint = _restartTime.GetNextTime(utcNow); // update next close time and return false for only this call
                else
                    _isWorkingTime = false; // toggle to not-working state until next re-open time

                return false;
            }
            _utcTimePoint = _restartTime.GetNextTime(utcNow);
            _isWorkingTime = true;
            return true;
        }
    }

}
