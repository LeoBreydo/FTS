using System;

namespace BrokerInterfaces
{
    public interface ISchedule
    {
        bool IsScheduleTime(DateTime utcNow);
    }
}
