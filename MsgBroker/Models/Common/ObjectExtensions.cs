using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MsgBroker.Models.Common
{
    public static class ObjectExtensions
    {
        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
