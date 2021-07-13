namespace CommonStructures
{
    public class StrategyStatusInfo
    {
        public long StrategyID;
        public bool IsReal;
        public string Symbol;
        public string Account;

        public string CurrentStrategyStateWithoutSchedulerInfoAsString;
        public bool HasExecutionProblem;

        public bool TradeScheduleIsSwitchedOn;
        public bool MarketFilterIsOn;
        public bool TrendMonitorIsActive;
        public string TrendValue;
        public string SchedulerInfo;

        public long TradingAmount;
        public double InitialStopLevel;
        public double TrailingActivationLevel;
        public double TrailingStopLevel;
        public double TargetLevel;
        public DynamicalRestrictionState DynamicalTargetState;
        public DynamicalRestrictionState DynamicalStopState;


        public decimal BaseCurrencyExposure; // filled for real strategies only
        public decimal QuoteCurrencyExposure;// filled for real strategies only

    }
}
