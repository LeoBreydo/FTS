using System;
using CommonStructures;
using ProductInterfaces;
using Utilities;

namespace ProductClasses
{
    /// <summary>
    /// The default implementation of IQtChannel
    /// </summary>
    /// <remarks>
    /// Implements thread safe registration of the listeners and direct call of listeners Handle method from publisher thread.
    /// </remarks>
    public class QtChannel : IQtChannel
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
        protected readonly SafeList<IQtListener> Listeners = new SafeList<IQtListener>();
        /// <summary>
        /// Register listener
        /// </summary>
        /// <remarks>
        /// Listener should be registered in the channel to receive messages.
        /// If listener has already registered, re-registration will ignored. 
        /// </remarks>
        /// <param name="listener">Listener to register, cannot be null</param>
        public void Register(IQtListener listener)
        {
            if (listener == null) throw new ArgumentNullException();
            Listeners.Add(listener);
        }

        /// <summary>
        /// Unregister previously registered listener.
        /// </summary>
        /// <param name="listener"></param>
        public void Unregister(IQtListener listener)
        {
            Listeners.Remove(listener);
        }

        private readonly object _locker=new object();
        /// <summary>
        /// Publish message to channel
        /// </summary>
        /// <param name="quoteUpdate">message to transmit to listeners</param>
        public void Publish(QuoteUpdate quoteUpdate)
        {
            lock (_locker)
            {
                quoteUpdate.MessageCreatedTime = CurrentTimeInstance.GetUtcNow();

                foreach (IQtListener item in Listeners.GetItems())
                    item.Handle(quoteUpdate);
            }
        }
    }
}
