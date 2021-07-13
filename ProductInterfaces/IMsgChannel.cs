namespace ProductInterfaces
{
    /// <summary>
    /// The channel interface
    /// </summary>
    /// <remarks>
    /// <para>
    /// Interface IChannel is an abstraction to transmit data from publisher(s) to receiver(s). 
    /// The publisher (IPublisher) sends messages to the channel.
    /// The listener (IListener) is registered in the channel in the begin of work, then receives and processes the incomming messages.
    /// The channel arranges the data transmission from publisher to listener (calls Handle method of registered listeners).
    /// </para>
    /// <para>
    /// Channel is the main communication tool between product components. It is intended to reduce dependencies between product components.
    /// Publisher should not to care about how the will transmitted, it just publish messages.
    /// The final recipient should not to care about how the message is come to him. Just  method Handle will called for each incomming message.
    /// The architecture (configuration) task is to define: 
    /// how much channels are used in the product; 
    ///  how dense is the message flows;
    /// how much publishers and listeners uses the channel (the best if each channel has only one publisher);
    /// how the messages will delivered from publisher to listener (direct call in the same thread, put the the messages queue and call from other thread (see class MessagesQueue), usage of invoke etc.);
    /// It is assumed that the product constructor will give the channel to the publishers and listeners during product initialization.
    /// It is assumed that if the message should to be proceeded by final recipient not directly but in another thread
    /// then the intermediate IListener is used which put received messages to the messages queue and arranges the final recipient call from other thread.
    /// </para>
    /// </remarks>
    public interface IMsgChannel : IMsgPublisher, IRegistry<IMsgListener>
    {
    }
}
