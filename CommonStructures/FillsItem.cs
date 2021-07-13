using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CommonStructures
{
    /// <summary>
    /// Fills item
    /// </summary>
    public class FillsItem
    {
        #region the FillsItem key
        // these 3 field gives an unique identification of the FillsItem
        public string ClOrdId;
        public string OrderId;
        public long CumQty;
        #endregion

        public long BrokerID;
        public string ExecID;

        public string Symbol;
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide OrderSide;
        public long Qty;
        public double Price;
        public TimeStamp TransactTime { get; set; }
        public TimeStamp FillsReceivedTime;

        public long SgnQty
        {
            get
            {
                switch (OrderSide)
                {
                    case OrderSide.Buy:
                        return Qty;
                    case OrderSide.Sell:
                        return -Qty;
                    default:
                        return 0;

                }
            }
        }

        public string ManualFillByUser;


        public FillsItem() { }

        public FillsItem(FillsItem from)
        {
            ClOrdId = from.ClOrdId;
            OrderId = from.OrderId;
            CumQty = from.CumQty;
            BrokerID = from.BrokerID;
            Symbol = from.Symbol;
            OrderSide = from.OrderSide;
            Qty = from.Qty;
            Price = from.Price;
            TransactTime = from.TransactTime;
            FillsReceivedTime = from.FillsReceivedTime;
            ExecID = from.ExecID;
            ManualFillByUser = from.ManualFillByUser;
        }

        public FillsItem Separate(long separatedQty)
        {
            if (separatedQty <= 0 || separatedQty>=Qty) throw new ArgumentException("separatedQty");
            Qty -= separatedQty;
            // the field CumQty is not corrected here: it is not used by acceptor (TransactionManager) and its required an additional coding to update it.
            return new FillsItem
            {
                ClOrdId = ClOrdId,
                OrderId = OrderId,
                CumQty = CumQty, 
                BrokerID = BrokerID,
                Symbol = Symbol,
                OrderSide = OrderSide,
                Qty = separatedQty,
                Price = Price,
                TransactTime = TransactTime,
                FillsReceivedTime = FillsReceivedTime,
                ExecID = ExecID,
                ManualFillByUser = ManualFillByUser,
            };
        }

        public bool HasEqualsKeyWith(FillsItem other)
        {
            return ClOrdId == other.ClOrdId && CumQty == other.CumQty;
        }

        public override string ToString()
        {
            return string.Format(" ClOrdID={0}, OrderID={1}, BrokerID={2}, Symbol={3}, Side={4}, Qty={5}, Price={6}, CumQty={7}, ExecId={8}, Time={9}, ManualFillByUser={10}",
                ClOrdId, OrderId, BrokerID, Symbol, OrderSide, Qty, Price, CumQty, ExecID, TransactTime, ManualFillByUser);
        }
    }
}
