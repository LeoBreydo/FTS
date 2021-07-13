using System;
using System.Runtime.Serialization;

namespace CommonStructures
{
    [Serializable]
    [DataContract]
    public class ExecutionUpdates
    {
        [DataMember]
        public long FillsUpdatesSequenceNumber;
        [DataMember]
        public string[] FillsUpdates;

        [DataMember]
        public long NewCompletedBarsSequenceNumber;

        [DataMember]
        public string[] NewCompletedBars;
        [DataMember]
        public string[] ComputingBars;

        [DataMember] public string BidAskSnapshots;

        [DataMember] public string LastTradingServerStartedTime;

        [DataMember]
        public string LastTradingCfgStartedAt;
        [DataMember]
        public string LastScheduleCfgStartedAt;
        [DataMember]
        public string LastTradingRestrictionsCfgStartedAt;
        [DataMember]
        public string LastTimeGridStartedAt;

        [DataMember] 
        public string StrategyStatusInfos;

        [DataMember]
        public string BrokerStatusInfos;
    }
}