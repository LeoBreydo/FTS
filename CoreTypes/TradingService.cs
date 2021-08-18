using System;
using System.Collections.Generic;
using System.Linq;
using static CoreTypes.MessageStringProducer;


namespace CoreTypes
{
    // aliases
    using LSSS = List<(string market,string exchange,string txt)>;
    using LSS = List<(string symbol, string exchange)>;
    using LS = List<string>;
    using LMOD = List<MarketOrderDescription>;
    using LC = List<ICommand>;

    public struct OutInfo
    {
        public LSS Subscriptions;
        public LMOD Orders;
        public TradingServiceState State;
        public LS TicksInfo;
        public LS BarsInfo;
        public LS TradesInfo;
        public LSSS Errors;
        public List<(string, int, double)> NewBpvMms;

        public OutInfo(LSS subscriptions, LMOD orders, TradingServiceState state,
            LS ticksInfo, LS barsInfo, LS tradesInfo, LSSS errors, List<(string, int, double)> newBpvMms)
        {
            Subscriptions = subscriptions;
            Orders = orders;
            State = state;
            TicksInfo = ticksInfo;
            BarsInfo = barsInfo;
            TradesInfo = tradesInfo;
            Errors = errors;
            NewBpvMms = newBpvMms;
        }
    }
    public class TradingService : ICommandReceiver
    {
        public int Id;

        #region structure
        // it seems that sorted list is faster than dictionary for static collections
        public SortedList<string, ExchangeTrader> Exchanges { get; } = new();

        public SortedList<string, CurrencyPosition> Positions = new();
        public readonly SortedList<int, ICommandReceiver> CommandReceivers = new();
        // key: marketCode+exchange
        private readonly SortedList<string, PriceProvider> _priceProviderMap = new();
        private readonly SortedList<string, BarAggregator> _barAggregatorMap = new();
        private readonly SortedList<string, ContractDetailsManager> _contractManagers = new();

        // clOrderId -> Idx of MarketTrader
        // it is dynamical collection, so - dictionary
        private readonly Dictionary<int, MarketTrader> _reportsRoutingMap = new();

        private void RegisterExchangeTrader(ExchangeTrader ct)
        {
            var currency = ct.Currency;
            if(!Positions.ContainsKey(currency)) Positions.Add(currency,new CurrencyPosition());
            Positions[currency].RegisterExchangePosition(ct);
            Exchanges.Add(ct.Exchange,ct);
            CommandReceivers.Add(ct.Id,ct);
        }

        private void RegisterMarketTrader(string exchangeName, MarketTrader mt)
        {
            if (Exchanges.ContainsKey(exchangeName))
            {
                var et = Exchanges[exchangeName];
                et.RegisterMarketTrader(mt);
                CommandReceivers.Add(mt.Id, mt);
                var key = mt.MarketCode + exchangeName;
                _priceProviderMap.Add(key, mt.Position.PriceProvider);
                _barAggregatorMap.Add(key, new BarAggregator(
                    new XSecondsAggregationRules(60), mt.MarketCode + et.Exchange, 1));
                _contractManagers.Add(key, mt.ContractManager);
            }
        }

        private List<string> GetTicksInfo(DateTime utcNow) => 
            _priceProviderMap.Select(kvp => PriceProviderString(kvp.Value,utcNow, kvp.Key)).ToList();
        private static List<string> GetBarsInfo(List<Tuple<Bar, string, bool>> bars) => 
            bars.Select(t => BarInfoString(t.Item1,t.Item2,t.Item3)).ToList();

        private void RegisterStrategyTrader(string exchangeName, string marketName, StrategyTrader st)
        {
            if (Exchanges.ContainsKey(exchangeName))
            {
                var et = Exchanges[exchangeName];
                if (et.Markets.ContainsKey(marketName))
                {
                    var mt = et.Markets[marketName];
                    mt.RegisterStrategyTrader(st);
                    CommandReceivers.Add(st.Id, st);
                }
            }
        }

        #endregion // structure

        private TradingRestriction _currentRestriction;
        public ServiceRestrictionsManager RestrictionManager { get; } = new();
        public IValueTracker<int,WorkingState> ErrorTracker { get; }
        private DateTime _lastDayStart = DateTime.UtcNow.AddDays(-1);

