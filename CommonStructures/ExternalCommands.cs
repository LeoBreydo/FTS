using System;

namespace CommonStructures
{//command_id = 0? start, 1 - stop, 2 - schedule, 3- unschedule, 4- startInLong, 5- startInShort, 6 - softStop, 7- resetOrder, 8 - repeatOrder*@    
    [Serializable]
    public enum ExternalCommands
    {
        Start = 0,
        Stop,

        SetScheduled,
        SetUnscheduled,

        StartLong,
        StartShort,
        SoftStop,
        ResetOrder,
        RestartOrder,

        RestartOrderAvoidBadBrokers,
        StartToTheoreticalPosition,

        MarketFilterOn = 11,
        MarketFilterOff = 12, 

        TrendMonitorOn=13,
        TrendMonitorOff = 14,

        EnableAndStart,     // group command for strategies
        Enable,   // group command for strategies
        // new 
        RestartAfterCriticalLoss,
        RestartAfterSymbolLoss
    }
}
