using System;

namespace CommonStructures
{
    public enum Comparison { More, MoreEq, Less, LessEq, Eq, NotEq }

    public static class ComparisonEx
    {
        public static string ComparisonToString(this Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.Eq: return "=";
                case Comparison.NotEq: return "!=";
                case Comparison.Less: return "<";
                case Comparison.LessEq: return "<=";
                case Comparison.More: return ">";
                case Comparison.MoreEq: return ">=";
                default:
                    throw new Exception("Unexpected value " + comparison);
            }
        }
    }

}
