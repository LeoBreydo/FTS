using System;

namespace CommonStructures
{
    /// <summary>
    ///  Get the current system time 
    /// </summary>
    public interface ICurrentTime
    {
        DateTime GetUtcNow();
    }

    public class CurrentTime:ICurrentTime
    {
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
    public static class CurrentTimeInstance
    {
        public static ICurrentTime Instance=new CurrentTime();
        public static DateTime GetUtcNow()
        {
            return Instance.GetUtcNow();
        }
    }
}
