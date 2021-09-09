using System;

namespace CoreTypes
{
    #region PositionGards
    public interface IPositionGuard
    {
        void SetLastPrice(double price);
        bool PositionMustBeClosed();
        void UpdatePosition(StrategyPosition position);
    }
    public class FakePositionGuard : IPositionGuard
    {
        public void SetLastPrice(double price)
        {

        }

        public bool PositionMustBeClosed()
        {
            return false;
        }

        public void UpdatePosition(StrategyPosition position)
        {

        }
    }
    public class FixedStopLossPositionGuard : IPositionGuard
    {
        private double _level;
        private readonly double _delta;
        private int _posState;
        private double _last;

        public FixedStopLossPositionGuard(double delta)
        {
            if (delta < 0) delta = 0;
            _delta = delta;
            _posState = 0;
        }


        public void SetLastPrice(double price)
        {
            _last = price;
        }

        public bool PositionMustBeClosed()
        {
            if (_posState == 0) return false;

            if (_posState > 0)
            {
                if (_last <= _level) return true;
            }
            else
            {
                if (_last >= _level) return true;
            }
            return false;
        }

        public void UpdatePosition(StrategyPosition position)
        {
            if (position.Size == 0)
            {
                _posState = 0;
                return;
            }
            _posState = Math.Sign(position.Size);
            _level = _posState == 1
                ? position.WeightedOpenQuote - _delta
                : position.WeightedOpenQuote + _delta;
        }
    }
    public class TakeProfitPositionGuard : IPositionGuard
    {
        private double _level;
        private readonly double _delta;
        private int _posState;
        private double _last;

        public TakeProfitPositionGuard(double delta)
        {
            if (delta < 0) delta = 0;
            _delta = delta;
            _posState = 0;
        }


        public void SetLastPrice(double price)
        {
            _last = price;
        }

        public bool PositionMustBeClosed()
        {
            if (_posState == 0) return false;
            if (_posState > 0)
            {
                if (_last >= _level) return true;
            }
            else
            {
                if (_last <= _level) return true;
            }
            return false;
        }

        public void UpdatePosition(StrategyPosition position)
        {
            if (position.Size == 0)
            {
                _posState = 0;
                return;
            }
            _posState = Math.Sign(position.Size);
            _level = _posState > 0
                ? position.WeightedOpenQuote + _delta
                : position.WeightedOpenQuote - _delta;
        }
    }
    public class TrailedStopLossPositionGuard : IPositionGuard
    {
        private bool _activated;
        private double _openQuote;
        private readonly double _activationProfit;
        private readonly double _trailingDelta;
        private double _lastLevel;
        private readonly double _initialDelta;
        private int _posState;
        private double _last;

        public TrailedStopLossPositionGuard(double initialDelta, double trailingDelta, double activationProfit)
        {
            if (initialDelta < 0) initialDelta = 0;
            if (trailingDelta < 0) trailingDelta = 0;
            if (activationProfit < 0) activationProfit = 0;

            _initialDelta = initialDelta;
            _trailingDelta = trailingDelta;
            _activationProfit = activationProfit;
            _posState = 0;
        }

        public void SetLastPrice(double price)
        {
            _last = price;
        }

        public bool PositionMustBeClosed()
        {
            if (_posState == 0) return false;
            if (_activated)
            {
                if (_posState > 0)
                {
                    if (_last - _lastLevel > _trailingDelta) _lastLevel = _last - _trailingDelta;
                    else if (_last <= _lastLevel) return true;
                }
                else
                {
                    if (_lastLevel - _last > _trailingDelta) _lastLevel = _last + _trailingDelta;
                    else if (_last >= _lastLevel) return true;
                }
            }
            else
            {
                if (_posState > 0)
                {
                    if (_last - _openQuote >= _activationProfit)
                    {
                        _lastLevel = _last - _trailingDelta;
                        _activated = true;
                    }
                    else if (_last <= _lastLevel) return true;
                }
                else
                {
                    if (_openQuote - _last >= _activationProfit)
                    {
                        _lastLevel = _last + _trailingDelta;
                        _activated = true;
                    }
                    else if (_last >= _lastLevel) return true;
                }
            }
            return false;
        }

