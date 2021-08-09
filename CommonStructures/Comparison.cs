using System;

namespace CommonStructures
{
    public enum Comparison { More, MoreEq, Less, LessEq, Eq, NotEq }

    public static class ComparisonEx
    {
        public static string ComparisonToString(this Comparison comparison)
        {
            return comparison switch
            {
                Comparison.Eq => "=",
                Comparison.NotEq => "!=",
                Comparison.Less => "<",
                Comparison.LessEq => "<=",
                Comparison.More => ">",
                Comparison.MoreEq => ">=",
                _ => throw new Exception("Unexpected value " + comparison)
            };
        }
    }

}
