using System;
using Indicators.Common;

namespace CoreTypes
{
    class ScaledTimeGridBarAggregator
    {
        private readonly int _barSizeInMinutes, _synchronizationMinute;

        private readonly Action<DateTime,double[]> EjectBar;
        private AggregatingBar mAggregatingBar;

        class AggregatingBar
        {
            public DateTime End;
            public double O,H,L,C;

            public double[] OHLC() { return new[] { O, H, L, C }; }
        }
        public ScaledTimeGridBarAggregator(Action<DateTime,double[]> ejectBar, int barSizeInMinutes, int synchronizationMinute)
        {
            _barSizeInMinutes = barSizeInMinutes;
            _synchronizationMinute = synchronizationMinute;
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
            TimeFrameHelper.GetBarTimesM(_barSizeInMinutes, _synchronizationMinute, tm, out _, out mAggregatingBar.End);
        }

        private void EjectCurrentBar()
        {
            EjectBar(mAggregatingBar.End, mAggregatingBar.OHLC());
            mAggregatingBar = null;
        }
    }
    class ScaledTimeGridTimeFrame
    {
        public readonly int BarSizeInMinutes;
        public readonly int SynchronizationMinute;
        public TimeFrameData Data { get; }
        private readonly ScaledTimeGridBarAggregator Aggregator;

        public ScaledTimeGridTimeFrame(int barSizeInMinutes, int synchronizationMinute=0)
        {
            BarSizeInMinutes = barSizeInMinutes;
            SynchronizationMinute = synchronizationMinute;
            Data = new TimeFrameData(100);
            Aggregator = new ScaledTimeGridBarAggregator((end,ohlc) => Data.Push(end, ohlc),  barSizeInMinutes, synchronizationMinute);
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