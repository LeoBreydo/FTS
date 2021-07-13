using System;
using System.Linq;

namespace CommonStructures
{
    /// <summary>
    /// The snapshot of the last market price for specified instrument. 
    /// </summary>
    /// <remarks>
    /// The collection of snaphots transmitted by tradingServer by request and deserialized at the client side.
    /// See also CommonInterfaces.IBidAskSnapshotsMaker
    /// </remarks>
    public class BidAskSnapshot
    {
        public long BrokerID;
        public string Symbol;
        public double Bid;
        public double Ask;
        public DateTime Time;
        public bool NotTradable { get { return Time == DateTime.MinValue; } }

        public BidAskSnapshot(long brokerID, string symbol, double bid, double ask, DateTime time)
        {
            BrokerID = brokerID;
            Symbol = symbol;
            Bid = bid;
            Ask = ask;
            Time = time;
        }
        public static BidAskSnapshot MakeNotTradable(long brokerID, string symbol)
        {
            return new BidAskSnapshot(brokerID, symbol, 0, 0, DateTime.MinValue);
        }
        public static BidAskSnapshot[] DeseializeFromString(string serializedSnapshotsList)
        {
            return serializedSnapshotsList.Split('\n').Select(ParseStrItem).Where(ba => ba != null).ToArray();
        }
        private static BidAskSnapshot ParseStrItem(string strItem)
        {
            if (string.IsNullOrEmpty(strItem)) return null;
            string[] cells = strItem.Split('\t');
            switch (cells.Length)
            {
                case 2:
                case 5:
                    break;
                default:
                    return null;
            }
            long brokerID;
            string symbol;
            if (!long.TryParse(cells[0], out brokerID) || brokerID <= 0) return null;
            if (string.IsNullOrEmpty(symbol = cells[1])) return null;
            if (cells.Length == 2) return MakeNotTradable(brokerID, symbol);

            double bid, ask;
            DateTime time;
            if (!double.TryParse(cells[2], out bid)) return null;
            if (!double.TryParse(cells[3], out ask)) return null;
            if (!cells[4].TryParseDateTime(out time)) return null;
            return new BidAskSnapshot(brokerID, symbol, bid, ask, time);
        }
    }
}
