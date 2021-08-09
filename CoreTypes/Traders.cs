using System;
using System.Collections.Generic;
using Messages;

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
            //_marketMap.Add(mt.InternalID,mt);
            Markets.Add(mt.MarketCode,mt);
            Position.RegisterMarketPosition(mt);
        }

        public void RegisterStrategyTrader(StrategyTrader st)
        {
            //_strategyMap.Add(st.InternalID,st);
        }
        #endregion // structure

        private TradingRestriction _currentRestriction;
        private readonly ExchangeRestrictionsManager _restrictionsManager = new();

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
            Position = new ExchangePosition();
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
        public Dictionary<int, List<StrategyOrderInfo>> PostedOrderMap { get; } = new();
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
        public (MarketTrader, MarketOrderDescription order, List<Trade>) GenerateOrders(DateTime utcNow)
        {
            return OrderGenerator.GenerateOrders(this, utcNow);
        }
        public (List<Trade> tlist, bool isOrderFinished) ApplyOrderReport(DateTime utcTime, OrderReportBase report)
        {
            return OrderReportsProcessor.ApplyOrderReport(this, utcTime, report);
        }
        public static List<Trade> SendPartialVirtualFill(StrategyTrader s, decimal quote,
            DateTime utcNow, int virtuallyFilled, string clOrderId, int clBasketId)
        {
            var r =
                new List<(Execution, int)>
                    {(new Execution(clOrderId, clBasketId, utcNow, quote), virtuallyFilled)};
            var t = s.Position.ProcessNewDeals(r);
            s.CurrentOperationAmount -= virtuallyFilled;
            return t;
        }
        public static List<Trade> SendVirtualFill(StrategyTrader s, decimal quote, DateTime utcNow,
            string clOrderID, int clBasketID)
        {
            var r =
                new List<(Execution, int)>
                    {(new Execution(clOrderID, clBasketID, utcNow, quote), s.CurrentOperationAmount)};
            var t = s.Position.ProcessNewDeals(r);
            s.CurrentOperationAmount = 0;
            return t;
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

        private Signal _lastSignal;
        private readonly StrategyRestrictionsManager _restrictionsManager = new();
        private TradingRestriction _currentRestriction;

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
                return;
            }

            if (_currentRestriction == TradingRestriction.SoftStop)
            {
                if (_lastSignal == Signal.TO_LONG)
                    CurrentOperationAmount = Position.Size >= 0 ? 0 : -Position.Size;
                else // last signal = Signal.TO_SHORT
                    CurrentOperationAmount = Position.Size <= 0 ? 0 : -Position.Size;

                return;
            }

            CurrentOperationAmount = ((int)_lastSignal) * ContractsNbr - Position.Size;
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
                return new StrategyOrderInfo(InternalID, 0);
            SetOrderSize(lastQuotations.last);
            _lastSignal = Signal.NO_SIGNAL;
            return new StrategyOrderInfo(InternalID, CurrentOperationAmount);
        }
    }
}
