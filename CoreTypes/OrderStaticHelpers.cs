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
                    caller.PostedOrderMap.Add(basketID, unfilledOrders);
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
                        var tl = MarketTrader.SendVirtualFill(caller.StrategyMap[o.StrategyId], virtualExecutionQuote, utcNow, clOrderId, clBasketId);
                        if (tl.Count > 0) trades.AddRange(tl);
                    }
                    else
                    {
                        var tl = MarketTrader.SendPartialVirtualFill(caller.StrategyMap[o.StrategyId], virtualExecutionQuote, utcNow,
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
                    var tl = MarketTrader.SendVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID);
                    if (tl.Count > 0) tList.AddRange(tl);
                }
                foreach (var o in sellOrders)
                {
                    var tl = MarketTrader.SendVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID);
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
                        var tl = MarketTrader.SendVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID);
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
                        var tl = MarketTrader.SendVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID);
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
        public static (List<Trade> tlist, bool isOrderFinished) ApplyOrderReport(MarketTrader owner, DateTime soCurrentUtcTime, OrderReportBase report)
        {
            var clId = report.ClOrdID;
            (List<Trade> tlist, bool isOrderFinished) ret = new();
            if (!owner.PostedOrderMap.ContainsKey(clId))
            {
                // error
            }

            switch (report.MessageNumber)
            {
                case (int)MessageNumbers.OrderPosting:
                    // message to log
                    break;
                case (int)MessageNumbers.AcknowledgementReport:
                    // message to log
                    break;
                case (int)MessageNumbers.RejectionReport:
                    // how to handle?
                    break;
                case (int)MessageNumbers.OrderPostRejection:
                    //HandleOrderPostRejection(report);
                    break;
                case (int)MessageNumbers.OrderPosted:
                    // message to ?
                    break;
                case (int)MessageNumbers.OrderStoppedReport:
                    // execution takes to much time - client must decide (wait/forget)
                    break;
                case (int)MessageNumbers.OrderFillReport:
                    ret = HandleFill(owner, (OrderFillReport)report);
                    break;
            }

            return ret;
        }

        private static (List<Trade> tlist, bool isOrderFinished) HandleFill(MarketTrader owner, OrderFillReport report)
        {
            var trades = new List<Trade>();
            var clOrdId = report.ClOrdID;
            if (!owner.PostedOrderMap.ContainsKey(clOrdId))
            {
                // process error and
                return (
                    tlist: trades,
                    isOrderFinished: false
                );
            }

            if (report.Fill.Qty == 0)
            {
                return (
                    tlist: trades,
                    isOrderFinished: false
                );
            }

            var bindings = owner.PostedOrderMap[clOrdId];
            var total = bindings.Sum(b => b.NbrOfContracts);
            if (total == 0)
            {
                owner.PostedOrderMap.Remove(clOrdId);
                // error
                return (
                    tlist: trades,
                    isOrderFinished: true
                );
            }
            var cnt = (int)report.Fill.SgnQty;
            if (Math.Sign(total) != Math.Sign(cnt))
            {
                // error
                return (
                    tlist: trades,
                    isOrderFinished: false
                );
            }

            if (Math.Abs(cnt) > Math.Abs(total))
            {
                //error
                return (
                    tlist: trades,
                    isOrderFinished: false
                );
            }

            foreach (var b in bindings)
            {
                if (Math.Abs(cnt) >= Math.Abs(b.NbrOfContracts))
                {
                    cnt -= b.NbrOfContracts;
                    var t = owner.SendVirtualFill(owner.StrategyMap[b.StrategyId], (decimal)report.Fill.Price,
                        report.Fill.TransactTime,
                        report.Fill.ExecID, report.Fill.OrderId);
                    if (t.Count > 0) trades.AddRange(t);
                    b.NbrOfContracts = 0;
                }
                else
                {
                    if (cnt == 0) break;
                    var t = owner.SendPartialVirtualFill(owner.StrategyMap[b.StrategyId], (decimal)report.Fill.Price,
                        report.Fill.TransactTime, cnt,
                        report.Fill.ExecID, report.Fill.OrderId);
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
            return (
                tlist: trades,
                isOrderFinished: false
            );
        }
    }
}
