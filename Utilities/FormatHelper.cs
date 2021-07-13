using System.Text;

namespace Utilities
{
    public static class FormatHelper
    {
        public static string ToStringAsCurrency(this long value)
        {
            return SpacedNumericValue(value.ToString());
        }
        public static string ToStringAsCurrency(this int value)
        {
            return SpacedNumericValue(value.ToString());
        }
        public static string ToStringAsCurrency(this double value)
        {
            return SpacedNumericValue(value.ToString());
        }
        private static string SpacedNumericValue(string s)
        {
            int ixFirstPos = s.IndexOf('.');
            if (ixFirstPos >= 0)
                --ixFirstPos;
            else
            {
                ixFirstPos = s.IndexOfAny(new[] { 'e', 'E' });
                if (ixFirstPos >= 0)
                    --ixFirstPos;
                else
                    ixFirstPos = s.Length - 1;
            }
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; ++i)
            {
                sb.Append(s[i]);
                if (char.IsDigit(s[i]) && i < ixFirstPos && (i - ixFirstPos) % 3 == 0)
                    sb.Append(',');
            }
            return sb.ToString();
        }

    }
}
