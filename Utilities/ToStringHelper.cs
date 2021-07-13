using System;
using System.Globalization;

namespace Utilities
{
    /// <summary>
    /// Auxiliary class to cast simple types (int, long, double, enumeration)  to string and back using invariant culture
    /// </summary>
    public static class ToStringHelper
    {
        /// <summary>
        /// convert long value to string using CultureInfo.InvariantCulture
        /// </summary>
        public static string TOString(this long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// convert int value to string using CultureInfo.InvariantCulture
        /// </summary>
        public static string TOString(this int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// convert double value to string using CultureInfo.InvariantCulture
        /// </summary>
        public static string TOString(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);            
        }

        /// <summary>
        /// Converts the string representation of enumeration to the enumeration value(calls Enum.Parse)
        /// </summary>
        public static TEnum ParseEnum<TEnum>(this string value)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }
    }
}
