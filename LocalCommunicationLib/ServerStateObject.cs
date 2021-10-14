using System.Collections.Generic;
using ProtoBuf;

namespace LocalCommunicationLib
{
    [ProtoContract]
    public class ServerStateObject
    {
        [ProtoMember(1)]
        public TradingServicesSummary Summary;
        [ProtoMember(2)]
        public Dictionary<string, ExchangeDetails> Details;
    }
    [ProtoContract]
    public class TradingServicesSummary
    {
        [ProtoMember(1)]
        public int Id;
        [ProtoMember(2)]
        public bool IsConnected;
        [ProtoMember(3)]
        public Restrictions RestrictionDetails;
        [ProtoMember(4)]
        public int DayErrorNbr;
        [ProtoMember(5)]
        public List<CurrencyGroupSummary> CGSummaries = new();
        [ProtoMember(6)]
        public List<ExchangeSummary> ExSummaries = new();
        [ProtoMember(7)]
        public List<Message> MessagesToShow;
    }
    [ProtoContract]
    public class CurrencyGroupSummary
    {
        [ProtoMember(1)]
        public string Currency;
        [ProtoMember(2)]
        public decimal UPL;
        [ProtoMember(3)]
        public decimal RPL;
    }
    [ProtoContract]
    public class ExchangeSummary
    {
        [ProtoMember(1)]
        public int Id;
        [ProtoMember(2)]
        public string Name;
        [ProtoMember(3)]
        public string Currency;
        [ProtoMember(4)]
        public decimal UPL;
        [ProtoMember(5)]
        public decimal RPL;
        [ProtoMember(6)]
        public Restrictions RestrictionDetails;
    }
    [ProtoContract]
    public class MarketOrStrategyDetails
    {
        [ProtoMember(1)]
        public bool IsMarket;
        [ProtoMember(2)]
        public int Id;
        [ProtoMember(3)]
        public string Name;
        [ProtoMember(4)]
        public decimal UPL;
        [ProtoMember(5)]
        public decimal RPL;
        [ProtoMember(6)]
        public int Position;
        [ProtoMember(7)]
        public Restrictions RestrictionDetails;
        [ProtoMember(8)]
        public decimal SessionResult;
        [ProtoMember(9)]
        public string Info;
    }
    [ProtoContract]
    public class ExchangeDetails
    {
        [ProtoMember(1)]
        public int Id;
        [ProtoMember(2)]
        public string Name;
        [ProtoMember(3)]
        public string Currency;
        [ProtoMember(4)]
        public bool IsConnected;
        [ProtoMember(5)]
        public decimal UPL;
        [ProtoMember(6)]
        public decimal RPL;
        [ProtoMember(7)]
        public Restrictions RestrictionDetails;
        [ProtoMember(8)]
        public string Info;
        [ProtoMember(9)]
        public List<Message> MessagesToShow;
        [ProtoMember(10)]
        public List<MarketOrStrategyDetails> MktOrStrategies = new();
        [ProtoMember(11)] public int DayErrorNbr;
    }

    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public string Tag;
        [ProtoMember(2)]
        public string Body;
    }

    [ProtoContract]
    public class Restrictions
    {
        [ProtoMember(1)]
        public int userStyle = 0;
        [ProtoMember(2)]
        public int schedStyle = 0;
        [ProtoMember(3)]
        public int lossStyle = 0;
        [ProtoMember(4)]
        public int parStyle = 0;
        [ProtoMember(5)]
        public int contrStyle = 0;
        [ProtoMember(6)]
        public int sessStyle = 0;
        [ProtoMember(7)]
        public int errStyle = 0;
        [ProtoMember(8)]
        public int mktStyle = 0;
    }

    public interface IServerStateObjectProvider
    {
        ServerStateObject GetState { get; }
    }
}
