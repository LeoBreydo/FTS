using System.IO;
using Newtonsoft.Json;

namespace CommonStructures
{
    public static class JsonEx
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings());
        public static string ToJson(this object obj)
        {
            var jw = new StringWriter();
            _serializer.Serialize(jw, obj);
            return jw.ToString();
        }
        public static T FromJson<T>(this string str)
        {
            var r = new StringReader(str);
            return (T)_serializer.Deserialize(r, typeof(T));
        }
        public static bool TryFromJson<T>(this string str, out T result)
        {
            try
            {
                var r = new StringReader(str);
                result = (T)_serializer.Deserialize(r, typeof(T));
                return true;

            }
            catch
            {
                result = default(T);
                return false;
            }
        }

    }
}
