using System;
using System.Collections.Generic;
using Messages;

namespace CoreTypes
{
    public class StateObject
    {
        public StateObject(DateTime currentUtcTime, List<TickInfo> tickInfoList, List<ContractInfo> contractInfoList, 
            List<Bar5s> barUpdateList, List<BaseMessage> baseMessageList, 
            List<OrderReportBase> orderReportBaseList, List<Tuple<string,string>> textMessageList)
        {
            CurrentUtcTime = currentUtcTime;
            TickInfoList = tickInfoList;
            ContractInfoList = contractInfoList;
            BarUpdateList = barUpdateList;
            BaseMessageList = baseMessageList;
            OrderReportBaseList = orderReportBaseList;
            TextMessageList = textMessageList;
        }

        public DateTime CurrentUtcTime { get; }
        public List<TickInfo> TickInfoList { get; }
        public List<ContractInfo> ContractInfoList { get; }
        public List<Bar5s> BarUpdateList { get; }
        public List<BaseMessage> BaseMessageList { get; }
        public List<OrderReportBase> OrderReportBaseList { get; }
        public List<Tuple<string,string>> TextMessageList { get; }

    }

}
