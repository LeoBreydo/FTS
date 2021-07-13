using CommonStructures;

namespace ProductInterfaces
{
    /// <summary>
    /// Interface for listener of a QuoteUpdate messages 
    /// </summary>
    public interface IQtListener
    {
        /// <summary>
        /// Process the incomming quote message
        /// </summary>
        /// <param name="message">received quote message</param>
        void Handle(QuoteUpdate message);
    }
}
