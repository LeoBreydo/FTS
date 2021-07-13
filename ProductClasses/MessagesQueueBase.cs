using System.Collections.Concurrent;

namespace ProductClasses
{
    /// <summary>
    /// The base class for the MessagesQueue and QtMessagesQueue (see details in the MessagesQueue class description)
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessagesQueueBase<TMessage>
    {
        protected abstract void HandleMessage(TMessage msg);
        protected readonly ConcurrentQueue<TMessage> IncommingMessages = new ConcurrentQueue<TMessage>();
        protected bool OverFlow;

        /// <summary>
        /// returns true if is started (after IRoutine.Start was called)
        /// </summary>
        //public bool IsStarted { get; private set; }
        /// <summary>
        /// The messages queue max size
        /// </summary>
        public readonly int QueueMaxSize;
        private int CheckFrequency;
        private int callsBeforeCheck;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="queueMaxSize">max size ofmessages quoeue</param>
        protected MessagesQueueBase(int queueMaxSize)
        {
            QueueMaxSize = queueMaxSize;
            InitChecker();
        }
        private void InitChecker()
        {
            CheckFrequency = QueueMaxSize / 100;
            if (CheckFrequency <= 0)
                CheckFrequency = 1;
            callsBeforeCheck = CheckFrequency;
        }
        public void Handle(TMessage message)
        {
            if (OverFlow)
            {
                if (MessagesQueueOverflowPolicyInstance.Instance != null)
                    MessagesQueueOverflowPolicyInstance.Instance.OnOverflow(message);
                return; // do not add new messages to processing if overflow
            }

            //if (!IsStarted)
            //{
            //    if (MessagesQueueOverflowPolicyInstance.Instance != null)
            //        MessagesQueueOverflowPolicyInstance.Instance.OnMessageHandledBeforeStart(message);
            //}

            if (--callsBeforeCheck <= 0)
            {
                callsBeforeCheck = CheckFrequency;
                int cnt = IncommingMessages.Count;
                if (cnt >= QueueMaxSize)
                {
                    OnOverflow();
                    if (MessagesQueueOverflowPolicyInstance.Instance != null)
                        MessagesQueueOverflowPolicyInstance.Instance.OnOverflow(message);
                    return;
                }
            }
            IncommingMessages.Enqueue(message);
        }
        //public virtual void Start()
        //{
        //    IsStarted = true;
        //}
        //public virtual void Stop()
        //{
        //    IsStarted = false;
        //    ProcessReceivedMessages();
        //}

        //public virtual void Call()
        //{
        //    ProcessReceivedMessages();
        //}
        public void ProcessReceivedMessages()
        {
            OverFlow = false;
            TMessage message;
            while (IncommingMessages.TryDequeue(out message))
                HandleMessage(message);
        }

        public void Clear()
        {
            OverFlow = false;
            TMessage message;
            while (IncommingMessages.TryDequeue(out message))
            {
            }
        }
        protected virtual void OnOverflow()
        {
            OverFlow = true;
        }
    }
}
