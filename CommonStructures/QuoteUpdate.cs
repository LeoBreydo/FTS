//#if TRANSMIT_FULLQUOTEBOOK 
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
    /// Member of the QuoteInfo. Contains the values and sizes of bid and ask.
    /// </summary>
    [Serializable]
    public struct BidAskInfo
    {
        /// <summary>
        /// ask quote 
        /// </summary>
        public double Ask { get; private set; }
        /// <summary>
        /// bid quote 
        /// </summary>
        public double Bid { get; private set; }
        /// <summary>
        /// bid size
        /// </summary>
        public long BidSize { get; private set; }
        /// <summary>
        /// ask size
        /// </summary>
        public long AskSize { get; private set; }
        /// <summary>
        /// explicit ctor
        /// </summary>
        public BidAskInfo(double bid, long bidSize, double ask, long askSize)
            : this()
        {
            Bid = bid;
            BidSize = bidSize;
            Ask = ask;
            AskSize = askSize;
        }
        /// <summary>
        /// copy ctor
        /// </summary>
        public BidAskInfo(BidAskInfo from)
            : this()
        {
            Bid = from.Bid;
            BidSize = from.BidSize;
            Ask = from.Ask;
            AskSize = from.AskSize;
        }

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
#if TRANSMIT_FULLQUOTEBOOK // not used now (was used previously to transmit the whole orderbook from provider to research and display in the ConformanceTest gui)
        /// <summary>
        /// Identifier of the broker
        /// </summary>
        public readonly long BrokerID;

        public string Symbol;
        public long Volume;

        public double BestBid { get { return QuoteInfo.BestBid; } }
        public double BestAsk { get { return QuoteInfo.BestAsk; } }
        public long BestBidSize { get { return QuoteInfo.BestBidSize; } }
        public long BestAskSize { get { return QuoteInfo.BestAskSize; } }

        public QuoteTypes QuoteType { get { return QuoteInfo.QuoteType; } }
        public bool IsQuoteCancelFlag { get { return QuoteInfo.IsQuoteCancelFlag; } }
        public bool IsTradable { get { return QuoteInfo.IsTradable; } }
        public TimeStamp TransactTime { get { return QuoteInfo.TransactTime; } }
        public string QuoteID { get { return QuoteInfo.QuoteID; } }
                
        // введено для совместимости с кодом ConformanceTest
        public BidAskInfo[] BidAskInfos { get { return QuoteInfo.BidAskInfos; } }

        /// <summary>
        /// True value used when QuoteCancel is sent for the all broker instruments
        /// </summary>
        public readonly bool ForAllSymbols;
        /// <summary>
        /// The transmitted market data
        /// </summary>
        public QuoteInfo QuoteInfo;

        /// <summary>
        /// Ctor
        /// </summary>
        public QuoteUpdate(long brokerId, string symbol, long volume, 
            QuoteTypes quoteType, TimeStamp transactTime, double bestBid, long bestBidSize, double bestAsk, long bestAskSize, string quoteID)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            Volume = volume;
            ForAllSymbols = false;
            QuoteInfo = new QuoteInfo(quoteType, transactTime, bestBid, bestBidSize, bestAsk, bestAskSize, quoteID);
        }
        /// <summary>
        /// Ctor to transmit the full bool
        /// </summary>
        public QuoteUpdate(long brokerId, string symbol, long volume,
            QuoteTypes quoteType, string quoteID, TimeStamp transactTime, BidAskInfo[] bidAskInfos)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            Volume = volume;
            ForAllSymbols = false;
            QuoteInfo = new QuoteInfo(quoteType, quoteID, transactTime, bidAskInfos);
        }

        public QuoteUpdate(long brokerId, string symbol, long volume, QuoteInfo quoteInfo)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            Volume = volume;
            ForAllSymbols = false;
            QuoteInfo = quoteInfo;
        }
        /// <summary>
        /// ctor for unit tests
        /// </summary>
        public QuoteUpdate(long brokerId, string symbol, double bid, double ask, TimeStamp time, bool isIndicative = false)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            Volume = 0;
            ForAllSymbols = false;

            QuoteInfo = new QuoteInfo(isIndicative ? QuoteTypes.Indicative : QuoteTypes.Tradable, time, bid, 1, ask, 1, "");
        }


        /// <summary>
        /// Ctor for QuoteCancelFlag for the all broker instruments
        /// </summary>
        /// <param name="brokerId">broker ID</param>
        /// <param name="timeStamp">time of the quote update</param>
        private QuoteUpdate(long brokerId, TimeStamp timeStamp)
        {
            BrokerID = brokerId;
            ForAllSymbols = true;
            QuoteInfo = QuoteInfo.MakeQuoteCancelFlag(timeStamp);
        }

        /// <summary>
        /// Ctor for QuoteCancelFlag for the specified broker instrument
        /// </summary>
        private QuoteUpdate(long brokerId, string symbol, long volume, TimeStamp timeStamp)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            Volume = volume;
            ForAllSymbols = false;
            QuoteInfo = QuoteInfo.MakeQuoteCancelFlag(timeStamp);
        }

        /// <summary>
        /// Creates the new QuoteCancelFlag for the all broker instruments
        /// </summary>
        /// <param name="brokerID">brokerID</param>
        /// <param name="timeStamp">message time (can be unspecified for cases like connection lost)</param>
        /// <returns></returns>
        public static QuoteUpdate MakeQuoteCancelFlagForAllSymbols(long brokerID, TimeStamp timeStamp)
        {
            return new QuoteUpdate(brokerID, timeStamp);
        }

        /// <summary>
        /// Creates the new QuoteCancelFlag for the specified broker instrument
        /// </summary>
        public static QuoteUpdate MakeQuoteCancelFlagForSymbol(long brokerID, string symbol, long volume, TimeStamp timeStamp)
        {
            return new QuoteUpdate(brokerID, symbol, volume, timeStamp);
        }
