using System;
using CommonStructures;

namespace ProductInterfaces
{
    /// <summary>
    /// Interface of the policy how to process MessagesQueue overflow event (see description of the MessagesQueue)
    /// </summary>
    /// <remarks>
    /// Specifies the reaction for the situation when messages transmission failed via messages queue overflow
    /// (see description of the MessagesQueue) 
    /// The event occurence reveals a serious problem in the configuration settings. 
    /// The implementation of using IMessagesQueueOverflowPolicy policy should to be instantiated in the MessagesQueueOverflowPolicyInstance.Instance .
    /// </remarks>
    public interface IMessagesQueueOverflowPolicy
    {
        /// <summary>
        /// Processes the Overflow event of the MessageQueue (message will ignored)
        /// </summary>
        /// <param name="message">ignored message</param>
        void OnOverflow<TMessage>(TMessage message);
    }
    /// <summary>
    /// Processes messages from the public channels and saves messages to the general log, 
    /// saves to the log activation/deactivation markers,
    /// fixes if new messages cames or not
    /// </summary>
    public interface ILogServiceWorker : IMsgListener
    {
        // todo to review the deactivation - HasNewReceivedMessages is not the best way to detect that the system is deactivated
        bool HasNewReceivedMessages { get; }
        void Flush();
        void DirectMessageOutput(IMsg message);
         //void SaveActivationMarker(); //excluded: the activation marker must be saved directly from the ctor to avoid the windsor instantiation mismatches
        void SaveDeactivationMarker();
    }

    /// <summary>
    /// Specifies the explicit declaration for the entity responslible to subscribe and listen the all public channels for the output to log
    /// </summary>
    public interface IPublicChannelListener : IMsgListener
    { }

    /// <summary>
    /// Provides the output of messages to the general log. Provides particular storage and exclusive write rights.
    /// </summary>
    public interface IGeneralLog:IDisposable
    {
        void OutputMsg(IMsg message);
        void Flush();
    }
}
