using System;
using System.Collections.Generic;
using System.Linq;
using TimeZoneConverter;

namespace CoreTypes
{
    public class ContractDetailsManager
    {
        private ContractInfo _currentContract;
        private DateTime _lastReqDateTime;
        private readonly MarketTrader _owner;

        public ContractDetailsManager(MarketTrader owner)
        {
            _lastReqDateTime = DateTime.UtcNow.AddHours(-24);
            _owner = owner;
        }

        public bool IsReadyToBeStopped
        {
            get
            {
                if (_owner.StrategyMap.Values
                    .All(st => st.CurrentOperationAmount == 0 && st.OffsetDealAmount == 0 && st.Position.Size == 0))
                    return _owner.PostedOrderMap.Count == 0;
                return false;
            }
        }

        private bool IsOutOfSession()
        {
            if (_currentContract == null) return true;
            return _owner.RestrictionsManager.GetRestriction(CommandSource.EndOfSession) == TradingRestriction.HardStop;
        }

        private bool IsOutOfMarket()
        {
            if (_currentContract == null) return true;
            return _owner.RestrictionsManager.GetRestriction(CommandSource.OutOfMarket) == TradingRestriction.HardStop;
        }

        public void SetNewContractInfo(ContractInfo ci, InfoCollector ic, StateObject so)
        {
            if (ci != null)
            {
                _currentContract = ci;
                var (b, mm) = _owner.UpdateMinMoveAndBPV(_currentContract.Multiplier, _currentContract.MinTick);
                if (b > 0 || mm > 0) ic.Accept((_owner.MarketCode + _owner.Exchange, b, mm));
                _owner.ContractCode = ci.LocalSymbol;
                if (!AdjustDateTimes())
                {
                    _currentContract = null;
                    so.TextMessageList.Add(new Tuple<string, string>("SubscriptionError", 
                        $"Unknown time zone id detected for {_owner.Exchange}/{_owner.MarketCode}"));
                }
            }
        }
        public void ProcessContractInfo(DateTime utcNow, InfoCollector ic)
        {
            if (_currentContract == null)
            {
                _owner.RestrictionsManager.SetEndOfContractRestriction(TradingRestriction.HardStop);
                _owner.RestrictionsManager.SetEndOfSessionRestriction(TradingRestriction.HardStop);
                _owner.RestrictionsManager.SetOutOfMarketRestriction(TradingRestriction.HardStop);
                // request contract details for first time and every 5 minutes till it gets them
                if ((utcNow - _lastReqDateTime).TotalMinutes > 4)
                {
                    _lastReqDateTime = utcNow;
                    var mc = _owner.MarketCode;
                    var ex = _owner.Exchange;
                    ic.Accept(mc,ex);
                    ic.Accept(mc+ex,1); // set restriction
                }
            }
            else if (utcNow.Second == 0 && utcNow.Minute % 10 == 0) // every ten minutes
            {
                _owner.RestrictionsManager.SetEndOfContractRestriction(_currentContract.LastTradeTime <= utcNow
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);

                var sessionCondition = utcNow < _currentContract.StartLiquidHours
                                || utcNow > _currentContract.EndLiquidHours;

                var marketCondition = utcNow < _currentContract.OpenMarket
                                       || utcNow > _currentContract.CloseMarket;

                var outOfSessionBefore = IsOutOfSession();
                _owner.RestrictionsManager.SetEndOfSessionRestriction(sessionCondition
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);
                var outOfSessionAfter = IsOutOfSession();

                var outOfMarketBefore = IsOutOfMarket();
                _owner.RestrictionsManager.SetOutOfMarketRestriction(marketCondition
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);
                var outOfMarketAfter = IsOutOfMarket();
                switch (outOfMarketBefore)
                {
                    case true when !outOfMarketAfter:
                        // market is just opened
                        ic.Accept(_owner.MarketCode+_owner.Exchange,-1); //reset restriction
                        break;
                    case false when outOfMarketAfter:
                        // market is just closed
                        ic.Accept(_owner.MarketCode + _owner.Exchange, 1); // set restriction
                        break;
                }

                switch (outOfSessionBefore)
                {
                    case false when outOfSessionAfter:
                        {
                            ic.Accept(_owner.MarketCode, _owner.Exchange);
                            foreach (var s in _owner.StrategyMap.Values)
                                s.Position.ClearStopLossRestrictions();
                            break;
                        }
                    case true when !outOfSessionAfter:
                        {
                            _owner.Position.StartNewSession();
                            foreach (var s in _owner.StrategyMap.Values)
                                s.Position.StartNewSession();
                            break;
                        }
                }
            }
        }

        private bool AdjustDateTimes()
        {
            var tzi = TZConvert.GetTimeZoneInfo(_currentContract.TimeZoneId);
            if (tzi == null) return false;
            var dt = DateTime.SpecifyKind(_currentContract.StartLiquidHours, DateTimeKind.Unspecified);
            _currentContract.StartLiquidHours = TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
            dt = DateTime.SpecifyKind(_currentContract.EndLiquidHours, DateTimeKind.Unspecified);
            _currentContract.EndLiquidHours = TimeZoneInfo.ConvertTimeToUtc(dt, tzi);

            dt = DateTime.SpecifyKind(_currentContract.OpenMarket, DateTimeKind.Unspecified);
            _currentContract.OpenMarket = TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
            dt = DateTime.SpecifyKind(_currentContract.CloseMarket, DateTimeKind.Unspecified);
            _currentContract.CloseMarket = TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
            _currentContract.CloseMarket = _currentContract.CloseMarket.AddMinutes(-10);

            dt = _currentContract.LastTradeTime.AddDays(-1);
            if (dt.DayOfWeek == DayOfWeek.Sunday) dt = dt.AddDays(-2);
            else if (dt.DayOfWeek == DayOfWeek.Saturday) dt = dt.AddDays(-1);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            dt = TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
            _currentContract.LastTradeTime =
                new DateTime(dt.Year, dt.Month, dt.Day,
                    _currentContract.EndLiquidHours.Hour, _currentContract.EndLiquidHours.Minute, 0);
            return true;
        }

        public List<int> CancelOutdatedOrders(DateTime currentUtcTime)
        {
            return _owner.CancelOutdatedOrders(currentUtcTime);
        }
    }
}