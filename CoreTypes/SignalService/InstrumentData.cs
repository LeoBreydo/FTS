using System;
using System.Collections.Generic;
using System.Linq;
using Indicators.Common;

namespace CoreTypes
{
    class InstrumentData
    {
        public readonly InstrumentInfo InstrumentInfo;
        public readonly TimeFrameData MinTimeFrame;
        public readonly List<ScaledTimeGridTimeFrame> ScaledTimeframes;
        public readonly TimeFrameData MinmoveHolder,BpvHolder;

        private static readonly DateTime _someVeryOldTime = new DateTime(2000, 1, 1);
        public InstrumentData(InstrumentInfo instrumentInfo)
        {
            InstrumentInfo = instrumentInfo;
            MinTimeFrame = new TimeFrameData();
            ScaledTimeframes = new List<ScaledTimeGridTimeFrame>();

            MinmoveHolder= new TimeFrameData();
            MinmoveHolder.Push(_someVeryOldTime, new[] { InstrumentInfo.MinMove });

            BpvHolder = new TimeFrameData();
            BpvHolder.Push(_someVeryOldTime, new[] {InstrumentInfo.BigPointValue});
        }
        public TimeFrameData GetTimeGridTimeFrame(int barSizeInMinutes)
        {
            if (barSizeInMinutes <= 0) return null;

            if (barSizeInMinutes == 1) return MinTimeFrame;

            var scaledTf =ScaledTimeframes.FirstOrDefault(item => item.BarSizeInMinutes == barSizeInMinutes);
            if (scaledTf == null)
                ScaledTimeframes.Add(scaledTf = new ScaledTimeGridTimeFrame(barSizeInMinutes));

            return scaledTf.Data;
        }

        public void UpdateSettings(DateTime currentTime, double minMove, double bpv)
        {
            if (minMove > 0 && Math.Abs(minMove - InstrumentInfo.MinMove) > 1e-10)
            {
                InstrumentInfo.MinMove = minMove;
                MinmoveHolder.Push(currentTime, new[] { minMove });
            }
            if (bpv > 0 && Math.Abs(bpv - InstrumentInfo.BigPointValue) > 1e-10)
            {
                InstrumentInfo.BigPointValue = bpv;
                BpvHolder.Push(currentTime, new[] { bpv });
            }
        }
        public void AddMinuteBar(Bar bar)
        {
            MinTimeFrame.Push(bar.End, new[] {bar.O, bar.H, bar.L, bar.C});
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.ApplyBar(bar);
        }

        public void ProcessTime(DateTime time)
        {
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.ProcessTime(time);

        }
        public void StartNewContract()
        {
            MinTimeFrame.Reset();
            foreach (var scaledTf in ScaledTimeframes)
                scaledTf.Reset();
        }
    }


}
