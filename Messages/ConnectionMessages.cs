using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messages
{
    public enum SessionConnectionStatus
    {
        /// <summary>
        /// connection estrablished successfully
        /// </summary>
        ConnectionEstablished,
        /// <summary>
        /// rejection when tried to establish connection
        /// </summary>
        ConnectionRejected,
        /// <summary>
        /// fatal error when tried to establish connection
        /// </summary>
        /// <remarks>
        /// this status was used when we used Onix. QFix can't detect if error is fatal or not
        /// </remarks>
        ConnectionFatalError,
        /// <summary>
        /// disconnected by ourselves initiation
        /// </summary>
        ByUser,
        /// <summary>
        /// disconnected from counterparty
        /// </summary>
        ByCounterParty, 
        /// <summary>
        /// connection lost
        /// </summary>
        ConnectionLost,

        StoppedByIBEngine
    }
    /// <summary>
    /// Provider session connection status event (Notifies that connection to the provider session was established or closed or the connection try failed)
    /// </summary>
    public class BrokerConnectionStatus : BaseMessage
    {
        public long BrokerID;
        public bool IsQuoteSession;
        [JsonConverter(typeof(StringEnumConverter))]
        public SessionConnectionStatus ConnectionStatus;
        [JsonIgnore]
        public bool ConnectionEstablished { get { return ConnectionStatus == SessionConnectionStatus.ConnectionEstablished; } }
        public string Details;

        public BrokerConnectionStatus() : base(MessageNumbers.BrokerConnectionStatus) { }
        public BrokerConnectionStatus (long brokerID, bool isQuoteSession, SessionConnectionStatus connectionStatus, string details)
            : base(MessageNumbers.BrokerConnectionStatus)
        {
            BrokerID = brokerID;
            IsQuoteSession = isQuoteSession;
            ConnectionStatus = connectionStatus;
            Details = details ?? "";
        }
        public static BrokerConnectionStatus MakeDisconnected(long brokerID, bool isQuoteSession, SessionConnectionStatus connectionStatus, string details = null)
        {
            return new BrokerConnectionStatus(brokerID, isQuoteSession, connectionStatus, details);
        }
        public static BrokerConnectionStatus MakeConnected(long brokerID, bool isQuoteSession, string details = null)
        {
            return new BrokerConnectionStatus(brokerID, isQuoteSession, SessionConnectionStatus.ConnectionEstablished, details);
        }

    }

    /// <summary>
    /// Notification that the session is ready or not ready to accept requests (is the trade session ready or not to accept orders)
    /// </summary>
    /// <remarks>
    /// отправляется при потере соединения или при обнаружении неполученных входящих сообщений до момента их получения
    /// Помимо IsReady требуется также, чтобы брокер котировал соответствующую валютную пару.
    /// В части Citi опытным путем установлено, что  попытка отправить ордер до получения пропушенных сообщений влечет фактическую остановку торговли до ResetSeqNum
    /// </remarks>
    public class SessionReadiness : BaseMessage
    {
        public long BrokerID;
        public bool IsQuoteSession;
        public bool IsReady;
        public SessionReadiness() : base(MessageNumbers.SessionReadiness) { }
        public SessionReadiness(long brokerID, bool isQuoteSession, bool isReady)
            : base(MessageNumbers.SessionReadiness)
        {
            BrokerID = brokerID;
            IsQuoteSession = isQuoteSession;
            IsReady = isReady;
        }
    }

    public class PenaltyAddedNotification : BaseMessage
    {
        public long BrokerID;
        public int TotalDayPenalties;
        public PenaltyAddedNotification(): base(MessageNumbers.PenaltyAddedNotification) { }
        public PenaltyAddedNotification(long brokerID,int totalDayPenalties)
            : base(MessageNumbers.PenaltyAddedNotification)
        {
            BrokerID = brokerID;
            TotalDayPenalties = totalDayPenalties;
        }
    }
    public class PenaltiesResetNotification : BaseMessage
    {
        public long BrokerID;

        public PenaltiesResetNotification()
            : base(MessageNumbers.PenaltiesResetNotification) { }
        public PenaltiesResetNotification(long brokerID)
            : base(MessageNumbers.PenaltiesResetNotification)
        {
            BrokerID = brokerID;
        }
    }
}
