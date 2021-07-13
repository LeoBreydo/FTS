using System;
using System.IO;
using System.Threading;
using CommonStructures;
using ProductInterfaces;
using Utilities;

namespace ProductClasses
{
    public class GeneralLogOwner:IDisposable
    {
        private static readonly Mutex _mutex;
        private static readonly object _lock=new object();

        private static GeneralLogOwner Owner;

        static GeneralLogOwner()
        {
            _mutex = new Mutex(false, "GeneralLogOwnerA");
            GC.KeepAlive(_mutex);
        }
        public GeneralLogOwner()
        {
            lock(_lock)
            {
                if (Owner!=null)
                    throw new Exception("General log is locked by another entity");

                if (!_mutex.WaitOne(0))
                    throw new Exception("General log is locked by another process");

                Owner = this;
            }
        }

        public void Dispose()
        {
            if (Owner==this)
            {
                lock (_lock)
                {
                    Owner = null;
                    _mutex.ReleaseMutex();
                }
            }
        }
    }
    public class GeneralLog : IGeneralLog
    {
        private LogFileSelector _logFileSelector;
        private StreamWriter lastStreamWriter;
        private GeneralLogOwner _logOwner;

        public GeneralLog(string resultsFolderName)
        {
            _logOwner = new GeneralLogOwner();
            _logFileSelector = new LogFileSelector(resultsFolderName, ".log", string.Empty);
        }
        public void OutputMsg(IMsg message)
        {
            lastStreamWriter = _logFileSelector.GetStreamWriter(message.Time.UtcDateTime);
            lastStreamWriter.WriteLine(message.Serialize());
        }

        public void Flush()
        {
            if (lastStreamWriter!=null)
                lastStreamWriter.Flush();
        }
        public void Dispose()
        {
            if(_logFileSelector!=null)
            {
                _logFileSelector.Dispose();
                _logFileSelector = null;
            }
            if (_logOwner != null)
            {
                _logOwner.Dispose();
                _logOwner = null;
            }
        }
    }


}