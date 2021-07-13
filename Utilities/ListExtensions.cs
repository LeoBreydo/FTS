using System;
using System.Collections.Generic;

namespace Utilities
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this List<T> list, Random rnd)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rnd.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }
    }
}
