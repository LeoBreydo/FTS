//#if DEBUG
//#define PARSE_OLD_FORMAT
//#endif

using CommonStructures;
using Newtonsoft.Json;

namespace Messages
{
    public abstract class OrderReportBase : BaseMessage
    {
        public string ClOrdID;
        public long BrokerID;
        protected OrderReportBase(MessageNumbers messageNumber) : base(messageNumber) { }
        protected OrderReportBase(MessageNumbers messageNumber,long brokerID, string clOrdID)
            : base(messageNumber)
        {
            BrokerID = brokerID;
            ClOrdID = clOrdID;
        }
    }
    public class OrderPosting : OrderReportBase
    {
        public string Symbol;
        public string Side;
        public long Qty;
        public string OrderType;
        public double OrderPrice;

        public string OrderID; // used by IB only

        public OrderPosting() : base(MessageNumbers.OrderPosting) { }
        public OrderPosting(long brokerID, string clOrdID,string symbol,string side,long qty,string orderType,double orderPrice)
            : base(MessageNumbers.OrderPosting, brokerID, clOrdID)
        {
            Symbol = symbol;
            Side = side;
            Qty = qty;
            OrderType = orderType;
            OrderPrice = orderPrice;
        }
        //public override string ToString()
        //{
        //    return string.Format("OrderPosting, BrokerID={0}, ClOrdID={1}, Symbol={2}, Side={3}, Qty={4}, OrderType={5}, OrderPrice={6}",
        //        BrokerID, ClOrdID,Symbol,Side,Qty,OrderType,OrderPrice);
        //}
    }
    public class OrderPosted : OrderReportBase
    {
        public string Symbol;
        public string Side;
        public long Qty;
        public string OrderType;
        public double OrderPrice;

        public string OrderID; // used by IB only


        public OrderPosted() : base(MessageNumbers.OrderPosted) { }
        public OrderPosted(long brokerID, string clOrdID, string symbol, string side, long qty, string orderType, double orderPrice)
            : base(MessageNumbers.OrderPosted, brokerID, clOrdID)
        {
            Symbol = symbol;
            Side = side;
            Qty = qty;
            OrderType = orderType;
            OrderPrice = orderPrice;
        }
        //public override string ToString()
        //{
        //    return string.Format("OrderPosted, BrokerID={0}, ClOrdID={1}, Symbol={2}, Side={3}, Qty={4}, OrderType={5}, OrderPrice={6}",
        //        BrokerID, ClOrdID, Symbol, Side, Qty, OrderType, OrderPrice);
        //}

    }
    public class OrderPostRejection : OrderReportBase
    {
        public string RejectionReason;
        public OrderPostRejection() : base(MessageNumbers.OrderPostRejection) { }
        public OrderPostRejection(long brokerID, string clOrdID, string rejectionReason)
            : base(MessageNumbers.OrderPostRejection, brokerID, clOrdID)
        {
            RejectionReason = rejectionReason;
        }
        ////public override string ToString() { return "OrderPostRejection, BrokerID=" + BrokerID + ", ClOrdID=" + ClOrdID + ", RejectionReason=" + RejectionReason; }
        //public override string ToString()
        //{ return string.Format("OrderPostRejection, BrokerID={0}, ClOrdID={1}, RejectionReason={2}" ,
        //    BrokerID, ClOrdID,RejectionReason.ToJson());
        //}
        //public static OrderPostRejection Restore(string txt)
        //{
        //    var tags = new UnwrappedTags(txt);
        //    return new OrderPostRejection(
        //        tags.GetTagValueLong("BrokerID"),
        //        tags.GetTagValue("ClOrdID", false),
        //        tags.GetTagValue("RejectionReason", true));
        //}
    }
    public class CancelAllOrdersPosting:BaseMessage
    {
        public long BrokerID;
        public CancelAllOrdersPosting() : base(MessageNumbers.CancelAllOrdersPosting) { }
        public CancelAllOrdersPosting(long brokerId)
            : base(MessageNumbers.CancelAllOrdersPosting)
        {
            BrokerID = brokerId;
        }
        //public override string ToString(){return "CancelAllOrdersPosting, BrokerID=" + BrokerID;}
    }
    public class CancelAllOrdersPosted : BaseMessage
    {
        public long BrokerID;
        public CancelAllOrdersPosted() : base(MessageNumbers.CancelAllOrdersPosted) { }
        public CancelAllOrdersPosted(long brokerId)
            : base(MessageNumbers.CancelAllOrdersPosted)
        {
            BrokerID = brokerId;
        }
        //public override string ToString() { return "CancelAllOrdersPosted, BrokerID=" + BrokerID; }
    }
    public class OrderCancelPosted : OrderReportBase
    {
        public string OrderID;
        public OrderCancelPosted() : base(MessageNumbers.OrderCancelPosted) { }
        public OrderCancelPosted(long brokerID, string clOrdID,string orderID)
            : base(MessageNumbers.OrderCancelPosted, brokerID, clOrdID)
        {
            OrderID = orderID;
        }
        //public override string ToString() { return "OrderCancelPosted, BrokerID=" + BrokerID + ", ClOrdID=" + ClOrdID + ", OrderID=" + OrderID; }
    }
    public class OrderCancelPostRejection : OrderReportBase
    {
        public string OrderID;
        public string RejectionReason;
        public OrderCancelPostRejection() : base(MessageNumbers.OrderCancelPostRejection) { }
        public OrderCancelPostRejection(long brokerID, string clOrdID, string orderID, string rejectionReason)
            : base(MessageNumbers.OrderCancelPostRejection, brokerID, clOrdID)
        {
            OrderID = orderID;
            RejectionReason = rejectionReason;
        }
        //public override string ToString() { return "OrderCancelPostRejection, BrokerID=" + BrokerID + ", ClOrdID=" + ClOrdID + ", OrderID=" + OrderID + ", RejectionReason=" + RejectionReason.ToJson(); }
    }

