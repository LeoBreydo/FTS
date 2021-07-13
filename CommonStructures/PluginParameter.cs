using System.Collections.Generic;
using System.Linq;

namespace CommonStructures
{
    public class PluginParameter
    {
        public string Name;
        public string Value;
        public PluginParameter() { }
        public PluginParameter(PluginParameter from)
        {
            Name = from.Name;
            Value = from.Value;
        }

        public PluginParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public bool EqualsTo(PluginParameter other)
        {
            return Name == other.Name && Value == other.Value;
        }
    }

    public static class PluginParametersHelper
    {
        public static List<PluginParameter> Clone(this List<PluginParameter> pps)
        {
            return pps.Select(p => new PluginParameter(p)).ToList();
        }

        public static bool EqualsTo(this List<PluginParameter> one, List<PluginParameter> other)
        {
            if (one.Count != other.Count) return false;
            for (int i = 0; i < one.Count; ++i)
                if (!one[i].EqualsTo(other[i])) return false;
            return true;
        }

        public static string GetIntParamValue(this List<PluginParameter> pps, string paramName, out int value)
        {
            value = 0;
            PluginParameter pp = pps.FirstOrDefault(item => item.Name == paramName);
            if (pp == null) return string.Format("Parameter is not specified '{0}'", paramName);
            if (!int.TryParse(pp.Value, out value))
                return string.Format("Parameter '{0}' must be an integer", paramName);

            return null;
        }
        public static string GetDoubleParamValue(this List<PluginParameter> pps, string paramName, out double value)
        {
            value = 0;
            PluginParameter pp = pps.FirstOrDefault(item => item.Name == paramName);
            if (pp == null) return string.Format("Parameter is not specified '{0}'", paramName);
            if (!double.TryParse(pp.Value,out value))
                return string.Format("Parameter '{0}' must be number", paramName);

            return null;
        }
        public static string GetBoolParamValue(this List<PluginParameter> pps, string paramName, out bool value)
        {
            PluginParameter pp = pps.FirstOrDefault(item => item.Name == paramName);
            if (pp == null)
            {
                value = false;
                return string.Format("Parameter is not specified '{0}'", paramName);
            }
            switch (pp.Value.ToLower())
            {
                case "true":
                    value = true;
                    return null;
                case "false":
                    value = false;
                    return null;
                default:
                    value = false;
                    return string.Format("Parameter '{0}' must be boolean", paramName);
            }
        }

    }
}