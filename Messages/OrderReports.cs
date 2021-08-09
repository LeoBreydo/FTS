//#if DEBUG
//#define PARSE_OLD_FORMAT
//#endif

using System;
using CommonStructures;
using Newtonsoft.Json;

namespace Messages
{
    public abstract class OrderReportBase : BaseMessage
    {
        public readonly int ClOrdID;
        public long BrokerID;
        protected OrderReportBase(MessageNumbers messageNumber) : base(messageNumber) { }
        protected OrderReportBase(MessageNumbers messageNumber,long brokerID, int clOrdID)
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
        public OrderPosting(long brokerID, int clOrdID,string symbol,string side,long qty,string orderType,double orderPrice)
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
        public OrderPosted(long brokerID, int clOrdID, string symbol, string side, long qty, string orderType, double orderPrice)
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
        public OrderPostRejection(long brokerID, int clOrdID, string rejectionReason)
            : base(MessageNumbers.OrderPostRejection, brokerID, clOrdID)
        {
            RejectionReason = rejectionReason;
        }
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
        // order state
        public long CumQty;
        public long LeavesQty;
        public DateTime TransactTime; // значение тега 60(TransactTime) последнего отчета (utc)
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        public string OrdStatus;  // to display in Conformance test
        public string Text;

        protected BaseExecutionReport(MessageNumbers messageNumber) : base(messageNumber) { }

        protected BaseExecutionReport(MessageNumbers messageNumber, long brokerID, int clOrdID)
            : base(messageNumber, brokerID, clOrdID)
        {
        }
    }
    public class AcknowledgementReport : BaseExecutionReport
    {
        public AcknowledgementReport() : base(MessageNumbers.AcknowledgementReport) { }
        public AcknowledgementReport(long brokerID, int clOrdID) : 
            base(MessageNumbers.AcknowledgementReport, brokerID, clOrdID) { }
    }
    public class RejectionReport : BaseExecutionReport
    {
        public string RejectionReason;
        public RejectionReport() : base(MessageNumbers.RejectionReport) { }
        public RejectionReport(long brokerID, int clOrdID) 
            : base(MessageNumbers.RejectionReport, brokerID, clOrdID) { }
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
    }
    public class OrderStoppedReport : BaseExecutionReport
    {
        public OrderStoppedReport() : base(MessageNumbers.OrderStoppedReport) { }
        public OrderStoppedReport(long brokerID, int clOrdID) : base(MessageNumbers.OrderStoppedReport, brokerID, clOrdID) { }
    }
    public class ManualOrderFills:BaseMessage
    {
        public long BrokerID;        
        public string Symbol;
        public string Currency;
        public OrderSide Side;
        public long Qty;
        public double Price;
        public DateTime TransactTime;
        public int ClOrderID;
        public string OrderID;
        public string ExecID;
#if cu
        public long StrategyID;
#endif

        public ManualOrderFills() : base(MessageNumbers.ManualOrderFills) { }
    }
}