    public class OrderReplacePosted : OrderReportBase
    {
        public string OldClOrdID;
        public string OrderID;
        public string NewClOrdID;
        public double NewPrice;
        public long NewAmount;
        public bool ForexReq;

        public OrderReplacePosted() : base(MessageNumbers.OrderReplacePosted) { }
        public OrderReplacePosted(long brokerID, string oldClOrdId, string orderID, string newClOrdId, double newPrice, long newAmount, bool forexReq)
            : base(MessageNumbers.OrderReplacePosted,brokerID, oldClOrdId)
        {
            OldClOrdID = oldClOrdId;
            OrderID = orderID;
            NewClOrdID = newClOrdId;
            NewPrice = newPrice;
            NewAmount = newAmount;
            ForexReq = forexReq;
        }
        //public override string ToString()
        //{
        //    return
        //        string.Format(
        //            "OrderReplacePosted, BrokerID={0}, ClOrdID={1}, OrderID={2}, NewClOrdID={3}, NewPrice={4}, NewAmount={5}, ForexReq={6}",
        //            BrokerID, ClOrdID, OrderID, NewClOrdID, NewPrice, NewAmount, ForexReq);
        //}
    }
    public class OrderReplacePostRejection : OrderReportBase
    {
        public string OrderID;
        public string RejectionReason;
        public OrderReplacePostRejection() : base(MessageNumbers.OrderReplacePostRejection) { }
        public OrderReplacePostRejection(long brokerID, string clOrdID, string orderID,  string rejectionReason)
            : base(MessageNumbers.OrderReplacePostRejection, brokerID, clOrdID)
        {
            OrderID = orderID;
            RejectionReason = rejectionReason;
        }
        //public override string ToString() { return "OrderReplacePostRejection, BrokerID=" + BrokerID + ", ClOrdID=" + ClOrdID + ", OrderID=" + OrderID + ", RejectionReason=" + RejectionReason.ToJson(); }
    }

    // order report which allows to restore the order settings and actual state
    public abstract class BaseExecutionReport : OrderReportBase
    {
        public string OrderID;

        public string Symbol;
        public string Currency; // (3.2 notes - может быть и базовой и котируемой)
        public OrderSide Side;
        public long OrderQty;
        public OrderType OrderType;
        public double OrderPrice;

         //public string Accout; // optional, it is not declared in API doc. The field was not specified in the RejectionReport(39=8|150=8)

        // order state
        public long CumQty;
        public long LeavesQty;
        public TimeStamp TransactTime; // значение тега 60(TransactTime) последнего отчета (utc)
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        public string OrdStatus;  // to display in Conformance test
        public string Text;

        protected BaseExecutionReport(MessageNumbers messageNumber) : base(messageNumber) { }

