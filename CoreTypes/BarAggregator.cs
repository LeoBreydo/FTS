using System;

namespace CoreTypes
{
    public class Bar
    {
        /// <summary>
        /// Open (first bar quotation)
        /// </summary>
        public double O { get; private set; }
        /// <summary>
        /// High (the highest bar quotation)
        /// </summary>
        public double H { get; private set; }
        /// <summary>
        /// Low )the lowest bar quotation)
        /// </summary>
        public double L { get; private set; }
        /// <summary>
        /// Close (last bar quotation)
        /// </summary>
        public double C { get; private set; }

        /// <summary>
        /// Time of start the first one-minute bar used for building this bar
        /// </summary>
        public DateTime Start { get; }
        /// <summary>
        /// Time of end of the last one-minute bar used for building this bar
        /// </summary>
        public DateTime End { get; }
        /// <summary>
        /// Time when bar was created
        /// </summary>
        public DateTime Processed { get; private set; }

        // all data must be correct
        public Bar(double o, double h, double l, double c, DateTime start, DateTime end)
        {
            O = o;
            H = h;
            L = l;
            C = c;
            Start = start;
            End = end;
            Processed = end;
        }

        public void Append(double newPrice)
        {
            if (H < newPrice) H = newPrice;
            if (L > newPrice) L = newPrice;
            C = newPrice;
        }
        public void SetProcessedTime(DateTime whenProcessed)
        {
            Processed = whenProcessed;
        }

        //public static Bar operator +(Bar current, Bar5s next)
        //    => new(current.O, Math.Max(current.H, next.High), Math.Min(current.L, next.Low),
        //        next.Close, current.Start, next.BarOpenTime.AddSeconds(5));

    }
    public class BarAggregator
    {
        private readonly int _gapInMinutes;
        private readonly MinuteAggregationRules _rule;
        public Bar Current { get; private set; }

        public string SymbolExchange { get; }
        public string ContractCode { get; private set; }

        public BarAggregator(MinuteAggregationRules rule, string symbolExchange, int gapInMinutes = int.MaxValue)
        {
            _gapInMinutes = gapInMinutes < 1 ? 1 : gapInMinutes;
            _rule = rule ?? throw new Exception("rule is null");
            SymbolExchange = symbolExchange;
            ContractCode = string.Empty;
            Current = null;
        }

        /// <summary>
        /// returns aggregated bar (if there is aggregating bar and it must be finished at current currentTime)
        /// + SymbolExchange + isNewContract
        /// </summary>
        /// <param name="utcNow">current time</param>
        /// <param name="ic">collector of completed minute bars</param>
        /// <returns>aggregated bar or null</returns>
        public void ProcessTime(DateTime utcNow, InfoCollector ic)
        {
            if (Current != null && _rule.IsBarCompleted(Current, utcNow, _gapInMinutes))
            {
                ic.AcceptMinuteBar(SymbolExchange, ContractCode, Current);
                Current = null;
            }
        }

        /// <summary>
        /// returns aggregated bar (if there is aggregating bar)
        /// + SymbolExchange + isNewContract
        /// </summary>
        /// <param name="utcNow">current time</param>
        /// <returns>aggregated bar or null</returns>
        public Tuple<Bar, string, string> ForceClose(DateTime utcNow)
        {
            if (Current == null) return null;

            var completed = Current;
            completed.SetProcessedTime(utcNow);
            Current = null;
            return new Tuple<Bar, string, string>(completed, SymbolExchange, ContractCode);
        }

        public void ProcessTick(DateTime time, string contractCode, double value, InfoCollector ic)
        {
            if (ContractCode != contractCode)
            {
                Current = null;
                ContractCode = contractCode;
            }

            if (Current == null)
                Current = BarFromTick(time, value);
            else if (time >= Current.End)
            {
                ic.AcceptMinuteBar(SymbolExchange, ContractCode, Current);
                Current = BarFromTick(time, value);
            }
            else
                Current.Append(value);
        }

        private static Bar BarFromTick(DateTime time, double value)
        {
            var begin = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, time.Kind);
            return new Bar(value, value, value, value, begin, begin.AddMinutes(1));
        }
    }
    public class XSecondsAggregationRules
    {
        private readonly int _seconds;

        // seconds%5 should be zero!
        public XSecondsAggregationRules(int seconds)
        {
            seconds = (seconds / 5) * 5;
            if (seconds < 5) seconds = 5;
            _seconds = seconds;
        }

        public bool IsBarCompleted(Bar currentBar, DateTime currentTime, int gapInMinutes)
        {
            return (currentTime - currentBar.Start).TotalSeconds >= _seconds
                   || (currentTime - currentBar.End).TotalMinutes >= gapInMinutes;
        }

        public bool IsBarCompleted(Bar currentBar)
        {
            return (currentBar.End - currentBar.Start).TotalSeconds >= _seconds;
        }
    }
    public class MinuteAggregationRules
    {
        public bool IsBarCompleted(Bar currentBar, DateTime currentTime, int gapInMinutes) =>
            currentBar.Start.Minute != currentTime.Minute || (currentTime - currentBar.Start).TotalSeconds >= 60;
    }

}
