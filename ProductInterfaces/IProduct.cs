namespace ProductInterfaces
{
    public interface IProduct
    {
        /// <summary>
        /// Start work
        /// </summary>
        void Start();
        /// <summary>
        /// Stop work
        /// </summary>
        void Stop();
        /// <summary>
        /// Returns true if work is started
        /// </summary>
        bool IsStarted { get; }
    }

    public interface IProductBuilder
    {
        IProduct Build();
        IResolver Resolver { get; }
    }
}
