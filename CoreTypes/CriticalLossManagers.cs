namespace CoreTypes
{
    public class MarketCriticalLossManager
    {
        public decimal CriticalLoss { get; }

        public MarketCriticalLossManager(decimal criticalLoss = decimal.MinValue)
        {
            CriticalLoss = criticalLoss;
        }

        public decimal SessionResult { get; set; } = 0;
        public bool StoppedByCriticalLoss { get; private set; }

        public void UpdateState()
        {
            StoppedByCriticalLoss = SessionResult <= CriticalLoss;
        }
    }

    public class StrategyCriticalLossManager
    {
        private readonly StrategyPosition _strategyPosition;
        private decimal _resultAtSessionStart;

        public StrategyCriticalLossManager(StrategyPosition strategyPosition,
            decimal criticalLoss = decimal.MinValue)
        {
            _strategyPosition = strategyPosition;
            CriticalLoss = criticalLoss;
        }

        public decimal CriticalLoss { get; set; }

        public decimal SessionResult { get; private set; }

        public bool StoppedByCriticalLoss { get; private set; }

        public void UpdateState()
        {
            SessionResult = _strategyPosition.TotalResult - _resultAtSessionStart;
            StoppedByCriticalLoss = SessionResult <= CriticalLoss;
        }

        public void StartNewSession(decimal currentResult) =>
            _resultAtSessionStart = currentResult;
    }
}
