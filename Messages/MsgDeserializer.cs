using System;
using System.IO;
using System.Text.RegularExpressions;
using CommonStructures;
using Newtonsoft.Json;

namespace Messages
{
    /// <summary>
    /// Helporary class to restore serialized messages
    /// </summary>
    public static class MsgDeserializer
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings());

        private static readonly string ptnTime = "^\\{'Time':'(?<time>.{0,}?)',".Replace('\'', '\"');
        public static DateTime GetMessageTime(this string serializedMsg)
        {
            try
            {
                return DateTime.Parse(Regex.Match(serializedMsg, ptnTime).Groups["time"].Value);
            }
            catch (Exception exception)
            {
                throw new Exception("row has invalid format "+serializedMsg,exception);
            }
        }
        private static readonly string ptnMessageNumber = ",'MessageNumber':(\\d+),".Replace('\'', '\"');
        public static int GetMessageNumber(this string serializedMsg)
        {
            try
            {
                return int.Parse(Regex.Match(serializedMsg, ptnMessageNumber).Groups[1].Value);
            }
            catch (Exception exception)
            {
                throw new Exception("row has invalid format " + serializedMsg, exception);
            }

        }
        private static readonly string ptnMessageType = ",'MessageType':'\\b(?<msgType>\\w+)\\b'".Replace('\'', '\"');
        public static string GetStrMessageType(this string serializedMsg)
        {
            try
            {
                return Regex.Match(serializedMsg, ptnMessageType).Groups["msgType"].Value;
            }
            catch (Exception exception)
            {
                throw new Exception("row has invalid format " + serializedMsg, exception);
            }
        }

        public static IMsg DeserializeMsg(this IMsgFactory factory,string jsonMessage)
        {
            int msgNumber = GetMessageNumber(jsonMessage);
            Type type = factory.GetTypeOfTheMessage(msgNumber);
            return (IMsg) _serializer.Deserialize(new StringReader(jsonMessage), type);
        }
        
    }
}