using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace CommonStructures
{
    /// <summary>
    /// Stores UTC time values
    /// </summary>
    /// <remarks>
    /// This class intended to use instead of DateTime for the next goals:
    /// 1) to care about all the values are presented in the UTC;
    /// 2) to have explicit clear and compact values presentation when the owner structure is saved to xml file (to store states);
    /// 3) to speed up data transfer between processes or domains (uses windows UtcFileTime)
    /// 4) to declare explicit unspecified value (see TimeStamp.Null)
    /// The input DateTime values rounded up to 1 millisecond (minimmal discreteness)
    /// The input values with the Year lessEq than 1601 are interpreted as unspecified time (the UtcDateTime property matching to DateTime.MinValue)
    /// </remarks>
    [Serializable]
    [JsonConverter(typeof(TimeStampConverter))]
    public struct TimeStamp : IComparable<TimeStamp>
    {
        private long mUtcFileTime;
        private bool mIsMsSpecified;
        /// <summary>
        /// The stored value (uses windows UtcFileTime)
        /// </summary>
        [XmlIgnore]
        public long UtcFileTime
        {
            get { return mUtcFileTime; }
            set { mUtcFileTime = (value < 0) ? 0 : (value - value % 10000); }
        }
        //[XmlIgnore]
        public bool IsMillisecondValueSpecified
        {
            get { return mIsMsSpecified; }
        }

        /// <summary>
        /// The value text representation for Xml serialization (uses TimeHelper.TimeFormat)
        /// </summary>
        public string TxtValue
        {
            get { return (UtcFileTime == 0) ? "" : UtcDateTime.DateTimeToApiString(mIsMsSpecified); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    UtcFileTime = 0;
                    mIsMsSpecified = false;
                }
                else
                    UtcFileTime = value.ParseDateTimeUtc(out mIsMsSpecified).ToFileTimeUtc();
            }
        }
        /// <summary>
        /// Converts the string presentation of the value to the TimeStamp instance
        /// </summary>
        public static TimeStamp Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Null;

            bool msec;
            DateTime dt = value.ParseDateTimeUtc(out msec);
            return new TimeStamp(dt.ToFileTimeUtc(),msec);
        }
        public static bool TryParse(string value,out TimeStamp result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result= Null;
                return true;
            }

            DateTime dt;
            if (!value.TryParseDateTime(out dt))
            {
                result = Null;
                return false;
            }
            result = new TimeStamp(dt.ToFileTimeUtc(), value.Length >17);
            return true;
        }
        /// <summary>
        /// ctor from utcFileTime
        /// </summary>
        public TimeStamp(long utcFileTime, bool isMillisecondValueSpecified=true)
            : this()
        {
            UtcFileTime = utcFileTime;
            mIsMsSpecified = (utcFileTime != 0) && isMillisecondValueSpecified;
        }
        /// <summary>
        /// copy ctor
        /// </summary>
        public TimeStamp(TimeStamp other)
            : this()
        {
            UtcFileTime = other.UtcFileTime;
            mIsMsSpecified = other.mIsMsSpecified;
        }

        /// <summary>
        /// ctor, get TimeStamp for specified time.
        /// </summary>
        /// <remarks>
        /// The input time values with Kind=DateTimeKind.Unspecified are interpreted as UTC.
        /// The input time values rounded up to 1 millisecond.
        /// The input values with the Year lessEq than 1601 are interpreted as DateTime.MinValue
        /// </remarks>
        public TimeStamp(DateTime time,bool isMillisecondValueSpecified=true)
            : this()
        {
            if (time.Year <= 1601)
            {
                UtcFileTime = 0;
                mIsMsSpecified = false;
            }
            else
            {
                if (!isMillisecondValueSpecified)
                    time = time.AddMilliseconds(-time.Millisecond);
                mIsMsSpecified = isMillisecondValueSpecified;
                UtcFileTime = time.ToFileTimeUtc();
            }
            UtcFileTime = (time.Year <= 1601) ? 0 : time.ToFileTimeUtc();
            mIsMsSpecified = time!=DateTime.MinValue && isMillisecondValueSpecified;
        }

        /// <summary>
        /// Converts the time from unix time format (a number of seconds sincse 1970.01.01) to the TimeStamp value
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static TimeStamp FromUtcUnixTime(int unixTime)
        {
            return new TimeStamp(UnixBase.AddSeconds(unixTime), false);
        }

        /// <summary>
        /// Get the DateTime value
        /// </summary>
        public DateTime UtcDateTime { get { return (UtcFileTime == 0) ? DateTime.MinValue : DateTime.FromFileTimeUtc(UtcFileTime); } }
        private static readonly DateTime UnixBase = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// Converts the value to unix time format (a number of seconds sincse 1970.01.01)
        /// </summary>
        /// <returns></returns>
        public int ToUnixTime()
        {
            if (UtcFileTime == 0) return 0;
            var tm = UtcDateTime;
            if (tm <= UnixBase) return 0;
            return (int)(tm - UnixBase).TotalSeconds;
        }
        /// <summary>
        /// Get the Timestamp for currect utc time on the PC
        /// </summary>
        public static TimeStamp UtcNow
        {
            get
            {
                return new TimeStamp(DateTime.UtcNow);
            }
        }
        /// <summary>
        /// Get the Null Timestamp (means as unspecified time, matching to DateTime.MinValue)
        /// </summary>
        public static TimeStamp Null
        {
            get
            {
                return new TimeStamp(0);
            }
        }
        /// <summary>
        /// Is the value Null (unspecified) or not
        /// </summary>
        public bool IsNull { get { return UtcFileTime == 0; } }

        #region comparisons

        public int CompareTo(TimeStamp other)
        {
            return UtcFileTime.CompareTo(other.UtcFileTime);
        }

        /// <summary>
        /// returns true if a==b
        /// </summary>
        public static bool operator ==(TimeStamp a, TimeStamp b)
        {
            return a.UtcFileTime == b.UtcFileTime;
        }
        /// <summary>
        /// returns true if a!=b
        /// </summary>
        public static bool operator !=(TimeStamp a, TimeStamp b)
        {
            return a.UtcFileTime != b.UtcFileTime;
        }
        /// <summary>
        /// returns true if a less than b (Null is less than not-null values, the two null values are equals)
        /// </summary>
        public static bool operator <(TimeStamp a, TimeStamp b)
        {
            return a.UtcFileTime < b.UtcFileTime;
        }
        /// <summary>
        /// returns true if a greater than b (Null is less than not-null values, the two null values are equals)
        /// </summary>
        public static bool operator >(TimeStamp a, TimeStamp b)
        {
            return a.UtcFileTime > b.UtcFileTime;
        }
        /// <summary>
        /// returns true if a less or equals than b (Null is less than not-null values, the two null values are equals)
        /// </summary>
        public static bool operator <=(TimeStamp a, TimeStamp b)
        {
            return a.UtcFileTime <= b.UtcFileTime;
        }
        /// <summary>
        /// returns true if a rgeater or equals than b (Null is less than not-null values, the two null values are equals)
        /// </summary>
        public static bool operator >=(TimeStamp a, TimeStamp b)
        {
            return a.UtcFileTime >= b.UtcFileTime;
        }
        /// <summary>
        /// Get the max of two TimeStamp values
        /// </summary>
        public static TimeStamp Max(TimeStamp a, TimeStamp b)
        {
            return (a >= b) ? a : b;
        }
        /// <summary>
        /// Get the min of two TimeStamp values
        /// </summary>
        public static TimeStamp Min(TimeStamp a, TimeStamp b)
        {
            return (a <= b) ? a : b;
        }

        public override bool Equals(object obj)
        {
            return (obj is TimeStamp) && (UtcFileTime == ((TimeStamp)obj).UtcFileTime);
        }
        public override int GetHashCode()
        {
            return (int)UtcFileTime;
        }
        /// <summary>
        /// Compare two TimeStamp values (Null is less than not-null values, the two null values are equals)
        /// </summary>
        public static int Compare(TimeStamp a, TimeStamp b)
        {
            return a.UtcFileTime.CompareTo(b.UtcFileTime);
        }

        #endregion

        /// <summary>
        /// returns the string representation of the TimeStamp
        /// </summary>
        public override string ToString()
        {
            return TxtValue;
        }
        public string ToStringDate()
        {
            return TxtValue.Substring(0,8);
        }

    }

    public class TimeStampConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((TimeStamp)value).TxtValue);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeStamp);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new TimeStamp { TxtValue = (string)reader.Value };
        }

    }

}
