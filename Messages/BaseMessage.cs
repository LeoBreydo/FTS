using CommonStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messages
{
    /// <summary>
    /// The core class for all messages using in the system except quotes
    /// </summary>
    /// <remarks>
    /// All the implementing messages must have uniquie MessageNumber 
    /// and override the method ToString (return human readable string with desirable possibility of the next message full content restoration from the string)
    /// </remarks>
    public abstract class BaseMessage : IMsg
    {
        [JsonProperty(Order = -4)]
        public TimeStamp Time { get; set; }

        [JsonProperty(Order = -3)]
        public int MessageNumber { get { return (int)MessageType; } }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(Order = -2)]
        public MessageNumbers MessageType;
        //public override int MessageNumber { get { return (int)MessageType; } }

        protected BaseMessage()
        {
            Time = TimeStamp.UtcNow;
        }
        protected BaseMessage(MessageNumbers messageNumber)
        {
            Time = TimeStamp.UtcNow;
            MessageType = messageNumber;
        }

        public static T Restore<T>(string body)
        {
            return JsonConvert.DeserializeObject<T>(body);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
        public override string ToString()
        {
            return Serialize();
        }
    }
}