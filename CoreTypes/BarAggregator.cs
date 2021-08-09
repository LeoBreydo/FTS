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
        public DateTime Start { get; private set; }
        /// <summary>
        /// Time of end of the last one-minute bar used for building this bar
        /// </summary>
        public DateTime End { get; private set; }
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
            => new Bar(current.O, Math.Max(current.H, next.High), Math.Min(current.L, next.Low),
                next.Close, current.Start, next.BarOpenTime.AddSeconds(5));
    }
    public class BarAggregator
    {
        private readonly int _gapInMinutes;
        private readonly XSecondsAggregationRules _rule;
        public Bar Current { get; private set; }

        public string MarketCode { get; }
        public string ContractCode { get; private set; }

        private bool _newContract = false;

        public BarAggregator(XSecondsAggregationRules rule, string marketCode, int gapInMinutes = int.MaxValue)
        {
            _gapInMinutes = gapInMinutes < 1 ? 1 : gapInMinutes;
            _rule = rule ?? throw new Exception("rule is null");
            MarketCode = marketCode;
            ContractCode = string.Empty;
            Current = null;
        }

        /// <summary>
        /// returns aggregated bar (if there is aggregating bar and it must be finished at current currentTime)
        /// + MarketCode + isNewContract
        /// </summary>
        /// <param name="utcNow">current time</param>
        /// <returns>aggregated bar or null</returns>
        public Tuple<Bar, string, bool> ProcessTime(DateTime utcNow)
        {
            if (Current == null) return null;
            if (_rule.IsBarCompleted(Current, utcNow, _gapInMinutes))
            {
                var completed = Current;
                completed.SetProcessedTime(utcNow);
                Current = null;
                var flag = _newContract;
                _newContract = false;
                return new Tuple<Bar, string, bool>(completed, MarketCode, flag);
            }
            return null;
        }

        /// <summary>
        /// returns aggregated bar (if there is aggregating bar)
        /// + MarketCode + isNewContract
        /// </summary>
        /// <param name="utcNow">current time</param>
        /// <returns>aggregated bar or null</returns>
        public Tuple<Bar, string, bool> ForceClose(DateTime utcNow)
        {
            if (Current == null) return null;
            else
            {
                var completed = Current;
                completed.SetProcessedTime(utcNow);
                Current = null;
                var flag = _newContract;
                _newContract = false;
                return new Tuple<Bar, string, bool>(completed, MarketCode, flag);
            }
        }
        /// <summary>
        /// returns aggregated bar if it will be finished by aggregating rules
        /// + MarketCode + isNewContract
        /// </summary>
        /// <param name="bar">new five-sec bar</param>
        /// <returns>aggregated bar or null</returns>
        public Tuple<Bar,string, bool> ProcessBar(Bar5s bar, DateTime utcNow)
        {
            if (MarketCode != bar.Symbol) return null;
            if (ContractCode != bar.ContractCode)
            {
                ContractCode = bar.ContractCode;
                _newContract = true;
                var completed = Current;
                Current = new Bar(bar);
                return completed == null ? null : new Tuple<Bar, string, bool>(completed, MarketCode, false);
            }
            if (Current == null)
            {
                Current = new Bar(bar);
                
                if (_rule.IsBarCompleted(Current))
                {
                    var completed = Current;
                    completed.SetProcessedTime(utcNow);
                    Current = null;
                    var flag = _newContract;
                    _newContract = false;
                    return new Tuple<Bar, string, bool>(completed, MarketCode, flag);
                }
                return null;
            }

            Current += bar;
            if (_rule.IsBarCompleted(Current))
            {
                var completed = Current;
                completed.SetProcessedTime(utcNow);
                Current = null;
                var flag = _newContract;
                _newContract = false;
                return new Tuple<Bar, string, bool>(completed, MarketCode, flag);
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
}
