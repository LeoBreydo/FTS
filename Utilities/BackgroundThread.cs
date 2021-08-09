using System;
using System.Globalization;
using System.Threading;

namespace Utilities
{
    /// <summary>
    /// The base class for background tasks representable as a cycle "while(!exitSignal) Loop()"
    /// </summary>
    /// <remarks>
    /// Manages the background thread start/stop.
    /// The methods BeforeLoop, Loop, AfterLoop are empty, to be overridden in a derived classes
    /// </remarks>
    public class BackgroundThread
    {
        /// <summary>
        /// signal to break loop
        /// </summary>
        private volatile bool mbStopFlag;
        private volatile bool _isThreadStarted;
        private readonly ManualResetEvent mThreadStoppedEvent = new(false);
        private readonly bool IsBackgroundThread;
        private readonly bool SetInvariantCulture;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="isBackgroundThread">true (by default) means "forcible terminate of the backgound thread when application closes"</param>
        /// <param name="setInvariantCulture">setup invariant cluture for the thread if value is true</param>
        public BackgroundThread(bool isBackgroundThread = true, bool setInvariantCulture = true)
        {
            IsBackgroundThread = isBackgroundThread;
            SetInvariantCulture = setInvariantCulture;
        }

        /// <summary>
        /// start background thread
        /// </summary>
        /// <remarks>
        /// ignored if thread is already started
        /// </remarks>
        public void StartThread()
        {
            if (_isThreadStarted) return;
            _isThreadStarted = true;
            mbStopFlag = false;

            var thread = new Thread(Run) { IsBackground = IsBackgroundThread };
            if (SetInvariantCulture)
                thread.CurrentCulture = CultureInfo.InvariantCulture;
            thread.Start();
        }

        /// <summary>
        /// is background thread working or not
        /// </summary>
        public bool IsThreadStarted
        {
            get { return _isThreadStarted; }
        }
        /// <summary>
        /// stop background thread
        /// </summary>
        /// <param name="toWaitForTheThreadDone">if true then wait for background thread done</param>
        public void StopThread(bool toWaitForTheThreadDone)
        {
            if (!_isThreadStarted) return;

            mThreadStoppedEvent.Reset();
            mbStopFlag = true;
            if (toWaitForTheThreadDone)
                mThreadStoppedEvent.WaitOne();
        }
        /// <summary>
        /// called when thread starts before the loop; to be overriden; does nothing by default
        /// </summary>
        protected virtual void BeforeLoop()
        {
        }
        /// <summary>
        /// called inside the loop until call of the StopThread; to be overriden; does nothing by default
        /// </summary>
        protected virtual void Loop()
        {
        }
        /// <summary>
        /// called when thread stops after leaves the loop; to be overriden; does nothing by default
        /// </summary>
        protected virtual void AfterLoop()
        {
        }
        protected virtual void OnException(Exception exception)
        {
        }
        /// <summary>
        ///  the background thread method
        /// </summary>
        private void Run()
        {
            try
            {
                BeforeLoop();
                while (!mbStopFlag)
                    Loop();
                AfterLoop();

            }
            catch (Exception exception)
            {
                OnException(exception);
                string txt =
                    DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss.fff") + "\n" +
                    exception.GetDebugString() + "\n";
                System.IO.File.AppendAllText("Exception.txt",txt);
                Console.WriteLine("Exception!!!\n"+txt);
            }

            mbStopFlag = false;
            _isThreadStarted = false;
            mThreadStoppedEvent.Set();
        }

    }
}
