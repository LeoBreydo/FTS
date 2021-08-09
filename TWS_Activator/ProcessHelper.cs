using System.Diagnostics;
// ReSharper disable EmptyGeneralCatchClause

namespace TWS_Activator
{
    public static class ProcessHelper
    {
        public static void TryKillProcess(this Process p)
        {
            try
            {
                p.Kill();
            }
            catch
            {
            }
        }
        public static string GetMainWindowTitle(this Process p)
        {
            try
            {
                if (p == null || p.HasExited) return "";
                return Process.GetProcessById(p.Id).MainWindowTitle;
            }
            catch
            {
                return "";
            }
        }
    }
}
