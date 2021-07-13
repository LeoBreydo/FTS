using System;
using System.Collections.Generic;

namespace Utilities
{
    public enum BinarySearchTarget
    {
        Less,
        LessEq,
        Eq,
        MoreEq,
        More
    } ;

    public static class BinarySearch
    {
        // движок
        private static void Find<TStorage, T>(T val, TStorage storage, int NumItemsInStorage, Func<T, TStorage, int, int> fCompareValToStorageItem, out int iBefore, out int iAfter)
        {
            iBefore = iAfter = -1;
            if (NumItemsInStorage == 0) return;
            switch (Math.Sign(fCompareValToStorageItem(val, storage, 0)))
            {
                case -1:
                    iAfter = 0;
                    return;
                case 0:
                    iBefore = iAfter = 0;
                    return;
            }
            iAfter = NumItemsInStorage - 1;
            switch (Math.Sign(fCompareValToStorageItem(val, storage, iAfter)))
            {
                case 0:
                    iBefore = iAfter;
                    return;
                case 1:
                    iBefore = iAfter;
                    iAfter = -1;
                    return;
            }
            // ограничиваем поиск
            iBefore = 0;
            ++iAfter; // вместо правого ограничения используем следующее за ним значения для корректного получения серидинного значения при делении пополам

            // пока интервал поиска достаточно велик, ищем делением пополам
            while (iAfter - iBefore >= 12)
            {
                int iMiddle = (iBefore + iAfter) / 2;
                int cmp = Math.Sign(fCompareValToStorageItem(val, storage, iMiddle));
                switch (cmp)
                {
                    // запрашиваемая дата совпала с серидиной
                    case 0:
                        iBefore = iAfter = iMiddle;
                        return;
                    // запрашиваемая дата больше середины
                    case 1:
                        iBefore = iMiddle;
                        continue;
                    // запрашиваемая дата меньше середины
                    case -1:
                        iAfter = iMiddle + 1;
                        continue;
                }
            }
            iAfter--;
            // подгоняем границы
            for (int i = iBefore + 1; i < iAfter; ++i)
            {
                int cmp = Math.Sign(fCompareValToStorageItem(val, storage, i));
                switch (cmp)
                {
                    // запрашиваемая дата совпала со временем бара
                    case 0:
                        iBefore = iAfter = i;
                        return;
                    // запрашиваемая дата больше времени бара
                    case 1:
                        continue;
                }
                // запрашиваемая дата меньше времени бара
                iAfter = i;
                iBefore = i - 1;
                return;
            }
            iBefore = iAfter - 1;
        }
        /// <summary>
        /// Найти в строго упорядоченном контейнере элемент, ближайший к указанному значению (общий формат вызова)
        /// </summary>
        /// <typeparam name="TStorage">Тип контейнера</typeparam>
        /// <typeparam name="T">Тип данных, по которому упорядочнен контейнер</typeparam>
        /// <param name="target">Тип искомого элемента</param>
        /// <param name="Value">Искомое значение</param>
        /// <param name="storage">контейрен</param>
        /// <param name="NumItemsInStorage">количество элементов контейнера к поиску</param>
        /// <param name="fCompareValToStorageItem">сравнить значение с элементом контейнера</param>
        /// <returns>-1 или индекс найденного элемента</returns>
        public static int FindNearestItem<TStorage, T>(TStorage storage, BinarySearchTarget target, T Value, int NumItemsInStorage, Func<T, TStorage, int, int> fCompareValToStorageItem)
        {
            int iBefore, iAfter;
            Find(Value, storage, NumItemsInStorage, fCompareValToStorageItem, out iBefore, out iAfter);
            switch (target)
            {
                case BinarySearchTarget.Less:
                    if (iBefore < 0) return -1;
                    return (iAfter == iBefore) ? iBefore - 1 : iBefore;
                case BinarySearchTarget.LessEq:
                    return iBefore;

                case BinarySearchTarget.Eq:
                    return (iBefore == iAfter) ? iBefore : -1;

                case BinarySearchTarget.MoreEq:
                    return iAfter;

                case BinarySearchTarget.More:
                    if (iAfter < 0) return -1;
                    if (iAfter != iBefore) return iAfter;
                    if (iAfter + 1 == NumItemsInStorage) return -1;
                    return iAfter + 1;
                default:
                    throw new Exception("Unreachabe state");
            }
        }
        public static int FindNearestItem<Rec, T>(this List<Rec> list, BinarySearchTarget target, T Value, Func<Rec, T> fRecordFieldToCompare)
            where T : IComparable<T>
        {
            return FindNearestItem(list, target, Value, list.Count,
                                   (v, S, i) => v.CompareTo(fRecordFieldToCompare(S[i])));
        }
        public static int FindNearestItem<Rec, T>(this List<Rec> list, BinarySearchTarget target, T Value, Func<Rec, T> fRecordFieldToCompare, int NumItemsToSearch)
            where T : IComparable<T>
        {
            return FindNearestItem(list, target, Value, NumItemsToSearch,
                                   (v, S, i) => v.CompareTo(fRecordFieldToCompare(S[i])));
        }

        public static int FindFirstIndexMoreEq<TStorage, T>(this TStorage storage, T val, int NumItemsInStorage, Func<T, TStorage, int, int> compareValToStorageItem, int szEndBinary = 25)
        {
            if (NumItemsInStorage <= 0) return -1;
            
            if (compareValToStorageItem(val, storage, 0)<=0) return 0; // val<=storage.First
            if (compareValToStorageItem(val, storage, NumItemsInStorage-1) > 0) return -1; // val>storage.Last
            int iBegin=0, iEnd=NumItemsInStorage;
            // val>storage[iBegin]
            // val<=storage[iEnd-1]
            while (iEnd - iBegin > szEndBinary)
            {
                int iMdl = (iEnd + iBegin)/2;
                if (compareValToStorageItem(val, storage, iMdl) > 0) //val>storage[iMdl]
                    iBegin = iMdl;
                else
                    iEnd = iMdl;
            }
            for(int i=iBegin+1;i<=iEnd;++i)
            {
                if (compareValToStorageItem(val, storage, i) <= 0)
                    return i;
            }
            throw new Exception("Impossible position");
        }

    }
}
