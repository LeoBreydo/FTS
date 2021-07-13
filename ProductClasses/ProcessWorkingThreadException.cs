using System;
using Messages;
using ProductInterfaces;
using Utilities;

namespace ProductClasses
{
    public class ProcessWorkingThreadException:IProcessWorkingThreadException
    {
        private readonly ILogServiceWorker _logServiceWorker;
        public ProcessWorkingThreadException(ILogServiceWorker logServiceWorker)
        {
            _logServiceWorker = logServiceWorker;
        }
        public void ProcessException(Exception exception)
        {
            _logServiceWorker.DirectMessageOutput(new TextMessage(TextMessageTypes.ALARM, "EXCEPTION in the working thread:\n"+exception.GetDebugString()));
            Environment.Exit(-1);
        }
    }
}