        protected BaseExecutionReport(MessageNumbers messageNumber, long brokerID, string clOrdID)
            : base(messageNumber, brokerID, clOrdID)
        {
        }
    }
    public class PendingNewReport : BaseExecutionReport
    {
        public PendingNewReport() : base(MessageNumbers.PendingNewReport) { }
        public PendingNewReport(long brokerID, string clOrdID) : base(MessageNumbers.PendingNewReport, brokerID, clOrdID) { }
        //public override string ToString()
        //{
        //    return string.Format("PendingNewReport, BrokerID={0}, ClOrdID={1}, OrderID={2}, Time={3}, Symbol={4}, Side={5}, OrderType={6}, OrderPrice={7}",
        //                         BrokerID, ClOrdID, OrderID, TransactTime, Symbol, Side, OrderType, OrderPrice);
        //}
    }
    public class AcknowledgementReport : BaseExecutionReport
    {
        public AcknowledgementReport() : base(MessageNumbers.AcknowledgementReport) { }
        public AcknowledgementReport(long brokerID, string clOrdID) : base(MessageNumbers.AcknowledgementReport, brokerID, clOrdID) { }
        //public override string ToString()
        //{
        //    return string.Format("AcknowledgementReport, BrokerID={0}, ClOrdID={1}, OrderID={2}, Time={3}, Symbol={4}, Side={5}, OrderType={6}, OrderPrice={7}",
        //                         BrokerID, ClOrdID, OrderID, TransactTime, Symbol, Side, OrderType, OrderPrice);
        //}
        //public static AcknowledgementReport Restore(string txt)
        //{
        //    var tags = new UnwrappedTags(txt);
        //    return new AcknowledgementReport(
        //        tags.GetTagValueLong("BrokerID"),
        //        tags.GetTagValue("ClOrdID", false))
        //               {
        //                   OrderID = tags.GetTagValue("OrderID", false),
        //                   TransactTime = tags.GetTagValueTimeStamp("Time"),
        //                   Symbol = tags.GetTagValue("Symbol", false),
        //                   Side = (OrderSide) Enum.Parse(typeof (OrderSide), tags.GetTagValue("Side", false)),
        //                   OrderType = (OrderType) Enum.Parse(typeof (OrderType), tags.GetTagValue("OrderType", false)),
        //                   OrderPrice = tags.GetTagValueDouble("OrderPrice")
        //               };
        //}

    }
    public class RejectionReport : BaseExecutionReport
    {
        public string RejectionReason;
        public RejectionReport() : base(MessageNumbers.RejectionReport) { }
        public RejectionReport(long brokerID, string clOrdID) : base(MessageNumbers.RejectionReport, brokerID, clOrdID) { }
//        public override string ToString()
//        {
//            return string.Format("RejectionReport, BrokerID={0}, ClOrdID={1}, Time={2}, Symbol={3}, RejectionReason={4}, Txt={5}",
//                                 BrokerID, ClOrdID, TransactTime, Symbol, RejectionReason.ToJson(), Text.ToJson());
//        }
//        public static RejectionReport Restore(string txt)
//        {
//            var tags = new UnwrappedTags(txt);
//#if PARSE_OLD_FORMAT
//            string text = tags.GetTagValue("Txt", false);
//            if (text.StartsWith("\""))
//                text = text.UnwrapTextTag();

//            string ordRejectionReason = tags.GetTagValue("RejectionReason", false);
//            if (ordRejectionReason.StartsWith("\""))
//                ordRejectionReason = ordRejectionReason.UnwrapTextTag();
//#else
//            string text = tags.GetTagValue("Txt", true);
//            string ordRejectionReason = tags.GetTagValue("RejectionReason", true);
            
//#endif
//            return new RejectionReport(
//                tags.GetTagValueLong("BrokerID"),
//                tags.GetTagValue("ClOrdID", false))
//                       {
//                           TransactTime = tags.GetTagValueTimeStamp("Time"),
//                           Symbol = tags.GetTagValue("Symbol", false),
//                           RejectionReason = ordRejectionReason,
//                           Text = text,
//                       };
//        }

    }
    public class OrderFillReport : BaseExecutionReport
    {
        public FillsItem Fill;
        public bool IsFullFilled;

        public OrderFillReport() : base(MessageNumbers.OrderFillReport) { }

        public OrderFillReport(FillsItem fill, bool isFullFilled,long leavesQty=-1)
            : base(MessageNumbers.OrderFillReport, fill.BrokerID, fill.ClOrdId)
        {
            Fill = fill;
            IsFullFilled = isFullFilled;
            CumQty = fill.CumQty;
            LeavesQty = leavesQty;
        }

        //public override string ToString()
        //{
        //    return string.Format("OrderFillReport, {0}, IsFullFilled={1}, OrderType={2}, OrderPrice={3}", Fill, IsFullFilled, OrderType, OrderPrice);
        //    //return "OrderFillReport, "+ Fill + ", IsFullFilled=" + IsFullFilled;
        //}
    }
    public class OrderStatusReport : BaseExecutionReport
    {
        public double AvgPx;
        public long MinQty;
        public long CxlQty;

