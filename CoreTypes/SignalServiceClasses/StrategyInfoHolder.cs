using System;
using System.Collections.Generic;
using Binosoft.TraderLib.Indicators;
using SignalGenerators;

namespace CoreTypes.SignalServiceClasses
{
    class StrategyInfoHolder
    {
        private readonly int _id;
        private readonly IByMarketStrategy _strategy;
        private readonly List<Indicator> _indicators;
        private readonly Indicator _irClose;
        private readonly double[] _inputsBuf;
        private readonly bool _ignoreTradingZones;

        private Signal _decision;
        private string CalculationError;
        private DateTime _lastProceededEndOfBar;

        private readonly StrategyDynamicGuards _dynamicGuards;
        public StrategyInfoHolder(int id,IByMarketStrategy strategy, List<Indicator> strategyIndicators, int ixCloseIndicator,bool ignoreTradingZones, StrategyDynamicGuards dynamicGuards)
        {
            _id = id;
            _strategy = strategy;
            _indicators = strategyIndicators;
            _irClose = strategyIndicators[ixCloseIndicator];
            _inputsBuf = new double[_indicators.Count];
            _ignoreTradingZones = ignoreTradingZones;

            _dynamicGuards = dynamicGuards;

            _decision = Signal.NO_SIGNAL;
            _lastProceededEndOfBar = DateTime.MinValue;
        }
        public Signal GetResetLastDecision()
        {
            var ret = _decision;
            _decision = Signal.NO_SIGNAL;
            return ret;
        }

        public void UpdateDecision()
        {
            if (CalculationError!=null)
            {
                _decision = Signal.TO_FLAT; 
                return;
            }

            if (_irClose.Count == 0) return;
            DateTime lastBarTime = _irClose.CalculationTime(0);
            if (lastBarTime == _lastProceededEndOfBar) return;

            _lastProceededEndOfBar = lastBarTime;
            DebugLog.AddMsg(string.Format("Strategy {0} proceeding endOfBar {1}", _id,
                lastBarTime.ToString("yyyyMMdd-HHmmss.fff")));

            int ix = -1;
            bool allIndicatorsHasNewValues = true;
            foreach (var indicator in _indicators)
            {
                ++ix;
                var exceptionInfo = indicator.CalculationExceptionInfo; 
                if (exceptionInfo != null)
                {
                    CalculationError = exceptionInfo.ToString(); // !!+ todo to output msg about occurred problem
                    DebugLog.AddMsg(string.Format("Strategy calculation error {0}, {1}", _id, CalculationError));
                    _decision = Signal.TO_FLAT;
                    return;
                }

                if (indicator.Count == 0 || indicator.CalculationTime(0) != lastBarTime)
                {
                    allIndicatorsHasNewValues = false;
                    _inputsBuf[ix] = double.NaN;
                    if (indicator.Count > 0)
                    {
                        DebugLog.AddMsg(string.Format(
                            "Strategy Id={0}, Indicator[{1}].Time!=EndOfBar ({2}!={3})",
                            _id,
                            ix,
                            indicator.CalculationTime(0).ToString("yyyyMMdd-HHmmss.fff"),
                            lastBarTime.ToString("yyyyMMdd-HHmmss.fff")));
                    }
                }
                else
                {
                    _inputsBuf[ix] = indicator[0];

                }
            }

            if (!allIndicatorsHasNewValues)
            {
                DebugLog.AddMsg(string.Format(
                    "Strategy {0} EndOfBar, but not all input indicators has new values : ({1})", _id,
                    string.Join(",", _inputsBuf)));
                return;
            }

            _decision = Math.Sign(_strategy.GenerateSignal(_inputsBuf, _ignoreTradingZones)) switch
            {
                1 => Signal.TO_LONG,
                -1 => Signal.TO_SHORT,
                _ => Signal.TO_FLAT
            };

            DebugLog.AddMsg(string.Format("Strategy {0} new decision formed {1}", _id, _decision));
            _dynamicGuards?.UpdateValues(_inputsBuf);
        }

        public (bool, bool) GetMustClosePositionByDynamicGuard(int position, double weightedOpenPrice)
        {
            return _dynamicGuards?.GetMustClosePosition(position, weightedOpenPrice)
                   ?? (false, false);
        }
    }
}