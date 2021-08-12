using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes
{
    public class TradingService : ICommandReceiver
    {
        private const int InternalId = 0; // always!

        #region structure
        // it seems that sorted list is faster than dictionary for static collections
        public SortedList<string, ExchangeTrader> Exchanges { get; } = new();

        public SortedList<string, CurrencyPosition> Positions = new();
        public readonly SortedList<int, ICommandReceiver> CommandReceivers = new();
        // key: marketCode+exchange
        private readonly SortedList<string, PriceProvider> _priceProviderMap = new();
        private readonly SortedList<string, BarAggregator> _barAggregatorMap = new();
        private readonly SortedList<string, ContractDetailsManager> _contactManagers = new();

        // clOrderId -> Idx of MarketTrader
        // it is dynamical collection, so - dictionary
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

        private List<string> GetTicksInfo(DateTime utcNow) => 
            _priceProviderMap.Select(kvp => kvp.Value.AsString(utcNow, kvp.Key)).ToList();
        private List<string> GetBarsInfo(List<Tuple<Bar, string, bool>> bars) => 
            bars.Select(t => t.Item1.AsString(t.Item2,t.Item3)).ToList();

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
        public ServiceRestrictionsManager RestrictionManager => _restrictionManager;

        private readonly ErrorCollector _errorCollector;
        public ErrorCollector Collector => _errorCollector;
        private DateTime _lastDayStart = DateTime.UtcNow.AddDays(-1);

        public TradingService(TradingConfiguration cfg)
        {
            _currentRestriction = _restrictionManager.GetCurrentRestriction();
            CommandReceivers.Add(InternalId,this);
            _errorCollector = new ErrorCollector(cfg.MaxErrorsPerDay);
        }


        public (List<(string, string)> subscriptions, List<MarketOrderDescription> orders, TradingServiceState state,
            List<string> ticksInfo, List<string> barsInfo, List<string> tradesInfo, List<(string,string,string)> errors)
            ProcessCurrentState(StateObject so, List<ICommand> clCmdList, List<ICommand> schCmdList)
        {
            _errorCollector.ForgetErrors = false;
            if (clCmdList?.Count > 0) ApplyCommands(clCmdList);
            if (schCmdList?.Count > 0) ApplyCommands(schCmdList);
            ApplyNewTicks(so);
            var newBars = MakeNewOneMinuteBars(so);
            // TODO inject new bars (and ticks?) to indicator container
            var newTrades = ApplyOrderReports(so, out var errorMessages, out int errorsNbr);
            UpdateErrorCollector(so, errorsNbr);
            UpdateProfitLossInfos();
            var subscriptionList = ProcessContractInfos(so);
            UpdateParentRestrictions();
            var ordersToPost = GenerateOrders(so.CurrentUtcTime, newTrades);

            var currentState = GetCurrentState(errorMessages, so);
            return (
                subscriptions: subscriptionList,
                orders: ordersToPost,
                state: currentState,
                ticksInfo: GetTicksInfo(so.CurrentUtcTime),
                barsInfo: GetBarsInfo(newBars),
                tradesInfo: newTrades,
                errors: errorMessages
                    );
        }

        #region privates

        private void UpdateErrorCollector(StateObject so, int errorsNbr)
        {
            if ((so.CurrentUtcTime - _lastDayStart).TotalDays >= 1)
            {
                _errorCollector.Reset();
                _lastDayStart = so.CurrentUtcTime;
            }

            _errorCollector.ApplyErrors(errorsNbr);
            if (_errorCollector.ForgetErrors)
            {
                _errorCollector.Reset();
                _errorCollector.ForgetErrors = false;
            }
            _restrictionManager.SetErrorsNbrRestriction(_errorCollector.IsStopped
                ? TradingRestriction.HardStop
                : TradingRestriction.NoRestrictions);
        }
        private TradingServiceState GetCurrentState(List<(string market, string exchange, string txt)> errorMessages, StateObject so)
        {
            var msgToClient = so.TextMessageList;
            msgToClient.AddRange(errorMessages.Select(em => new Tuple<string,string>("Error",em.txt)));
            return new TradingServiceState(msgToClient, errorMessages, this);
        }

        private List<MarketOrderDescription> GenerateOrders(DateTime utcNow, List<string> newTrades)
        {
            List<(MarketTrader, MarketOrderDescription, List<string>)> ogs = 
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
        private List<string> ApplyOrderReports(StateObject so, out List<(string market, string exchange, string txt)> errorMessages, out int errorsNbr)
        {
            static string ReportUnknownExecution(OrderStateMessage report)
            {
                switch (report.MyType)
                {
                    case OrderStateMessageType.Cancel:
                        {
                            var r = ((OrderCancelMessage)report);
                            return $"Rejection report for unknown order detected for {r.OrderId} by reason: {r.CancelReason}";
                        }
                    case OrderStateMessageType.Execution:
                        {
                            var r = ((OrderExecutionMessage)report);
                            return $"OrderFill report for unknown order detected for {r.Symbol} at {r.Exchange} (order id is {r.OrderId})";
                        }
                    default: return "OrderReport for unknown order detected";
                }
            }

            errorsNbr = 0;
            errorMessages = new();
            if (so.OrderStateMessageList.Count == 0) return null;
            List<string> newTrades = new();
            
            foreach (var report in so.OrderStateMessageList)
            {
                if (!_reportsRoutingMap.ContainsKey(report.ClOrderId))
                {
                    errorMessages.Add(("UNK","UNK",ReportUnknownExecution(report)));
                    errorsNbr++;
                }
                else
                {
                    var mt = _reportsRoutingMap[report.ClOrderId];
                    var (tradeList, isOrderFinished) = mt.ApplyOrderReport(so.CurrentUtcTime, 
                        report, out var errorMessage);
                    if (errorMessage != null)
                    {
                        errorMessages.Add((mt.MarketCode,mt.Exchange,errorMessage));
                        errorsNbr++;
                    }
                    if (isOrderFinished) _reportsRoutingMap.Remove(report.ClOrderId);
                    if (tradeList?.Count > 0) newTrades.AddRange(tradeList);
                }
            }
            // timeout management
            foreach (var cm in _contactManagers.Values)
            {
                var msgList = cm.ApplyCurrentTime(so.CurrentUtcTime, out var idList);
                var idx = 0;
                foreach (var id in idList)
                {
                    if (_reportsRoutingMap.ContainsKey(id))
                    {
                        var mt = _reportsRoutingMap[id];
                        errorMessages.Add((mt.MarketCode,mt.Exchange,msgList[idx]));
                        _reportsRoutingMap.Remove(id);
                        errorsNbr++;
                    }
                    ++idx;
                }
            }
            return newTrades;
        }
        private List<(string symbol, string exchange)> ProcessContractInfos(StateObject so)
        {
            List<(string marketCode, string exchange, string error)> subscriptionList = 
                (from ci in so.ContractInfoList 
                    let key = ci.MarketName + ci.Exchange 
                    where _contactManagers.ContainsKey(key) 
                    select _contactManagers[key].ProcessContractInfo(ci, so.CurrentUtcTime) 
                    into res
                    select res).ToList();
            subscriptionList.AddRange(_contactManagers.Values.Select(cm => cm.ProcessContractInfo(null, so.CurrentUtcTime)));
            foreach (var (_,_,error) in subscriptionList.Where(t => t.error != string.Empty))
                so.TextMessageList.Add(new Tuple<string, string>("SubscriptionError", error));
            return subscriptionList.Where(t=>t.marketCode != string.Empty && t.error == string.Empty)
                .Select(t=> (t.marketCode, t.exchange))
                .ToList();
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
                    else if (command is ErrorsForgetCommand errorsCommand)
                        _errorCollector.ForgetErrors = true;
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
