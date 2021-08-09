using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Channels;
using Messages;

namespace CoreTypes
{
    public class TradingService : ICommandReceiver
    {
        private const int InternalId = 1; // always!

        #region structure

        public Dictionary<string, ExchangeTrader> Exchanges { get; } = new();
        public Dictionary<string, CurrencyPosition> Positions = new();
        public readonly Dictionary<int, ICommandReceiver> CommandReceivers = new();
        // key: marketCode+exchange
        private readonly Dictionary<string, PriceProvider> _priceProviderMap = new();
        private readonly Dictionary<string, BarAggregator> _barAggregatorMap = new();
        private readonly Dictionary<string, ContractDetailsManager> _contactManagers = new();

        // clOrderId -> Idx of MarketTrader
        private readonly Dictionary<int, MarketTrader> _reportsRoutingMap = new();

        private void RegisterExchangeTrader(ExchangeTrader ct)
        {
            var currency = ct.Currency;
            if(!Positions.ContainsKey(currency)) Positions.Add(currency,new CurrencyPosition());
            Positions[currency].RegisterExchangePosition(ct);
            Exchanges.Add(ct.Exchange,ct);
            CommandReceivers.Add(ct.InternalID,ct);
        }

        private void RegisterMarketTrader(string exchangeName, MarketTrader mt)
        {
            if (Exchanges.ContainsKey(exchangeName))
            {
                var et = Exchanges[exchangeName];
                et.RegisterMarketTrader(mt);
                CommandReceivers.Add(mt.InternalID, mt);
                var key = mt.MarketCode + exchangeName;
                _priceProviderMap.Add(key, mt.Position.PriceProvider);
                _barAggregatorMap.Add(key, new BarAggregator(
                    new XSecondsAggregationRules(60), mt.MarketCode + et.Exchange, 1));
                _contactManagers.Add(key, mt.ContractManager);
            }
        }

        private void RegisterStrategyTrader(string exchangeName, string marketName, StrategyTrader st)
        {
            if (Exchanges.ContainsKey(exchangeName))
            {
                var et = Exchanges[exchangeName];
                if (et.Markets.ContainsKey(marketName))
                {
                    var mt = et.Markets[marketName];
                    mt.RegisterStrategyTrader(st);
                    CommandReceivers.Add(st.InternalID, st);
                }
            }
        }

        #endregion // structure

        private readonly ServiceRestrictionsManager _restrictionManager = new();
        private TradingRestriction _currentRestriction;

        public TradingService(TradingConfiguration cfg)
        {
            _currentRestriction = _restrictionManager.GetCurrentRestriction();
        }


        public (List<(string, string)> subscriptions, List<MarketOrderDescription> orders, TradingServiceState state)
            ProcessCurrentState(StateObject so, List<ICommand> clCmdList, List<ICommand> schCmdList)
        {
            if (clCmdList?.Count > 0) ApplyCommands(clCmdList);
            if (schCmdList?.Count > 0) ApplyCommands(schCmdList);
            ApplyNewTicks(so);
            var newBars = MakeNewOneMinuteBars(so);
            // TODO inject new bars (and ticks?) to indicator container
            var newTrades = ApplyOrderReports(so, out var errorMessages);
            UpdateProfitLossInfos();
            var subscriptionList = ProcessContractInfos(so);
            UpdateParentRestrictions();
            // TODO realize GetCurrentState(so) - now it is just a stub
            TradingServiceState currentState = GetCurrentState(so);
            // TODO log all information - ticks, bars, trades, reports etc
            // some logging library?
            var ordersToPost = GenerateOrders(so.CurrentUtcTime, newTrades);
            return (
                subscriptions: subscriptionList,
                orders: ordersToPost,
                state: currentState
                    );
        }

        #region privates

