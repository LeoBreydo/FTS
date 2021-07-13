using System;
using System.Collections.Generic;
using System.Threading;
using ProductInterfaces;
using Utilities;

namespace ProductClasses
{
    /// <summary>
    /// The working thread which contains and provides a routine tasks (registers and call task routines)
    /// </summary>
    /// <remarks>
    /// Use Register method to add routine tasks to the service, use Unregister method to remove routine tasks from service.
    /// Working thread arranges the call of items methods: IRoutine.Start when service starts; IRoutine.Stop when service stops, IRoutine.Call each N milliseconds while working thread is started.
    /// The best if routine tasks will registered before service start but allows also to register/unregister items when service is already started.
    /// </remarks>
    public class WorkingThread : BackgroundThread, IWorkingThread,IDisposable
    {
        protected readonly SafeList<IRoutine> Routines = new SafeList<IRoutine>();
        /// <summary>
        /// The timeinterval between calls of IRoutine.Call
        /// </summary>
        private readonly int _callFrequencyInMilliseconds;
        protected virtual int GetMillisecondToSleep()
        {
            return _callFrequencyInMilliseconds;
        }
        
        private readonly IProcessWorkingThreadException _processWorkingThreadException;
        private readonly ILogServiceWorker _logServiceWorker;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="callFrequencyInMilliseconds">call frequency in milliseconds</param>
        /// <param name="logServiceWorker">specifies the logServiceWorker to flush collected log messages at the end of the each loop</param>
        /// <param name="processWorkingThreadException">specifies the reaction to the exception in the working thread</param>
        public WorkingThread(int callFrequencyInMilliseconds, ILogServiceWorker logServiceWorker = null, IProcessWorkingThreadException processWorkingThreadException = null)
        {
            _callFrequencyInMilliseconds = callFrequencyInMilliseconds;
            _processWorkingThreadException = processWorkingThreadException;
            _logServiceWorker = logServiceWorker;
            if (_callFrequencyInMilliseconds < 0) throw new ArgumentException("CallFrequencyInMilliseconds must be >=0, received value=" + callFrequencyInMilliseconds);
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="callFrequencyInMilliseconds">call frequency in milliseconds</param>
        /// <param name="routines">routines to register in the service</param>
        /// <param name="logServiceWorker">specifies the logServiceWorker to flush collected log messages at the end of the each loop</param>
        /// <param name="processWorkingThreadException">specifies the reaction to the exception in the working thread</param>
        public WorkingThread(int callFrequencyInMilliseconds, List<IRoutine> routines, ILogServiceWorker logServiceWorker = null, IProcessWorkingThreadException processWorkingThreadException = null)
        {
            _callFrequencyInMilliseconds = callFrequencyInMilliseconds;
            _processWorkingThreadException = processWorkingThreadException;
            _logServiceWorker = logServiceWorker;
            if (_callFrequencyInMilliseconds < 0) throw new ArgumentException("CallFrequencyInMilliseconds must be >=0, received value=" + callFrequencyInMilliseconds);
            foreach (IRoutine routine in routines)
                Register(routine);
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="callFrequencyInMilliseconds">call frequency in milliseconds</param>
        /// <param name="resolver">resolver</param>
        /// <param name="routineIds">routines to register in the service</param>
        /// <param name="logServiceWorker">specifies the logServiceWorker to flush collected log messages at the end of the each loop</param>
        /// <param name="processWorkingThreadException">specifies the reaction to the exception in the working thread</param>
        public WorkingThread(int callFrequencyInMilliseconds, IResolver resolver, List<string> routineIds, ILogServiceWorker logServiceWorker = null, IProcessWorkingThreadException processWorkingThreadException = null)
        {
            _callFrequencyInMilliseconds = callFrequencyInMilliseconds;
            _processWorkingThreadException = processWorkingThreadException;
            _logServiceWorker = logServiceWorker;
            if (_callFrequencyInMilliseconds < 0) throw new ArgumentException("CallFrequencyInMilliseconds must be >=0, received value=" + callFrequencyInMilliseconds);
            foreach (string routineId in routineIds)
                Register(resolver.ResolveById<IRoutine>(routineId));
        }
        // ReSharper restore ParameterTypeCanBeEnumerable.Local

        /// <summary>
        /// register task routine to call
        /// </summary>
        public void Register(IRoutine item)
        {
            if (Routines.Contains(item)) return;
            if (IsThreadStarted)
                item.Start();

            Routines.Add(item);
        }
        /// <summary>
        /// unregister task routine
        /// </summary>
        public void Unregister(IRoutine item)
        {
            if (!Routines.Contains(item)) return;
            Routines.Remove(item);
            if (IsThreadStarted)
                item.Stop();
        }

        /// <summary>
        /// start service
        /// </summary>
        public void Start()
        {
            StartThread();
        }
        /// <summary>
        /// stop service
        /// </summary>
        public void Stop()
        {
            StopThread(true);
        }
        /// <summary>
        /// Is service started or not
        /// </summary>
        public bool IsStarted
        {
            get { return IsThreadStarted; }
        }
        /// <summary>
        /// the action to call when the service starts
        /// </summary>
        protected override void BeforeLoop()
        {
            foreach (IRoutine routine in Routines.GetItems())
                routine.Start();
        }

        /// <summary>
        /// the periodical action to call when service is started
        /// </summary>
        protected override void Loop()
        {
            foreach (IRoutine routine in Routines.GetItems())
                routine.Call();
            if (_logServiceWorker!=null)
                _logServiceWorker.Flush();
            Thread.Sleep(GetMillisecondToSleep());
        }
        /// <summary>
        /// the action to call when the service stops
        /// </summary>
        protected override void AfterLoop()
        {
            foreach (IRoutine routine in Routines.GetItems())
                routine.Stop();
            if (_logServiceWorker != null)
                _logServiceWorker.Flush();
        }
        protected override void OnException(Exception exception)
        {
            if (_processWorkingThreadException == null)
                throw exception;
            _processWorkingThreadException.ProcessException(exception);
        }
        public virtual void Dispose()
        {
            foreach (IRoutine routine in Routines.GetItems())
            {
                var disposable = routine as IDisposable;
                if (disposable!=null)
                    disposable.Dispose();
            }
            Routines.Clear();
        }

    }

    /// <summary>
    /// Расчитывает количество милисекунд до следующего пробуждения SecondPulseThread
    /// </summary>
    public class SecondPulseThread_MsSleepTime
    {
        // миллисекунда дня, когда поток должен был активироваться
        private int _ms = int.MaxValue; 
        public int GetTimeToSleep(DateTime utcNow)
        {
            var nowMs=(int) Math.Floor(utcNow.TimeOfDay.TotalMilliseconds);
            int lastWorkDuration = nowMs - _ms;
            if (lastWorkDuration>1000) // если расчетное время ежесекундных операций превысило секунду (либо реально долго исполняется либо тред не разбудили ко времени)
            {
                // просим, чтобы нас активизировали немедленно
                _ms = ++nowMs;
                return 1;
            }
            int sleepTime = 1000 - (nowMs%1000);
            _ms = nowMs + sleepTime;
            return sleepTime;
        }
    }
    /// <summary>
    /// The working thread which attaches to itself and calls with periodicity = 1 seconds all registered ISecondPulseRoutine enitities
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class SecondPulseThread : WorkingThread
    {
        private readonly SecondPulseThread_MsSleepTime _msToSleep=new SecondPulseThread_MsSleepTime();
        public SecondPulseThread(IResolver resolver, ILogServiceWorker logServiceWorker, IProcessWorkingThreadException processWorkingThreadException)
            : base(1000, logServiceWorker, processWorkingThreadException)
        {
            foreach (ISecondPulseRoutine r in resolver.ResolveAll<ISecondPulseRoutine>())
                Register(r);
        }

        public SecondPulseThread(ILogServiceWorker logServiceWorker, IProcessWorkingThreadException processWorkingThreadException, IEnumerable<ISecondPulseRoutine> routines)
            : base(1000, logServiceWorker, processWorkingThreadException)
        {
            foreach (ISecondPulseRoutine r in routines)
                Register(r);
        }

        protected override int GetMillisecondToSleep()
        {
            return _msToSleep.GetTimeToSleep(DateTime.UtcNow);
        }

    }
}
