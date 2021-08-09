using System;
using System.Collections.Generic;
using Utilities;

namespace TWS_Activator
{
    public class TWSActivatorScheduleInfo
    {
        public TimeOfWeek Begin;
        public TimeOfWeek End;
        public TimeOfDay RestartTime;
        public int RestartDurationInSeconds;

        public bool IsValid()
        {
            return Begin != null && Begin.IsValid() &&
                   End != null && End.IsValid() &&
                   (RestartTime == null || RestartTime.IsValid() && RestartDurationInSeconds >= 0);
        }
    }

    public interface IActivatorScheduler
    {
        bool IsWorkingTime(DateTime utcNow);
    }

    public class ActivatorScheduler_AlwaysWorking : IActivatorScheduler
    {
        public bool IsWorkingTime(DateTime utcNow)
        {
            return true;
        }
    }
    public class ActivatorScheduler : IActivatorScheduler
    {
        private readonly ActivitySchedule _workingSchedule;
        private readonly RestartScheduler _restartScheduler;

        public ActivatorScheduler(TWSActivatorScheduleInfo twsActivatorSchedule)
        {
            _workingSchedule = new ActivitySchedule
            {
                WorkingIntervals = new List<WorkingInterval>
                {
                    new() {Begin = twsActivatorSchedule.Begin, End = twsActivatorSchedule.End}
                }
            };
            if (twsActivatorSchedule.RestartTime != null)
                _restartScheduler = new RestartScheduler(
                    twsActivatorSchedule.RestartTime,
                    twsActivatorSchedule.RestartDurationInSeconds);

        }
        public bool IsWorkingTime(DateTime utcNow)
        {
            bool isWorkingTime = _workingSchedule.IsWorkingTime(utcNow);
            if (_restartScheduler == null) return isWorkingTime;

            bool isWorkingTime2 = _restartScheduler.IsWorkingTime(utcNow);
            return isWorkingTime && isWorkingTime2;
        }
    }
}
