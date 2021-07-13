using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CommonStructures
{
    [Serializable]
    [DataContract]
    public class MarketStateFiltersInformation
    {
        [DataMember]
        public bool EmptyModel { get; set; }
        [DataMember]
        public List<GroupFilterStates> GroupInfos { get; set; }
        [DataMember]
        public List<FilterInfo> FilterData { get; set; }
        [DataMember]
        public List<GroupDescription> GroupDescriptions { get; set; }


        public MarketStateFiltersInformation()
        {
            GroupInfos = new List<GroupFilterStates>();
            FilterData = new List<FilterInfo>();
            GroupDescriptions = new List<GroupDescription>();
            EmptyModel = false;
        }
    }


    [Serializable]
    [DataContract]
    public class GroupFilterStates
    {
        [DataMember]
        public long GroupId { get; set; }
        [DataMember]
        public List<FilterState> FilterInfos { get; set; }
        [DataMember]
        public int GroupLongState { get; set; }
        [DataMember]
        public int GroupShortState { get; set; }
        [DataMember]
        public int UserLongState { get; set; }
        [DataMember]
        public int UserShortState { get; set; }
    }

    [Serializable]
    [DataContract]
    public class GroupDescription
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public HashSet<long> StrategyIds { get; set; } // only real StrategyIds to pass to WebController
        public HashSet<long> VirtualStrategyIds { get; set; } // not a DataMember
    }

    [Serializable]
    [DataContract]
    public class FilterState
    {
        [DataMember]
        public int FilterId { get; set; }
        [DataMember]
        public bool Enabled { get; set; }
    }

    [DataContract]
    public class FilterInfo
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string N { get; set; }
        [DataMember]
        public string Flt { get; set; }
        [DataMember]
        public bool Cv { get; set; }
        [DataMember]
        public bool Lv { get; set; }

        // target state
        [DataMember]
        public int T { get; set; }
        [DataMember]
        public bool Mr { get; set; }
    }
}