        public OrderStatusReport() : base(MessageNumbers.OrderStatusReport) { }

        public OrderStatusReport(long brokerID, string clOrdId) : base(MessageNumbers.OrderStatusReport, brokerID, clOrdId) { }
        //public override string ToString()
        //{
        //    return string.Format("OrderStatusReport, BrokerID={0}, ClOrdID={1}, OrderID={2}, Symbol={3}, Currency={4}, Side={5}, OrderQty={6}, OrderType={7}, OrderPrice={8}, CumQty={9}, LeavesQty={10}, TransactTime={11}, OrdStatus={12}, AvgPx={13}, MinQty={14}, CxlQty={15}",
        //             BrokerID, ClOrdID, OrderID, Symbol, Currency, Side, OrderQty, OrderType, OrderPrice, CumQty, LeavesQty, TransactTime, OrdStatus,
        //             AvgPx, MinQty, CxlQty);
        //}
    }

    public class PendingCancelReport : BaseExecutionReport
    {
        public PendingCancelReport() : base(MessageNumbers.PendingCancelReport) { }
        public PendingCancelReport(long brokerID, string clOrdID) : base(MessageNumbers.PendingCancelReport, brokerID, clOrdID) { }
        //public override string ToString()
        //{
        //    return string.Format("PendingCancelReport, BrokerID={0}, ClOrdId={1}, OrderID={2}, Time={3}, Symbol={4}",
        //                         BrokerID, ClOrdID, OrderID, TransactTime, Symbol);
        //}
    }
    public class OrderCancelledReport : BaseExecutionReport
    {
        public OrderCancelledReport() : base(MessageNumbers.OrderCancelledReport) { }
        public OrderCancelledReport(long brokerID, string clOrdID) : base(MessageNumbers.OrderCancelledReport, brokerID, clOrdID) { }
        //public override string ToString()
        //{
        //    return string.Format("OrderCancelledReport, BrokerID={0}, ClOrdID={1}, OrderID={2}, Time={3}, Symbol={4}",
        //                         BrokerID, ClOrdID, OrderID, TransactTime, Symbol);

        //}
        //public static OrderCancelledReport Restore(string txt)
        //{
        //    var tags = new UnwrappedTags(txt);
        //    return new OrderCancelledReport(
        //        tags.GetTagValueLong("BrokerID"),
        //        tags.GetTagValue("ClOrdID", false))
        //    {
        //        OrderID = tags.GetTagValue("OrderID", false),
        //        TransactTime = tags.GetTagValueTimeStamp("Time"),
        //        Symbol = tags.GetTagValue("Symbol", false)
        //    };
        //}

    }
    public class OrderStoppedReport : BaseExecutionReport
    {
        public OrderStoppedReport() : base(MessageNumbers.OrderStoppedReport) { }
        public OrderStoppedReport(long brokerID, string clOrdID) : base(MessageNumbers.OrderStoppedReport, brokerID, clOrdID) { }
        //public override string ToString()
        //{
        //    return string.Format("OrderStoppedReport, BrokerID={0}, ClOrdID={1}, OrderID={2}, Time={3}, Symbol={4}",
        //                         BrokerID, ClOrdID, OrderID, TransactTime, Symbol);