        public void UpdatePosition(StrategyPosition position)
        {
            if (position.Size == 0)
            {
                _posState = 0;
                return;
            }
            _posState = Math.Sign(position.Size);
            _openQuote = position.WeightedOpenQuote;
            _lastLevel = _posState > 0
                ? _openQuote - _initialDelta
                : _openQuote + _initialDelta;
            _activated = false;
        }
    }
    #endregion

    #region StopLossRestrictionPolicies
    public interface IReenterRestrictionAfterStoploss
    {
        void ClearRestriction();
        void OnStoplossFired(int currentPosition);
        void UpdateAtStartOfBar();
        void UpdateByNewTargetPosition(int targetPos);
        bool IsLongPositionAdmissible { get; }
        bool IsShortPositionAdmissible { get; }

    }
    public class NoRenterRestriction : IReenterRestrictionAfterStoploss
    {
        public void ClearRestriction() { }
        public void OnStoplossFired(int currentPosition) { }
        public void UpdateAtStartOfBar() { }
        public void UpdateByNewTargetPosition(int targetPos) { }
        public bool IsLongPositionAdmissible => true;
        public bool IsShortPositionAdmissible => true;
    }
    public class ReenterRestrictionAfterStoploss : IReenterRestrictionAfterStoploss
    {
        private readonly int _maxBarsToWait;
        private readonly bool _flatLifter;

        private int _barCountSoFar;
        private int _lockedDirection;

        public ReenterRestrictionAfterStoploss(int maxBarsToWaitForOppositeSignal = 0, bool goToFlatMustLiftRestriction = false)
        {
            _maxBarsToWait = maxBarsToWaitForOppositeSignal + 1;
            _barCountSoFar = 0;
            _flatLifter = goToFlatMustLiftRestriction;
            IsLongPositionAdmissible = true;
            IsShortPositionAdmissible = true;
        }
        public void ClearRestriction()
        {
            _lockedDirection = 0;
            _barCountSoFar = 0;
            IsLongPositionAdmissible = true;
            IsShortPositionAdmissible = true;
        }

        public void UpdateAtStartOfBar()
        {
            if (--_barCountSoFar <= 0)
                ClearRestriction();
        }

        public void UpdateByNewTargetPosition(int targetPos)
        {
            if (_lockedDirection == 0) return;
            if (_flatLifter && targetPos == 0 || targetPos * _lockedDirection < 0)
                ClearRestriction();
        }

        public bool IsLongPositionAdmissible { get; private set; }
        public bool IsShortPositionAdmissible { get; private set; }
        public void OnStoplossFired(int currentPosition)
        {
            if (currentPosition == 0) return; //TODO to discuss
            _lockedDirection = Math.Sign(currentPosition);
            _barCountSoFar = _maxBarsToWait;
            IsLongPositionAdmissible = _lockedDirection != 1;
            IsShortPositionAdmissible = _lockedDirection != -1;
        }

    }
    #endregion

    public class PositionValidator
    {
        #region members

        private readonly StrategyPosition _currentPosition;

