using CfgDescription;

namespace Messages
{
    public class LogicalInstrumentCreated : BaseMessage
    {
        public string LogicalInstrumentID;
        public string Symbol;
        public InstrumentKeyProperties InstrumentKeyProperties; // todo !!! may be to save it as string?

        public LogicalInstrumentCreated() : base(MessageNumbers.LogicalInstrumentCreated) { }
        public LogicalInstrumentCreated(string logicalInstrumentID, string currencyPair, InstrumentKeyProperties instrumentKeyProperties)
            : base(MessageNumbers.LogicalInstrumentCreated)
        {
            LogicalInstrumentID = logicalInstrumentID;
            Symbol = currencyPair;
            InstrumentKeyProperties = instrumentKeyProperties;
        }
    }

    public class LogicalInstrumentProviderEvent : BaseMessage
    {
        public string LogicalInstrumentID;
        public long ProviderID;
        public bool ProviderIsWorking;
        public long? CurrentProvierID;

        public LogicalInstrumentProviderEvent() : base(MessageNumbers.LogicalInstrumentProviderEvent) { }
        public LogicalInstrumentProviderEvent(string logicalInstrumentID, long providerID, bool providerIsWorking, long? currentProvierID)
            : base(MessageNumbers.LogicalInstrumentProviderEvent)
        {
            LogicalInstrumentID = logicalInstrumentID;
            ProviderID = providerID;
            ProviderIsWorking = providerIsWorking;
            CurrentProvierID = currentProvierID;
        }
    }
    public class LogicalInstrumentGapEvent : BaseMessage
    {
        public string LogicalInstrumentID;

        public LogicalInstrumentGapEvent() : base(MessageNumbers.LogicalInstrumentGapEvent) { }
        public LogicalInstrumentGapEvent(string logicalInstrumentID)
            : base(MessageNumbers.LogicalInstrumentGapEvent)
        {
            LogicalInstrumentID = logicalInstrumentID;
        }
    }
    public class StrategyBindedToLogicalInstrument:BaseMessage
    {
        public long StrategyID;
        public string LogicalInstrumentID;

        public StrategyBindedToLogicalInstrument() : base(MessageNumbers.StrategyBindedToLogicalInstrument) { }
        public StrategyBindedToLogicalInstrument(long strategyID, string logicalInstrumentID)
            : base(MessageNumbers.StrategyBindedToLogicalInstrument)
        {
            StrategyID = strategyID;
            LogicalInstrumentID = logicalInstrumentID;
        }
    }
}
