using System;
using System.Collections.Generic;
using Binosoft.TraderLib.Indicators;
using SignalGenerators;

namespace CoreTypes.SignalServiceClasses
{
    class StrategyInfoHolder
    {
        private readonly IByMarketStrategy _strategy;
        private readonly List<Indicator> _indicators;
        private readonly double[] _inputsBuf;
        private readonly bool _ignoreTradingZones;

        private Signal _decision = Signal.NO_SIGNAL;
        private string CalculationError;

        private readonly StrategyDynamicGuards _dynamicGuards;
        public StrategyInfoHolder(IByMarketStrategy strategy, List<Indicator> strategyIndicators,bool ignoreTradingZones, StrategyDynamicGuards dynamicGuards)
        {
            _strategy = strategy;
            _indicators = strategyIndicators;
            _inputsBuf = new double[_indicators.Count];
            _ignoreTradingZones = ignoreTradingZones;

            _dynamicGuards = dynamicGuards;
        }
        public Signal GetResetLastDecision()
        {
            var ret = _decision;
            _decision = Signal.NO_SIGNAL;
            return ret;
        }

        public void UpdateDecision(DateTime currentTime)
        {
            if (CalculationError!=null)
            {
                _decision = Signal.TO_FLAT; 
                return;
            }

            int i = -1;
            bool allIndicatorsHasNewValues = true;
            foreach (var indicator in _indicators)
            {
                ++i;
                var exceptionInfo = indicator.CalculationExceptionInfo; 
                if (exceptionInfo != null)
                {
                    CalculationError = exceptionInfo.ToString(); // !!+ todo to output msg about occurred problem
                    _decision = Signal.TO_FLAT;
                    return;
                }

                if (indicator.Count == 0 || indicator.CalculationTime(0) != currentTime)
                    allIndicatorsHasNewValues = false;
                else
                    _inputsBuf[i] = indicator[0];
            }
            if (!allIndicatorsHasNewValues) return;

            switch (Math.Sign(_strategy.GenerateSignal(_inputsBuf, _ignoreTradingZones)))
            {
                case 1:
                    _decision = Signal.TO_LONG;
                    break;
                case -1:
                    _decision = Signal.TO_SHORT;
                    break;
                case 0:
                    _decision = Signal.TO_FLAT;
                    break;
            }
            _dynamicGuards?.UpdateValues(_inputsBuf);
        }

        public (bool, bool) GetMustClosePositionByDynamicGuard(int position, double weightedOpenPrice)
        {
            return _dynamicGuards?.GetMustClosePosition(position, weightedOpenPrice)
                   ?? (false, false);
        }
    }
}