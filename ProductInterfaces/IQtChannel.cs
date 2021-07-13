using CommonStructures;
namespace ProductInterfaces
{
    /// <summary>
    /// Interface for Quotes channel (like IMsgChannel but especially for the quotes stream)
    /// </summary>
    /// <remarks>
    /// The main channel idea is descripbed for the class IMsgChannel.
    /// But the quotes messages transmission is separated from other messages transmission as far as
    /// it should be optimized according to the high density of communication
    /// </remarks>
    public interface IQtChannel : IQtPublisher, IRegistry<IQtListener>
    {
    }

    public class NullQtChannel : IQtChannel
    {
        public void Publish(QuoteUpdate quoteUpdate)
        {
        }

        public void Register(IQtListener item)
        {
        }

        public void Unregister(IQtListener item)
        {
        }
    }
}
