using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes
{
    public class ExchangeTrader : ICommandReceiver
    {
        public int InternalID { get; }
        public string Exchange { get; }
        public string Currency { get; }

        #region structure
        public Dictionary<string, MarketTrader> Markets { get; } = new();

        public void RegisterMarketTrader(MarketTrader mt)
        {
            Markets.Add(mt.MarketCode,mt);
            Position.RegisterMarketPosition(mt);
        }

        #endregion // structure

        private TradingRestriction _currentRestriction;
        private readonly ExchangeRestrictionsManager _restrictionsManager = new();
        public ExchangeRestrictionsManager RestrictionsManager => _restrictionsManager;

        public void ApplyCommand(ICommand command)
        {
            var s = command.Source;
            if (s == CommandSource.User && command is RestrictionCommand restrictionCommand)
                _restrictionsManager.SetUserRestriction(restrictionCommand.Restriction);
        }

        public void UpdateParentRestrictions(TradingRestriction parentRestriction)
        {
            _restrictionsManager.SetParentRestriction(parentRestriction);
            _currentRestriction = _restrictionsManager.GetCurrentRestriction();
            foreach (var mt in Markets.Values) mt.UpdateParentRestrictions(_currentRestriction);
        }

        public ExchangePosition Position { get; }

        public ExchangeTrader(int internalId, string exchange, string currency)
        {
            InternalID = internalId;
            Exchange = exchange;
            Currency = currency;
            Position = new ExchangePosition(this);
            _currentRestriction = _restrictionsManager.GetCurrentRestriction();
        }
    }

    public class MarketTrader : ICommandReceiver
    {
        public int InternalID { get; }
        public string MarketCode { get; }
        public string ContractCode { get; set; } = string.Empty;
        public string Exchange { get; private set; }
        public readonly ContractDetailsManager ContractManager;

        #region structure
        public List<StrategyTrader> Strategies { get; } = new();
        public Dictionary<int, StrategyTrader> StrategyMap { get; } = new();
        public Dictionary<int, (List<StrategyOrderInfo>,DateTime)> PostedOrderMap { get; } = new();
        public void RegisterStrategyTrader(StrategyTrader st)
        {
            StrategyMap.Add(st.InternalID, st);
            Strategies.Add(st);
            Position.RegisterStrategyPosition(st);
        }
        #endregion // structure

        public MarketRestrictionsManager RestrictionsManager { get; } = new();
        private TradingRestriction _currentRestriction;
        public void ApplyCommand(ICommand command)
        {
            var s = command.Source;
            if (s == CommandSource.User && command is RestrictionCommand restrictionCommand)
                RestrictionsManager.SetUserRestriction(restrictionCommand.Restriction);
        }
        public void UpdateParentRestrictions(TradingRestriction parentRestriction)
        {
            RestrictionsManager.SetParentRestriction(parentRestriction);
            RestrictionsManager.SetCriticalLossRestriction(
                Position.LossManager.StoppedByCriticalLoss
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);
            _currentRestriction = RestrictionsManager.GetCurrentRestriction();
            foreach (var st in Strategies) st.UpdateParentRestrictions(_currentRestriction);
        }
        public MarketPosition Position { get; }
        // arguments must be correct
        public MarketTrader(int internalId, string marketCode, string exchange, decimal criticalLoss)
        {
            InternalID = internalId;
            Position = new MarketPosition(criticalLoss);
            MarketCode = marketCode;
            Exchange = exchange;
            _currentRestriction = RestrictionsManager.GetCurrentRestriction();
            ContractManager = new(this);
        }
        public (MarketTrader, MarketOrderDescription order, List<string>) GenerateOrders(DateTime utcNow)
        {
            return OrderGenerator.GenerateOrders(this, utcNow);
        }
        public (List<string> tlist, bool isOrderFinished) ApplyOrderReport(DateTime utcTime, OrderStateMessage report, out string errorMessage)
        {
            return OrderReportsProcessor.ApplyOrderReport(this, utcTime, report, out errorMessage);
        }
        public static List<string> ApplyPartialVirtualFill(StrategyTrader s, decimal quote,
            DateTime utcNow, int virtuallyFilled, string clOrderId, int clBasketId)
        {
            var r =
                new List<(Execution, int)>
                    {(new Execution(clOrderId, clBasketId, utcNow, quote), virtuallyFilled)};
            var t = s.Position.ProcessNewDeals(r);
            s.CurrentOperationAmount -= virtuallyFilled;
            return t;
        }
        public static List<string> ApplyVirtualFill(StrategyTrader s, decimal quote, DateTime utcNow,
            string clOrderID, int clBasketID)
        {
            var r =
                new List<(Execution, int)>
                    {(new Execution(clOrderID, clBasketID, utcNow, quote), s.CurrentOperationAmount)};
            var t = s.Position.ProcessNewDeals(r);
            s.CurrentOperationAmount = 0;
            return t;
        }

        public static List<string> ApplyRealFill(StrategyTrader s, OrderExecutionMessage report) => 
            ApplyRealFill(s, report, report.SgnQty);

        public static List<string> ApplyRealFill(StrategyTrader s, OrderExecutionMessage report, int amount)
        {
            var opAmountBefore = s.CurrentOperationAmount;
            var r =
                new List<(Execution, int)>
                    {(new Execution(report.ExecId, report.OrderId, report.TransactTime, report.Price), amount)};
            var t = s.Position.ProcessNewDeals(r);
            if (opAmountBefore == 0) // no operation expected
            {
                if (amount != 0) // but something is executed
                {
                    s.CurrentOperationAmount = 0;
                    s.OffsetDealAmount = -amount;
                }
            }
            else if (amount != 0 && Math.Sign(opAmountBefore) != Math.Sign(amount)) 
            {
                // buy/sell mismatch
                s.CurrentOperationAmount = 0;
                s.OffsetDealAmount = -amount;
            }
            else
            {
                s.CurrentOperationAmount -= amount;
                if (s.CurrentOperationAmount != 0 && opAmountBefore * s.CurrentOperationAmount < 0)
                {
                    // operation is overfilled
                    s.OffsetDealAmount = -s.CurrentOperationAmount;
                    s.CurrentOperationAmount = 0;
                }
            }
            return t;
        }

        public List<string> ApplyCurrentTime(DateTime currentUtcTime, out List<int> idList)
        {
            idList = new();
            List<string> messages = new();
            List<int> keysToRemove = new();
            foreach (var kvp in PostedOrderMap)
            {
                if ((currentUtcTime - kvp.Value.Item2).TotalMinutes >= 3)
                {
                    messages.Add($"order for market {MarketCode} at {Exchange} was cancelled by timeout");
                    foreach (var s in kvp.Value.Item1.Select(b => StrategyMap[b.StrategyId]))
                        s.CurrentOperationAmount = 0;
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                PostedOrderMap.Remove(key);
                idList.Add(key);
            }
            return messages;
        }
    }

    public class StrategyTrader : ICommandReceiver
    {
        public int InternalID { get; }
        public int Id { get; private set; }
        public SignalService SignalService { get; private set; }
        public StrategyPosition Position { get; private set; }
        public int CurrentOperationAmount { get; set; } = 0;
        public int ContractsNbr { get; set; } = 1;

        public int OffsetDealAmount = 0; // mandatory execution!!

        private Signal _lastSignal;
        private readonly StrategyRestrictionsManager _restrictionsManager = new();
        private TradingRestriction _currentRestriction;
        public StrategyRestrictionsManager RestrictionsManager => _restrictionsManager;

        public StrategyTrader(int internalId, int id, StrategyPosition position, SignalService signalService)
        {
            InternalID = internalId;
            Id = id;
            Position = position;
            SignalService = signalService;
            _currentRestriction = _restrictionsManager.GetCurrentRestriction();
        }

        public void ApplyCommand(ICommand command)
        {
            var s = command.Source;
            switch (command)
            {
                case RestrictionCommand restrictionCommand:
                {
                    var r = restrictionCommand.Restriction;
                    switch (s)
                    {
                        case CommandSource.User:
                            _restrictionsManager.SetUserRestriction(r);
                            break;
                        case CommandSource.Scheduler:
                            _restrictionsManager.SetSchedulerRestriction(r);
                            break;
                    }
                }
                    break;
                case OrderForgetCommand forgetCommand:
                    break;
                case OrderRepeatCommand repeatCommand:
                    break;
                case ManualFillCommand manualCommand:
                    break;
            }
        }

        public void UpdateParentRestrictions(TradingRestriction parentRestriction)
        {
            _restrictionsManager.SetParentRestriction(parentRestriction);
            _restrictionsManager.SetCriticalLossRestriction(
                Position.LossManager.StoppedByCriticalLoss
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);
            _currentRestriction = _restrictionsManager.GetCurrentRestriction();
        }

        // call when CurrentOperationAmount is zero
        private void SetOrderSize(decimal currentQuotation)
        {
            if (_lastSignal == Signal.TO_FLAT || _currentRestriction == TradingRestriction.HardStop)
            {
                CurrentOperationAmount = -Position.Size;
                OffsetDealAmount = 0;
                return;
            }

            if (_currentRestriction == TradingRestriction.SoftStop)
            {
                if (_lastSignal == Signal.TO_LONG)
                    CurrentOperationAmount = Position.Size >= 0 ? OffsetDealAmount : -Position.Size;
                else // last signal = Signal.TO_SHORT
                    CurrentOperationAmount = Position.Size <= 0 ? OffsetDealAmount : -Position.Size;
                OffsetDealAmount = 0;
                return;
            }

            CurrentOperationAmount = ((int)_lastSignal) * ContractsNbr - Position.Size + OffsetDealAmount;
            OffsetDealAmount = 0;
        }

        public StrategyOrderInfo GenerateOrder((decimal bid, decimal ask, decimal last) lastQuotations)
        {
            var newSignal = SignalService.GetSignal(InternalID);
            if (CurrentOperationAmount != 0)
            {
                // we can not send order (real position is unknown),
                // but we must save new signal (if any) for later
                if (newSignal != Signal.NO_SIGNAL) _lastSignal = newSignal;
                return new StrategyOrderInfo(InternalID, 0);
            }

            if (_lastSignal == Signal.NO_SIGNAL) // no signal at the moment
            {
                var amount = OffsetDealAmount != 0 ? OffsetDealAmount : 0;
                OffsetDealAmount = 0;
                return new StrategyOrderInfo(InternalID, amount);
            }
            SetOrderSize(lastQuotations.last);
            _lastSignal = Signal.NO_SIGNAL;
            return new StrategyOrderInfo(InternalID, CurrentOperationAmount);
        }
    }
}
