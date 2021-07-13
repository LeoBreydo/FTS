using CommonStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messages
{
    public class OrdStorage_OrderAddedMsg : BaseMessage
    {
        public long BrokerID;
        public string ClientOrderID;
        public bool IsAlien;
        public string OrderInfoSnapshot;

        public OrdStorage_OrderAddedMsg() : base(MessageNumbers.OrdStorage_OrderAddedMsg) { }
        public OrdStorage_OrderAddedMsg(long brokerID, string clientOrderID, string orderInfoSnapshot, bool isAlien)
            : base(MessageNumbers.OrdStorage_OrderAddedMsg)
        {
            BrokerID = brokerID;
            ClientOrderID = clientOrderID;
            IsAlien = isAlien;
            OrderInfoSnapshot = orderInfoSnapshot;
        }
    }
    public class OrdStorage_OrderUpdatedMsg : BaseMessage
    {
        public long BrokerID;
        public string ClientOrderID;
        public string OrderInfoSnapshot;

        public OrdStorage_OrderUpdatedMsg() : base(MessageNumbers.OrdStorage_OrderUpdatedMsg) { }
        public OrdStorage_OrderUpdatedMsg(long brokerID, string clientOrderID, string orderInfoSnapshot)
            : base(MessageNumbers.OrdStorage_OrderUpdatedMsg)
        {
            BrokerID = brokerID;
            ClientOrderID = clientOrderID;
            OrderInfoSnapshot = orderInfoSnapshot;
        }
    }
    public class OrdStorage_OrderFinishedMsg : BaseMessage
    {
        public long BrokerID;
        public string ClientOrderID;
        public string OrderInfoSnapshot;
        [JsonConverter(typeof(StringEnumConverter))]
        public FinishedStates FinishedState;

        public OrdStorage_OrderFinishedMsg() : base(MessageNumbers.OrdStorage_OrderFinishedMsg) { }
        public OrdStorage_OrderFinishedMsg(long brokerID, string clientOrderID, string orderInfoSnapshot, FinishedStates finishedState)
            : base(MessageNumbers.OrdStorage_OrderFinishedMsg)
        {
            BrokerID = brokerID;
            ClientOrderID = clientOrderID;
            OrderInfoSnapshot = orderInfoSnapshot;
            FinishedState = finishedState;
        }
    }
    public class OrdStorage_OrderFillsMsg : BaseMessage
    {
        public long BrokerID;
        public string ClientOrderID;
        public string OrderInfoSnapshot;
        public FillsItem Fill;
        public OrdStorage_OrderFillsMsg() : base(MessageNumbers.OrdStorage_OrderFillsMsg) { }
        public OrdStorage_OrderFillsMsg(long brokerID, string clientOrderID, string orderInfoSnapshot, FillsItem fill)
            : base(MessageNumbers.OrdStorage_OrderFillsMsg)
        {
            BrokerID = brokerID;
            ClientOrderID = clientOrderID;
            OrderInfoSnapshot = orderInfoSnapshot;
            Fill = fill;
        }
    }

    public class OrdStorage_OrderTimeoutMsg : BaseMessage
    {
        public long BrokerID;
        public string ClientOrderID;
        public string OrderInfoSnapshot;
        public OrdStorage_OrderTimeoutMsg() : base(MessageNumbers.OrdStorage_OrderTimeoutMsg) { }
        public OrdStorage_OrderTimeoutMsg(long brokerID, string clientOrderID, string orderInfoSnapshot)
            : base(MessageNumbers.OrdStorage_OrderTimeoutMsg)
        {
            BrokerID = brokerID;
            ClientOrderID = clientOrderID;
            OrderInfoSnapshot = orderInfoSnapshot;
        }
    }

}
