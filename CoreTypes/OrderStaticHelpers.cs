using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes
{
    public static class OrderGenerator
    {
        public static (MarketTrader, MarketOrderDescription order)
            GenerateOrders(MarketTrader caller, DateTime utcNow, InfoCollector ic)
        {
            var (bid, ask, last) = caller.Position.PriceProvider.LastPrices;
            if (bid == -1 || ask == -1 || last == -1) return (caller, null);
            var so = caller.StrategyMap.Values
                .Select(s => s.GenerateOrder((bid, ask, last)))
                .Where(t => t.NbrOfContracts != 0)
                .ToList();

            var unfilledOrders = Reduce(caller, so, bid, utcNow, ic, out var basketID);
            if (unfilledOrders is { Count: > 0 })
            {
                var amount = unfilledOrders.Sum(o => o.NbrOfContracts);
                if (amount != 0)
                {
                    caller.PostedOrderMap.Add(basketID, (unfilledOrders,utcNow));
                    return (caller,
                        new MarketOrderDescription(basketID, caller.MarketCode, caller.Exchange, amount));
                }
            }
            return (caller, null);
        }

        private static List<StrategyOrderInfo> Reduce(MarketTrader caller, List<StrategyOrderInfo> so, decimal bid, 
            DateTime utcNow, InfoCollector ic,
            out int clBasketID)
        {
            List<StrategyOrderInfo> ReduceNotFilledOrders(List<StrategyOrderInfo> strategyOrderInfos,
                int virtuallyFilled, decimal virtualExecutionQuote,
                string clOrderId, int clBasketId)
            {
                var lst = strategyOrderInfos.OrderBy(o => o.NbrOfContracts).ToList();
                foreach (var o in lst)
                {
                    var rest = virtuallyFilled - o.NbrOfContracts;
                    if (rest >= 0)
                        MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], virtualExecutionQuote, utcNow,
                            clOrderId, clBasketId, ic);
                    else
                    {
                        MarketTrader.ApplyPartialVirtualFill(caller.StrategyMap[o.StrategyId], virtualExecutionQuote, utcNow,
                            virtuallyFilled, clOrderId, clBasketId, ic);
                        break;
                    }

                    if (rest == 0) break;
                    virtuallyFilled = rest;
                }

                lst.RemoveAll(item => item.NbrOfContracts == 0);
                return lst;
            }

            var (buyOrders, sellOrders) =
                (so.Where(o => o.NbrOfContracts > 0).ToList(), so.Where(o => o.NbrOfContracts < 0).ToList());

            clBasketID = ClientOIdProvider.GetNextClientOrderId();
            var clOrderID = clBasketID + "_internal";

            List<StrategyOrderInfo> notFilledOrders;
            var pAmount = buyOrders.Sum(o => o.NbrOfContracts);
            var nAmount = sellOrders.Sum(o => o.NbrOfContracts);
            var amountToSend = pAmount + nAmount;
            if (amountToSend == 0)
            {
                // all orders are executed virtually :)
                foreach (var o in buyOrders)
                    MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID,
                        ic);
                foreach (var o in sellOrders)
                    MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID, clBasketID,
                        ic);

                return null;
            }
            if (amountToSend > 0)
            {
                notFilledOrders = buyOrders;
                if (nAmount != 0)
                {
                    // we'll send to broker 'buy' order and virtually execute all 'sell' strategy orders and some number of 'buy' strategy orders
                    // NB! only one 'buy' order can be executed partially
                    foreach (var o in sellOrders)
                        MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID,
                            clBasketID, ic);
                    sellOrders.Clear();
                    notFilledOrders = ReduceNotFilledOrders(notFilledOrders, -nAmount,
                        bid, clOrderID, clBasketID);
                }
            }
            else // amountToSend < 0
            {
                notFilledOrders = sellOrders;
                if (pAmount != 0)
                {
                    foreach (var o in buyOrders)
                        MarketTrader.ApplyVirtualFill(caller.StrategyMap[o.StrategyId], bid, utcNow, clOrderID,
                            clBasketID, ic);
                    buyOrders.Clear();
                    notFilledOrders = ReduceNotFilledOrders(notFilledOrders, -pAmount,
                        bid, clOrderID, clBasketID);
                }
            }
            return notFilledOrders;
        }
    }

    public static class OrderReportsProcessor
    {
        public static bool ApplyOrderReport(MarketTrader owner,
            DateTime utcNow, OrderStateMessage report, InfoCollector ic)
        {
            return report.MyType switch
            {
                OrderStateMessageType.Cancel => Handle(owner, report, ic),
                OrderStateMessageType.Execution => Handle(owner, (OrderExecutionMessage) report, ic, utcNow),
                _ => false
            };
        }

        private static bool Handle(MarketTrader owner, OrderStateMessage report, InfoCollector ic)
        {
            owner.ErrorTracker.ChangeValueBy(1);
            //errorMessage = null;
            var clOrdId = report.ClOrderId;
            var mkt = owner.MarketCode;
            var exch = owner.Exchange;
            if (owner.PostedOrderMap.ContainsKey(clOrdId))
            {
                // 1) cancel operation amounts
                var bindings = owner.PostedOrderMap[clOrdId].Item1;
                foreach (var b in bindings)
                {
                    var st = owner.StrategyMap[b.StrategyId];
                    var errorMessage =
                        $"Strategy order is cancelled. (strategyName: {st.Position.StrategyName}, " +
                        $"cancelled amount: {st.CurrentOperationAmount}, order id: {report.OrderId}).";
                    st.CurrentOperationAmount = 0;
                    ic.Accept((mkt,exch,errorMessage));
                }
                // 2) remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
            }
            // 3) mark order as executed
            return true;
        }

        private static bool Handle(MarketTrader owner, 
            OrderExecutionMessage report, InfoCollector ic, DateTime utcNow)
        {
            var mkt = owner.MarketCode;
            var exch = owner.Exchange;
            var clOrdId = report.ClOrderId;
            if (!owner.PostedOrderMap.ContainsKey(clOrdId))
            {
                // unknown order detected
                // 1) apply fill to first available strategy
                if (report.SgnQty != 0)
                {
                    MarketTrader.ApplyRealFill(owner.StrategyMap.First().Value, report, ic);
                    // 2) generate error message to client/log
                    var errorMessage =
                        $"Execution error detected - unexpected operation for {owner.ContractCode} for {report.SgnQty} contracts " +
                        $"was executed (order id is {report.OrderId}). Offset deal is auto-generated.";
                    ic.Accept((mkt,exch,errorMessage));
                    owner.ErrorTracker.ChangeValueBy(1);
                }

                // 3) mark order as executed
                return true;
            }

            if (report.SgnQty == 0)
                return false;

            var bindings = owner.PostedOrderMap[clOrdId].Item1;
            var total = bindings.Sum(b => b.NbrOfContracts);
            if (total == 0)
            {
                // rather impossible situation
                // 1) apply fill to the first available strategy,
                MarketTrader.ApplyRealFill(owner.StrategyMap.First().Value, report, ic);
                // 2) generate error message to client/log
                var errorMessage =
                    $"Execution error detected - no operation was expected for {owner.ContractCode}, but deal for {report.SgnQty}" +
                    $" contracts was executed (order id is {report.OrderId}). Offset deal is auto-generated.";
                ic.Accept((mkt,exch,errorMessage));
                owner.ErrorTracker.ChangeValueBy(1);
                // 3) remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
                // 4) mark order as executed
                return true;
            }
            var cnt = report.SgnQty;
            if (Math.Sign(total) != Math.Sign(cnt))
            {
                // error processing:
                //1)apply fill to the first binding
                MarketTrader.ApplyRealFill(owner.StrategyMap[bindings[0].StrategyId], report, ic);
                //2)generate error message to client/log
                var errorMessage =
                    $"Execution error detected - buy/sell mismatch for {owner.ContractCode} (order id is {report.OrderId}). " +
                    $"Offset deal is auto-generated.";
                ic.Accept((mkt,exch,errorMessage));
                owner.ErrorTracker.ChangeValueBy(1);
                //3)cancel waiting orders at all others bindings 
                var bcnt = bindings.Count;
                for (var i = 1; i < bcnt; ++i)
                {
                   var st = owner.StrategyMap[bindings[i].StrategyId];
                    var error =
                        $"Strategy order is cancelled by execution error. (strategyName: {st.Position.StrategyName}, " +
                        $"cancelled amount: {st.CurrentOperationAmount}, order id: {report.OrderId}).";
                    st.CurrentOperationAmount = 0;
                    ic.Accept((mkt, exch, error));
                }
                //4)remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
                //5)mark order as executed
                return true;
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
                    MarketTrader.ApplyRealFill(s, report, amount, ic);
                }
                //2)generate error message to client/log
                var errorMessage =
                    $"Execution error detected - order for {owner.ContractCode} is overfilled by {surplus}  contracts " +
                    $"(order id is {report.OrderId}). Offset deal is auto-generated.";
                ic.Accept((mkt,exch,errorMessage));
                owner.ErrorTracker.ChangeValueBy(1);
                //3)remove order from postedOrdersMap
                owner.PostedOrderMap.Remove(clOrdId);
                //4)mark order as executed
                return true;
            }

            foreach (var b in bindings)
            {
                if (Math.Abs(cnt) >= Math.Abs(b.NbrOfContracts))
                {
                    cnt -= b.NbrOfContracts;
                    MarketTrader.ApplyRealFill(owner.StrategyMap[b.StrategyId], report, b.NbrOfContracts, ic);
                    b.NbrOfContracts = 0;
                }
                else
                {
                    if (cnt == 0) break;
                    MarketTrader.ApplyRealFill(owner.StrategyMap[b.StrategyId], report, cnt, ic);
                    b.NbrOfContracts -= cnt;
                    break;
                }
            }
            bindings.RemoveAll(b => b.NbrOfContracts == 0);
            if (bindings.Count == 0)
            {
                owner.PostedOrderMap.Remove(clOrdId);
                return true;
            }
            var strategyOrderInfos = owner.PostedOrderMap[clOrdId].Item1;
            owner.PostedOrderMap[clOrdId] = (strategyOrderInfos, utcNow);
            return false;
        }
    }
}
