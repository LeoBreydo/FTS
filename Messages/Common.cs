namespace Messages
{
    /// <summary>
    /// Indicates when the TradingServer work started
    /// </summary>
    public class BeginOfWorkMsg : BaseMessage
    {
        public BeginOfWorkMsg() : base(MessageNumbers.TRADING___SERVICE___ACTIVATED) { }
    }
    /// <summary>
    /// Indicates when the TradingServer work was stopped
    /// </summary>
    /// <remarks>The missed deactivation message indicates that work was terminated without correct exit</remarks>
    public class EndOfWorkMsg : BaseMessage
    {
        public EndOfWorkMsg() : base(MessageNumbers.TRADING___SERVICE___DEACTIVATED) { }
    }

    /// <summary>
    /// A centralized command to reconnection managers to activate work
    /// </summary>
    /// <remarks>
    /// Published to the commands channel in the begin of the work when all components are ready.
    /// </remarks>
    public class Cmd_ActivateConnections : BaseMessage
    {
        public Cmd_ActivateConnections() : base(MessageNumbers.Cmd_ActivateConnections) { }
    }
    /// <summary>
    /// A centralized command to reconnection managers to close connections and deactivate work
    /// </summary>
    public class Cmd_DeactivateConnections : BaseMessage
    {
        public Cmd_DeactivateConnections() : base(MessageNumbers.Cmd_DeactivateConnections) { }        
    }
}
