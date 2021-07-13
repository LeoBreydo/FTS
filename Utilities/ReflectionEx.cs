using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Utilities
{
    public static class ReflectionEx
    {
        public static List<PropertyInfo> GetBrowsableProperties(this object sample)
        {
            return sample.GetType().GetProperties().Where(pi => pi.GetGetMethod() != null).ToList();
        }
        public static IEnumerable<string> GetPropertyNames(this IEnumerable<PropertyInfo> propertyInfos)
        {
            return propertyInfos.Select(pi => pi.Name);
        }
        public static IEnumerable<string> GetPropertyValues(this object obj, IEnumerable<PropertyInfo> propertyInfos)
        {
            return propertyInfos.Select(propertyInfo => propertyInfo.GetValue(obj, null).ToString());
        }
    }

    public static class OutputTableEx
    {
        public static void WriteTableToStreamWriter(this  List<object> items, StreamWriter sw, string delimiter)
        {
            var properties = items[0].GetBrowsableProperties();

            sw.WriteLine(string.Join(delimiter, properties.GetPropertyNames()));
            foreach (var item in items)
                sw.WriteLine(string.Join(delimiter, item.GetPropertyValues(properties)));
            
        }
    }
}
