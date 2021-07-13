using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Utilities
{
    public static class ExceptionSaver // intended to logout exceptions occured in GUI applications
    {
        public static void Save(Exception exception, string prefix = null)
        {
            string mainAssembly = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;

            string thisAssemblyPath = Path.GetDirectoryName(mainAssembly);
            if (string.IsNullOrEmpty(thisAssemblyPath)) return;

            string exceptionDir = Path.Combine(thisAssemblyPath, "EXCEPTIONS");
            if (!Directory.Exists(exceptionDir))
                Directory.CreateDirectory(exceptionDir);

            //var aexception = exception as AggregateException;
            string txt;
            AggregateException aexception;
            ReflectionTypeLoadException loadException;
            if (null != (aexception = exception as AggregateException))
                txt = "AX;" + aexception.InnerExceptions[0].GetDebugString();
            else if (null != (loadException = exception as ReflectionTypeLoadException) && loadException.LoaderExceptions.Length > 0)
                txt = "LX;" + loadException.LoaderExceptions[0].GetDebugString();
            else
                txt = exception.GetDebugString();

            string strNow = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string fname = Path.Combine(exceptionDir,
                                        strNow + "." + Path.GetRandomFileName() + ".exception");
            File.WriteAllText(fname, string.Format("{0}\n{1}\n{2}\n{3}\n", strNow, mainAssembly, prefix ?? "", txt));
        }
        public static void SaveText(string txt)
        {
            string mainAssembly = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;

            string thisAssemblyPath = Path.GetDirectoryName(mainAssembly);
            if (string.IsNullOrEmpty(thisAssemblyPath)) return;

            string exceptionDir = Path.Combine(thisAssemblyPath, "EXCEPTIONS");
            if (!Directory.Exists(exceptionDir))
                Directory.CreateDirectory(exceptionDir);

            //var aexception = exception as AggregateException;

            string strNow = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string fname = Path.Combine(exceptionDir,
                strNow + "." + Path.GetRandomFileName() + ".exception");
            File.WriteAllText(fname, string.Format("{0}\n{1}\n{2}\n", strNow, mainAssembly, txt));
        }

    }

    public static class ExceptionEx
    {
        public static string GetDebugString(this Exception ex)
        {
            var sb = new StringBuilder();
            FillDebugString(sb, ex);
            return sb.ToString();
        }
        private static void FillDebugString(StringBuilder sb, Exception ex)
        {
            sb.AppendLine("Message:" + ex.Message);
            sb.AppendLine("Stack trace:");
            sb.Append(ex.StackTrace);
            if (ex.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("Inner exception:");
                try
                {
                    FillDebugString(sb, ex.InnerException);
                }
                catch
                {
                    sb.AppendLine("can't retrive data");
                }
            }
        }
    }
}
