using System;
using CommonStructures;
using ProductInterfaces;
using Utilities;

namespace ProductClasses
{
    /// <summary>
    /// The default implementation of IChannel
    /// </summary>
    /// <remarks>
    /// Implements thread safe registration of the listeners and direct call of listeners Handle method from publisher thread.
    /// </remarks>
    public class Channel : IMsgChannel
    {
#if DEBUG
        // used to identify channel in the debug mode
#pragma warning disable 169
        public string Title;
#pragma warning restore 169
#endif
        /// <summary>
        /// registered listeners
        /// </summary>
        protected readonly SafeList<IMsgListener> Listeners = new SafeList<IMsgListener>();
        /// <summary>
        /// Register listener
        /// </summary>
        /// <remarks>
        /// Listener should be registered in the channel to receive messages.
        /// If listener has already registered, re-registration will ignored. 
        /// </remarks>
        /// <param name="listener">Listener to register, cannot be null</param>
        public void Register(IMsgListener listener)
        {
            if (listener == null) throw new ArgumentNullException();
            Listeners.Add(listener);
        }

        /// <summary>
        /// Unregister previously registered listener.
        /// </summary>
        /// <param name="listener"></param>
        public void Unregister(IMsgListener listener)
        {
            Listeners.Remove(listener);
        }
        /// <summary>
        /// Publish message to channel
        /// </summary>
        /// <param name="message">message to transmit to listeners</param>
        public void Publish(IMsg message)
        {
            foreach (IMsgListener item in Listeners.GetItems())
                item.Handle(message);
        }
    }
}
