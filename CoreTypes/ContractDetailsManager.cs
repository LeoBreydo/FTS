using System;
using System.Collections.Generic;
using TimeZoneConverter;

namespace CoreTypes
{
    public class ContractDetailsManager
    {
        private ContractInfo _currentContract;
        private DateTime _lastReqDateTime;
        private DateTime _previousSessionStart;
        private bool _processRightNow = true;
        private readonly MarketTrader _owner;

        public ContractDetailsManager(MarketTrader owner)
        {
            _lastReqDateTime = DateTime.UtcNow.AddHours(-24);
            _previousSessionStart = _lastReqDateTime;
            _owner = owner;
        }

        public (string markeCode, string exchange, string error) ProcessContractInfo(ContractInfo ci, DateTime utcNow)
        {
            string err = string.Empty;
            var ret = (
                marketCode: string.Empty,
                exchange: string.Empty,
                error: string.Empty
            );
            if (ci != null)
            {
                _processRightNow = false;
                _currentContract = ci;
                _owner.ContractCode = ci.LocalSymbol;
                if (!AdjustDateTimes())
                {
                    _currentContract = null;
                    err = $"Unknown time zone id detected for {_owner.Exchange}";
                }
            }
            else
            {
                if (!_processRightNow)
                {
                    _processRightNow = true;
                    return ret;
                }
            }
            if (_currentContract == null)
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
            else if (utcNow.Second == 0 && utcNow.Minute % 10 == 0) // every ten minutes
            {
                _owner.RestrictionsManager.SetEndOfContractRestriction(_currentContract.LastTradeTime <= utcNow
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);

                var condition = utcNow < _currentContract.StartLiquidHours
                                || utcNow > _currentContract.EndLiquidHours;

                _owner.RestrictionsManager.SetEndOfSessionRestriction(condition
                    ? TradingRestriction.HardStop
                    : TradingRestriction.NoRestrictions);
                if (condition && (utcNow - _lastReqDateTime).TotalHours > 23)
                {
                    _lastReqDateTime = utcNow;
                    ret = (
                        marketCode: _owner.MarketCode,
                        exchange: _owner.Exchange,
                        error: string.Empty
                    );
                }

                if (!condition && (utcNow - _previousSessionStart).TotalHours > 23)
                {
                    _previousSessionStart = utcNow;
                    foreach (var s in _owner.Strategies) s.Position.StartNewSession();
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

        public List<string> ApplyCurrentTime(DateTime currentUtcTime, out List<int> idList)
        {
            return _owner.ApplyCurrentTime(currentUtcTime, out idList);
        }
    }
}