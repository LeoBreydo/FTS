using System;

namespace CoreTypes
{
    public class Bar
    {
        /// <summary>
        /// Open (first bar quotation)
        /// </summary>
        public double O { get; }
        /// <summary>
        /// High (the highest bar quotation)
        /// </summary>
        public double H { get; }
        /// <summary>
        /// Low )the lowest bar quotation)
        /// </summary>
        public double L { get; }
        /// <summary>
        /// Close (last bar quotation)
        /// </summary>
        public double C { get; }

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

        public Bar(Bar5s bar) :
            this(bar.Open, bar.High, bar.Low, bar.Close, bar.BarOpenTime, bar.BarOpenTime.AddSeconds(5))
        { }

        public void SetProcessedTime(DateTime whenProcessed)
        {
            Processed = whenProcessed;
        }

        public static Bar operator +(Bar current, Bar5s next)
            => new(current.O, Math.Max(current.H, next.High), Math.Min(current.L, next.Low),
                next.Close, current.Start, next.BarOpenTime.AddSeconds(5));
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
        /// <returns>aggregated bar or null</returns>
        public Tuple<Bar, string, string> ProcessTime(DateTime utcNow)
        {
            if (Current == null) return null;
            if (_rule.IsBarCompleted(Current, utcNow, _gapInMinutes))
            {
                var completed = Current;
                completed.SetProcessedTime(utcNow);
                Current = null;
                return new Tuple<Bar, string, string>(completed, SymbolExchange, ContractCode);
            }
            return null;
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

        /// <summary>
        /// returns aggregated bar if it will be finished by aggregating rules
        /// + SymbolExchange + isNewContract
        /// </summary>
        /// <param name="bar">new five-sec bar</param>
        /// <param name="utcNow"></param>
        /// <returns>aggregated bar or null</returns>
        public Tuple<Bar, string, string> ProcessBar(Bar5s bar, DateTime utcNow)
        {
            if (SymbolExchange != bar.SymbolExchange) return null;
            if (ContractCode != bar.ContractCode)
            {
                var prevCC = ContractCode;
                ContractCode = bar.ContractCode;
                var completed = Current;
                Current = new Bar(bar);
                return completed == null ? null : new Tuple<Bar, string, string>(completed, SymbolExchange, prevCC);
            }
            if (Current == null)
            {
                Current = new Bar(bar);
                
                if (_rule.IsBarCompleted(Current))
                {
                    var completed = Current;
                    completed.SetProcessedTime(utcNow);
                    Current = null;
                    return new Tuple<Bar, string, string>(completed, SymbolExchange, ContractCode);
                }
                return null;
            }

            Current += bar;
            if (_rule.IsBarCompleted(Current))
            {
                var completed = Current;
                completed.SetProcessedTime(utcNow);
                Current = null;
                return new Tuple<Bar, string, string>(completed, SymbolExchange, ContractCode);
            }
            return null;
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

        public bool IsBarCompleted(Bar currentBar)=> currentBar.End.Second == 0;
    }

}
