using System;
using System.Collections.Generic;
using TimeZoneConverter;

namespace Configurator.ViewModel
{
    static class ConvertHelper
    {
        public static int? MaxErrorsToUser(int maxErrorsPerDay)
        {
            if (maxErrorsPerDay == int.MaxValue) return null;
            return maxErrorsPerDay;
        }
        public static int AcceptMaxErrors(int? value)//maxErrorsPerDay_or_null
        {
            if (!value.HasValue)
                return Int32.MaxValue;

            int v = value.Value;
            if (v < 0) throw new Exception("MaxErrorsPerDay must be not-negative or empty");
            return v;
        }

        public static DateTime RoundDateTime(this DateTime dt, int stepInMinutes, bool down)
        {
            if (stepInMinutes < 1 || stepInMinutes > 30)
                throw new Exception("Invalid stepInMinutes, value must be in range [1..30]: " + stepInMinutes);

            // Leos version
            var min = dt.Minute;
            var rest = min % stepInMinutes;
            if (rest != 0)
            {
                if (down)
                    min -= rest;
                else
                    min += stepInMinutes - rest;
            }

            return dt.Date.AddMinutes(dt.Hour * 60 + min);

            //var q = n / stepInMinutes;
            //var ret = down
            //    ? dt.AddMinutes(stepInMinutes * q - n)
            //    : dt.AddMinutes(stepInMinutes * (q + 1) - n);
            //return ret;
        }
        private static readonly Dictionary<string, TimeZoneInfo> tziMap = new Dictionary<string, TimeZoneInfo>();
        public static DateTime TimeToUtc(this DateTime dt, string timeZoneName)
        {
            if (!tziMap.TryGetValue(timeZoneName, out TimeZoneInfo tzi))
                tziMap.Add(timeZoneName, tzi = TZConvert.GetTimeZoneInfo(timeZoneName));
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified), tzi);

        }

    }
}