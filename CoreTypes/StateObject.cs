using System;
using System.Collections.Generic;

namespace CoreTypes
{
    public class StateObject
    {
        public StateObject(DateTime currentUtcTime, List<TickInfo> tickInfoList, List<ContractInfo> contractInfoList, 
            List<Bar5s> barUpdateList, 
            List<OrderStateMessage> orderStateMessageList, List<Tuple<string,string>> textMessageList)
        {
            CurrentUtcTime = currentUtcTime;
            TickInfoList = tickInfoList;
            ContractInfoList = contractInfoList;
            BarUpdateList = barUpdateList;
            OrderStateMessageList = orderStateMessageList;
            TextMessageList = textMessageList;
        }

        public DateTime CurrentUtcTime { get; }
        public List<TickInfo> TickInfoList { get; }
        public List<ContractInfo> ContractInfoList { get; }
        public List<Bar5s> BarUpdateList { get; }
        public List<OrderStateMessage> OrderStateMessageList { get; }
        public List<Tuple<string,string>> TextMessageList { get; }

    }

}
