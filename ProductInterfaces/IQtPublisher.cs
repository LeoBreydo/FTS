using CommonStructures;

namespace ProductInterfaces
{
    /// <summary>
    /// Interface to publish QuoteUpdate messages to the QuotesChannel (see the IQtChannel)
    /// </summary>
    public interface IQtPublisher
    {
        /// <summary>
        /// publish the quotes
        /// </summary>
        /// <param name="quoteUpdate">quote info</param>
        void Publish(QuoteUpdate quoteUpdate);
    }
}
