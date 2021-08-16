using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes
{
    public class ExchangeTrader : ICommandReceiver
    {
        public int Id { get; }
        public string Exchange { get; }
        public string Currency { get; }

        #region structure
        public SortedList<string, MarketTrader> Markets { get; } = new();

        public void RegisterMarketTrader(MarketTrader mt)
        {
            Markets.Add(mt.MarketCode,mt);
            Position.RegisterMarketPosition(mt);
        }

        #endregion // structure

        private TradingRestriction _currentRestriction;
        private readonly ExchangeRestrictionsManager _restrictionsManager = new();
        public ExchangeRestrictionsManager RestrictionsManager => _restrictionsManager;
        public IValueTracker<int, WorkingState> ErrorTracker { get; }

        public void ApplyCommand(ICommand command)
        {
            var s = command.Source;
            if (s == CommandSource.User)
            {
                switch (command)
                {
                    case RestrictionCommand restrictionCommand:
                        _restrictionsManager.SetUserRestriction(restrictionCommand.Restriction);
                        break;
                    case ErrorsForgetCommand:
                        ErrorTracker.ResetState();
                        break;
                }
            }
        }

        public void UpdateParentRestrictions(TradingRestriction parentRestriction)
        {
            _restrictionsManager.SetParentRestriction(parentRestriction);
            _currentRestriction = _restrictionsManager.GetCurrentRestriction();
            foreach (var mt in Markets.Values) mt.UpdateParentRestrictions(_currentRestriction);
        }

        public ExchangePosition Position { get; }

        public ExchangeTrader(int id, string exchange, string currency, int maxErrorsPerDay)
        {
            Id = id;
            Exchange = exchange;
            Currency = currency;
            Position = new ExchangePosition(this);
            _currentRestriction = _restrictionsManager.GetCurrentRestriction();
            ErrorTracker = new ErrorTracker(maxErrorsPerDay);
        }
    }

    public class MarketTrader : ICommandReceiver
    {
        public int Id { get; }
        public string MarketCode { get; }
        public string ContractCode { get; set; } = string.Empty;
        public string Exchange { get; }
        public ContractDetailsManager ContractManager { get; }
        public IValueTracker<int, WorkingState> ErrorTracker { get; }

        public int BigPointValue { get; set; }
        public double MinMove { get; set; }

        #region structure
        public Dictionary<int, StrategyTrader> StrategyMap { get; } = new();
        public Dictionary<int, (List<StrategyOrderInfo>,DateTime)> PostedOrderMap { get; } = new();
        public void RegisterStrategyTrader(StrategyTrader st)
        {
            StrategyMap.Add(st.Id, st);
            Position.RegisterStrategyPosition(st);
            st.Position.SetBigPointValue(BigPointValue);
        }
        #endregion // structure

        public MarketRestrictionsManager RestrictionsManager { get; } = new();
        private TradingRestriction _currentRestriction;
        public void ApplyCommand(ICommand command)
        {
            var s = command.Source;
            if (s == CommandSource.User)
            {
                switch (command)
                {
                    case RestrictionCommand restrictionCommand:
                        RestrictionsManager.SetUserRestriction(restrictionCommand.Restriction);
                        break;
                    case ErrorsForgetCommand:
                        ErrorTracker.ResetState();
                        break;
                }
            }
        }
        public void UpdateParentRestrictions(TradingRestriction parentRestriction)
        {
            RestrictionsManager.SetParentRestriction(parentRestriction);
            _currentRestriction = RestrictionsManager.GetCurrentRestriction();
            foreach (var st in StrategyMap.Values) st.UpdateParentRestrictions(_currentRestriction);
        }
        public MarketPosition Position { get; }
        // arguments must be correct
        public MarketTrader(int id, string marketCode, string exchange, int maxErrorsPerDay, 
            decimal criticalLoss, int bpv, double minMove)
        {
            Id = id;
            Position = new MarketPosition(this, criticalLoss);
            MarketCode = marketCode;
            Exchange = exchange;
            _currentRestriction = RestrictionsManager.GetCurrentRestriction();
            ContractManager = new(this);
            ErrorTracker = new ErrorTracker(maxErrorsPerDay);
            BigPointValue = bpv;
            MinMove = minMove;
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

        public List<int> CancelOutdatedOrders(DateTime currentUtcTime)
        {
            List<int> orderIdsToCancel = new();
            foreach (var (key, value) in PostedOrderMap
                .Where(kvp => (currentUtcTime - kvp.Value.Item2).TotalMinutes >= 2))
            {
                foreach (var s in value.Item1.Select(b => StrategyMap[b.StrategyId]))
                    s.CurrentOperationAmount = 0;
                orderIdsToCancel.Add(key);
            }
            ErrorTracker.ChangeValueBy(orderIdsToCancel.Count);
            foreach (var key in orderIdsToCancel) PostedOrderMap.Remove(key);
            return orderIdsToCancel;
        }

        public (int,double) UpdateMinMoveAndBPV(int bpv, double minMove)
        {
            int b = -1;
            double mm = -1;
            if (bpv != BigPointValue)
            {
                BigPointValue = bpv;
                foreach (var st in StrategyMap.Values)
                    st.Position.SetBigPointValue(bpv);
                b = bpv;
            }

            if (Math.Abs(minMove - MinMove) > 1e-8)
            {
                MinMove = minMove;
                mm = minMove;
            }

            return (b, mm);
        }
    }

    public class StrategyTrader : ICommandReceiver
    { 
        public int Id { get; }
        public SignalService SignalService { get; }
        public StrategyPosition Position { get; }
        public int CurrentOperationAmount { get; set; }
        public int ContractsNbr { get; }

        public int OffsetDealAmount; // mandatory execution!!

        private Signal _lastSignal;
        private readonly StrategyRestrictionsManager _restrictionsManager = new();
        private TradingRestriction _currentRestriction;
        public StrategyRestrictionsManager RestrictionsManager => _restrictionsManager;

        public PositionValidator PositionValidator;

        public StrategyTrader(int id, StrategyPosition position, int contractsNumber, 
            SignalService signalService, PositionValidator positionValidator)
            
        {
            Id = id;
            Position = position;
            contractsNumber = Math.Abs(contractsNumber);
            if (contractsNumber < 1) contractsNumber = 1;
            ContractsNbr = contractsNumber;
            SignalService = signalService;
            _currentRestriction = _restrictionsManager.GetCurrentRestriction();
            PositionValidator = positionValidator;
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
                case OrderForgetCommand:
                    break;
                case OrderRepeatCommand:
                    break;
                case ManualFillCommand:
                    break;
            }
        }

        public void UpdateParentRestrictions(TradingRestriction parentRestriction)
        {
            _restrictionsManager.SetParentRestriction(parentRestriction);
            _currentRestriction = _restrictionsManager.GetCurrentRestriction();
        }

        // call when CurrentOperationAmount is zero
        private void SetOrderSize()
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
            var newSignal = SignalService.GetSignal(Id);
            if (newSignal != Signal.NO_SIGNAL)
            {
                PositionValidator.UpdateAtStartOfBar((double)lastQuotations.last);
                var tp = newSignal switch
                {
                    Signal.TO_FLAT => 0,
                    Signal.TO_LONG => 1,
                    _ => -1
                };
                PositionValidator.UpdateStopLossRestrictionByNewTargetPosition(tp);
            }
            else PositionValidator.UpdateLastPrice((double)lastQuotations.last);

            if (CurrentOperationAmount != 0)
            {
                // we can not send order (real position is unknown),
                // but we must save new signal (if any) for later
                if (newSignal != Signal.NO_SIGNAL) _lastSignal = newSignal;
                return new StrategyOrderInfo(Id, 0);
            }

            if (_lastSignal == Signal.NO_SIGNAL) // no signal at the moment
            {
                var amount = OffsetDealAmount != 0 ? OffsetDealAmount : 0;
                OffsetDealAmount = 0;
                return new StrategyOrderInfo(Id, amount);
            }
            if (!PositionValidator.ValidateCurrentPosition())
            {
                _lastSignal = Signal.NO_SIGNAL;
                CurrentOperationAmount = -Position.Size;
                return new StrategyOrderInfo(Id, CurrentOperationAmount);
            }
            SetOrderSize();
            _lastSignal = Signal.NO_SIGNAL;
            if (CurrentOperationAmount != 0)
            {
                var tp = CurrentOperationAmount - Position.Size;
                if (PositionValidator.ValidateSuggestedPosition(tp))
                    return new StrategyOrderInfo(Id, CurrentOperationAmount);
                CurrentOperationAmount = 0;
                return new StrategyOrderInfo(Id, 0);
            }
            return new StrategyOrderInfo(Id, 0);
        }
    }
}
