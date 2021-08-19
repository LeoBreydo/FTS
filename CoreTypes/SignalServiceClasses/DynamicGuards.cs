using System;

namespace CoreTypes.SignalServiceClasses
{
    public interface IDynamicGuard
    {
        internal const bool MustClosePositionIfNoData = false;

        void UpdateValues(double[] inputs);
        bool GetMustClosePosition(int position, double weightedOpenPrice);
    }

    public class NullDynmaicGuard: IDynamicGuard
    {
        public void UpdateValues(double[] inputs){}
        public bool GetMustClosePosition(int position, double weightedOpenPrice) => false;
    }

    public class LastPriceHolder
    {
        public double LastPrice;
        public bool IsNotSet => LastPrice <= 0;
    }
    public class DynamicStopGuard : IDynamicGuard
    {
        private readonly bool OrderPriceMode;
        private readonly int _ixLongValue;
        private readonly int _ixShortValue;

        private readonly LastPriceHolder _lastPriceHolder;

        private double _longValue, _shortValue;

        public DynamicStopGuard(DynamicGuardMode dynamicGuardMode, int ixLongValue, int ixShortValue, LastPriceHolder lastPriceHolder)
        {
            if (dynamicGuardMode == DynamicGuardMode.NotUse)
                throw new Exception("NullGuard must be used for DynamicGuardMode.NotUse");
            if (ixLongValue < 0 || ixShortValue < 0) throw new Exception("invalid indicator index");

            OrderPriceMode = dynamicGuardMode == DynamicGuardMode.OrderPrice;
            _lastPriceHolder = lastPriceHolder;

            _ixLongValue = ixLongValue;
            _ixShortValue = ixShortValue;

            _longValue = _shortValue = double.NaN;
        }

        public void UpdateValues(double[] inputs)
        {
            _longValue = inputs[_ixLongValue];
            _shortValue = inputs[_ixShortValue];
        }

        public bool GetMustClosePosition(int position, double weightedOpenPrice)
        {
            if (_lastPriceHolder.IsNotSet) return IDynamicGuard.MustClosePositionIfNoData;
            if (position > 0)
            {
                if (double.IsNaN(_longValue)) return IDynamicGuard.MustClosePositionIfNoData;

                if (OrderPriceMode)
                    return _lastPriceHolder.LastPrice <= _longValue;

                // to close is OpenResult <= -longValue, where  OpenResult == _lastPriceHolder.LastPrice - weightedOpenPrice; transformed to:
                return _lastPriceHolder.LastPrice + _longValue <= weightedOpenPrice;
            }
            else
            {
                if (double.IsNaN(_shortValue)) return IDynamicGuard.MustClosePositionIfNoData;

                if (OrderPriceMode)
                    return _lastPriceHolder.LastPrice >= _shortValue;

                // to close is OpenResult <= -_shortValue, where  OpenResult == weightedOpenPrice - _lastPriceHolder.LastPrice; transformed to:
                return weightedOpenPrice + _shortValue <= _lastPriceHolder.LastPrice;
            }
        }

    }
    public class DynamicTargetGuard : IDynamicGuard
    {
        private readonly bool OrderPriceMode;
        private readonly int _ixLongValue;
        private readonly int _ixShortValue;

        private readonly LastPriceHolder _lastPriceHolder;

        private double _longValue, _shortValue;

        public DynamicTargetGuard(DynamicGuardMode dynamicGuardMode, int ixLongValue, int ixShortValue, LastPriceHolder lastPriceHolder)
        {
            if (dynamicGuardMode == DynamicGuardMode.NotUse)
                throw new Exception("NullGuard must be used for DynamicGuardMode.NotUse");
            if (ixLongValue < 0 || ixShortValue < 0) throw new Exception("invalid indicator index");

            OrderPriceMode = dynamicGuardMode == DynamicGuardMode.OrderPrice;
            _lastPriceHolder = lastPriceHolder;

            _ixLongValue = ixLongValue;
            _ixShortValue = ixShortValue;

            _longValue = _shortValue = double.NaN;
        }

        public void UpdateValues(double[] inputs)
        {
            _longValue = inputs[_ixLongValue];
            _shortValue = inputs[_ixShortValue];
        }

        public bool GetMustClosePosition(int position, double weightedOpenPrice)
        {
            if (_lastPriceHolder.IsNotSet) return IDynamicGuard.MustClosePositionIfNoData;
            if (position > 0)
            {
                if (double.IsNaN(_longValue)) return IDynamicGuard.MustClosePositionIfNoData;

                if (OrderPriceMode)
                    return _lastPriceHolder.LastPrice >= _longValue;

                return _lastPriceHolder.LastPrice - weightedOpenPrice >= _longValue;
            }
            else
            {
                if (double.IsNaN(_shortValue)) return IDynamicGuard.MustClosePositionIfNoData;

                if (OrderPriceMode)
                    return _lastPriceHolder.LastPrice <= _shortValue;

                return weightedOpenPrice - _lastPriceHolder.LastPrice >= _shortValue;
            }
        }

    }

    public class StrategyDynamicGuards
    {
        private readonly IDynamicGuard StopGuard;
        private readonly IDynamicGuard TargetGuard;

        public StrategyDynamicGuards(IDynamicGuard stopGuard, IDynamicGuard targetGuard)
        {
            StopGuard = stopGuard;
            TargetGuard = targetGuard;
        }
        public void UpdateValues(double[] inputs)
        {
            StopGuard.UpdateValues(inputs);
            TargetGuard.UpdateValues(inputs);
        }

        public (bool, bool) GetMustClosePosition(int position, double weightedOpenPrice)
        {
            return 
                (StopGuard.GetMustClosePosition(position, weightedOpenPrice),
                    TargetGuard.GetMustClosePosition(position, weightedOpenPrice));
        }
    }
}