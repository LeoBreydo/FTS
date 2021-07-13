namespace ProductInterfaces
{
    /// <summary>
    /// Interface of a task routine (a task which should to be called with specified periodicity)
    /// </summary>
    /// <remarks>
    /// The task routines are registered and works in a particular working threads (see IWorkingThread)
    /// </remarks>
    public interface IRoutine
    {
        /// <summary>
        /// To be called at the begin of work
        /// </summary>
        void Start();
        /// <summary>
        /// To be called with specified periodicity 
        /// </summary>
        void Call();
        /// <summary>
        /// To be called at the begin of work
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// Declares that the implemented routine Call method has to be called with periodicity = 1 second
    /// </summary>
    /// <remarks>
    /// See also class 
    /// </remarks>
    public interface ISecondPulseRoutine:IRoutine
    {
        
    }
}
