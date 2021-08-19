using System;
using PluginsInterfaces;

namespace CoreTypes
{
    public static class TimeFrameHelper 
    {
        public static int GetTimeGridSizeInMinutes(this string timeFrame)
        {
            if (string.IsNullOrEmpty(timeFrame)) return -1;

            if (int.TryParse(timeFrame, out int val) && val > 0 && val <= 1440) return val;

            int mult;
            switch (timeFrame.ToLower()[0])
            {
                case 'm':
                    mult = 1;
                    break;
                case 'h':
                    mult = 60;
                    break;
                case 'd':
                    return (int.TryParse(timeFrame.Substring(1), out val) && val == 1) ? 1440 : -1;
                default:
                    return -1;
            }
            if (!int.TryParse(timeFrame.Substring(1), out val) || val <= 0) return -1;

            val *= mult;
            return val <= 1440 ? val : -1;
        }
        public static void GetBarTimesM(int barSizeInMinutes, int synchronizationMinute, DateTime tm, out DateTime openTime, out DateTime closeTime)
        {
            DateTime date = tm.Date;
            DateTime nextSynchroTime;
            DateTime lastSynchroTime = nextSynchroTime = date.AddMinutes(synchronizationMinute);
            if (lastSynchroTime > tm)
                lastSynchroTime = lastSynchroTime.AddDays(-1);
            else
                nextSynchroTime = nextSynchroTime.AddDays(1);

            var finishedMinutes = (int)(tm - lastSynchroTime).TotalMinutes;
            int finishedBars = finishedMinutes / barSizeInMinutes;
            openTime = lastSynchroTime.AddMinutes(finishedBars * barSizeInMinutes);
            closeTime = openTime.AddMinutes(barSizeInMinutes);
            if (closeTime > nextSynchroTime)
                closeTime = nextSynchroTime;
        }
        public static bool SeparatePrefixFromTimeFrameExpression(this string timeFrame,
            out BarFormingPolicy policy, out string tfWithoutPrefix)
        {
            policy = default(BarFormingPolicy);
            tfWithoutPrefix = null;

            if (timeFrame == null || timeFrame.Length <= 2) return false;
            if (timeFrame.StartsWith("b:", StringComparison.OrdinalIgnoreCase))
            {
                policy = BarFormingPolicy.Bid;
                tfWithoutPrefix = timeFrame.Substring(2);
                return true;
            }
            if (timeFrame.StartsWith("a:", StringComparison.OrdinalIgnoreCase))
            {
                policy = BarFormingPolicy.Ask;
                tfWithoutPrefix = timeFrame.Substring(2);
                return true;
            }
            if (timeFrame.StartsWith("m:", StringComparison.OrdinalIgnoreCase))
            {
                policy = BarFormingPolicy.Middle;
                tfWithoutPrefix = timeFrame.Substring(2);
                return true;
            }

            return false;
        }
    }
}
