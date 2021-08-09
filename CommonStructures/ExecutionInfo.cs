using System;

namespace CommonStructures
{
    public enum ExecutionInfoType
    {
        State=0, // summarize opened positions when TradingServer starts, stops and in the begin & end of the each file
        Fills,
        StateUpdated,
        // informational events
        OutdatedFillIgnored,
        WrongFillIgnored,
        PostRejection,
        Rejection,
        AttemtsLeft,
        Timeout,
        OrderCancelled,
        ResetOrder,

        VirtualState,
        VirtualFills,
        VirtualStateUpdated
    }
    public class ExecutionInfo
    {
        public DateTime Time;
        public string Symbol;
        public string Account;
        public long StrategyID;
        public long SymbolOpenedPosition;
        public long StrategyOpenedPosition;
        public ExecutionInfoType Type;

        // if Type==Fills or IgnoredFills
        public long FilledAmount;
        public double FilledPrice;
        public DateTime TransactTime;

        public UnwrappedTags Tags { get; private set; }

        //private string _tags="";

        public string TagsText
        {
            get { return Tags.Text; }
            set { Tags = new UnwrappedTags(value); }
        }
        public ExecutionInfo()
        {
            Tags = UnwrappedTags.Empty;
        }


        public bool IsValid()
        {
            // validate obligatory fields 
            return StrategyID > 0 && !string.IsNullOrEmpty(Symbol);
        }
        public string GetTagValue(string tagName)
        {
            return Tags.GetTagValue(tagName, FillsLogFormat.IsTextTag(tagName));
        }

        public override string ToString()
        {
            return string.Format(" {0} {1} {2}/{3} {4}", Time, Type, Symbol, StrategyID, Tags.Text);
        }
    }
}
