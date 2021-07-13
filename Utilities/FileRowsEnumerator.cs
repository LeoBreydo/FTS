using System.Collections.Generic;
using System.IO;

namespace Utilities
{
    public static class FileRowsEnumerator
    {
        public static IEnumerable<string> ForeachRow(this StreamReader streamReader,bool skipFirstRow)
        {
            while (true)
            {
                string row = streamReader.ReadLine();
                if (row == null)
                    break;
                if (skipFirstRow)
                    skipFirstRow = false;
                else
                    yield return row;
            }
        }
    }
}
