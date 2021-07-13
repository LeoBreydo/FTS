using CommonStructures;
using ProductInterfaces;

namespace ProductClasses
{
    /// <summary>
    /// The intermediate messages listener, which provides the reception of messages directly from channel and the transmission of messages to final recipient from a separated thread (see descriptions of IChannel, IWorkingThread)
    /// </summary>
    /// <remarks>
    /// <para>
    /// To arrange transmission of messages to final recipient in a separated thread do call of static method ListenChannel (ListenChannel) with specified arguments 
    /// or create manually the MessagesQueue with specified final recipient and register the MessagesQueue in channel(s) to listen and in processing thread to work in.
    /// </para>
    /// <para>
    /// The QueueMaxSize should be specified in the ctor (by default 500000 messages).
    /// The overflow event is possible if messages are comes from channel publisher but processing of messages is not called from processig thread (or messages flow is too dense).
    /// In this case MessagesQueue calls the overflow policy method IMessagesQueueOverflowPolicy.OnOverflow and ignores incomming messages. 
    /// </para>
    /// </remarks>
    public class MessagesQueue : MessagesQueueBase<IMsg>, IMsgListener
    {
        protected readonly IMsgListener Receiver;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="receiver">final messages recipient </param>
        public MessagesQueue(IMsgListener receiver)
            : base(500000)
        {
            Receiver = receiver;
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="receiver">final messages recipient </param>
        /// <param name="queueMaxSize">max size ofmessages quoeue</param>
        public MessagesQueue(IMsgListener receiver, int queueMaxSize)
            : base(queueMaxSize)
        {
            Receiver = receiver;
        }
        protected override void HandleMessage(IMsg message)
        {
            Receiver.Handle(message);
        }
    }

    public class MessagesQueueRoutine : MessagesQueue, IRoutine
    {
        public MessagesQueueRoutine(IMsgListener receiver) : base(receiver) { }
        public MessagesQueueRoutine(IMsgListener receiver, int queueMaxSize) : base(receiver, queueMaxSize) { }

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
#if not_used
        /// <summary>
        /// Setup messages processing from specified channel in specified thread by specified messages listener using MessageQueue
        /// </summary>
        /// <param name="processor">The final messages recipient</param>
        /// <param name="workingThread">The thread in which the messages will be processed by final recipient</param>
        /// <param name="channelRegistration">The channel registry to subscribe to the channel messages</param>
        public static void ListenChannel(IMsgListener processor, IWorkingThread workingThread, IRegistry<IMsgListener> channelRegistration)
        {
            // create the queue, specify the final messages processor
            var queue = new MessagesQueueRoutine(processor);
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
        public static void ListenChannels(IMsgListener processor, IWorkingThread workingThread,
            params IRegistry<IMsgListener>[] channelRegistrations)
        {
            // create the queue, specify the final messages processor
            var queue = new MessagesQueueRoutine(processor);
            // register the queue in the channel
            foreach (IRegistry<IMsgListener> channelRegistration in channelRegistrations)
                channelRegistration.Register(queue);

            // specify the background thread to get messages from the queue
            workingThread.Register(queue);
        }
#endif
    }

}
