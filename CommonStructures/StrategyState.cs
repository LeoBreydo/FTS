namespace CommonStructures
{
    public enum StrategyState
    {
        Disabled = 0,        
        Running,
        SoftStopping,
        HardStopping,
        Stopped,        
        StoppedByScheduler,
        StoppingByScheduler, // the brand new state for attempting to close position (as assumed in the SoftStopping state) before pass to StoppedByScheduler state
        StoppingByCriticalLoss,
        StoppedByCriticalLoss
    }    
}