using System;
using System.Collections.Generic;
using Binosoft.TraderLib.Indicators;
using Indicators.Common;
using PluginsInterfaces;

//using Primitives;

namespace Configurator.ViewModel
{
    public static class IndicatorsVerificator
    {
        class DPVerificatorForSingleInstrument : IDataProvider
        {
            public const string INSTRUMENT = "INSTRUMENT";
            private readonly bool _allowsRenkoBars;
            public DPVerificatorForSingleInstrument(bool allowsRenkoBars)
            {
                _allowsRenkoBars = allowsRenkoBars;
            }
            public bool ExistsInstrument(string instrumentName)
            {
                return !string.IsNullOrEmpty(instrumentName) && string.Equals(instrumentName, INSTRUMENT, StringComparison.OrdinalIgnoreCase);
            }

            public TimeFrameData GetTimeGridTimeFrame(string instrument, string timeframe)
            {
                return string.Equals(instrument, INSTRUMENT, StringComparison.OrdinalIgnoreCase)
                    ? new TimeFrameData()
                    : null;
            }

            public TimeFrameData GetAggregatedTimeFrame(string instrument, string timeframe)
            {
                if (!_allowsRenkoBars) return null;
                BarFormingPolicy policy;
                double barSize;
                int gapSizeInMinutes, resetDay, resetMinute;
                return string.Equals(instrument, INSTRUMENT, StringComparison.OrdinalIgnoreCase) &&
                       timeframe.ParseRenkoTimeFrame(
                           out policy, out barSize, out gapSizeInMinutes, out resetMinute, out resetDay)
                    ? new TimeFrameData()
                    : null;
            }

            public bool GetInstrumentConstant(string instrument, string constantName, out double value)
            {
                if (string.Equals(instrument, INSTRUMENT, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(constantName, "MinMove", StringComparison.OrdinalIgnoreCase))
                {
                    value = 1e-5;
                    return true;
                }
                value = 0;
                return false;
            }

        }
        public static string TryCreateIndicators(string timeframe, params string[] expressions)
        {
            var errors = new List<string>();

            var container = new CommonIndicatorsContainer(new DPVerificatorForSingleInstrument(true));
            foreach (string expr in expressions)
            {
                Indicator ir = container.GetIndicator(1, DPVerificatorForSingleInstrument.INSTRUMENT, timeframe, expr);
                if (ir == null || !ir.IsInited)
                    errors.Add(expr + ": " + container.LastError);
            }
            if (errors.Count == 0) return null;
            return "Can't create the following indicators:\n " + string.Join("\n ", errors);
        }
        public static bool CanCreateIndicator(string timeframe, string expression)
        {
            if (string.IsNullOrEmpty(timeframe) || string.IsNullOrEmpty(expression)) return false;
            return TryCreateIndicators(timeframe, expression) == null;
        }
        public static bool IsValidTimeFrame(string timeframe, bool allowsRenkoBars, out string normalizedTimeFrame)
        {
            normalizedTimeFrame = null;

            var container = new CommonIndicatorsContainer(new DPVerificatorForSingleInstrument(allowsRenkoBars));
            foreach (string ohlc in new[] { "open", "high", "low", "close" })
            {
                Indicator ir = container.GetIndicator(1, DPVerificatorForSingleInstrument.INSTRUMENT, timeframe, ohlc);
                if (ir == null || !ir.IsInited)
                    return false;
            }
            normalizedTimeFrame = timeframe.NormalizeTimeGridTimeFrame(false) ?? timeframe;
            return true;
        }

        public static bool VerifyAdditionalTimeFrame(string timeframe)
        {
            string normalizedTimeFrame;
            return IsValidTimeFrame(timeframe, true, out normalizedTimeFrame);
        }
    }
    public static class TimeGridHelper
    {
        public const string BID_PREFIX = "b:";
        public const string ASK_PREFIX = "a:";
        public const string MIDDLE_PREFIX = "m:";
        public static readonly string[] Prefixes = { BID_PREFIX, ASK_PREFIX, MIDDLE_PREFIX };

        public static bool IsTimeGridTimeFrame(this string strAgregator)
        {
            return strAgregator.NormalizeTimeGridTimeFrame(false) != null;
        }
        public static bool IsTicksOrTimeGridTimeFrame(this string strAgregator)
        {
            return string.Equals(strAgregator, "t", StringComparison.OrdinalIgnoreCase) ||
                   strAgregator.NormalizeTimeGridTimeFrame(false) != null;
        }

        public static string TimeFrameWithoutBarFormingPolicy(string timeframeExpression)
        {
            foreach (string pfx in Prefixes)
            {
                if (timeframeExpression.StartsWith(pfx, StringComparison.OrdinalIgnoreCase))
                    return timeframeExpression.Substring(pfx.Length);
            }
            return timeframeExpression;
        }
        public static bool SeparatePrefixFromTimeFrameExpression(string timeframeExpression, out string prefix, out string timeframeExpressionWithoutPrefix)
        {
            if (!string.IsNullOrEmpty(timeframeExpression))
                foreach (string pfx in Prefixes)
                {
                    if (timeframeExpression.StartsWith(pfx, StringComparison.OrdinalIgnoreCase))
                    {
                        prefix = pfx;
                        timeframeExpressionWithoutPrefix = timeframeExpression.Substring(pfx.Length);
                        return true;
                    }
                }
            prefix = null;
            timeframeExpressionWithoutPrefix = timeframeExpression;
            return false;
        }

        public static string NormalizeTimeGridTimeFrame(this string strAgregator, bool allowTicks = true)
        {
            string prefix, tf;
            SeparatePrefixFromTimeFrameExpression(strAgregator, out prefix, out tf);
            string res = NormalizeTimeGridTimeFrameImpl(tf, allowTicks && string.IsNullOrEmpty(prefix));
            return res == null ? null : prefix + res;
        }
        /// <summary>
        /// Возвращает нормализованное представление таймфреймов временной сетки
        /// </summary>
        private static string NormalizeTimeGridTimeFrameImpl(this string strAgregator, bool allowTicks)
        {
            if (string.IsNullOrEmpty(strAgregator)) return null;

            //if (string.IsNullOrEmpty(strAgregator)) return null;
            strAgregator = strAgregator.ToLower();

            string strValue;
            char chTimeFrameType;
            switch (strAgregator[0])
            {
                case 't':
                    if (!allowTicks) return null;
                    return (strAgregator.Length == 1) ? "t" : null;
                case 's':
                case 'm':
                case 'h':
                case 'd':
                case 'w':
                case 'n':
                case 'y':
                    chTimeFrameType = strAgregator[0];
                    strValue = strAgregator.Substring(1);
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    chTimeFrameType = 'm';
                    strValue = strAgregator.Substring(0);
                    break;
                default:
                    return null;
            }
            int Val;
            if (strValue == string.Empty)
                Val = 1;
            else if (!int.TryParse(strValue, out Val) || Val <= 0) return null;
            if (chTimeFrameType == 'w' && Val != 1) return null;

            if (chTimeFrameType == 's' && Val % 60 == 0)
            {
                chTimeFrameType = 'm';
                Val /= 60;
            }
            if (chTimeFrameType == 'm' && Val % 60 == 0)
            {
                chTimeFrameType = 'h';
                Val /= 60;
            }
            if (chTimeFrameType == 'h' && Val % 24 == 0)
            {
                chTimeFrameType = 'd';
                Val /= 24;
            }
            if (chTimeFrameType == 'm') return Val.ToString();
            return chTimeFrameType + Val.ToString();
        }
    }

}