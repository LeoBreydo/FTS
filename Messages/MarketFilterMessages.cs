namespace Messages
{
    public class MarketFilterStates : BaseMessage
    {
        public string JsonSerializedMarketFilterStates;

        public MarketFilterStates() : base(MessageNumbers.MarketFilterStates) { }
        public MarketFilterStates(string jsonSerializedMarketFilterStates)
            : base(MessageNumbers.MarketFilterStates)
        {
            JsonSerializedMarketFilterStates = jsonSerializedMarketFilterStates;
        }
    }

    public class FilterCurrentResultChanged : BaseMessage
    {
        public string FilterName;
        public MarketState State;
        public FilterCurrentResultChanged() : base(MessageNumbers.FilterCurrentResultChanged) { }
        public FilterCurrentResultChanged(string filterName, MarketState state)
            : base(MessageNumbers.FilterCurrentResultChanged)
        {
            FilterName = filterName;
            State = state;
        }
    }
    public class FilterLockedResultChanged : BaseMessage
    {
        public string FilterName;
        public MarketState State;
        public FilterLockedResultChanged() : base(MessageNumbers.FilterLockedResultChanged) { }
        public FilterLockedResultChanged(string filterName, MarketState state)
            : base(MessageNumbers.FilterLockedResultChanged)
        {
            FilterName = filterName;
            State = state;
        }
    }
    public class FilterGroupStateChanged : BaseMessage
    {
        public string GroupName;
        public MarketState LongState;
        public MarketState ShortState;
        public FilterGroupStateChanged() : base(MessageNumbers.FilterGroupStateChanged) { }
        public FilterGroupStateChanged(string groupName, MarketState longState, MarketState shortState)
            : base(MessageNumbers.FilterGroupStateChanged)
        {
            GroupName = groupName;
            LongState = longState;
            ShortState = shortState;
        }
    }

    public class TransactionCancelledByMarketFilter : BaseMessage
    {
        public long StrategyID;
        public string TransactionID;
        public TransactionCancelledByMarketFilter() : base(MessageNumbers.TransactionCancelledByMarketFilter) { }
        public TransactionCancelledByMarketFilter(string transactionID,long strategyID)
            : base(MessageNumbers.TransactionCancelledByMarketFilter)
        {
            TransactionID = transactionID;
            StrategyID = strategyID;
        }
    }
}