        private TradingServiceState GetCurrentState(StateObject so) => new TradingServiceState();
        private List<MarketOrderDescription> GenerateOrders(DateTime utcNow, List<Trade> newTrades)
        {
            List<(MarketTrader, MarketOrderDescription, List<Trade>)> ogs = 
                (from et in Exchanges.Values 
                    from mt in et.Markets.Values 
                    select mt.GenerateOrders(utcNow))
                .ToList();
            List<MarketOrderDescription> ordersToPost = new();
            foreach (var (mt, o, trades) in ogs)
            {
                _reportsRoutingMap.Add(o.ClOrdId, mt);
                ordersToPost.Add(o);
                if (trades.Count > 0) newTrades.AddRange(trades);
            }
            return ordersToPost;
        }
        private void UpdateProfitLossInfos()
        {
            foreach (var kvp in Positions) kvp.Value.Update();
        }
        private List<Trade> ApplyOrderReports(StateObject so, out List<string> errorMessages)
        {
            static string ReportUnknownExecution(OrderReportBase report)
            {
                switch (report.MessageNumber)
                {
                    case (int)MessageNumbers.OrderPosting:
                        {
                            var r = ((OrderPosting)report);
                            return $"OrderPosting report for unknown order detected for {r.Symbol} (order id is {r.OrderID})";
                        }
                    case (int)MessageNumbers.AcknowledgementReport:
                        {
                            var r = ((AcknowledgementReport)report);
                            return $"Acknowledgement report for unknown order detected for {r.Symbol} (order id is {r.OrderID})";
                        }
                    case (int)MessageNumbers.RejectionReport:
                        {
                            var r = ((RejectionReport)report);
                            return $"Rejection report for unknown order detected for {r.Symbol} (order id is {r.OrderID})";
                        }
                    case (int)MessageNumbers.OrderPostRejection:
                        {
                            var r = ((OrderPostRejection)report);
                            return $"OrderPostRejection report for unknown order detected (reason is {r.RejectionReason})";
                        }
                    case (int)MessageNumbers.OrderPosted:
                        {
                            var r = ((OrderPosted)report);
                            return $"OrderPosted report for unknown order detected for {r.Symbol} (order id is {r.OrderID})";
                        }
                    case (int)MessageNumbers.OrderStoppedReport:
                        {
                            var r = ((OrderStoppedReport)report);
                            return $"OrderStopped report for unknown order detected for {r.Symbol} (order id is {r.OrderID})";
                        }
                    case (int)MessageNumbers.OrderFillReport:
                        {
                            var r = ((OrderFillReport)report);
                            return $"OrderFill report for unknown order detected for {r.Symbol} (order id is {r.OrderID})";
                        }
                    default: return "OrderReport for unknown order detected";
                }
            }
            errorMessages = new();
            if (so.OrderReportBaseList.Count == 0) return null;
            List<Trade> newTrades = new();
            
            foreach (var report in so.OrderReportBaseList)
            {
                if (!_reportsRoutingMap.ContainsKey(report.ClOrdID))
                    errorMessages.Add(ReportUnknownExecution(report));
                else
                {
                    var (tradeList, isOrderFinished) = _reportsRoutingMap[report.ClOrdID].ApplyOrderReport(so.CurrentUtcTime, report);
                    if (isOrderFinished) _reportsRoutingMap.Remove(report.ClOrdID);
                    if (tradeList?.Count > 0) newTrades.AddRange(tradeList);
                }
            }
            return newTrades;
        }
        private List<(string SymbolDocumentInfo, string exchange)> ProcessContractInfos(StateObject so)
        {
            List<(string marketCode, string exchange)> subscriptionList = 
                (from ci in so.ContractInfoList 
                    let key = ci.MarketName + ci.Exchange 
                    where _contactManagers.ContainsKey(key) 
                    select _contactManagers[key].ProcessContractInfo(ci, so.CurrentUtcTime) 
                    into res
                    select res).ToList();
            subscriptionList.AddRange(_contactManagers.Values.Select(cm => cm.ProcessContractInfo(null, so.CurrentUtcTime)));
            return subscriptionList.Where(t=>t.marketCode != string.Empty).ToList();
        }
        private List<Tuple<Bar, string, bool>> MakeNewOneMinuteBars(StateObject so)
        {
            HashSet<string> acc = new();
            List<Tuple<Bar, string, bool>> newBars = new();
            foreach (var b in so.BarUpdateList)
            {
                var ret = _barAggregatorMap[b.SymbolExchange].ProcessBar(b, so.CurrentUtcTime);
                if (ret != null) newBars.Add(ret);
                acc.Add(b.SymbolExchange);
            }

            foreach (var mc in _priceProviderMap.Keys)
            {
                if (acc.Contains(mc)) continue;
                var ret = _barAggregatorMap[mc].ProcessTime(so.CurrentUtcTime);
                if (ret != null) newBars.Add(ret);
            }

            return newBars;
        }
        private void ApplyNewTicks(StateObject so)
        {
            foreach (var ti in so.TickInfoList) _priceProviderMap[ti.SymbolExchange].Update(so.CurrentUtcTime, ti);
        }
        public void ApplyCommand(ICommand command)
        {
            var s = command.Source;
            switch (s)
            {
                case CommandSource.User:
                    if(command is RestrictionCommand userCommand)
                        _restrictionManager.SetUserRestriction(userCommand.Restriction);
                    break;
                case CommandSource.Scheduler:
                    if (command is RestrictionCommand schedulerCommand)
                        _restrictionManager.SetSchedulerRestriction(schedulerCommand.Restriction);
                    break;
            }
        }
        private void ApplyCommands(List<ICommand> cmdList)
        {
            foreach (var cmd in cmdList.Where(cmd => CommandReceivers.ContainsKey(cmd.DestinationId)))
                CommandReceivers[cmd.DestinationId].ApplyCommand(cmd);
        }
        private void UpdateParentRestrictions()
        {
            _currentRestriction = _restrictionManager.GetCurrentRestriction();
            foreach (var cm in Exchanges.Values) cm.UpdateParentRestrictions(_currentRestriction);
        }

        #endregion
    }
}
