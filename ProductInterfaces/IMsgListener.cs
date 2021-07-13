using CommonStructures;

namespace ProductInterfaces
{
    /// <summary>
    /// Interface for listener of messages sending via channel (see the details in the IChannel description)
    /// </summary>
    public interface IMsgListener
    {
        /// <summary>
        /// Process the incomming message
        /// </summary>
        /// <param name="message">received message</param>
        void Handle(IMsg message);
    }

}
