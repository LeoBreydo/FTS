using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Configurator.ViewModel
{
    public class PropertyValue
    {
        public string Property { get; }
        public string Value { get; }

        private PropertyValue(string property, string value)
        {
            Property = property;
            Value = value;
        }

        public static List<PropertyValue> LoadFromFile(string fileName, char delimiter = '=')
        {
            try
            {
                if (!File.Exists(fileName)) return null;
                var ret= File.ReadAllLines(fileName).Select(row =>
                {
                    if (string.IsNullOrWhiteSpace(row)) return null;
                    var ixDelim = row.IndexOf(delimiter);
                    if (ixDelim < 0)
                        return new PropertyValue(row.Trim(), String.Empty);
                    return new PropertyValue(row.Substring(0, ixDelim).Trim(), row.Substring(ixDelim + 1).Trim());
                }).Where(item => item != null).ToList();
                return ret;

                //return new Tuple<string, List<PropertyValue>>(null,ret);
            }
            catch (Exception e)
            {
                return null;
                //return new Tuple<string, List<PropertyValue>>(e.Message, null);
            }
        }
    }

}