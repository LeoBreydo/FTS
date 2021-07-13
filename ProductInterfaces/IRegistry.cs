namespace ProductInterfaces
{
    /// <summary>
    /// Interface to register some items
    /// </summary>
    /// <remarks>
    /// Used for several goals: listeners registration in channels, services registration in product etc.
    /// Registered entities should not be duplicated.
    /// </remarks>
    /// <typeparam name="T">Registry items type</typeparam>
    public interface IRegistry<in T>
    {
        /// <summary>
        /// register participant
        /// </summary>
        void Register(T item);
        /// <summary>
        /// unregister participant
        /// </summary>
        void Unregister(T item);
    }
}
