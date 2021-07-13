namespace CommonStructures
{
    /// <summary>
    /// used to fix the last trading server start/stop times
    /// </summary>
    public class StartStopTimes
    {
        public TimeStamp LastStartedTime = TimeStamp.Null;
        public TimeStamp LastStoppedTime = TimeStamp.Null;
    }
}
