using System;
using System.Collections.Generic;

namespace CoreTypes
{
    public class StateObject
    {
        public StateObject(DateTime currentUtcTime,
            bool isConnectionEstablished,
            List<TickInfo> tickInfoList, 
            List<ContractInfo> contractInfoList, 
            List<(string mktExch, string contrCode, List<Bar> historicalBars)> historicalData,
            List<OrderStateMessage> orderStateMessageList, List<Tuple<string,string>> textMessageList)
        {
            CurrentUtcTime = currentUtcTime;
            IsConnectionEstablished = isConnectionEstablished;
            TickInfoList = tickInfoList;
            ContractInfoList = contractInfoList;
            HistoricalData = historicalData;
            OrderStateMessageList = orderStateMessageList;
            TextMessageList = textMessageList;
        }

        public DateTime CurrentUtcTime { get; }
        public bool IsConnectionEstablished { get; }
        public List<TickInfo> TickInfoList { get; }
        public List<ContractInfo> ContractInfoList { get; }
        public List<(string mktExch, string contrCode, List<Bar> historicalBars)> HistoricalData;
        public List<OrderStateMessage> OrderStateMessageList { get; }
        public List<Tuple<string,string>> TextMessageList { get; }

    }

}
