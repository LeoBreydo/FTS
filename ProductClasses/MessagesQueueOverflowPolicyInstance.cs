using ProductInterfaces;

namespace ProductClasses
{
    /// <summary>
    /// Stores the implementation of the IMessagesQueueOverflowPolicy (the Singleton) to use in the MessagesQueue
    /// </summary>
    public static class MessagesQueueOverflowPolicyInstance // : IService
    {
        /// <summary>
        /// Policy to use for MessageQueue events Overflow and MessageHandledBeforeStart
        /// </summary>
        public static IMessagesQueueOverflowPolicy Instance;
    }
}
