using System;

namespace ProductInterfaces
{
    /// <summary>
    /// Specifies the policy how to process exceptions in the working threads
    /// </summary>
    public interface IProcessWorkingThreadException
    {
        void ProcessException(Exception exception);
    }
}
