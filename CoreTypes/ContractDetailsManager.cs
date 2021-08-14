using System;
using System.Collections.Generic;
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

        private bool IsOutOfSession()
        {
            if (_currentContract == null) return true;
            return _owner.RestrictionsManager.GetRestriction(CommandSource.EndOfSession) == TradingRestriction.HardStop;
        }

        private bool _toProcessNow ;
        public (string markeCode, string exchange, string error) ProcessContractInfo(ContractInfo ci, DateTime utcNow)
        {
            var err = string.Empty;
            var ret = (
                marketCode: string.Empty,
                exchange: string.Empty,
                error: string.Empty
            );
            if (ci != null)
            {
                _toProcessNow = true;
                _currentContract = ci;
                _owner.SetBigPointValue(_currentContract.Multiplier);
                _owner.ContractCode = ci.LocalSymbol;
                if (!AdjustDateTimes())
                {
                    _currentContract = null;
                    err = $"Unknown time zone id detected for {_owner.Exchange}";
                }
            }
            else
            {
                _toProcessNow = false;
            }
            if (_currentContract == null)
            {
                if (_toProcessNow)
                {
                    _owner.RestrictionsManager.SetEndOfContractRestriction(TradingRestriction.HardStop);
                    _owner.RestrictionsManager.SetEndOfSessionRestriction(TradingRestriction.HardStop);
                    // request contract details for first time and every 5 minutes till it gets them
                    if ((utcNow - _lastReqDateTime).TotalMinutes > 4)
                    {
                        _lastReqDateTime = utcNow;
                        ret = (
                            marketCode: _owner.MarketCode,
                            exchange: _owner.Exchange,
                            error: err
                        );
                    }
                }
            }
            else if (utcNow.Second == 0 && utcNow.Minute % 10 == 0) // every ten minutes
            {
                _owner.RestrictionsManager.SetEndOfContractRestriction(_currentContract.LastTradeTime <= utcNow
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);

                var condition = utcNow < _currentContract.StartLiquidHours
                                || utcNow > _currentContract.EndLiquidHours;
                var outOfSessionBefore = IsOutOfSession(); 
                _owner.RestrictionsManager.SetEndOfSessionRestriction(condition
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);
                var outOfSessionAfter = IsOutOfSession();
                switch (outOfSessionBefore)
                {
                    case false when outOfSessionAfter:
                    {
                        var settlementPrice = _owner.Position.PriceProvider.LastPrice;
                        foreach (var s in _owner.StrategyMap.Values)
                            s.Position.ProcessSettlementPrice(settlementPrice);
                        ret = (
                            marketCode: _owner.MarketCode,
                            exchange: _owner.Exchange,
                            error: string.Empty
                        );
                        break;
                    }
                    case true when !outOfSessionAfter:
                    {
                        foreach (var s in _owner.StrategyMap.Values)
                            s.Position.StartNewSession();
                        break;
                    }
                }
            }

            return ret;
        }

        private bool AdjustDateTimes()
        {
            var tzi = TZConvert.GetTimeZoneInfo(_currentContract.TimeZoneId);
            if (tzi == null) return false;
            var dt = DateTime.SpecifyKind(_currentContract.StartLiquidHours, DateTimeKind.Unspecified);
            _currentContract.StartLiquidHours = TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
            dt = DateTime.SpecifyKind(_currentContract.EndLiquidHours, DateTimeKind.Unspecified);
            _currentContract.EndLiquidHours = TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
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