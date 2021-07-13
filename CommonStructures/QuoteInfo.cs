//#if TRANSMIT_FULLQUOTEBOOK

namespace CommonStructures
{
#if TRANSMIT_FULLQUOTEBOOK
    /// <summary>
    /// Market data transmitted from broker for specified instrument. The member of QuoteUpdate message.
    /// </summary>
    /// <remarks>
    /// Data can be subdivided into 3 categories:
    /// 1) single bid/ask tuples (used for instruments at specified volume,  contains single simple bid/ask tuples) 
    /// 2) array of bid/ask tuples (used for market stacks)
    /// 3) special value indicating the termination of quotes stream (QuoteType=QuoteTypes.QuoteCancelFlag with no BidAskInfos)
    /// </remarks>
    [Serializable]
    public struct QuoteInfo
    {
        /// <summary>
        /// the type of quote
        /// </summary>
        public QuoteTypes QuoteType { get; private set; }
        /// <summary>
        /// Unique ID of the quote assigned by broker.
        /// </summary>
        /// <remarks>
        /// Required for PreviouslyQuoted orders execution
        /// </remarks>
        public string QuoteID { get; private set; } // required for Previously quoted orders (conformance test, can be excluded afterwards)
        /// <summary>
        /// time of the data
        /// </summary>
        public TimeStamp TransactTime { get; private set; }
        /// <summary>
        /// Contains: 0 items if represents QuoteCancelFlag, single item for SingleQuote, array of items for the stack (MassQuote)
        /// </summary>
        public BidAskInfo[] BidAskInfos { get; private set; }

        public double BestBid { get { return BidAskInfos.Length == 0 ? 0 : BidAskInfos[0].Bid; }}
        public long BestBidSize { get { return BidAskInfos.Length == 0 ? 0 : BidAskInfos[0].BidSize; } }
        public double BestAsk { get { return BidAskInfos.Length == 0 ? 0 : BidAskInfos[0].Ask; } }
        public long BestAskSize { get { return BidAskInfos.Length == 0 ? 0 : BidAskInfos[0].AskSize; } }

        public bool IsQuoteCancelFlag { get { return QuoteType == QuoteTypes.QuoteCancelFlag; } }
        public bool IsTradable
        {
            get
            {
                switch (QuoteType)
                {
                    case QuoteTypes.Tradable:
                    case QuoteTypes.RestrictedTradable:
                        break;

                    default:
                        // QuoteCancelFlag or Indicative quote
                        return false;
                }

                return (BidAskInfos.Length > 0 && BidAskInfos[0].BidSize > 0 && BidAskInfos[0].AskSize > 0);
            }
        }

        /// <summary>
        /// explicit ctor
        /// </summary>
        public QuoteInfo(QuoteTypes quoteType, string quoteID, TimeStamp transactTime, BidAskInfo[] bidAskInfos)
            : this()
        {
            QuoteType = quoteType;
            QuoteID = quoteID;
            TransactTime = transactTime;
            BidAskInfos = bidAskInfos;
            if (BidAskInfos.Length==0)
                QuoteType=QuoteTypes.QuoteCancelFlag;
        }
        public QuoteInfo(QuoteTypes quoteType, TimeStamp transactTime, double bid,long bidSize,double ask,long askSize,string quoteID)
            : this()
        {
            QuoteType = quoteType;
            QuoteID = quoteID;
            TransactTime = transactTime;
            BidAskInfos = new[] {new BidAskInfo(bid, bidSize, ask, askSize)};
        }
        /// <summary>
        /// copy ctor
        /// </summary>
        public QuoteInfo(QuoteInfo from)
            : this()
        {
            QuoteType = from.QuoteType;
            QuoteID = from.QuoteID;
            TransactTime = from.TransactTime;
            BidAskInfos = from.BidAskInfos;
        }
        /// <summary>
        /// returns new item with QuoteType=QuoteCancelFlag
        /// </summary>
        public static QuoteInfo MakeQuoteCancelFlag(TimeStamp time)
        {
            return new QuoteInfo(QuoteTypes.QuoteCancelFlag, "", time, new BidAskInfo[0]);
        }
    }
#else
#endif
}
