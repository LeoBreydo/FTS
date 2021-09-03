using System;
using System.Collections.Generic;
using System.Linq;
using Indicators.Common;

namespace CoreTypes.SignalServiceClasses
{
    class InstrumentData
    {
        private readonly MarketConfiguration InstrumentInfo;
        private double _minMove;
        private int _bpv;

        public readonly TimeFrameData MinTimeFrame;
        public readonly List<ScaledTimeGridTimeFrame> ScaledTimeframes;
        public readonly TimeFrameData MinmoveHolder, BpvHolder;

        private static readonly DateTime _someVeryOldTime = new DateTime(2000, 1, 1);
        public InstrumentData(MarketConfiguration instrumentInfo)
        {
            InstrumentInfo = instrumentInfo;
            MinTimeFrame = new TimeFrameData();
            ScaledTimeframes = new List<ScaledTimeGridTimeFrame>();

            _minMove = InstrumentInfo.MinMove;
            MinmoveHolder = new TimeFrameData();
            MinmoveHolder.Push(_someVeryOldTime, new[] { _minMove });

            _bpv = InstrumentInfo.BigPointValue;
            BpvHolder = new TimeFrameData();
            BpvHolder.Push(_someVeryOldTime, new[] { (double)_bpv });
        }
        public TimeFrameData GetTimeGridTimeFrame(int barSizeInMinutes)
        {
            if (barSizeInMinutes <= 0) return null;

            if (barSizeInMinutes == 1) return MinTimeFrame;

            var scaledTf = ScaledTimeframes.FirstOrDefault(item => item.BarSizeInMinutes == barSizeInMinutes);
            if (scaledTf == null)
                ScaledTimeframes.Add(scaledTf = new ScaledTimeGridTimeFrame(barSizeInMinutes));

            return scaledTf.Data;
        }

        public void UpdateSettings(DateTime currentTime, int bpv, double minMove)
        {
            if (bpv > 0 && bpv != _bpv)
            {
                InstrumentInfo.BigPointValue = _bpv = bpv;
                BpvHolder.Push(currentTime, new[] { (double)_bpv });
            }
            if (minMove > 0 && Math.Abs(minMove - _minMove) > 1e-10)
            {
                InstrumentInfo.MinMove = _minMove = minMove;
                MinmoveHolder.Push(currentTime, new[] { _minMove });
            }
        }

        private string _currentContract = string.Empty;
        public void AddMinuteBar(Bar bar,string contractCode)
        {
            if (_currentContract != contractCode)
            {
                _currentContract = contractCode;
                StartNewContract();
            }

            MinTimeFrame.Push(bar.End, new[] { bar.O, bar.H, bar.L, bar.C });
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.ApplyBar(bar);
        }

        public void ProcessTime(DateTime reachedTime)
        {
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.ProcessTime(reachedTime);

        }
        public void StartNewContract()
        {
            MinTimeFrame.Reset();
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.Reset();
        }
    }


}