        public TradingService(TradingConfiguration cfg, SignalService signalService)
        {
            Id = cfg.Id;
            _currentRestriction = RestrictionManager.GetCurrentRestriction();
            CommandReceivers.Add(Id,this);
            ErrorTracker = new ErrorTracker(cfg.MaxErrorsPerDay);
            foreach (var cet in cfg.Exchanges)
            {
                var et = new ExchangeTrader(cet.Id, cet.ExchangeName, cet.Currency, cet.MaxErrorsPerDay);
                RegisterExchangeTrader(et);
                foreach (var cmt in cet.Markets)
                {
                    var mt = new MarketTrader(cmt.Id, cmt.MarketName,
                        cet.ExchangeName,cmt.MaxErrorsPerDay,
                        cmt.SessionCriticalLoss,cmt.BigPointValue, cmt.MinMove);
                    RegisterMarketTrader(cet.ExchangeName,mt);
                    foreach (var cst in cmt.Strategies)
                    {
                        var position = new StrategyPosition(cst.Id, cst.StrategyName, cst.SessionCriticalLoss);
                        var pv = new PositionValidator(position);
                        var targetLevel = (cst.UseTakeProfitGuard) ? cst.TakeProfitDelta : 0;
                        var initialStopLevel = 0d;
                        var trailingStopLevel = 0d;
                        var trailingActivation = 0d;

                        switch (cst.StopLossPositionGuardType)
                        {
                            case StopLossPositionGuardTypes.Fixed:
                                initialStopLevel = cst.FixedStopLossDelta;
                                break;
                            case StopLossPositionGuardTypes.Trailed:
                                initialStopLevel = cst.TrailedStopLossInitialDelta;
                                trailingStopLevel = cst.TrailingDelta;
                                trailingActivation = cst.ActivationProfit;
                                break;
                            case StopLossPositionGuardTypes.No:
                                break;
                        }
                        pv.Init(targetLevel, initialStopLevel, trailingStopLevel, trailingActivation,
                            cst.StoplossRestriction_MaxBarsToWaitForOppositeSignal,
                            cst.StoplossRestriction_GoToFlatMustLiftRestriction);

                        var st = new StrategyTrader(cst.Id, position, cst.NbrOfContracts, signalService, pv);
                        position.Owner = st;
                        RegisterStrategyTrader(cet.ExchangeName, cmt.MarketName,st);
                    }
                }
            }
        }

        public bool IsReadyToBeStopped => _contractManagers.All(kvp => kvp.Value.IsReadyToBeStopped);

        public OutInfo ProcessCurrentState(StateObject so, LC clCmdList, LC schCmdList)
        {
            if (clCmdList?.Count > 0) ApplyCommands(clCmdList);
            if (schCmdList?.Count > 0) ApplyCommands(schCmdList);

            ApplyNewTicks(so);
            var newBars = MakeNewOneMinuteBars(so);
            //if (newBars != null && newBars.Count > 0)
            //{
            //    int a = 1; check that we have stream of minute bars
            //}

            // TODO inject new bars (and ticks?) to indicator container
            var subscriptionList = ProcessContractInfos(so, out var bm);
            var newTrades = ApplyOrderReports(so, out var errorMessages);
            
            UpdateProfitLossInfos(so);
            UpdateParentRestrictions();

            var ordersToPost = GenerateOrders(so.CurrentUtcTime, newTrades);
            var currentState = GetCurrentState(errorMessages, so);

             return new(
                subscriptions: subscriptionList,
                orders: ordersToPost,
                state: currentState,
                ticksInfo: GetTicksInfo(so.CurrentUtcTime),
                barsInfo: GetBarsInfo(newBars),
                tradesInfo: newTrades,
                errors: errorMessages,
                newBpvMms: bm
            );
        }

        #region privates
        private TradingServiceState GetCurrentState(LSSS errorMessages, 
            StateObject so)
        {
            var msgToClient = new List<Tuple<string, string>>();
            msgToClient.AddRange(so.TextMessageList);
            msgToClient.AddRange(errorMessages.Select(em => new Tuple<string,string>("Error",em.txt)));
            return new TradingServiceState(msgToClient, this);
        }

        private LMOD GenerateOrders(DateTime utcNow, LS newTrades)
        {
            List<(MarketTrader, MarketOrderDescription, List<string>)> ogs = 
                (from et in Exchanges.Values 
                    from mt in et.Markets.Values 
                    select mt.GenerateOrders(utcNow))
                .ToList();
            LMOD ordersToPost = new();
            foreach (var (mt, o, trades) in ogs)
            {
                if (o != null)
                {
                    _reportsRoutingMap.Add(o.ClOrdId, mt);
                    ordersToPost.Add(o);
                }
                if (trades.Count > 0) newTrades.AddRange(trades);
            }
            return ordersToPost;
        }
        private void UpdateProfitLossInfos(StateObject so)
        {
            var startNeDay = (so.CurrentUtcTime - _lastDayStart).TotalDays >= 1;
            var totalErrors = Positions.Aggregate(0, (current, kvp) => current + kvp.Value.Update(startNeDay));
            ErrorTracker.SetExternalTrackingValue(totalErrors);
            ErrorTracker.CalculateState();
            if (startNeDay)
            {
                ErrorTracker.StartNewTrackingPeriod();
                _lastDayStart = so.CurrentUtcTime;
            }
            RestrictionManager.SetErrorsNbrRestriction(ErrorTracker.State == WorkingState.Stopped
                ? TradingRestriction.HardStop
                : TradingRestriction.NoRestrictions);
        }

