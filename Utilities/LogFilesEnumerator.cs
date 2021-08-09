using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Utilities
{
    /// <summary>
    /// Вспомогательный класс для чтения лог-файлов
    /// </summary>
    /// <remarks>
    /// Возможны выбросы исключений! 
    /// </remarks>
    public static class LogFilesEnumerator
    {
        private const string _dateFormat = "yyyyMMdd";

        public static List<string> GetLogFiles(string folder, bool logsSplittedByDateSubfolders, string extension)
        {
            return GetLogFiles(folder, logsSplittedByDateSubfolders, extension, DateTime.MinValue, DateTime.MaxValue);
        }

        /// <summary>
        /// Перечисляет имена лог-файлов, разбитых по датам по указанному формализму
        /// </summary>
        /// <param name="folder">папка, содержащая логи</param>
        /// <param name="logsSplittedByDateSubfolders">Находятся ли лог-файлы в подпапках основной папки</param>
        /// <param name="extension">расширение логов</param>
        /// <param name="timeBegin">начало полуоткрытого интервала [begin,end); допустимо значение DateTime.MinValue</param>
        /// <param name="timeEnd">конец полуоткрытого интервала [begin,end); допустимо значение DateTime.MaxValue</param>
        /// <returns></returns>
        public static List<string> GetLogFiles(string folder, bool logsSplittedByDateSubfolders, string extension, DateTime timeBegin, DateTime timeEnd)
        {
            if (!Directory.Exists(folder))
                return new List<string>();
            string firstDate = timeBegin.ToString(_dateFormat);
            string endDate = (timeEnd.Year > 2200 || timeEnd == timeEnd.Date)
                                 ? timeEnd.ToString(_dateFormat)
                                 : timeEnd.AddDays(1).ToString(_dateFormat);

            var filesToProcess = new List<string>();
            if (!logsSplittedByDateSubfolders)
            {
                string fileMask = "*" + extension;
                foreach (string fileName in Directory.GetFiles(folder, fileMask))
                {
                    string yyyyMMdd = Path.GetFileNameWithoutExtension(fileName);
                    if (!DateTime.TryParseExact(yyyyMMdd, _dateFormat, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                        continue;

                    if (string.CompareOrdinal(yyyyMMdd, firstDate) < 0 || string.CompareOrdinal(yyyyMMdd, endDate) >= 0)
                        continue;
                    filesToProcess.Add(fileName);
                }
            }
            else
            {
                foreach (string subDir in Directory.GetDirectories(folder))
                {
                    string yyyyMMdd = Path.GetFileName(subDir);
                    if (!DateTime.TryParseExact(yyyyMMdd, _dateFormat, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                        continue;

                    if (string.CompareOrdinal(yyyyMMdd, firstDate) < 0 || string.CompareOrdinal(yyyyMMdd, endDate) >= 0)
                        continue;

                    string fileName = Path.Combine(subDir, yyyyMMdd + extension);
                    if (!File.Exists(fileName)) continue;
                    filesToProcess.Add(fileName);
                }
            }
            filesToProcess.Sort();
            return filesToProcess;
        }
        /// <summary>
        /// Выдает первую и последнюю даты, для лог-файлов с указанным формализмом
        /// </summary>
        /// <param name="folder">папка, содержащая логи</param>
        /// <param name="logsSplittedByDateSubfolders">Находятся ли лог-файлы в подпапках основной папки</param>
        /// <param name="extension">расширение логов</param>
        /// <returns>Возвращает tuple(firstDateInUtc,lastDateInUtc); при отсутствии файлов возвращает tuple(DateTime.MinValue;DateTime.MinValue)</returns>
        public static Tuple<DateTime, DateTime> GetFirstLastDates(string folder, bool logsSplittedByDateSubfolders, string extension)
        {
            if (!Directory.Exists(folder))
                return new Tuple<DateTime, DateTime>(DateTime.MinValue, DateTime.MinValue);

            DateTime firstDate = DateTime.MaxValue;
            DateTime lastDate = DateTime.MinValue;
            if (!logsSplittedByDateSubfolders)
            {
                string fileMask = "*" + extension;
                foreach (string fileName in Directory.GetFiles(folder, fileMask))
                {
                    string yyyyMMdd = Path.GetFileNameWithoutExtension(fileName);
                    if (!DateTime.TryParseExact(yyyyMMdd, _dateFormat, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                        continue;
                    if (firstDate > dt)
                        firstDate = dt;
                    if (lastDate < dt)
                        lastDate = dt;
                }
            }
            else
            {
                foreach (string subDir in Directory.GetDirectories(folder))
                {
                    string yyyyMMdd = Path.GetFileName(subDir);
                    if (!DateTime.TryParseExact(yyyyMMdd, _dateFormat, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                        continue;
                    string fileName = Path.Combine(subDir, yyyyMMdd + extension);
                    if (!File.Exists(fileName)) continue;

                    if (firstDate > dt)
                        firstDate = dt;
                    if (lastDate < dt)
                        lastDate = dt;
                }
            }
            return (firstDate == DateTime.MaxValue)
                       ? new Tuple<DateTime, DateTime>(DateTime.MinValue, DateTime.MinValue)
                       : new Tuple<DateTime, DateTime>(firstDate, lastDate);
        }
        /// <summary>
        /// Построчное чтение лог файла с учетом того, что  файл может быть открыт другим процессом на запись
        /// </summary>
        /// <param name="fileName">имя файла</param>
        /// <param name="fileContainsHeader">имеется ли у лог-файла заголовок, который нужно пропустить при перечислении</param>
        public static IEnumerable<string> ReadAllLines(this string fileName, bool fileContainsHeader)
        {
            using var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var sr = new StreamReader(fs);
            if (fileContainsHeader)
                sr.ReadLine();
            while (true)
            {
                string row = sr.ReadLine();
                if (row == null) break;
                yield return row;
            }
        }

    }
}
