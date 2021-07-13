namespace ProductInterfaces
{
    /// <summary>
    /// Interface of the working thread (a background thread which registers and provides a routine tasks)
    /// </summary>
    public interface IWorkingThread : IService, IRegistry<IRoutine>
    {
    }
}
