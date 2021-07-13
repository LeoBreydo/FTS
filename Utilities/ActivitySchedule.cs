using System;
using System.Collections.Generic;

namespace Utilities
{
    public class WorkingInterval
    {
        public TimeOfWeek Begin;
        public TimeOfWeek End;

        public void Update(DateTime utcNow,out bool isWorkingState, out DateTime nextUpdateTime)
        {
            Tuple<DateTime, DateTime> t1 = Begin.GetLastAndNextTime(utcNow);
            Tuple<DateTime, DateTime> t2 = End.GetLastAndNextTime(utcNow);

            isWorkingState = t1.Item1 > t2.Item1;
            nextUpdateTime = t1.Item2 < t2.Item2 ? t1.Item2 : t2.Item2;
        }
    }
    public class ActivitySchedule
    {
        public List<WorkingInterval> WorkingIntervals = new List<WorkingInterval>();

        private DateTime nextUpdateTime = DateTime.MinValue;
        private bool _hasWorkingState;
        public bool IsWorkingTime(DateTime utcNow)
        {
            if (utcNow >= nextUpdateTime)
                UpdateState(utcNow);
            return _hasWorkingState;
        }

        private void UpdateState(DateTime utcNow)
        {
            if (WorkingIntervals.Count == 0)
            {
                nextUpdateTime = DateTime.MaxValue;
                _hasWorkingState = true;
                return;
            }

            _hasWorkingState = false;
            nextUpdateTime = DateTime.MaxValue;
            foreach (var interval in WorkingIntervals)
            {
                interval.Update(utcNow, out bool item_isWorkingState, out DateTime item_nextUpdateTime);
                if (item_isWorkingState)
                    _hasWorkingState = true;
                if (nextUpdateTime > item_nextUpdateTime)
                    nextUpdateTime = item_nextUpdateTime;
            }
        }

    }
}