        //}
        //public static OrderStoppedReport Restore(string txt)
        //{
        //    var tags = new UnwrappedTags(txt);
        //    return new OrderStoppedReport(
        //        tags.GetTagValueLong("BrokerID"),
        //        tags.GetTagValue("ClOrdID", false))
        //    {
        //        OrderID = tags.GetTagValue("OrderID", false),
        //        TransactTime = tags.GetTagValueTimeStamp("Time"),
        //        Symbol = tags.GetTagValue("Symbol", false)
        //    };
        //}
    }
    public class OrderExpiredReport : BaseExecutionReport
    {
        public OrderExpiredReport() : base(MessageNumbers.OrderExpiredReport) { }
        public OrderExpiredReport(long brokerID, string clOrdID) : base(MessageNumbers.OrderExpiredReport, brokerID, clOrdID) { }
        //public override string ToString()
        //{
        //    return string.Format("OrderExpiredReport, BrokerID={0}, ClOrdID={1}, OrderID={2}, Time={3}, Symbol={4}",
        //             BrokerID, ClOrdID, OrderID, TransactTime, Symbol);
        //}
        //public static OrderExpiredReport Restore(string txt)
        //{
        //    var tags = new UnwrappedTags(txt);
        //    return new OrderExpiredReport(
        //        tags.GetTagValueLong("BrokerID"),
        //        tags.GetTagValue("ClOrdID", false))
        //    {
        //        OrderID = tags.GetTagValue("OrderID", false),
        //        TransactTime = tags.GetTagValueTimeStamp("Time"),
        //        Symbol = tags.GetTagValue("Symbol", false)
        //    };
        //}
    }
    public class PendingReplaceReport : BaseExecutionReport
    {
        public PendingReplaceReport() : base(MessageNumbers.PendingReplaceReport) { }
        public PendingReplaceReport(long brokerID, string clOrdID) : base(MessageNumbers.PendingReplaceReport, brokerID, clOrdID) { }
        //public override string ToString()
        //{
        //    return string.Format("PendingReplaceReport, BrokerID={0}, ClOrdID={1}, OrderID={2}, Time={3}, Symbol={4}",
        //             BrokerID, ClOrdID, OrderID, TransactTime, Symbol);
        //}
    }
    public class OrderReplacedReport:BaseExecutionReport
    {
        public string OrigClOrdID;
        public OrderReplacedReport() : base(MessageNumbers.OrderReplacedReport) { }
        public OrderReplacedReport(long brokerID, string clOrdID, string origClOrdID)
            : base(MessageNumbers.OrderReplacedReport, brokerID, clOrdID)
        {
            OrigClOrdID = origClOrdID;
        }
        //public override string ToString()
        //{
        //    return string.Format("OrderReplacedReport, BrokerID={0}, ClOrdID={1}, OrigClOrdID={2}, OrderID={3}, Time={4}, Symbol={5}",
        //             BrokerID, ClOrdID, OrigClOrdID, OrderID, TransactTime, Symbol);
        //}
    }

    public class OrderCancelRejectionReport : OrderReportBase
    {
        public TimeStamp RejectionTime;
        public string OrderID;
        public string CancelRequestID;
        public string RejectionReason;

        /// <summary>
        /// Rejected as far as order id not recognized on the broker side
        /// </summary>
        public bool TooLateOrUnknownOrder;

        public OrderCancelRejectionReport() : base(MessageNumbers.OrderCancelRejectionReport) { }
        public OrderCancelRejectionReport(long brokerID, string clOrdID,
            string requestId, string orderId, TimeStamp rejectionTime, string rejectionReason, bool tooLateOrUnknownOrder)
            : base(MessageNumbers.OrderCancelRejectionReport, brokerID, clOrdID)
        {
            CancelRequestID = requestId;
            OrderID = orderId;
            RejectionTime = rejectionTime;
            RejectionReason = rejectionReason;
            TooLateOrUnknownOrder = tooLateOrUnknownOrder;
        }
        //public override string ToString() { return "OrderCancelRejectionReport, BrokerID=" + BrokerID + ", ClOrdID=" + ClOrdID + ", OrderID=" + OrderID + ", RejectionReason=" + RejectionReason.ToJson(); }
    }
    public class OrderReplaceRejectionReport : OrderReportBase
    {
        public TimeStamp RejectionTime;
        public string OldClOrdID;
        public string NewClOrdID;
        public string OrderID;
        public string RejectionReason;

        /// <summary>
        /// Rejected as far as order id not recognized on the broker side
        /// </summary>
        public bool TooLateOrUnknownOrder;

        public OrderReplaceRejectionReport() : base(MessageNumbers.OrderReplaceRejectionReport) { }
        public OrderReplaceRejectionReport(long brokerID,
            string oldClOrdID, string newClOrdID, string orderId, TimeStamp rejectionTime, string rejectionReason, bool tooLateOrUnknownOrder)
            : base(MessageNumbers.OrderReplaceRejectionReport, brokerID, oldClOrdID)
        {
            OldClOrdID = oldClOrdID;
            NewClOrdID = newClOrdID;
            OrderID = orderId;
            RejectionTime = rejectionTime;
            RejectionReason = rejectionReason;
            TooLateOrUnknownOrder = tooLateOrUnknownOrder;
        }
        //public override string ToString() { return "OrderReplaceRejectionReport, BrokerID=" + BrokerID + ", ClOrdID=" + ClOrdID + ", OrderID=" + OrderID + ", RejectionReason=" + RejectionReason.ToJson(); }
    }

    public class ManualOrderFills:BaseMessage
    {
        public long BrokerID;        
        public string Symbol;
        public string Currency;
        public OrderSide Side;
        public long Qty;
        public double Price;
        public TimeStamp TransactTime;
        public string ClOrderID;
        public string OrderID;
        public string ExecID;
#if cu
        public long StrategyID;
#endif

        public ManualOrderFills() : base(MessageNumbers.ManualOrderFills) { }
    }
}
