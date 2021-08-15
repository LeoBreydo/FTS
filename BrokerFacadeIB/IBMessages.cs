using IBApi;

namespace BrokerFacadeIB
{
    // market data
    public abstract class MarketDataMessage
    {
        public MarketDataMessage(int requestId, int field)
        {
            RequestId = requestId;
            Field = field;
        }

        public int RequestId { get; set; }

        public int Field { get; set; }
    }

    public class TickPriceMessage : MarketDataMessage
    {
        public TickPriceMessage(int requestId, int field, double price, TickAttrib attribs)
            : base(requestId, field)
        {
            this.Price = price;
            this.Attribs = attribs;
        }

        public TickAttrib Attribs { get; set; }

        public double Price { get; set; }
    }

    public class RealTimeBarMessage
    {
        public int ReqId;
        public long Time;
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public long Volume;
        public double WAP;
        public int Count;
        public RealTimeBarMessage(int reqId, long time, double open, double high, double low, double close, long volume, double wap, int count)
        {
            ReqId = reqId;
            Time = time;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            WAP = wap;
            Count = count;
        }
    }

    public class TickSizeMessage : MarketDataMessage
    {
        public TickSizeMessage(int requestId, int field, int size) : base(requestId, field)
        {
            Size = size;
        }

        public int Size { get; set; }
    }

    // connection status
    public class ConnectionStatusMessage
    {
        public bool IsConnected { get; }

        public ConnectionStatusMessage(bool isConnected)
        {
            this.IsConnected = isConnected;
        }


    }

    // orders
    public abstract class OrderMessage
    {
        public int OrderId { get; set; }
    }

    public class OrderStatusMessage : OrderMessage
    {
        public string Status { get; private set; }
        public double Filled { get; private set; }
        public double Remaining { get; private set; }
        public double AvgFillPrice { get; private set; }
        public int PermId { get; private set; }
        public int ParentId { get; private set; }
        public double LastFillPrice { get; private set; }
        public int ClientId { get; private set; }
        public string WhyHeld { get; private set; }
        public double MktCapPrice { get; private set; }

        public OrderStatusMessage(int orderId, string status, double filled, double remaining, double avgFillPrice,
            int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
        {
            OrderId = orderId;
            Status = status;
            Filled = filled;
            Remaining = remaining;
            AvgFillPrice = avgFillPrice;
            PermId = permId;
            ParentId = parentId;
            LastFillPrice = lastFillPrice;
            ClientId = clientId;
            WhyHeld = whyHeld;
        }
    }

    public class OpenOrderMessage : OrderMessage
    {
        public OpenOrderMessage(int orderId, Contract contract, Order order, OrderState orderState)
        {
            OrderId = orderId;
            Contract = contract;
            Order = order;
            OrderState = orderState;
        }

        public Contract Contract { get; set; }

        public Order Order { get; set; }

        public OrderState OrderState { get; set; }
    }

    // execution
    public class ExecutionMessage
    {
        public ExecutionMessage(int reqId, Contract contract, Execution execution)
        {
            ReqId = reqId;
            Contract = contract;
            Execution = execution;
        }

        public Contract Contract { get; set; }

        public Execution Execution { get; set; }

        public int ReqId { get; set; }
    }

    public class MarketDataTypeMessage
    {
        public MarketDataTypeMessage(int requestId, int marketDataType)
        {
            RequestId = requestId;
            MarketDataType = marketDataType;
        }

        public int RequestId { get; set; }

        public int MarketDataType { get; set; }
    }

    public class TickReqParamsMessage
    {
        public int TickerId { get; private set; }
        public double MinTick { get; private set; }
        public string BboExchange { get; private set; }
        public int SnapshotPermissions { get; private set; }

        public TickReqParamsMessage(int tickerId, double minTick, string bboExchange, int snapshotPermissions)
        {
            TickerId = tickerId;
            MinTick = minTick;
            BboExchange = bboExchange;
            SnapshotPermissions = snapshotPermissions;
        }
    }

    public class HeadTimestampMessage
    {
        public int ReqId { get; private set; }
        public string HeadTimestamp { get; private set; }

        public HeadTimestampMessage(int reqId, string headTimestamp)
        {
            this.ReqId = reqId;
            this.HeadTimestamp = headTimestamp;
        }
    }

    public class OrderBoundMessage
    {
        public long OrderId { get; private set; }
        public int ApiClientId { get; private set; }
        public int ApiOrderId { get; private set; }

        public OrderBoundMessage(long orderId, int apiClientId, int apiOrderId)
        {
            OrderId = orderId;
            ApiClientId = apiClientId;
            ApiOrderId = apiOrderId;
        }
    }

    public class CompletedOrderMessage
    {
        public CompletedOrderMessage(Contract contract, Order order, OrderState orderState)
        {
            Contract = contract;
            Order = order;
            OrderState = orderState;
        }

        public Contract Contract { get; set; }

        public Order Order { get; set; }

        public OrderState OrderState { get; set; }
    }

    public class TickByTickBidAskMessage
    {
        public int ReqId { get; private set; }
        public long Time { get; private set; }
        public double BidPrice { get; private set; }
        public double AskPrice { get; private set; }
        public long BidSize { get; private set; }
        public long AskSize { get; private set; }
        public TickAttribBidAsk TickAttribBidAsk { get; private set; }

        public TickByTickBidAskMessage(int reqId, long time, double bidPrice, double askPrice, long bidSize, long askSize, TickAttribBidAsk tickAttribBidAsk)
        {
            ReqId = reqId;
            Time = time;
            BidPrice = bidPrice;
            AskPrice = askPrice;
            BidSize = bidSize;
            AskSize = askSize;
            TickAttribBidAsk = tickAttribBidAsk;
        }
    }

    //contract detail message

    public class ContractDetailsMessage
    {
        public int ReqId;
        public ContractDetails Details;
        public ContractDetailsMessage(int reqId, ContractDetails contractDetails) => 
            (ReqId, Details) = (reqId, contractDetails);
    }
}
