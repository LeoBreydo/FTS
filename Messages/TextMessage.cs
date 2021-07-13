using CommonStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messages
{
    /// <summary>
    /// text message types
    /// </summary>
    public enum TextMessageTypes
    {
        /// <summary>
        /// normal infromational message
        /// </summary>
        Info,
        /// <summary>
        /// slight problem detection, which has no effect
        /// </summary>
        WARNING,
        /// <summary>
        /// problem detected
        /// </summary>
        ERROR,
        /// <summary>
        /// serious problem detected which can follow financial risk. The immediate attention expected. 
        /// </summary>
        ALARM, //разновидность ошибки, несущая финансовый риск (потеря соединения, таймаут по исполнению ордера, непроидентифицированный ExecReport...)
        /// <summary>
        /// debug message to form detailed view of the events sequence
        /// </summary>
        Debug, //TODO проверить использование, убрать где не требуется
    }

    public class TextMessage : BaseMessage
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TextMessageTypes TextMessageType;
        public string Text;

        public TextMessage() : base(MessageNumbers.TextMessage) { }
        public TextMessage(TextMessageTypes textMessageType, string text)
            : base(MessageNumbers.TextMessage)
        {
            TextMessageType = textMessageType;
            Text = text;
        }
    }

    public class ClientMsg:BaseMessage
    {
        public ClientMessage ClientMessage;
        public string Account;
        public string CurrencyPair;

        public ClientMsg() : base(MessageNumbers.ClientMsg) { }
        public ClientMsg(ClientMessage clientMessage, string account, string currencyPair)
            : base(MessageNumbers.ClientMsg)
        {
            ClientMessage = clientMessage;
            Account = account;
            CurrencyPair = currencyPair;
        }
    }

    public class CantRestoreMessage : BaseMessage
    {
        public string Message;

        public CantRestoreMessage() : base(MessageNumbers.CantRestoreMessage) { }
        public CantRestoreMessage(string message)
            : base(MessageNumbers.CantRestoreMessage)
        {
            Message = message;
        }
    }
    public class ObsoletteMessage : BaseMessage
    {
        public string Message;

        public ObsoletteMessage() : base(MessageNumbers.ObsoletteMessage) { }
        public ObsoletteMessage(string message)
            : base(MessageNumbers.ObsoletteMessage)
        {
            Message = message;
        }
    }
}
