using System.Collections.Generic;

namespace CoreTypes
{
    public class MarketCriticalLossManager
    {
        private readonly List<StrategyPosition> _strategyPositions;
        public decimal CriticalLoss { get; private set; }

        public MarketCriticalLossManager(MarketPosition instrumentPosition, decimal criticalLoss = decimal.MinValue)
        {
            _strategyPositions = instrumentPosition.StrategyPositions;
            CriticalLoss = criticalLoss;
        }

        public decimal SessionResult { get; set; } = 0;
        public bool StoppedByCriticalLoss { get; private set; } = false;

        public void UpdateState()
        {
            StoppedByCriticalLoss = SessionResult <= CriticalLoss;
        }
    }

    public class StrategyCriticalLossManager
    {
        private readonly StrategyPosition _strategyPosition;
        private decimal _resultAtSessionStart = 0;

        public StrategyCriticalLossManager(StrategyPosition strategyPosition,
            decimal criticalLoss = decimal.MinValue)
        {
            _strategyPosition = strategyPosition;
            CriticalLoss = criticalLoss;
        }

        public decimal CriticalLoss { get; set; }

        public decimal SessionResult { get; private set; }

        public bool StoppedByCriticalLoss { get; private set; } = false;

        public void UpdateState()
        {
            SessionResult = _strategyPosition.TotalResult - _resultAtSessionStart;
            StoppedByCriticalLoss = SessionResult <= CriticalLoss;
        }

        public void StartNewSession(decimal currentResult) =>
            _resultAtSessionStart = currentResult;
    }
}
