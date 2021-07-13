using System;
using System.Collections.Generic;

namespace Utilities
{
    public static class MergeSortedEnums
    {
        /// <summary>
        /// Merge sorted enumerations to the united enumeration using the mergesort algorithm
        /// </summary>
        public static IEnumerable<T> MergeSortedEnumerations<T>(this IEnumerable<T>[] sortedEnumerations,

                                                                Func<T, T, int> compareItems)
        {
            switch (sortedEnumerations.Length)
            {
                case 0:
                    return new T[0];
                case 1:
                    return sortedEnumerations[0];
            }
            return MergeImpl(sortedEnumerations, compareItems);
        }

        /// <summary>
        /// Merge sorted enumerations to the united enumeration using the mergesort algorithm
        /// </summary>
        public static IEnumerable<T> MergeSortedEnumerations<T>(this IEnumerable<T>[] sortedEnumerations)
            where T:IComparable<T>
        {
            switch (sortedEnumerations.Length)
            {
                case 0:
                    return new T[0];
                case 1:
                    return sortedEnumerations[0];
            }
            return MergeImpl(sortedEnumerations, (x,y)=>x.CompareTo(y));
        }

        private static IEnumerable<T> MergeImpl<T>(IEnumerable<T>[] sortedEnumerations, Func<T, T, int> compareItems)
        {
            var enumerators = new IEnumerator<T>[sortedEnumerations.Length];
            int count = 0;
            T minValue = default(T);
            bool first = true;
            foreach (IEnumerable<T> enm in sortedEnumerations)
            {
                IEnumerator<T> er = enm.GetEnumerator();
                if (!er.MoveNext()) continue;
                    
                enumerators[count++] = er;
                if (first)
                {
                    first = false;
                    minValue = er.Current;
                }
                else if (compareItems(er.Current, minValue) < 0)
                    minValue = er.Current;
            }
            while (count > 1)
            {
                T prevMinValue = minValue;
                minValue = default(T);
                first = true;
                int fnishedEnumerators = 0;
                for (int i = 0; i < count; ++i)
                {
                    IEnumerator<T> er = enumerators[i];
                    while (true)
                    {
                        if (compareItems(er.Current, prevMinValue) > 0) break;

                        yield return er.Current;
                        if (!er.MoveNext())
                        {
                            er = null;
                            break;
                        }
                    }
                    if (er == null)
                    {
                        ++fnishedEnumerators;
                        continue;
                    }

                    if (first)
                    {
                        first = false;
                        minValue = er.Current;
                    }
                    else if (compareItems(er.Current, minValue) < 0)
                        minValue = er.Current;
                    if (fnishedEnumerators > 0)
                        enumerators[i - fnishedEnumerators] = er;
                }
                count -= fnishedEnumerators;
            }
            if (count == 1)
            {
                IEnumerator<T> er = enumerators[0];
                do
                {
                    yield return er.Current;
                } while (er.MoveNext());
            }

        }
    }
}
