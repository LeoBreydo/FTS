using System;
using System.IO;
using System.Reflection;

namespace TWS_Activator
{
    public class Logger : IDisposable
    {
        private StreamWriter sw;
        public Logger()
        {
            string logFileName = Assembly.GetAssembly(typeof(Logger))?.Location + ".log";
            sw = new StreamWriter(File.Open(logFileName, FileMode.Append, FileAccess.Write, FileShare.Read));
        }

        public void Dispose()
        {
            if (sw != null)
            {
                sw.Close();
                sw.Dispose();
                sw = null;
            }
        }
        public void WriteLine(string text)
        {
            try
            {
                sw?.WriteLine("{0} {1}", DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss.fff"), text);
                sw?.Flush();
            }
            catch
            {
                //sw = null;
            }

        }
    }
}
