using CommonStructures;
using ProductInterfaces;

namespace ProductClasses
{
    /// <summary>
    /// The intermediate messages listener, which provides the reception of quote updates directly from channel and the transmission of quote updates to final recipient from a separated thread (see descriptions of IChannel, IWorkingThread)
    /// </summary>
    /// <remarks>
    /// Equivalent to the class MessagesQueue but for the QuoteUpdate messages stream,
    /// see the MessagesQueue class description for the details
    /// </remarks>
    public class QtMessagesQueue : MessagesQueueBase<QuoteUpdate>, IQtListener
    {
        protected readonly IQtListener Receiver;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="receiver">final messages recipient </param>
        public QtMessagesQueue(IQtListener receiver)
            : base(500000)
        {
            Receiver = receiver;
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="receiver">final messages recipient </param>
        /// <param name="queueMaxSize">max size ofmessages quoeue</param>
        public QtMessagesQueue(IQtListener receiver, int queueMaxSize)
            : base(queueMaxSize)
        {
            Receiver = receiver;
        }
        protected override void HandleMessage(QuoteUpdate message)
        {
            Receiver.Handle(message);
        }

    }


    public class QtMessagesQueueRoutine : QtMessagesQueue, IRoutine
    {
        public QtMessagesQueueRoutine(IQtListener receiver) : base(receiver) { }
        public QtMessagesQueueRoutine(IQtListener receiver, int queueMaxSize) : base(receiver, queueMaxSize) { }

        public bool IsStarted { get; private set; }

        public void Start()
        {
            IsStarted = true;
        }
        public void Stop()
        {
            IsStarted = false;
            ProcessReceivedMessages();
        }
        public void Call()
        {
            ProcessReceivedMessages();
        }

        /// <summary>
        /// Setup messages processing from specified channel in specified thread by specified messages listener using MessageQueue
        /// </summary>
        /// <param name="processor">The final messages recipient</param>
        /// <param name="workingThread">The thread in which the messages will be processed by final recipient</param>
        /// <param name="channelRegistration">The channel registry to subscribe to the channel messages</param>
        public static void ListenChannel(IQtListener processor, IWorkingThread workingThread, IRegistry<IQtListener> channelRegistration)
        {
            // create the queue, specify the final messages processor
            var queue = new QtMessagesQueueRoutine(processor);
            // register the queue in the channel
            channelRegistration.Register(queue);
            // specify the background thread to get messages from the queue
            workingThread.Register(queue);
        }
        /// <summary>
        /// Setup messages processing from specified channels in specified thread by specified messages listener using MessageQueue
        /// </summary>
        /// <param name="processor">The final messages recipient</param>
        /// <param name="workingThread">The thread in which the messages will be processed by final recipient</param>
        /// <param name="channelRegistrations">The channels registries to subscribe to the channels messages</param>
        public static void ListenChannels(IQtListener processor, IWorkingThread workingThread,
            params IRegistry<IQtListener>[] channelRegistrations)
        {
            // create the queue, specify the final messages processor
            var queue = new QtMessagesQueueRoutine(processor);
            // register the queue in the channel
            foreach (IRegistry<IQtListener> channelRegistration in channelRegistrations)
                channelRegistration.Register(queue);

            // specify the background thread to get messages from the queue
            workingThread.Register(queue);
        }

    }

}
