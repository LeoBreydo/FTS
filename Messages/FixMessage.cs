using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messages
{
    /// <summary>
    /// The couse the publicated fix message
    /// </summary>
    public enum FixMessageCouse
    {
        /// <summary>
        /// in message of the session level (logon,logout, heardbeat, resend request etc)
        /// </summary>
        SIn,
        /// <summary>
        /// out messages of the session level
        /// </summary>
        SOut,
        /// <summary>
        /// in messages of the application level
        /// </summary>
        AIn,
        /// <summary>
        /// out messages of the application level
        /// </summary>
        AOut,
        /// <summary>
        /// resend the missed out application level message to the counterparty 
        /// </summary>
        Reout,
    }

    public class FixMessage : BaseMessage
    {
        public long BrokerID;
        [JsonConverter(typeof(StringEnumConverter))]
        public FixMessageCouse FixMessageCouse;
        public string MsgType;
        public string StrFixMessage;
        public string SessionKey;
        public FixMessage() : base(MessageNumbers.FixMessage) { }
        public FixMessage(long brokerID, FixMessageCouse fixMessageCouse, string sessionKey, string msgType_tag35, string strFixMessage)
            : base(MessageNumbers.FixMessage)
        {
            BrokerID = brokerID;
            FixMessageCouse = fixMessageCouse;
            SessionKey = sessionKey;
            MsgType = msgType_tag35;
            StrFixMessage = strFixMessage;
        }
        //public override string ToString()
        //{
        //    string strCouse;
        //    switch (FixMessageCouse)
        //    {
        //        case FixMessageCouse.SIn:
        //            strCouse = "<<S";
        //            break;
        //        case FixMessageCouse.SOut:
        //            strCouse = ">>S";
        //            break;
        //        case FixMessageCouse.AIn:
        //            strCouse = "<<A";
        //            break;
        //        case FixMessageCouse.AOut:
        //            strCouse = ">>A";
        //            break;
        //        case FixMessageCouse.Reout:
        //            strCouse = ">>R";
        //            break;
        //        default:
        //            strCouse = "?";
        //            break;
        //    }
        //    return string.Format("FixMessage, {0} {1} {2}:{3}", BrokerID, strCouse, SessionKey, StrFixMessage);
        //}
    }
}
