using System;
using System.IO;

namespace Utilities
{
    /// <summary>
    /// manages the daily switching of the log file (by utc 0:00)
    /// </summary>
    public class LogFileSelector : IDisposable
    {
        private readonly string ResultsFolderName;
        private readonly string Suffix;
        private readonly string TitleRow;
        private readonly bool SplitBySubfolders;

        private StreamWriter swLog;
        public DateTime CurrentDate { get; private set; }
        public bool IsOpened { get { return swLog != null; } }

        public bool NeedFlush { get; protected set; }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="resultsFolderName">Root folder of the log files</param>
        /// <param name="suffix">The log filename is 'yyyymmdd'+suffix specified here</param>
        /// <param name="titleRow">optional title row, will be saved as first row of the log file</param>
        /// <param name="splitBySubfolders">Specifies if need to split log files by subfolders: true=all files will be saved in the resultsFolderName;yes=log files will be saved to subfolders resultsFolderName\yyyymmdd</param>
        public LogFileSelector(string resultsFolderName, string suffix, string titleRow = null, bool splitBySubfolders = true)
        {
            ResultsFolderName = resultsFolderName;
            Suffix = suffix;
            TitleRow = titleRow;
            SplitBySubfolders = splitBySubfolders;
            CurrentDate = DateTime.MinValue;

            if (!Directory.Exists(ResultsFolderName))
                Directory.CreateDirectory(ResultsFolderName);
        }
        public StreamWriter GetStreamWriter(DateTime utcMessageTime)
        {
            DateTime msgDate = utcMessageTime.ToUniversalTime().Date;

            if (swLog != null && msgDate.Date == CurrentDate)
                return swLog;

            if (swLog != null)
                Close();

            // do no try/catch over creation of the log file:
            // the work must be terminated if we can't creare the log file as we cannot log the work
            CurrentDate = msgDate;
            string ymd = CurrentDate.ToString("yyyyMMdd");

            string dirName;
            if (!SplitBySubfolders)
                dirName = ResultsFolderName;
            else
            {
                dirName = Path.Combine(ResultsFolderName, ymd);

                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);
            }

            string fileName = Path.Combine(dirName, ymd + Suffix);
            bool createNewFile = !File.Exists(fileName);
            swLog = new StreamWriter(File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read));
            if (createNewFile && !string.IsNullOrEmpty(TitleRow))
            {
                swLog.WriteLine(TitleRow);
                swLog.Flush();
                NeedFlush = false;
            }
            return swLog;
        }
        public string GetFileName(DateTime time)
        {
            string ymd = time.ToUniversalTime().ToString("yyyyMMdd");
            string dirName = SplitBySubfolders ? Path.Combine(ResultsFolderName, ymd) : ResultsFolderName;
            return Path.Combine(dirName, ymd + Suffix);
        }
        public void Close()
        {
            if (swLog != null)
            {
                swLog.Close();
                swLog = null;
                CurrentDate = DateTime.MinValue;
                NeedFlush = false;
            }
        }
        public void Flush()
        {
            if (swLog != null)
            {
                swLog.Flush();
                NeedFlush = false;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }

}
