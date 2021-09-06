using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoreTypes
{
    public static class TimeZoneRegistry
    {
        private static readonly Dictionary<string, string> Cities;
        private static readonly Dictionary<string, string> CommonAbbreviations;
        private static readonly List<string> UtcOffsets;
        private static readonly Regex Regex;

        public static List<string> GetCities()
        {
            var res = Cities.Keys.ToList();
            res.Sort();
            return res;
        }
        public static List<string> GetCommonAbbreviations()
        {
            var res = CommonAbbreviations.Keys.ToList();
            res.Sort();
            return res;
        }
        public static List<string> GetUtcOffsets()
        {
            return UtcOffsets;
        }
        public static string GetTimeZoneId(string cityOrCommonAbbreviation)
        {
            string res;
            if (Cities.TryGetValue(cityOrCommonAbbreviation, out res))
                return res;
            if (CommonAbbreviations.TryGetValue(cityOrCommonAbbreviation, out res))
                return res;
            return null;
        }

        static TimeZoneRegistry()
        {
            Cities = new Dictionary<string, string>
            {
                {"TOKIO", "Tokyo Standard Time"},
                {"CAPE TOWN", "South Africa Standard Time"},
                {"BEIJING", "China Standard Time"},
                {"SHANGHAI", "China Standard Time"},
                {"SEOUL", "Korea Standard Time"},
                {"BRUSSELS", "W. Europe Standard Time"},
                {"COPENHAGEN", "W. Europe Standard Time"},
                {"MADRID", "W. Europe Standard Time"},
                {"PARIS", "W. Europe Standard Time"},
                {"AMSTERDAM", "W. Europe Standard Time"},
                {"BERLIN", "W. Europe Standard Time"},
                {"BERN", "W. Europe Standard Time"},
                {"ROME", "W. Europe Standard Time"},
                {"STOCKHOLM", "W. Europe Standard Time"},
                {"VIENNA", "W. Europe Standard Time"},
                {"LONDON", "GMT Standard Time"},
                {"SYDNEY", "AUS Eastern Standard Time"},
                {"NEW YORK", "US Eastern Standard Time"},
                {"CHICAGO", "Central Standard Time"}
            };

            CommonAbbreviations = new Dictionary<string, string>
            {
                {"UTC", TimeZoneInfo.Utc.Id},
                {"JST", "Tokyo Standard Time"},
                {"SAST", "South Africa Standard Time"},
                {"CST", "China Standard Time"},
                {"KST", "Korea Standard Time"},
                {"CET", "W. Europe Standard Time"},
                {"GMT", "GMT Standard Time"},
                {"AEST", "AUS Eastern Standard Time"},
                {"EST", "US Eastern Standard Time"},
                {"CT", "Central Standard Time"},
            };
            const string rangeString = "-12|-11:30|-11|-10:30|-10|-9:30|-9|-8:30|-8|-7:30|-7|-6:30|-6|-5:30|-5|-4:30|-4|-3:30|-3|-2:30|-2|-1:30|-1|-0:30|+0:0|+0:30|+1|+1:30|+2|+2:30|+3|+3:30|+4|+4:30|+5|+5:30|+5:45|+6|+6:30|+7|+7:30|+8|+8:30|+8:45|+9|+9:30|+10|+10:30|+11|+11:30|+12|+12:45|+13|+13:45|+14";
            UtcOffsets = rangeString.Split('|').Select(r => "UTC " + (r.Contains(':') ? r : r + ":00")).ToList();
            var range = rangeString.Replace("+", "\\+");
            var pattern = string.Format("UTC\\s*({0})", range);
            Regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public static TimeZoneInfo FindTimeZoneInfoByAnyAbbr(string abbr)
        {
            string err;
            return GetTimeZoneInfoByCity(abbr) ??
                   GetTimeZoneInfoByCommonAbbreviation(abbr) ??
                   GetTimeZoneByUtcOffset(abbr, out err);
        }

        public static bool IsValidAbbr(string abbr)
        {
            return Cities.ContainsKey(abbr) ||
                   CommonAbbreviations.ContainsKey(abbr) ||
                   UtcOffsets.Contains(abbr);
        }

        public static TimeZoneInfo GetTimeZoneInfoById(string tzId)
        {
            TimeZoneInfo tzInfo = null;
            try
            {
                tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            }
            catch (TimeZoneNotFoundException)
            {
                return null;
            }
            catch (InvalidTimeZoneException)
            {
                return null;
            }
            return tzInfo;
        }

        public static TimeZoneInfo GetTimeZoneInfoByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return null;
            city = city.Trim().ToUpperInvariant();
            if (!Cities.ContainsKey(city))
                return null;
            var tzId = Cities[city];
            return GetTimeZoneInfoById(tzId);
        }

        public static TimeZoneInfo GetTimeZoneInfoByCommonAbbreviation(string abbr)
        {
            if (string.IsNullOrWhiteSpace(abbr))
                return null;
            abbr = abbr.Trim().ToUpperInvariant();
            if (!CommonAbbreviations.ContainsKey(abbr))
                return null;
            var tzId = CommonAbbreviations[abbr];
            return GetTimeZoneInfoById(tzId);

        }

        public static TimeZoneInfo GetTimeZoneByUtcOffset(string utcOffset, out string error)
        {
            var t = ParseUtcOffset(utcOffset, out error);
            if (t == null)
                return null;
            utcOffset = utcOffset.Trim().ToUpperInvariant();

            var displayName = string.Format("({0}) CustomTimeZone", utcOffset);
            var standardName = displayName;
            var offset = new TimeSpan(t.Item1, t.Item2, 0);
            try
            {
                return TimeZoneInfo.CreateCustomTimeZone(standardName, offset, displayName, standardName);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Method parses utc offset string and returns [Hour,Minute] pair or null in a case of incorrect utcOffset string.
        /// </summary>
        /// <param name="utcOffset">Offset from UTC as string.
        /// formats:
        /// UTC+06:30
        /// UTC-05:00
        /// times must be as follows: (ref: https://en.wikipedia.org/wiki/List_of_UTC_time_offsets)
        /// [-12|-11:30|-11|-10:30|-10|-9:30|-9|-8:30|-8|-7:30|-7|-6:30|-6|-5:30|-5|-4:30|-4|-3:30|-3|-2:30|-2|-1:30|-1|-0:30|+0|+0:30|+1|+1:30|+2|+2:30|+3|+3:30|+4|+4:30|+5|+5:30|+5:45|+6|+6:30|+7|+7:30|+8|+8:30|+8:45|+9|+9:30|+10|+10:30|+11|+11:30|+12|+12:45|+13|+13:45|+14]
        /// offset minute must be in [0,15,30,45] list
        /// </param>
        /// <param name="error"></param>
        /// <returns></returns>
        private static Tuple<int, int> ParseUtcOffset(string utcOffset, out string error)
        {
            if (string.IsNullOrWhiteSpace(utcOffset))
            {
                error = "utcOffset is null or empty";
                return null;
            }
            utcOffset = utcOffset.Trim().ToUpperInvariant();
            if (!Regex.IsMatch(utcOffset))
            {
                error = "Bad format of utcOffset";
                return null;
            }
            var parts = utcOffset.Substring(3).Trim().Split(new[] { ':' });
            var hour = int.Parse(new string(parts[0].Skip(1).ToArray()));
            var minute = int.Parse(parts[1]);
            if (parts[0].StartsWith("-")) hour = -hour;
            error = string.Empty;
            return new Tuple<int, int>(hour, minute);
        }



    }
}
