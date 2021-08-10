using System;
using System.Collections.Generic;
using System.Linq;
using Messages;

namespace CoreTypes
{
    public static class OrderGenerator
    {
        public static (MarketTrader, MarketOrderDescription order, List<Trade>)
            GenerateOrders(MarketTrader caller, DateTime utcNow)
        {
            var (bid, ask, last) = caller.Position.PriceProvider.LastPrices;
            if (bid == -1 || ask == -1 || last == -1) return (caller, null, new());
            var so = caller.Strategies
                .Select(s => s.GenerateOrder((bid, ask, last)))
                .Where(t => t.NbrOfContracts != 0)
                .ToList();

            var (unfilledOrders, trades) = Reduce(caller, so, bid, utcNow, out var basketID);
            if (unfilledOrders is { Count: > 0 })
            {
                var amount = unfilledOrders.Sum(o => o.NbrOfContracts);
                if (amount != 0)
                {
                    caller.PostedOrderMap.Add(basketID, (unfilledOrders,utcNow));
                    return (caller,
                        new MarketOrderDescription(basketID, caller.MarketCode, caller.Exchange, amount),
                        trades);
                }
            }
            return (caller, null, trades);
        }

        private static (List<StrategyOrderInfo>, List<Trade>) Reduce(MarketTrader caller, List<StrategyOrderInfo> so, decimal bid, DateTime utcNow,
            out int clBasketID)
        {
            List<StrategyOrderInfo> ReduceNotFilledOrders(List<StrategyOrderInfo> strategyOrderInfos,
                int virtuallyFilled, decimal virtualExecutionQuote,
                string clOrderId, int clBasketId, List<Trade> trades)
            {
                var lst = strategyOrderInfos.OrderBy(o => o.NbrOfContracts).ToList();
                foreach (var o in lst)
                {
                    var rest = virtuallyFilled - o.NbrOfContracts;
                    if (rest >= 0)
                    {
                        var tl = MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], virtualExecutionQuote, utcNow, clOrderId, clBasketId);
                        if (tl.Count > 0) trades.AddRange(tl);
                    }
                    else
                    {
                        var tl = MarketTrader.ApplyPartialVirtualFill(caller.StrategyMap[o.StrategyId], virtualExecutionQuote, utcNow,
                            virtuallyFilled, clOrderId, clBasketId);
                        if (tl.Count > 0) trades.AddRange(tl);
                        break;
                    }

                    if (rest == 0) break;
                    virtuallyFilled = rest;
                }

                lst.RemoveAll(item => item.NbrOfContracts == 0);
                return lst;
            }

            List<Trade> tList = new();
            var (buyOrders, sellOrders) =
                (so.Where(o => o.NbrOfContracts > 0).ToList(), so.Where(o => o.NbrOfContracts < 0).ToList());

            clBasketID = IdGenerators.GetNextClientOrderId();
            var clOrderID = clBasketID + "_internal";

