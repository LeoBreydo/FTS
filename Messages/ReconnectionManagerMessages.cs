namespace Messages
{
    public class ReconnectionManager_Activated:BaseMessage
    {
        public long BrokerID;
        public ReconnectionManager_Activated() : base(MessageNumbers.ReconnectionManager_Activated) { }
        public ReconnectionManager_Activated(long brokerID)
            : base(MessageNumbers.ReconnectionManager_Activated)
        {
            BrokerID = brokerID;
        }
        //public override string ToString() { return "ReconnectionManager_Activated, BrokerID=" + BrokerID; }
    }
    public class ReconnectionManager_Deactivated : BaseMessage
    {
        public long BrokerID;
        public bool HasFatalError;
        public ReconnectionManager_Deactivated() : base(MessageNumbers.ReconnectionManager_Deactivated) { }
        public ReconnectionManager_Deactivated(long brokerID, bool hasFatalError)
            : base(MessageNumbers.ReconnectionManager_Deactivated)
        {
            BrokerID = brokerID;
            HasFatalError = hasFatalError;
        }
        //public override string ToString()
        //{
        //    if (HasFatalError)
        //        return "ReconnectionManager_Deactivated, HasFatalError=true, BrokerID=" + BrokerID;
        //    return "ReconnectionManager_Deactivated, BrokerID=" + BrokerID; 
        //}
    }

    public class ReconnectionManager_CallQuoteSessionConnect : BaseMessage
    {
        public long BrokerID;
        public ReconnectionManager_CallQuoteSessionConnect() : base(MessageNumbers.ReconnectionManager_CallQuoteSessionConnect) { }
        public ReconnectionManager_CallQuoteSessionConnect(long brokerID)
            : base(MessageNumbers.ReconnectionManager_CallQuoteSessionConnect)
        {
            BrokerID = brokerID;
        }
        //public override string ToString() { return "ReconnectionManager_CallQuoteSessionConnect, BrokerID=" + BrokerID; }
    }
    public class ReconnectionManager_CallTradeSessionConnect : BaseMessage
    {
        public long BrokerID;
        public ReconnectionManager_CallTradeSessionConnect() : base(MessageNumbers.ReconnectionManager_CallTradeSessionConnect) { }
        public ReconnectionManager_CallTradeSessionConnect(long brokerID)
            : base(MessageNumbers.ReconnectionManager_CallTradeSessionConnect)
        {
            BrokerID = brokerID;
        }
        //public override string ToString() { return "ReconnectionManager_CallTradeSessionConnect, BrokerID=" + BrokerID; }
    }

    public class ReconnectionManager_CallQuoteSessionDisconnect : BaseMessage
    {
        public long BrokerID;
        public ReconnectionManager_CallQuoteSessionDisconnect() : base(MessageNumbers.ReconnectionManager_CallQuoteSessionDisconnect) { }
        public ReconnectionManager_CallQuoteSessionDisconnect(long brokerID)
            : base(MessageNumbers.ReconnectionManager_CallQuoteSessionDisconnect)
        {
            BrokerID = brokerID;
        }
        //public override string ToString() { return "ReconnectionManager_CallQuoteSessionDisconnect, BrokerID=" + BrokerID; }
    }
    public class ReconnectionManager_CallTradeSessionDisconnect : BaseMessage
    {
        public long BrokerID;
        public ReconnectionManager_CallTradeSessionDisconnect() : base(MessageNumbers.ReconnectionManager_CallTradeSessionDisconnect) { }
        public ReconnectionManager_CallTradeSessionDisconnect(long brokerID)
            : base(MessageNumbers.ReconnectionManager_CallTradeSessionDisconnect)
        {
            BrokerID = brokerID;
        }
        //public override string ToString() { return "ReconnectionManager_CallTradeSessionDisconnect, BrokerID=" + BrokerID; }
    }
    public class ReconnectionManager_ScheduleEvent : BaseMessage
    {
        public long BrokerID;
        public bool QuoteSession;
        public bool SwitchedOn;
        public ReconnectionManager_ScheduleEvent() : base(MessageNumbers.ReconnectionManager_ScheduleEvent) { }
        public ReconnectionManager_ScheduleEvent(long brokerID, bool isQuoteSession, bool switchedOn)
            : base(MessageNumbers.ReconnectionManager_ScheduleEvent)
        {
            BrokerID = brokerID;
            QuoteSession = isQuoteSession;
            SwitchedOn = switchedOn;
        }
        //public override string ToString() { return string.Format("ReconnectionManager_ScheduleEvent, BrokerID={0}, QuoteSession={1}, SwitchedOn={2}", BrokerID, QuoteSession, SwitchedOn); }
    }
}
