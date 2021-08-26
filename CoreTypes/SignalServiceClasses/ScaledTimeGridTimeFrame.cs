using System;
using Indicators.Common;

namespace CoreTypes.SignalServiceClasses
{
    class ScaledTimeGridBarAggregator
    {
        private readonly int _barSizeInMinutes;

        private readonly Action<DateTime,double[]> EjectBar;
        private AggregatingBar mAggregatingBar;

        class AggregatingBar
        {
            public DateTime End;
            public double O,H,L,C;

            public double[] OHLC() { return new[] { O, H, L, C }; }
        }
        public ScaledTimeGridBarAggregator(Action<DateTime,double[]> ejectBar, int barSizeInMinutes)
        {
            _barSizeInMinutes = barSizeInMinutes;
            EjectBar = ejectBar;

        }

        // returns true if new bar completed
        public bool ProcessTime(DateTime time)
        {
            if (mAggregatingBar == null || time < mAggregatingBar.End) return false;
            EjectCurrentBar();
            return true;
        }

        public void ProcessBar(Bar minuteBar)
        {
            if (mAggregatingBar != null && minuteBar.Start >= mAggregatingBar.End)
                EjectCurrentBar();

            if (mAggregatingBar == null)
            {
                mAggregatingBar = new AggregatingBar();
                SetBarTimes(minuteBar.Start);
                mAggregatingBar.O = minuteBar.O;
                mAggregatingBar.H = minuteBar.H;
                mAggregatingBar.L = minuteBar.O;
                mAggregatingBar.C = minuteBar.O;
            }
            else
            {
                mAggregatingBar.H = Math.Max(mAggregatingBar.H, minuteBar.H);
                mAggregatingBar.L = Math.Min(mAggregatingBar.L, minuteBar.L);
                mAggregatingBar.C = minuteBar.C;
            }
            if (minuteBar.End >= mAggregatingBar.End)
                EjectCurrentBar();
        }

        public void ResetComputingBar()
        {
            mAggregatingBar = null;
        }

        private void SetBarTimes(DateTime tm)
        {
            TimeFrameHelper.GetBarTimesM(_barSizeInMinutes, 0, tm, out _, out mAggregatingBar.End);
        }

        private void EjectCurrentBar()
        {
            var ohlc = mAggregatingBar.OHLC();
            DebugLog.AddMsg(string.Format("EjectAggregatedBar ({0},{1},{2},{3},{4})", mAggregatingBar.End, ohlc[0], ohlc[1], ohlc[2], ohlc[3]));
            EjectBar(mAggregatingBar.End, mAggregatingBar.OHLC());
            mAggregatingBar = null;
        }
    }
    class ScaledTimeGridTimeFrame
    {
        public readonly int BarSizeInMinutes;
        public TimeFrameData Data { get; }
        private readonly ScaledTimeGridBarAggregator Aggregator;

        public ScaledTimeGridTimeFrame(int barSizeInMinutes)
        {
            BarSizeInMinutes = barSizeInMinutes;
            Data = new TimeFrameData(100);
            Aggregator = new ScaledTimeGridBarAggregator((end,ohlc) => Data.Push(end, ohlc),  barSizeInMinutes);
        }

        public void ApplyBar(Bar barOneMin)
        {
            Aggregator.ProcessBar(barOneMin);
        }

        public bool ProcessTime(DateTime time)
        {
            return Aggregator.ProcessTime(time);
        }
        public void Reset()
        {
            Data.Reset();
            Aggregator.ResetComputingBar();
        }
    }
}