using System;
using System.Diagnostics;
using System.IO;
using ProductInterfaces;

namespace ProductClasses
{
    public class StartStopProcess:IActivator
    {
        private readonly string _fileName;
        private readonly string _arguments;
        private Process _process;
        public StartStopProcess(string fileName)
        {
            if (!File.Exists(fileName))
                throw new Exception("File not found " + fileName);
            _fileName = fileName;
            _arguments = null;
        }
        public StartStopProcess(string fileName,string arguments)
        {
            if (!File.Exists(fileName))
                throw new Exception("File not found " + fileName);
            _fileName = fileName;
            _arguments = arguments;
        }
        public void Activate()
        {
            ProcessStartInfo pci=(_arguments==null)
                ?new ProcessStartInfo(_fileName)
                :new ProcessStartInfo(_fileName,_arguments);
            pci.WindowStyle = ProcessWindowStyle.Minimized;
            _process = Process.Start(pci);
        }

        public void Deactivate()
        {
            if (_process != null)
            {
                try
                {
                    _process.CloseMainWindow();
                    _process.WaitForExit(500);
                    if (!_process.HasExited)
                        _process.Kill();

                }
// ReSharper disable EmptyGeneralCatchClause
                catch
// ReSharper restore EmptyGeneralCatchClause
                {
                }
                _process = null;
            }
        }
    }
}
