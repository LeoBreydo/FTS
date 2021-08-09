using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommonStructures
{
    public static class FileEx
    {
        public static IEnumerable<string> ReadLinesReadonly(string fileName)
        {
            using var fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            while (true)
            {
                string row=sr.ReadLine();
                if (row == null) break;
                yield return row;
            }
        }
        public static IEnumerable<string> ReadAllLinesReadonly(string fileName)
        {
            return ReadLinesReadonly(fileName).ToList();
        }
    }
}