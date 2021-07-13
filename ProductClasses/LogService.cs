using System.Collections.Concurrent;
using System.Threading.Tasks;
using CommonStructures;
using Messages;
using ProductInterfaces;

namespace ProductClasses
{
    /// <summary>
    /// Processes messages from the public channels and saves messages to the general log, 
    /// saves to the log activation/deactivation markers,
    /// fixes if new messages cames or not
    /// </summary>
    public class LogServiceWorker : ILogServiceWorker, IPublicChannelListener
    {
        private readonly IGeneralLog _generalLog;

        private readonly ConcurrentQueue<IMsg>[] Buffers =
        {
            new ConcurrentQueue<IMsg>(),
            new ConcurrentQueue<IMsg>()
        };

        private volatile int ixCollectingBuffer;
        private bool _HasNewReceivedMessages;
        public bool HasNewReceivedMessages
        {
            get
            {
                var result = _HasNewReceivedMessages;
                _HasNewReceivedMessages = false;
                return result;
            }
        }

        public LogServiceWorker(IGeneralLog generalLog)
        {
            _generalLog = generalLog;
            Handle(new BeginOfWorkMsg());
        }

        public void Handle(IMsg message)
        {
            Buffers[ixCollectingBuffer].Enqueue(message);
            _HasNewReceivedMessages = true;
        }

        public void Flush()
        {
            FlushImpl(false);
        }

        private readonly object locker = new object();
        private Task _flushTask;

        private void FlushImpl(bool blocking)
        {
            if (_HasNewReceivedMessages)

                lock (locker)
                {
                    var workingFlushTask = _flushTask;
                    if (workingFlushTask != null)
                    {
                        if (!blocking) return;
                        workingFlushTask.Wait(); // check that previous flush is done
                    }

                    _HasNewReceivedMessages = false;
                    ixCollectingBuffer = 1 - ixCollectingBuffer;

                    
                    _flushTask = new Task(() =>
                    {
                        ConcurrentQueue<IMsg> savingBuf = Buffers[1 - ixCollectingBuffer];
                        IMsg msg;
                        while (savingBuf.TryDequeue(out msg))
                            _generalLog.OutputMsg(msg);

                        _generalLog.Flush();

                        _flushTask = null;
                    });
                    _flushTask.Start();

                    if (blocking)
                    {
                        workingFlushTask = _flushTask;
                        if (workingFlushTask != null)
                            workingFlushTask.Wait();
                    }
                }

        }

        public void DirectMessageOutput(IMsg message)
        {
            Handle(message); 
            FlushImpl(true);
        }


        private bool _deactivationMarkerIsSaved;
        public void SaveDeactivationMarker()
        {
            if (_deactivationMarkerIsSaved) return;
            Handle(new EndOfWorkMsg());
            FlushImpl(true);
            //_generalLog.OutputMsg(new EndOfWorkMsg());
            _deactivationMarkerIsSaved = true;
        }

    }

    public class LogServiceActivator:IActivator
    {
        private readonly ILogServiceWorker _logServiceWorker;
        public LogServiceActivator(ILogServiceWorker logServiceWorker)
        {
            _logServiceWorker = logServiceWorker;
        }
        public void Activate(){ }

        public void Deactivate()
        {
            _logServiceWorker.SaveDeactivationMarker();// ie. flush all messages
        }
    }

}
