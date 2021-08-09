using System;

namespace CommonStructures
{
    /// <summary>
    /// Identifies the type of quote
    /// </summary>
    [Serializable]
    public enum QuoteTypes
    {
        /// <summary>
        /// an indicative quote (market is not tradable)
        /// </summary>
        Indicative = 0,
        /// <summary>
        /// market is tradable for specified volume 
        /// </summary>
        Tradable = 1,
        /// <summary>
        /// market is restricted tradable
        /// </summary>
        /// <remarks>
        /// Used by Citi in MassQuote messages
        /// </remarks>
        RestrictedTradable = 2,

        S5BidCorrection=3,
        S5AskCorrection = 4,
        S5MidCorrection = 5,
        /// <summary>
        /// quotes stream terminated flag (market is not tradable)
        /// </summary>
        QuoteCancelFlag = 8,
    }
   
    /// <summary>
    /// Quote update message from broker, quotes stream item
    /// </summary>
    /// <remarks>
    /// <para>
    /// A unit of information transmitted via market data channel.  
    /// Includes instrument to which the data refered to  and the data itself.
    /// </para>
    /// <para>
    /// Рыночные данные по инструменту представляют собой поток QuoteUpdate.
    /// QuoteUpdate, имеющая тип QuoteTypes.QuoteCancelFlag означает прекращение потока котировок (время в этом сообщении может быть не установлено),
    /// Первая котировка после QuoteTypes.QuoteCancelFlag означает возобновление потока котировок.
    /// В случае потери соединения приходит сообщение с QuoteCancelFlag для всех котировок (свойство QuoteUpdate.ForAllSymbols)
    /// </para>
    /// </remarks>
    public class QuoteUpdate
    {
        /// <summary>
        /// Identifier of the broker
        /// </summary>
        public readonly long BrokerID;
        public string Symbol;

        /// <summary>
        /// the type of quote
        /// </summary>
        public QuoteTypes QuoteType { get; private set; }

        public double BestBid { get; private set; }
        public long BestBidSize { get; private set; }
        public double BestAsk { get; private set; }
        public long BestAskSize { get; private set; }
        public double High, Low;

        /// <summary>
        /// time of the data
        /// </summary>
        public readonly DateTime TransactTime;// { get; private set; }

        public DateTime MessageCreatedTime { get; set; } // todo torename to ReceivedTime


        /// <summary>
        /// Unique ID of the quote assigned by broker.
        /// </summary>
        /// <remarks>
        /// Required for PreviouslyQuoted orders execution
        /// </remarks>
        public string QuoteID { get; private set; } // required for Previously quoted orders (conformance test, can be excluded afterwards)

        public bool IsTradable
        {
            get
            {
                return QuoteType switch
                {
                    QuoteTypes.Tradable => (BestBidSize > 0 && BestAskSize > 0),
                    QuoteTypes.RestrictedTradable => (BestBidSize > 0 && BestAskSize > 0),
                    _ => false
                };
            }
        }
        public bool IsQuoteCancelFlag { get { return QuoteType == QuoteTypes.QuoteCancelFlag; } }

        /// <summary>
        /// True value used when QuoteCancel is sent for the all broker instruments
        /// </summary>
        public readonly bool ForAllSymbols;

        /// <summary>
        /// Ctor
        /// </summary>
        public QuoteUpdate(long brokerId, string symbol,
            QuoteTypes quoteType, DateTime transactTime, double bestBid, long bestBidSize, double bestAsk, long bestAskSize, string quoteID)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            ForAllSymbols = false;

            QuoteType = quoteType;
            QuoteID = quoteID;
            TransactTime = transactTime;
            BestBid = bestBid;
            BestBidSize = bestBidSize;
            BestAsk = bestAsk;
            BestAskSize = bestAskSize;
        }

        public QuoteUpdate(long brokerId, string symbol, double bid, double ask, DateTime transactTime, bool isIndicative = false)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            ForAllSymbols = false;

            QuoteType = isIndicative ? QuoteTypes.Indicative : QuoteTypes.Tradable;
            QuoteID = "";
            TransactTime = transactTime;
            BestBid = bid;
            BestBidSize = 1;
            BestAsk = ask;
            BestAskSize = 1;
        }

        public static QuoteUpdate Create5sBarUpdate(long brokerId, string symbol, QuoteTypes qt,double high,double low, DateTime barOpenTime)
        {
            return qt switch
            {
                QuoteTypes.S5BidCorrection => new QuoteUpdate(brokerId, symbol, qt, high, low, barOpenTime),
                QuoteTypes.S5AskCorrection => new QuoteUpdate(brokerId, symbol, qt, high, low, barOpenTime),
                QuoteTypes.S5MidCorrection => new QuoteUpdate(brokerId, symbol, qt, high, low, barOpenTime),
                _ => throw new Exception("Unexpected QuoteType " + qt)
            };
        }
        private QuoteUpdate(long brokerId, string symbol, QuoteTypes qt, double high, double low, DateTime barOpenTime)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            ForAllSymbols = false;

            QuoteType = qt;
            QuoteID = "";
            TransactTime = barOpenTime;
            BestBid = BestAsk = -1;
            BestBidSize = BestAskSize = 0;
            High = high;
            Low = low;
        }


        /// <summary>
        /// Ctor for QuoteCancelFlag for the all broker instruments
        /// </summary>
        /// <param name="brokerId">broker ID</param>
        /// <param name="transactTime">time of the quote update</param>
        private QuoteUpdate(long brokerId, DateTime transactTime)
        {
            BrokerID = brokerId;
            ForAllSymbols = true;
            QuoteType = QuoteTypes.QuoteCancelFlag;
            TransactTime = transactTime;
        }

        /// <summary>
        /// Ctor for QuoteCancelFlag for the specified broker instrument
        /// </summary>
        private QuoteUpdate(long brokerId, string symbol, DateTime transactTime)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            ForAllSymbols = false;
            QuoteType = QuoteTypes.QuoteCancelFlag;
            TransactTime = transactTime;
        }

        /// <summary>
        /// Creates the new QuoteCancelFlag for the all broker instruments
        /// </summary>
        /// <param name="brokerID">brokerID</param>
        /// <param name="transactTime">message time (can be unspecified for cases like connection lost)</param>
        /// <returns></returns>
        public static QuoteUpdate MakeQuoteCancelFlagForAllSymbols(long brokerID, DateTime transactTime)
        {
            return new(brokerID, transactTime);
        }

        /// <summary>
        /// Creates the new QuoteCancelFlag for the specified broker instrument
        /// </summary>
        public static QuoteUpdate MakeQuoteCancelFlagForSymbol(long brokerID, string symbol, DateTime transactTime)
        {
            return new(brokerID, symbol, transactTime);
        }
    }
}