#else
        /// <summary>
        /// Identifier of the broker
        /// </summary>
        public readonly long BrokerID;

        public string Symbol; 
        public long Volume; // used by Citi only as part of the instrument specification

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
        public readonly TimeStamp TransactTime;// { get; private set; }

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
                switch (QuoteType)
                {
                    case QuoteTypes.Tradable:
                    case QuoteTypes.RestrictedTradable:
                        return (BestBidSize > 0 && BestAskSize > 0);

                    default:
                        // QuoteCancelFlag, any S5BarCorrection or Indicative quote
                        return false;
                }
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
        public QuoteUpdate(long brokerId, string symbol, long volume, 
            QuoteTypes quoteType, TimeStamp transactTime, double bestBid, long bestBidSize, double bestAsk, long bestAskSize, string quoteID)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            Volume = volume;
            ForAllSymbols = false;

            QuoteType = quoteType;
            QuoteID = quoteID;
            TransactTime = transactTime;
            BestBid = bestBid;
            BestBidSize = bestBidSize;
            BestAsk = bestAsk;
            BestAskSize = bestAskSize;
        }

        public QuoteUpdate(long brokerId, string symbol, double bid, double ask, TimeStamp transactTime, bool isIndicative = false)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            Volume = 0;
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
            switch (qt)
            {
                case QuoteTypes.S5BidCorrection:
                case QuoteTypes.S5AskCorrection:
                case QuoteTypes.S5MidCorrection:
                    return new QuoteUpdate(brokerId, symbol, qt, high, low, new TimeStamp(barOpenTime));
                default:
                    throw new Exception("Unexpected QuoteType " + qt);
            }
        }
        public static QuoteUpdate Create5sBarUpdate(long brokerId, string symbol, QuoteTypes qt, double high, double low, TimeStamp barOpenTime)
        {
            switch (qt)
            {
                case QuoteTypes.S5BidCorrection:
                case QuoteTypes.S5AskCorrection:
                case QuoteTypes.S5MidCorrection:
                    return new QuoteUpdate(brokerId, symbol, qt, high, low, barOpenTime);
                default:
                    throw new Exception("Unexpected QuoteType " + qt);
            }
        }
        private QuoteUpdate(long brokerId, string symbol, QuoteTypes qt, double high, double low, TimeStamp barOpenTime)
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
        private QuoteUpdate(long brokerId, TimeStamp transactTime)
        {
            BrokerID = brokerId;
            ForAllSymbols = true;
            QuoteType = QuoteTypes.QuoteCancelFlag;
            TransactTime = transactTime;
            //QuoteID = "";
            //BestBid = 0;
            //BestBidSize = 0;
            //BestAsk = 0;
            //BestAskSize = 0;
        }

        /// <summary>
        /// Ctor for QuoteCancelFlag for the specified broker instrument
        /// </summary>
        private QuoteUpdate(long brokerId, string symbol, long volume, TimeStamp transactTime)
        {
            BrokerID = brokerId;
            Symbol = symbol;
            Volume = volume;
            ForAllSymbols = false;
            QuoteType = QuoteTypes.QuoteCancelFlag;
            TransactTime = transactTime;
            //QuoteID = "";
            //BestBid = 0;
            //BestBidSize = 0;
            //BestAsk = 0;
            //BestAskSize = 0;
        }

        /// <summary>
        /// Creates the new QuoteCancelFlag for the all broker instruments
        /// </summary>
        /// <param name="brokerID">brokerID</param>
        /// <param name="transactTime">message time (can be unspecified for cases like connection lost)</param>
        /// <returns></returns>
        public static QuoteUpdate MakeQuoteCancelFlagForAllSymbols(long brokerID, TimeStamp transactTime)
        {
            return new QuoteUpdate(brokerID, transactTime);
        }

        /// <summary>
        /// Creates the new QuoteCancelFlag for the specified broker instrument
        /// </summary>
        public static QuoteUpdate MakeQuoteCancelFlagForSymbol(long brokerID, string symbol, long volume, TimeStamp transactTime)
        {
            return new QuoteUpdate(brokerID, symbol, volume, transactTime);
        }
#endif

    }
}