        // stoploss guard
        private IPositionGuard StopLossPositionGuard;
        private double _targetLevel, _initialStopLevel, _trailingStopLevel, _trailingActivation;
        private IReenterRestrictionAfterStoploss _reenterRestrictionAfterStoploss;
        private void SetStopLossPositionGuard(double initialStopLevel, double trailingStopLevel, double trailingActivation)
        {
            if (initialStopLevel < 0) initialStopLevel = 0;
            if (trailingStopLevel < 0) trailingStopLevel = 0;
            if (trailingActivation < 0) trailingActivation = 0;

            _initialStopLevel = initialStopLevel;
            _trailingStopLevel = trailingStopLevel;
            _trailingActivation = trailingActivation;

            if (_initialStopLevel == 0)
                SetStopLossPositionGuard(new FakePositionGuard());
            else if (trailingStopLevel == 0)
                SetStopLossPositionGuard(new FixedStopLossPositionGuard(_initialStopLevel));
            else
                SetStopLossPositionGuard(new TrailedStopLossPositionGuard(_initialStopLevel, _trailingStopLevel, _trailingActivation));

            _reenterRestrictionAfterStoploss.ClearRestriction();//LockedDirection = 0;
        }
        private void SetStopLossPositionGuard(IPositionGuard stopLossPositionGuard)
        {
            if (stopLossPositionGuard == null) StopLossPositionGuard = new FakePositionGuard();
            else
            {
                StopLossPositionGuard = stopLossPositionGuard is TakeProfitPositionGuard
                                            ? new FakePositionGuard()
                                            : stopLossPositionGuard;
            }
            StopLossPositionGuard.UpdatePosition(_currentPosition);
        }
        // take profit guard
        private IPositionGuard TakeProfitPositionGuard;
        private void SetTakeProfitPositionGuard(double value)
        {
            if (value < 0) value = 0;
            _targetLevel = value;
            if (_targetLevel > 0)
                SetTakeProfitPositionGuard(new TakeProfitPositionGuard(_targetLevel));
            else
                SetTakeProfitPositionGuard(new FakePositionGuard());
        }
        private void SetTakeProfitPositionGuard(IPositionGuard takeProfitPositionGuard)
        {
            if (takeProfitPositionGuard == null) TakeProfitPositionGuard = new FakePositionGuard();
            else
            {
                TakeProfitPositionGuard = !(takeProfitPositionGuard is TakeProfitPositionGuard)
                                              ? new FakePositionGuard()
                                              : takeProfitPositionGuard;
            }
            TakeProfitPositionGuard.UpdatePosition(_currentPosition);
        }

        #endregion

        public PositionValidator(StrategyPosition currentPosition)
        {
            _currentPosition = currentPosition;
        }

        public void Init(double targetLevel, double initialStopLevel, double trailingStopLevel, double trailingActivation,
            int maxBarsToWaitForOppositeSignalAfterStopLoss = 0,
            bool goToFlatMustLiftStopLossRestriction = false)
        {
            if (maxBarsToWaitForOppositeSignalAfterStopLoss <= 0) _reenterRestrictionAfterStoploss = new NoRenterRestriction();
            else
                _reenterRestrictionAfterStoploss = new ReenterRestrictionAfterStoploss(maxBarsToWaitForOppositeSignalAfterStopLoss,
                        goToFlatMustLiftStopLossRestriction);

            SetTakeProfitPositionGuard(targetLevel);
            SetStopLossPositionGuard(initialStopLevel, trailingStopLevel, trailingActivation);
        }

        // after gap - after start of new session
        public void ClearStopLossRestriction()
        {
            _reenterRestrictionAfterStoploss.ClearRestriction();
        }
        public void UpdateLastPrice(double price)
        {
            StopLossPositionGuard.SetLastPrice(price);
            TakeProfitPositionGuard.SetLastPrice(price);
        }
        // after every deal
        public void UpdateGuards()
        {
            StopLossPositionGuard.UpdatePosition(_currentPosition);
            TakeProfitPositionGuard.UpdatePosition(_currentPosition);
        }

        public void UpdateStopLossRestrictionByNewTargetPosition(int newTargetPosition)
        {
            _reenterRestrictionAfterStoploss.UpdateByNewTargetPosition(newTargetPosition);
        }

        public bool IsTargetPositionAcceptedByStopLossRestriction(int targetPosition)
        {
            return targetPosition == 0 || (targetPosition > 0
                ? _reenterRestrictionAfterStoploss.IsLongPositionAdmissible
                : _reenterRestrictionAfterStoploss.IsShortPositionAdmissible);
        }

        public void UpdateAtStartOfBar(double price)
        {
            UpdateLastPrice(price);
            _reenterRestrictionAfterStoploss.UpdateAtStartOfBar();
        }

        // inject signals of dynamic guards
        public bool ValidateCurrentPosition((bool closePositionByStop, bool closePositionByTarget) dynamicRestrictions)
        {
            if (_currentPosition.Size == 0) return true;
            if (StopLossPositionGuard.PositionMustBeClosed() || dynamicRestrictions.closePositionByStop)
            {
                _reenterRestrictionAfterStoploss.OnStoplossFired(Math.Sign(_currentPosition.Size));
                return false;
            }
            return !(TakeProfitPositionGuard.PositionMustBeClosed() || dynamicRestrictions.closePositionByTarget);
        }

        public bool ValidateSuggestedPosition(int suggestedPosition)
        {
            return IsTargetPositionAcceptedByStopLossRestriction(suggestedPosition);
        }
    }
}
