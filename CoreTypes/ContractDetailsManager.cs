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
        private MarketTrader _owner;

        public ContractDetailsManager(MarketTrader owner)
        {
            _lastReqDateTime = DateTime.UtcNow.AddHours(-24);
            _previousSessionStart = _lastReqDateTime;
            _owner = owner;
        }

        public (string markeCode, string exchange) ProcessContractInfo(ContractInfo ci, DateTime utcNow)
        {
            var ret = (
                marketCode: string.Empty,
                exchange: string.Empty
            );
            if (ci != null)
            {
                _processRightNow = false;
                _currentContract = ci;
                _owner.ContractCode = ci.LocalSymbol;
                AdjustDateTimes();
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
                        exchange: _owner.Exchange
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
                        exchange: _owner.Exchange
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

        private void AdjustDateTimes()
        {
            var tzi = TZConvert.GetTimeZoneInfo(_currentContract.TimeZoneId);
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
        }

        public List<string> ApplyCurrentTime(DateTime currentUtcTime, out List<int> idList)
        {
            return _owner.ApplyCurrentTime(currentUtcTime, out idList);
        }
    }
}