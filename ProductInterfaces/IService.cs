namespace ProductInterfaces
{

    /// <summary>
    /// The interface of the service
    /// </summary>
    /// <remarks>
    /// service is a layer of functionality to be included to the product (see description of the Product)
    /// </remarks>
    public interface IService
    {
        /// <summary>
        /// Start service
        /// </summary>
        void Start();


        /// <summary>
        /// Stop service
        /// </summary>
        void Stop();
        /// <summary>
        /// Returns is service started or not
        /// </summary>
        bool IsStarted { get; }
    }
}
