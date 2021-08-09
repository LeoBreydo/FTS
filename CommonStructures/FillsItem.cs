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
        public int ClOrdId;
        public int OrderId;
        public long CumQty;
        #endregion

        public long BrokerID;
        public string ExecID;

        public string Symbol;
        public string ContractCode;
        public string Exchange;
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide OrderSide;
        public long Qty;
        public double Price;
        public DateTime TransactTime { get; set; }
        public DateTime FillsReceivedTime;

        public long SgnQty
        {
            get
            {
                return OrderSide switch
                {
                    OrderSide.Buy => Qty,
                    OrderSide.Sell => -Qty,
                    _ => 0
                };
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
            ContractCode = from.ContractCode;
            Exchange = from.Exchange;
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
                ContractCode = ContractCode,
                Exchange = Exchange,
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
            return
                $" ClOrdID={ClOrdId}, OrderID={OrderId}, BrokerID={BrokerID}, Symbol={Symbol}, ContractCode = {ContractCode}, Exchange = {Exchange}, Side={OrderSide}, Qty={Qty}, Price={Price}, CumQty={CumQty}, ExecId={ExecID}, Time={TransactTime}, ManualFillByUser={ManualFillByUser}";
        }
    }
}
