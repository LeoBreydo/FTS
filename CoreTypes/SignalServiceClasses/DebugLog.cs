using System;
using System.Collections.Generic;
using System.IO;

namespace CoreTypes.SignalServiceClasses
{
    public static class DebugLog
    {
        private static string _fileName;
        public static void SetLocation(string fileName)
        {
            _fileName = fileName;
            var dir = Path.GetDirectoryName(_fileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private static readonly  List<string> _messages = new List<string>();

#if DEBUG
        public const bool IsWorking=true;
#else
        public const bool IsWorking=false;
#endif

        public static void AddMsg(string txt,bool forceFlush=false)
        {
#if DEBUG
            lock (_messages)
            {
                _messages.Add(string.Format("{0} {1}", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss.fff"), txt));
                if (forceFlush)
                    Flush();
            }
#endif
        }

        public static void Flush()
        {
#if DEBUG
            lock (_messages)
            {
                if (_messages.Count > 0)
                {
                    File.AppendAllLines(_fileName, _messages);
                    _messages.Clear();
                }
            }
#endif
        }
    }
}