        private LS ApplyOrderReports(StateObject so, out LSSS errorMessages)
        {
            static string ProcessUnknownReport(OrderStateMessage report)
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
                    case OrderStateMessageType.Post:
                        {
                            var r = ((OrderPostMessage)report);
                            return $"OrderPost report for unknown order detected for {r.Symbol} at {r.Exchange} (order id is {r.OrderId})";
                        }
                    default: return "OrderReport for unknown order detected";
                }
            }

            errorMessages = new();
            if (so.OrderStateMessageList.Count == 0) return null;
            LS newTrades = new();
            
            foreach (var report in so.OrderStateMessageList)
            {
                if (!_reportsRoutingMap.ContainsKey(report.ClOrderId))
                {
                    errorMessages.Add(("UNK","UNK",ProcessUnknownReport(report)));
                    ErrorTracker.ChangeValueBy(1);
                }
                else
                {
                    if (report.MyType == OrderStateMessageType.Post)
                        continue;
                    var mt = _reportsRoutingMap[report.ClOrderId];
                    var (tradeList, isOrderFinished) = mt.ApplyOrderReport(so.CurrentUtcTime, 
                        report, out var errorMessage);
                    if (!string.IsNullOrEmpty(errorMessage))
                        errorMessages.Add((mt.MarketCode, mt.Exchange, errorMessage));
                    if (isOrderFinished) _reportsRoutingMap.Remove(report.ClOrderId);
                    if (tradeList?.Count > 0) newTrades.AddRange(tradeList);
                }
            }
            // timeout management
            foreach (var cm in _contractManagers.Values)
            {
                var orderIdsToCancel = cm.CancelOutdatedOrders(so.CurrentUtcTime);
                foreach (var id in orderIdsToCancel)
                {
                    if (!_reportsRoutingMap.ContainsKey(id)) continue;
                    var mt = _reportsRoutingMap[id];
                    errorMessages.Add((mt.MarketCode,mt.Exchange, $"order with client id {id} was cancelled by timeout"));
                    _reportsRoutingMap.Remove(id);
                }
            }
            return newTrades;
        }
        private LSS ProcessContractInfos(StateObject so, out List<(string, int, double)>  nbml)
        {
            List<(string, int, double)> bm = new ();
            LSSS subscriptionList = 
                (from ci in so.ContractInfoList 
                    let key = ci.MarketName + ci.Exchange 
                    where _contractManagers.ContainsKey(key) 
                    select _contractManagers[key].ProcessContractInfo(ci, so.CurrentUtcTime, bm) 
                    into res
                    select res).ToList();
            subscriptionList.AddRange(_contractManagers.Values.Select(cm => cm.ProcessContractInfo(null, so.CurrentUtcTime, bm)));
            foreach (var (_,_,txt) in subscriptionList.Where(t => t.txt != string.Empty))
                so.TextMessageList.Add(new Tuple<string, string>("SubscriptionError", txt));
            nbml = bm;
            return subscriptionList.Where(t=>t.market != string.Empty && t.txt == string.Empty)
                .Select(t=> (t.market, t.exchange))
                .ToList();
        }
        private List<Tuple<Bar, string, bool>> MakeNewOneMinuteBars(StateObject so)
        {
            HashSet<string> acc = new();
            List<Tuple<Bar, string, bool>> newBars = new();
            foreach (var b in so.BarUpdateList)
            {
                var ret = _barAggregatorMap[b.SymbolExchange].ProcessBar(b, utcNow: so.CurrentUtcTime);
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
                        RestrictionManager.SetUserRestriction(userCommand.Restriction);
                    else if (command is ErrorsForgetCommand)
                        ErrorTracker.ResetState();
                    break;
                case CommandSource.Scheduler:
                    if (command is RestrictionCommand schedulerCommand)
                        RestrictionManager.SetSchedulerRestriction(schedulerCommand.Restriction);
                    break;
            }
        }
        private void ApplyCommands(LC cmdList)
        {
            foreach (var cmd in cmdList.Where(cmd => CommandReceivers.ContainsKey(cmd.DestinationId)))
                CommandReceivers[cmd.DestinationId].ApplyCommand(cmd);
        }
        private void UpdateParentRestrictions()
        {
            _currentRestriction = RestrictionManager.GetCurrentRestriction();
            foreach (var cm in Exchanges.Values) cm.UpdateParentRestrictions(_currentRestriction);
        }

        #endregion
    }
}
