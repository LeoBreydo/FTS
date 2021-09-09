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

        private string _currentContract = string.Empty;
        private DateTime _lastBarCloseTime;
        public bool WorkingMode;  // meaning: true='load of history is done, process incoming minute bars in normal working way'; false='new completed bars placed to temp buffer, wait for historical data'
        private readonly List<Bar> _tempBuffer=new List<Bar>();

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

        public void StartNewContract()
        {
            MinTimeFrame.Reset();
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.Reset();
            _tempBuffer.Clear();
            _lastBarCloseTime = DateTime.MinValue;
        }

        public void AddMinuteBar(string contractCode, Bar bar)
        {
            if (_currentContract != contractCode)
            {
                _currentContract = contractCode;
                StartNewContract();
            }

            if (WorkingMode)
                AddMinuteBarImpl(bar);
            else
                _tempBuffer.Add(bar);
        }

        private void AddMinuteBarImpl(Bar bar)
        {
            if (bar.End <= _lastBarCloseTime) return; // bars must be strictly growing
            _lastBarCloseTime = bar.End;

            MinTimeFrame.Push(bar.End, new[] { bar.O, bar.H, bar.L, bar.C });
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.ApplyBar(bar);
        }

        public void AddHistoricalBars(string contractCode,List<Bar> historicalBars)
        {
            if (_currentContract != contractCode)
            {
                _currentContract = contractCode;
                StartNewContract();
            }

            foreach (Bar bar in historicalBars)
                AddMinuteBarImpl(bar);
            foreach (Bar bar in _tempBuffer)
                AddMinuteBarImpl(bar);
            _tempBuffer.Clear();

            WorkingMode = true;
        }

        public void ProcessTime(DateTime reachedTime)
        {
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.ProcessTime(reachedTime);
        }

    }


}