            List<StrategyOrderInfo> notFilledOrders;
            var pAmount = buyOrders.Sum(o => o.NbrOfContracts);
            var nAmount = sellOrders.Sum(o => o.NbrOfContracts);
            var amountToSend = pAmount + nAmount;
            if (amountToSend == 0)
            {
                // all orders are executed virtually :)
                foreach (var o in buyOrders)
                {
                    var tl = MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID);
                    if (tl.Count > 0) tList.AddRange(tl);
                }
                foreach (var o in sellOrders)
                {
                    var tl = MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID);
                    if (tl.Count > 0) tList.AddRange(tl);
                }

                return (null, tList);
            }
            if (amountToSend > 0)
            {
                notFilledOrders = buyOrders;
                if (nAmount != 0)
                {
                    // we'll send to broker 'buy' order and virtually execute all 'sell' strategy orders and some number of 'buy' strategy orders
                    // NB! only one 'buy' order can be executed partially
                    foreach (var o in sellOrders)
                    {
                        var tl = MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID);
                        if (tl.Count > 0) tList.AddRange(tl);
                    }
                    sellOrders.Clear();
                    notFilledOrders = ReduceNotFilledOrders(notFilledOrders, -nAmount,
                        bid, clOrderID, clBasketID, tList);
                }
            }
            else // amountToSend < 0
            {
                notFilledOrders = sellOrders;
                if (pAmount != 0)
                {
                    foreach (var o in buyOrders)
                    {
                        var tl = MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID);
                        if (tl.Count > 0) tList.AddRange(tl);
                    }
                    buyOrders.Clear();
                    notFilledOrders = ReduceNotFilledOrders(notFilledOrders, -pAmount,
                        bid, clOrderID, clBasketID, tList);
                }
            }
            return (notFilledOrders, tList);
        }
    }

    public static class OrderReportsProcessor
    {
        public static (List<Trade> tlist, bool isOrderFinished) ApplyOrderReport(MarketTrader owner,
            DateTime utcNow, OrderReportBase report, out string errorMessage)
        {
            errorMessage = null;
            var clId = report.ClOrdID;
            (List<Trade> tlist, bool isOrderFinished) ret = new();
            switch (report.MessageNumber)
            {
                case (int)MessageNumbers.OrderPosting:
                    break;
                case (int)MessageNumbers.AcknowledgementReport:
                    break;
                case (int)MessageNumbers.RejectionReport:
                    ret = Handle(owner, (RejectionReport)report, utcNow, out errorMessage);
                    break;
                case (int)MessageNumbers.OrderPostRejection:
                    ret = Handle(owner, (OrderPostRejection)report, utcNow, out errorMessage);
                    break;
                case (int)MessageNumbers.OrderPosted:
                    break;
                case (int)MessageNumbers.OrderStoppedReport:
                    // execution takes to much time - client must decide (wait/forget)
                    ret = Handle(owner, (OrderStoppedReport)report, utcNow, out errorMessage);
                    break;
                case (int)MessageNumbers.OrderFillReport:
                    ret = Handle(owner, (OrderFillReport)report, utcNow, out errorMessage);
                    break;
            }

            return ret;
        }

        private static (List<Trade> tlist, bool isOrderFinished) Handle(MarketTrader owner, 
            OrderStoppedReport report, DateTime utcNow, out string errorMessage)
        {
            errorMessage =
                $"Execution of order(order id is {report.OrderID}  for contract (contract code is {owner.ContractCode} was stopped by timeout";
            var clOrdId = report.ClOrdID;
            if (owner.PostedOrderMap.ContainsKey(clOrdId))
            {
                // 1) cancel operation amounts
                var bindings = owner.PostedOrderMap[clOrdId].Item1;
                foreach (var b in bindings) owner.StrategyMap[b.StrategyId].CurrentOperationAmount = 0;
                // 2) remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
            }
            // 3) mark order as executed
            return (
                tlist: new(),
                isOrderFinished: true
            );
        }

        private static (List<Trade> tlist, bool isOrderFinished) Handle(MarketTrader owner, 
            OrderPostRejection report, DateTime utcNow, out string errorMessage)
        {
            errorMessage =
                $"Order for contract (contract code is {owner.ContractCode} was rejected by {report.RejectionReason}";
            var clOrdId = report.ClOrdID;
            if (owner.PostedOrderMap.ContainsKey(clOrdId))
            {
                // 1) cancel operation amounts
                var bindings = owner.PostedOrderMap[clOrdId].Item1;
                foreach (var b in bindings) owner.StrategyMap[b.StrategyId].CurrentOperationAmount = 0;
                // 2) remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
            }
            // 3) mark order as executed
            return (
                tlist: new(),
                isOrderFinished: true
            );
        }

        private static (List<Trade> tlist, bool isOrderFinished) Handle(MarketTrader owner, 
            RejectionReport report, DateTime utcNow, out string errorMessage)
        {
            errorMessage =
                $"Order (id = {report.OrderID}) for contract (contract code is {owner.ContractCode} was rejected by {report.RejectionReason}";
            var clOrdId = report.ClOrdID;
            if (owner.PostedOrderMap.ContainsKey(clOrdId))
            {
                // 1) cancel operation amounts
                var bindings = owner.PostedOrderMap[clOrdId].Item1;
                foreach (var b in bindings) owner.StrategyMap[b.StrategyId].CurrentOperationAmount = 0;
                // 2) remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
            }
            // 3) mark order as executed
            return (
                tlist: new(),
                isOrderFinished: true
            );
        }

        private static (List<Trade> tlist, bool isOrderFinished) Handle(MarketTrader owner, 
            OrderFillReport report, DateTime utcNow, out string errorMessage)
        {
            errorMessage = null;
            var trades = new List<Trade>();
            var clOrdId = report.ClOrdID;
            if (!owner.PostedOrderMap.ContainsKey(clOrdId))
            {
                // unknown order detected
                // 1) apply fill to first available strategy
                if ((int) report.Fill.SgnQty != 0)
                {
                    var t = MarketTrader.ApplyRealFill(owner.StrategyMap.First().Value, (decimal) report.Fill.Price,
                        report.Fill.TransactTime,
                        report.Fill.ExecID, report.Fill.OrderId, (int) report.Fill.SgnQty);
                    if (t.Count > 0) trades.AddRange(t);
                    // 2) generate error message to client/log
                    errorMessage =
                        $"Execution error detected - unexpected operation for {owner.ContractCode} for {(int)report.Fill.SgnQty} contracts was executed (order id is {report.OrderID}). Offset deal is auto-generated.";
                }

                // 3) mark order as executed
                return (
                    tlist: trades,
                    isOrderFinished: true
                );
            }

            if (report.Fill.Qty == 0)
            {
                return (
                    tlist: trades,
                    isOrderFinished: false
                );
            }

            var bindings = owner.PostedOrderMap[clOrdId].Item1;
            var total = bindings.Sum(b => b.NbrOfContracts);
            if (total == 0)
            {
                // rather impossible situation
                // 1) apply fill to the first available strategy,
                var t = MarketTrader.ApplyRealFill(owner.StrategyMap.First().Value, (decimal)report.Fill.Price,
                    report.Fill.TransactTime,
                    report.Fill.ExecID, report.Fill.OrderId, (int)report.Fill.SgnQty);
                if (t.Count > 0) trades.AddRange(t);
                // 2) generate error message to client/log
                errorMessage =
                    $"Execution error detected - no operation was expected for {owner.ContractCode}, but deal for {(int) report.Fill.SgnQty} contracts was executed (order id is {report.OrderID}). Offset deal is auto-generated.";
                // 3) remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
                // 4) mark order as executed
                return (
                    tlist: trades,
                    isOrderFinished: true
                );
            }
            var cnt = (int)report.Fill.SgnQty;
            if (Math.Sign(total) != Math.Sign(cnt))
            {
                // error processing:
                //1)apply fill to the first binding
                var t = MarketTrader.ApplyRealFill(owner.StrategyMap.[bindings[0].StrategyId], (decimal)report.Fill.Price,
                    report.Fill.TransactTime,
                    report.Fill.ExecID, report.Fill.OrderId, (int)report.Fill.SgnQty);
                if (t.Count > 0) trades.AddRange(t);
                //2)generate error message to client/log
                errorMessage =
                    $"Execution error detected - buy/sell mismatch for {owner.ContractCode} (order id is {report.OrderID}). Offset deal is auto-generated.";
                //3)cancel waiting orders at all others bindings 
                var bcnt = bindings.Count;
                for (var i = 1; i < bcnt; ++i) owner.StrategyMap[bindings[i].StrategyId].CurrentOperationAmount = 0;
                //4)remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
                //5)mark order as executed
                return (
                    tlist: trades,
                    isOrderFinished: true
                );
            }

            if (Math.Abs(cnt) > Math.Abs(total))
            {
                //error processing:
                //1) sequentially apply fill to bindings (last binding will be overfilled)
                var surplus = Math.Sign(cnt) * (Math.Abs(cnt) - Math.Abs(total));
                var bcnt = bindings.Count;
                for (var i = 0; i < bcnt; ++i)
                {
                    var s = owner.StrategyMap[bindings[i].StrategyId];
                    var amount = i == bcnt - 1 ? s.CurrentOperationAmount + surplus : s.CurrentOperationAmount;
                    var t = MarketTrader.ApplyRealFill(s, (decimal) report.Fill.Price,
                        report.Fill.TransactTime,
                        report.Fill.ExecID, report.Fill.OrderId, amount);
                    if(t.Count > 0) trades.AddRange(t);
                }
                //2)generate error message to client/log
                errorMessage = $"Execution error detected - order for {owner.ContractCode} is overfilled by {surplus}  contracts (order id is {report.OrderID}). Offset deal is auto-generated."
                //3)remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
                //4)mark order as executed
                return (
                    tlist: trades,
                    isOrderFinished: true
                );
            }

            foreach (var b in bindings)
            {
                if (Math.Abs(cnt) >= Math.Abs(b.NbrOfContracts))
                {
                    cnt -= b.NbrOfContracts;
                    var t = MarketTrader.ApplyRealFill(owner.StrategyMap[b.StrategyId], (decimal)report.Fill.Price,
                        report.Fill.TransactTime,
                        report.Fill.ExecID, report.Fill.OrderId, b.NbrOfContracts);
                    if (t.Count > 0) trades.AddRange(t);
                    b.NbrOfContracts = 0;
                }
                else
                {
                    if (cnt == 0) break;
                    var t = MarketTrader.ApplyRealFill(owner.StrategyMap[b.StrategyId], (decimal)report.Fill.Price,
                        report.Fill.TransactTime, report.Fill.ExecID, report.Fill.OrderId, cnt);
                    if (t.Count > 0) trades.AddRange(t);
                    b.NbrOfContracts -= cnt;
                    break;
                }
            }
            bindings.RemoveAll(b => b.NbrOfContracts == 0);
            if (bindings.Count == 0)
            {
                owner.PostedOrderMap.Remove(clOrdId);
                return (
                    tlist: trades,
                    isOrderFinished: true
                );
            }
            var strategyOrderInfos = owner.PostedOrderMap[clOrdId].Item1;
            owner.PostedOrderMap[clOrdId] = (strategyOrderInfos, utcNow);
            return (
                tlist: trades,
                isOrderFinished: false
            );
        }
    }
}
