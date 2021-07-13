using CommonStructures;

namespace ProductInterfaces
{
    /// <summary>
    /// Interface for publisher of messages sending via channel (see the details in the IChannel description)
    /// </summary>
    public interface IMsgPublisher
    {
        /// <summary>
        /// publish the message
        /// </summary>
        /// <param name="message">publishing message</param>
        void Publish(IMsg message);
    }
}
