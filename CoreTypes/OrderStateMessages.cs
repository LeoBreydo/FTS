using System;

namespace CoreTypes
{
    public enum OrderStateMessageType
    {
        Cancel = 0,
        Execution = 1,
        Post = 2
    }
    public abstract class OrderStateMessage
    {
        public int ClOrderId;
        public DateTime UtcNow;

        protected OrderStateMessage(int clOrderId, DateTime utcNow)
        {
            ClOrderId = clOrderId;
            UtcNow = utcNow;
        }

        public abstract OrderStateMessageType MyType { get; }
        public abstract int OrderId { get; }
    }

    public class OrderCancelMessage : OrderStateMessage
    {
        public string CancelReason;

        public OrderCancelMessage(int clOrderId, DateTime utcNow, int orderId, string cancelReason) 
            : base(clOrderId, utcNow)
        {
            OrderId = orderId;
            CancelReason = cancelReason;
        }
        public override OrderStateMessageType MyType { get; } = OrderStateMessageType.Cancel;
        public override int OrderId { get; }

        public override string ToString() => 
            $"Cancel: Time: {UtcNow:yyyyMMdd:HHmmss}, CancelReason: {CancelReason}, OId: {OrderId}, ClientOId: {ClOrderId}";
    }

    public class OrderPostMessage : OrderStateMessage
    {
        public string Symbol;
        public string ContractCode;
        public string Exchange;
        public int SgnQty;

        public OrderPostMessage(int clOrderId, DateTime utcNow, int orderId, string symbol, string contractCode, string exchange, 
            int sgnQty) : base(clOrderId, utcNow)
        {
            OrderId = orderId;
            Symbol = symbol;
            ContractCode = contractCode;
            Exchange = exchange;
            SgnQty = sgnQty;
        }
        public override OrderStateMessageType MyType { get; } = OrderStateMessageType.Post;
        public override int OrderId { get; }

        public override string ToString() => 
            $"Post: Time: {UtcNow:yyyyMMdd:HHmmss}, Mkt: {Symbol}, Exch: {Exchange}, ContCode: {ContractCode},  SgnQty: {SgnQty}, OId: {OrderId}, ClientOId: {ClOrderId}";
    }

    public class OrderExecutionMessage : OrderStateMessage
    {
        public string ExecId;
        public string Symbol;
        public string ContractCode;
        public string Exchange;
        public int SgnQty;
        public int CumQty;
        public decimal Price;
        public DateTime TransactTime;

        public OrderExecutionMessage(int clOrderId, DateTime utcNow, 
            int orderId, string execId, string symbol, string contractCode, string exchange, 
            int sgnQty, int cumQty, decimal price, DateTime transactTime) 
            : base(clOrderId, utcNow)
        {
            OrderId = orderId;
            ExecId = execId;
            Symbol = symbol;
            ContractCode = contractCode;
            Exchange = exchange;
            SgnQty = sgnQty;
            CumQty = cumQty;
            Price = price;
            TransactTime = transactTime;
        }
        public override OrderStateMessageType MyType { get; } = OrderStateMessageType.Execution;
        public override int OrderId { get; }
        public override string ToString() => 
            $"Exec: Time: {TransactTime:yyyyMMdd:HHmmss}, Mkt{Symbol}, Exch: {Exchange}, ContCode: {ContractCode},  SgnQty: {SgnQty}, Price: {Price}, ExecId: {ExecId}, OId: {OrderId}, ClientOId: {ClOrderId}";
    }

    
}