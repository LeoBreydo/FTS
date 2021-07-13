using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CommonStructures
{
    /// <summary>
    /// OrderSide
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderSide
    {
        /// <summary>
        /// OrderSide is not set 
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Buy
        /// </summary>
        Buy = 1,
        /// <summary>
        /// Sell
        /// </summary>
        Sell = 2,
    }
    /// <summary>
    /// OrderType
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderType
    {
        /// <summary>
        /// OrderType is not set
        /// </summary>
        [EnumMember(Value = "0")]
        Unknown = '0',
        /// <summary>
        /// Market
        /// </summary>
        [EnumMember(Value = "1")]
        Market = '1',
        /// <summary>
        /// Limit
        /// </summary>
        [EnumMember(Value = "2")]
        Limit = '2',
        /// <summary>
        /// PrevouslyQuoted
        /// </summary>
        [EnumMember(Value = "D")]
        PrevouslyQuoted = 'D',
    }
    /// <summary>
    /// TimeInForce
    /// </summary>
    public enum TimeInForce
    {
        /// <summary>
        /// TimeInForce is not set
        /// </summary>
        UnkTimeInForce = ' ',
        /// <summary>
        /// Day
        /// </summary>
        DAY = '0',
        /// <summary>
        ///  Good till cancel
        /// </summary>
        GTC = '1',
        /// <summary>
        /// Immediate or Cancel 
        /// </summary>
        IOC = '3',
        /// <summary>
        /// Fill or Kill. !!! The CITI api doc contains contradictions for the FOK orders policies. Do not use for real trading.
        /// The Currenex defines FOK as IOC order that does not permit partial fills
        /// </summary>
        FOK = '4', 
        //GoodTillDate = '6',
        //GoodForSeconds = 'X'
    }

    #region OrderState
    /// <summary>
    /// The order state
    /// </summary>
    /// <remarks>
    /// Is designed to provide order lifecycle. 
    /// Describes intermediate and stable order states to determine the validity of commands and reports, to manage timeouts, to detect finalization.
    /// Is not intended to anwer the question of fills count/filled qty or the reason of order finalization.
    /// </remarks>
    public enum OrderStates
    {
        /// <summary>
        /// the new not started order
        /// </summary>
        NotStarted,            // ордер еще не отправлялся
        /// <summary>
        /// new order request posted to counterparty, wait for confirmation
        /// </summary>
        NewPosted,      // ордер отправлен, решение еще не пришло 
        /// <summary>
        /// counterparty confirms the new order request is received, but not accepted yet
        /// </summary>
        NewPostedPend,  // пришел PendingNew (есть OrderID, ордер можно отменить)
        /// <summary>
        /// order is active
        /// </summary>
        Active,         // ордер активен (принят, частично исполнен, модификация принята)
        /// <summary>
        /// orderCancel request posted, wait for confirmation
        /// </summary>
        CancelPosted,   // отправлен запрос на отмену ордера, решение не пришло
        /// <summary>
        /// counterparty confirms the orderCancel request is received, but not accepted yet
        /// </summary>
        CancelPostedPend,   // запрос на отмену ордера сервером получен, решение не принято
        /// <summary>
        /// orderReplace request posted, wait for confirmation
        /// </summary>
        ReplacePosted,   // отправлен запрос на замену ордера, решение не пришло
        /// <summary>
        /// counterparty confirms the orderReplace request is received, but not accepted yet
        /// </summary>
        ReplacePostedPend,  // запрос на замену ордера сервером получен, решение не принято
        /// <summary>
        /// order execution had finished (fullfilled, cancelled, expired, stopped, rejected ...), order is not active more
        /// </summary>
        Finished,       // исполнение ордера завершено (исполнен, истекло время ордера, ордер отменен,отвергнут)
        //Suspended,      
    }
    /// <summary>
    /// the substate for finished order, specifies the reason of the order finalization
    /// </summary>
    public enum FinishedStates
    {
        /// <summary>
        /// is not finished
        /// </summary>
        Unknown,
        /// <summary>
        /// Cancelled
        /// </summary>
        OrderCancelled,
        /// <summary>
        /// FullFilled
        /// </summary>
        OrderFullFilled,
        /// <summary>
        /// Stopped
        /// </summary>
        OrderStopped,
        /// <summary>
        /// Expired
        /// </summary>
        OrderExpired,
        /// <summary>
        /// rejected by couterparty
        /// </summary>
        OrderRejected,
        /// <summary>
        /// rejected locally 
        /// </summary>
        PostRejected,
        /// <summary>
        /// This state used to finalize order when OrderCancel or OrderReplaceCancel request rejected by reason "too late" or "unkown order" (tag 102)
        /// or order removed manually by synchronization
        /// </summary>
        /// <remarks>
        /// (ONLY FOR CONFORMANCE TEST) Order will finalized with this state in order execution terminated by timeout !
        /// </remarks>
        Lost,
    }

    public static class OrderStateHelper
    {
        // tag 39 enumeration to string
        public static string OrdStatusToString(this string ordStatus)
        {
            switch (ordStatus)
            {
                case "D":
                    return "AcceptBidding";
                case "B":
                    return "Calculated";
                case "4":
                    return "Canceled";
                case "3":
                    return "Done";
                case "C":
                    return "Expired";
                case "2":
                    return "Filled";
                case "0":
                    return "New";
                case "1":
                    return "Partial";
                case "6":
                    return "Pending_Cancel";
                case "A":
                    return "PendingNew";
                case "E":
                    return "PendingRep";
                case "8":
                    return "Rejected";
                case "5":
                    return "Replaced";
                case "7":
                    return "Stopped";
                case "9":
                    return "Suspended";
                default:
                    return "Unknown";
            }
        }
    }
    #endregion
    /// <summary>
    /// Order
    /// </summary>
    public class Order
    {
        #region Main info,is required for send/cancel/replace operations
        /// <summary>
        /// Currency pair
        /// </summary>
        public string Symbol;

        /// <summary>
        /// trading currency 
        /// </summary>
        /// <remarks>
        /// As usual is the base currency. Some apis allows to use quoted currency. 
        /// </remarks>
        public string Currency; 
        /// <summary>
        /// OrderSide (buy or sell)
        /// </summary>
        public OrderSide Side = OrderSide.Unknown;
        /// <summary>
        /// Order Quantity
        /// </summary>
        public long OrderQty;
        /// <summary>
        /// OrderType
        /// </summary>
        public OrderType OrderType = OrderType.Unknown;
        /// <summary>
        /// OrderPrice (not used if market order)
        /// </summary>
        public double Price;

        /// <summary>
        /// TimeInForce
        /// </summary>
        public TimeInForce TimeInForce = TimeInForce.UnkTimeInForce;        
        /// <summary>
        /// AllOrNothing (the ExecInst option)
        /// </summary>
        public bool AllOrNothing;

        /// <summary>
        /// the unique orderID assigned locally
        /// </summary>
        public string ClientOrderID;
        /// <summary>
        /// OrderID assigned at the broker side (must be filled for requests OrderCancel, OrderCancelReplace of the limit orders)
        /// </summary>
        public string OrderID;
        /// <summary>
        /// the new order Price for OrderCancelReplace request
        /// </summary>
        public double AmendPrice;

        #endregion

        // The id of the IBrokerFacade which sent the order
        public long BrokerID;
        /// <summary>
        /// QuoteID (used if PrevouslyQuoted order)  
        /// </summary>
        public string QuoteID; // required only to call SendOrder with OrderType.PrevouslyQuoted
        public Order() { }
        public Order(string symbol, string currency, OrderSide side, long qty, OrderType orderType,
            double price,  // ignored for market orders
            TimeInForce timeInForce = TimeInForce.DAY, bool allOrNothing = false)
        {
            Symbol = symbol;
            Currency = currency;
            Side = side;
            OrderQty = qty;
            OrderType = orderType;
            Price = price;

            TimeInForce = timeInForce;
            AllOrNothing = allOrNothing;
        }
        public Order(string symbol, OrderSide side, long qty, OrderType orderType, double price,  // price is ignored for market orders
            TimeInForce timeInForce = TimeInForce.DAY, bool allOrNothing = false)
        {
            Symbol = symbol;
            Currency = Symbol.Substring(0, 3); // base currency
            Side = side;
            OrderQty = qty;
            OrderType = orderType;
            Price = price;

            TimeInForce = timeInForce;
            AllOrNothing = allOrNothing;
        }

        public Order(Order from)
        {
            Symbol = from.Symbol;
            Currency = from.Currency;
            Side = from.Side;
            OrderQty = from.OrderQty;
            OrderType = from.OrderType;
            Price = from.Price;

            QuoteID = from.QuoteID;

            TimeInForce = from.TimeInForce;
            AllOrNothing = from.AllOrNothing;

            BrokerID = from.BrokerID;

            ClientOrderID = from.ClientOrderID;

            OrderID = from.OrderID;
            AmendPrice = from.AmendPrice;

        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(ClientOrderID))
            {
                sb.Append(ClientOrderID);
                //if (!string.IsNullOrEmpty(OriginalClientOrderID))
                //    sb.Append("(" + ClientOrderID + ")");
                sb.Append(". ");
            }

            sb.Append(Side + " " + OrderQty + " " + Symbol);
            if (!Symbol.StartsWith(Currency))
                sb.Append("(QuotedCurrency)");
            sb.Append(" at ");
            if (OrderType != OrderType.Market)
                sb.Append(Price + " ");
            sb.Append(OrderType);
            sb.AppendFormat(" ({0}{1})", TimeInForce, (AllOrNothing) ? ",AON" : "");

            sb.Append("|BrokerID=" + BrokerID);
            return sb.ToString();

        }
    }
}
