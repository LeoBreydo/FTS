using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<string> HistoricalBarsForLog()
        {
            if (HistoricalData == null || HistoricalData.Count == 0) return null;
            return HistoricalData.SelectMany(t =>
                    t.historicalBars.Select(b =>
                        MessageStringProducer.HistoricalBarInfoString(b, t.mktExch, t.contrCode)))
                .ToList();
        }
    }
}
