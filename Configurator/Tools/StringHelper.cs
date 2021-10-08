using System;

namespace Configurator
{
    static class StringHelper
    {
        public static bool IsIdentifier(this string txt)
        {
            if (string.IsNullOrEmpty(txt)) return false;
            if (!char.IsLetter(txt[0]) && txt[0] != '_') return false;
            for (int i = 1; i < txt.Length; ++i)
                if (!char.IsLetterOrDigit(txt[i]) && txt[i] != '_')
                    return false;

            return true;
        }

        //public static bool IsUnk(this string txt)
        //{
        //    return string.IsNullOrEmpty(txt) || string.Equals(txt, "UNK", StringComparison.OrdinalIgnoreCase);
        //}
    }
}