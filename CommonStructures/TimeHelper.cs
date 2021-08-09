using System;

namespace CommonStructures
{
    /// <summary>
    /// Helper class to convert DateTime values to string and back
    /// </summary>
    /// <remarks>
    /// Defines the time format in string representation. 
    /// Cares about conversion DateTimevalues to the universal time before convert to string. 
    /// </remarks>
    public static class TimeHelper
    {
        /// <summary>
        /// The time format in string representation (is equals to the format of the time passed in a fix-messages)
        /// </summary>
        public const string TimeFormat = "yyyyMMdd-HH:mm:ss.fff";
        public const string TimeFormatWithoutMilliseconds = "yyyyMMdd-HH:mm:ss";
        /// <summary>
        /// Convert DateTime to string
        /// </summary>
        public static string DateTimeToApiString(this DateTime tm,bool isMillisecondValueSpecified=true)
        {
            return tm.ToUniversalTime().ToString(isMillisecondValueSpecified ? TimeFormat : TimeFormatWithoutMilliseconds);
        }
        /// <summary>
        /// Convert the string representation of the DateTime to the DateTime instance
        /// </summary>
        /// <param name="strTime">string representation of the DateTime to convert</param>
        /// <param name="isMillisecondSpecified">is the millisecond value specified or not</param>
        /// <returns>converted value</returns>
        public static DateTime ParseDateTimeUtc(this string strTime,out bool isMillisecondSpecified)
        {
            //the format: 60=20120723-12:30:26.582 (millisec may by ommited)
            int y = int.Parse(strTime[..4]);
            int M = int.Parse(strTime.Substring(4, 2));
            int d = int.Parse(strTime.Substring(6, 2));

            int h = int.Parse(strTime.Substring(9, 2));
            int m = int.Parse(strTime.Substring(12, 2));
            int s = int.Parse(strTime.Substring(15, 2));
            if (strTime.Length >= 21)
            {
                isMillisecondSpecified = true;
                return new DateTime(y, M, d, h, m, s, int.Parse(strTime.Substring(18, 3)), DateTimeKind.Utc);
            }
            isMillisecondSpecified = false;
            return new DateTime(y, M, d, h, m, s, 0, DateTimeKind.Utc);
            //int ms = (strTime.Length >= 21)
            //             ? int.Parse(strTime.Substring(18, 3))
            //             : 0;
            //return new DateTime(y, M, d, h, m, s, ms, DateTimeKind.Utc);
        }
        public static DateTime ParseIBDateTime(this string strTime)
        {
            //the format: 20120723  12:30:26
            int y = int.Parse(strTime[..4]);
            int M = int.Parse(strTime.Substring(4, 2));
            int d = int.Parse(strTime.Substring(6, 2));

            int h = int.Parse(strTime.Substring(10, 2));
            int m = int.Parse(strTime.Substring(13, 2));
            int s = int.Parse(strTime.Substring(16, 2));           
            return new DateTime(y, M, d, h, m, s, 0, DateTimeKind.Utc);            
        }
        public static DateTime ParseDateTimeUtc(this string strTime)
        {
            //the format: 60=20120723-12:30:26.582 (millisec may by ommited)
            int y = int.Parse(strTime[..4]);
            int M = int.Parse(strTime.Substring(4, 2));
            int d = int.Parse(strTime.Substring(6, 2));

            int h = int.Parse(strTime.Substring(9, 2));
            int m = int.Parse(strTime.Substring(12, 2));
            int s = int.Parse(strTime.Substring(15, 2));
            if (strTime.Length >= 21)
            {
                return new DateTime(y, M, d, h, m, s, int.Parse(strTime.Substring(18, 3)), DateTimeKind.Utc);
            }
            return new DateTime(y, M, d, h, m, s, 0, DateTimeKind.Utc);
            //int ms = (strTime.Length >= 21)
            //             ? int.Parse(strTime.Substring(18, 3))
            //             : 0;
            //return new DateTime(y, M, d, h, m, s, ms, DateTimeKind.Utc);
        }

        /// <summary>
        /// Convert if possible the string representation of the DateTime to the DateTime instance
        /// </summary>
        /// <param name="strTime">string representation of the DateTime to convert</param>
        /// <param name="res">converted value</param>
        /// <returns>true if conversion succeeded</returns>
        public static bool TryParseDateTime(this string strTime, out DateTime res)
        {
            try
            {
                res = DateTime.MinValue;
                if (strTime.Length != 17 && strTime.Length != 21) return false;

                //the format: 60=20120723-12:30:26.582 (millisec may by ommited)
                int ms;
                if (!int.TryParse(strTime[..4], out var y) || y < 1601) return false;
                if (!int.TryParse(strTime.Substring(4, 2), out var M) || M < 1 || M > 12) return false;
                if (!int.TryParse(strTime.Substring(6, 2), out var d) || d < 1 || d > 31) return false;

                if (!int.TryParse(strTime.Substring(9, 2), out var h) || h < 0 || h > 23) return false;
                if (!int.TryParse(strTime.Substring(12, 2), out var m) || m < 0 || h > 59) return false;
                if (!int.TryParse(strTime.Substring(15, 2), out var s) || s < 0 || s > 59) return false;
                if (strTime.Length == 17)
                    ms = 0;
                else
                    if (!int.TryParse(strTime.Substring(18, 3), out ms) || ms < 0 || ms > 999) return false;

                res = new DateTime(y, M, d, h, m, s, ms, DateTimeKind.Utc);
                return true;
            }
            catch (Exception)
            {
                res = DateTime.MinValue;
                return false;
            }
        }

    }
}
