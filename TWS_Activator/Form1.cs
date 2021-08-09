using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.ServiceProcess;
using Utilities;

namespace TWS_Activator
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Utility will close TWS and exit if TS is not working during specified time interval.
        /// Also utility will exit if TWS is closed by user and TS is switched from working to not-working state
        /// (needed to avoid TWS reopen by utility after the explicit close of TWS by user).
        /// </summary>
        private const int DEACTIVATION_TIMEOUT_IN_MINUTES = 10;

        /// <summary>
        /// after TWS is launched utility will wait for specified timeout then check if TWS is in the login state.
        /// If it will detect that login is still not passed then we suppose that saved credentials are invalid,
        /// that saved credentials will be deleted (delete file 'IBClientInfo.xml') and 
        /// re-asked next time when user starts utility
        /// </summary>
        private const int LOGIN_FORM_TIMEOUT_IN_SECONDS = 60;

        // timer frequency
        private const int CHECK_FREQUENCY_TWSISWORKING_IN_SECONDS = 10;

        private const int PAUSE_BEFORE_NEXT_ATTEMPT_IN_SECONDS = 3 * 60;
        //#if DEBUG
        //        private const int PAUSE_BEFORE_NEXT_ATTEMPT_IN_SECONDS = 20;
        //#else
        //        private const int PAUSE_BEFORE_NEXT_ATTEMPT_IN_SECONDS = 3*60;
        //#endif
        private IBClientInfo clientInfo;

        private Process _twsProcess;
        private DateTime _serverStoppedTimeout = DateTime.MaxValue;

        private IActivatorScheduler _activatorScheduler;
        private Logger _log;
        public Form1()
        {
            InitializeComponent();
            Thread.Sleep(new Random().Next(100, 1000));
            Text = $@"{DateTime.Now:yyyyMMdd-HHmmss.fff} TWS Activator; required for TradingServer";
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            Visible = false;

#if !DEBUG // never hide icon in taskbar when working in debug mode
            if (ConfigurationManager.AppSettings["ShowInTaskbar"].ToLower() == "false")
            {
                ShowInTaskbar = false;
                ShowIcon = false;
        }
#endif
        }


        enum EState
        {
            NotStarted,
            Starting_CheckCredentials,
            Starting,
            Working,
            PauseBeforeNextAttempt
        }
        private EState _state;

        private DateTime _timeout;
        private void SetState(EState newState, DateTime? argTimeout = null)
        {
            var oldState = _state;
            _state = newState;
            bool usesTimeout = true;
            switch (_state)
            {
                case EState.Starting_CheckCredentials:
                case EState.Starting:
                    _timeout = argTimeout ?? DateTime.UtcNow.AddSeconds(LOGIN_FORM_TIMEOUT_IN_SECONDS);
                    break;
                case EState.PauseBeforeNextAttempt:
                    if (argTimeout != null)
                        _timeout = argTimeout.Value;
                    else
                    {
                        var now = DateTime.UtcNow;
                        _timeout = now.DayOfWeek == DayOfWeek.Saturday ? now.AddHours(1) : now.AddSeconds(PAUSE_BEFORE_NEXT_ATTEMPT_IN_SECONDS);
                    }
                    break;
                case EState.Working:
                    if (oldState == EState.Starting_CheckCredentials && clInfoToCheck != null)
                    {
                        clInfoToCheck.Save();
                        clInfoToCheck = null;
                    }
                    _timeout = argTimeout ?? DateTime.UtcNow.AddSeconds(CHECK_FREQUENCY_TWSISWORKING_IN_SECONDS);
                    break;
                default:
                    _timeout = DateTime.MinValue;
                    usesTimeout = false;
                    break;
            }
            if (usesTimeout)
                _log.WriteLine(string.Format("SetState {0}, nextTimePoint = {1}", newState, _timeout.ToString("yyyyMMdd-HH:mm:ss")));
            else
                _log.WriteLine("SetState " + newState);
        }

        private IBClientInfo clInfoToCheck;
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (CloseMeIfDuplicatedApp()) return;

            _log = new Logger();

            if (!InitSchedule())
            {
                _log.WriteLine("Activator not started via invalid activator schedule");
                return;
            }
            _log.WriteLine("---------- Activator started ----------");

            clientInfo = IBClientInfo.Load();
            if (clientInfo == null || !clientInfo.IsValid())
            {
                clientInfo = SetupForm.Call(clientInfo);
                if (clientInfo == null)
                {
                    _log.WriteLine("Exit. Credentials is not specified");
                    Close();
                    return;
                }

                clInfoToCheck = clientInfo;
                SetState(EState.Starting_CheckCredentials);
                StartTWS();
            }
            else
                SetState(EState.NotStarted);


            FormClosing += Form1_FormClosing;


            OnTimer();
            timer1.Interval = 1000;
            timer1.Start();

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                StopTWS();
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_log != null)
            {
                _log.WriteLine("---------- End of work ----------");
                _log.Dispose();
                _log = null;
            }
        }


        private bool CloseMeIfDuplicatedApp()
        {
            List<Process> activatorDuplicates = Process.GetProcessesByName("tws_activator").ToList();
            if (activatorDuplicates.Count > 1)
            {
                string firstCalledApp = activatorDuplicates.Min(p => p.MainWindowTitle);
                if (firstCalledApp != Text)
                {
                    Close();
                    return true;
                }
            }
            return false;
        }


        private bool InitSchedule()
        {
            string schedulefname = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "",
                "TWS_Activator.schedule.xml");

            TWSActivatorScheduleInfo twsActivatorSchedule;
            if (!File.Exists(schedulefname))
            {
                twsActivatorSchedule = new TWSActivatorScheduleInfo
                {
                    Begin = new TimeOfWeek("UTC", DayOfWeek.Sunday, 0, 0),
                    End = new TimeOfWeek("UTC", DayOfWeek.Friday, 23, 59, 30),
                    RestartTime = new TimeOfDay("UTC", 2, 11, 50),
                    RestartDurationInSeconds = 20
                };
                Serializer<TWSActivatorScheduleInfo>.Save(twsActivatorSchedule, schedulefname);
            }
            else
            {
                twsActivatorSchedule = Serializer<TWSActivatorScheduleInfo>.Open(schedulefname);
                if (twsActivatorSchedule == null || !twsActivatorSchedule.IsValid())
                {
                    var msg = $@"File '{schedulefname}' 
contains invalid TWSActivator ScheduleInfo and will be ignored

Will you continue?";
                    _log.WriteLine(msg);
                    if (DialogResult.Yes != MessageBox.Show(this, msg, $@"Warning", MessageBoxButtons.YesNo))
                        return false;
                    _activatorScheduler = new ActivatorScheduler_AlwaysWorking();
                }
            }
            _activatorScheduler = new ActivatorScheduler(twsActivatorSchedule);
            return true;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            OnTimer();
        }

        private string _twsMainTitle = "";
        private void OnTimer()
        {

            if (MustRestartTws())
                KillAllTwsProcesses();

            try
            {
                switch (_state)
                {
                    case EState.Starting_CheckCredentials:
                    case EState.Starting:
                        // don't care about TradingServer state when TWS is initializing
                        CheckStartingTWS();
                        return;
                }

                if (TradingServerStoppedTimeout(out bool isTradingServerStopped))
                {
                    // close this if trading server was stopped and not restarted during specified timeout
                    timer1.Stop();
                    _log.WriteLine("Close TWS and Exit via TradingServer is stopped");
                    StopTWS();
                    Close();
                    return;
                }

                if (isTradingServerStopped)
                {
                    UpdateTwsProcess();
                    if (_twsProcess == null)
                    {
                        timer1.Stop();
                        _log.WriteLine("Exit via TradingServer stopped event and TWS is not working");
                        StopTWS();
                        Close();
                        return;
                    }
                }

                bool isWorkingTime = _activatorScheduler.IsWorkingTime(DateTime.UtcNow);
                switch (_state)
                {
                    case EState.NotStarted:
                        if (isWorkingTime)
                        {
                            _log.WriteLine("Has to start TWS because of working time");
                            AttachOrStartTWS();
                        }
                        break;
                    case EState.PauseBeforeNextAttempt:
                        if (DateTime.UtcNow >= _timeout)
                        {
                            SetState(EState.NotStarted);
                            if (isWorkingTime)
                            {
                                _log.WriteLine("Has to start TWS because of working time");
                                AttachOrStartTWS();
                            }
                        }
                        break;
                    case EState.Working:
                        if (isWorkingTime && DateTime.UtcNow < _timeout) return; // check state not every second but with specified frequency only

                        UpdateTwsProcess();
                        if (_twsProcess == null)
                        {
                            if (isWorkingTime)
                            {
                                SetState(EState.PauseBeforeNextAttempt, DateTime.UtcNow.AddSeconds(30));
                            }
                            else
                                SetState(EState.NotStarted);
                            return;
                        }

                        if (!isWorkingTime)
                        {
                            _log.WriteLine("Has to stop TWS because of working time");
                            StopTWS();
                            return;
                        }

                        // if main window starts invalid?
                        var title = _twsProcess.GetMainWindowTitle() ?? "";
                        if (title != _twsMainTitle)
                        {
                            _twsMainTitle = title;
                            _log.WriteLine("MainWindow title changed to: " + title);
                        }

                        if (title.IndexOf("Interactive Brokers", StringComparison.OrdinalIgnoreCase) >= 0)
                            _timeout = DateTime.UtcNow.AddSeconds(CHECK_FREQUENCY_TWSISWORKING_IN_SECONDS); // all is ok, repeat check after N seconds

                        break;
                }
            }
            catch (Exception e)
            {
                _log.WriteLine("Exit via Exception " + e);
                timer1.Stop();
                Close();
            }
        }

        private void CheckStartingTWS()
        {
            UpdateTwsProcess();
            if (_twsProcess == null)
            {
                if (_state != EState.Starting_CheckCredentials)
                {
                    SetState(EState.PauseBeforeNextAttempt);
                    return;
                }

                timer1.Stop();
                MessageBox.Show(this, @"Failed to launch Interactive Brokers TWS,
TWS Activator will be stopped!", @"Attension!");
                SetState(EState.NotStarted);
                Close();
                return;
            }

            string title = _twsProcess.GetMainWindowTitle();
            if (title != _twsMainTitle)
            {
                _twsMainTitle = title;
                _log.WriteLine("MainWindow title changed to: " + title);
            }
            if (title.IndexOf("Interactive Brokers", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // launch succeeded
                SetState(EState.Working);
                return;
            }

            if (DateTime.UtcNow < _timeout) return;
            // timeout

            // Authentication is not passed for a long time
            _log.WriteLine(string.Format("On timeout for CheckStartingTWS, State={0}, MainTitle={1}", _state, title));

            if (_state == EState.Starting_CheckCredentials)
            {
                _log.WriteLine("Force stop TWS and exit TWS TWS Activator");
                timer1.Stop();
                MessageBox.Show(this, @"Failed to launch Interactive Brokers TWS,
TWS Activator will be closed!", @"Attension!");
                _twsProcess.TryKillProcess();
                SetState(EState.NotStarted);
                Close();
                return;
            }

            if (title.IndexOf("Login", StringComparison.OrdinalIgnoreCase) >= 0 ||
                title.IndexOf("Authenticating", StringComparison.OrdinalIgnoreCase) >= 0 ||
                DateTime.UtcNow >= _timeout.AddMinutes(5)) // if nothing changed during a very long time
            {
                _log.WriteLine("Force kill TWS processs and restart TWS");
                _twsProcess.TryKillProcess();
                SetState(EState.NotStarted);
            }

        }

        private void StartTWS()
        {
            try
            {
                if (_state != EState.Starting_CheckCredentials)
                    SetState(EState.Starting);

                var startInfo =
                    new ProcessStartInfo(clientInfo.ClientLocation,
                        $"username={clientInfo.Login} password={clientInfo.Password}")
                    {
                        WorkingDirectory = Path.GetDirectoryName(clientInfo.ClientLocation) ?? string.Empty
                    };

                _twsProcess = new Process
                {
                    StartInfo = startInfo
                };

                _twsProcess.Start();
            }
            catch (Exception e)
            {
                _log.WriteLine("Start TWS failed: " + e);
                SetState(EState.NotStarted);
                if (_state == EState.Starting_CheckCredentials)
                {
                    timer1.Stop();
                    MessageBox.Show(this, e.Message, @"Rejection");
                    Close();
                }
            }
        }

        private void StopTWS()
        {
            UpdateTwsProcess();

            try
            {
                if (_twsProcess == null) return;
                _log.WriteLine("StopTWS");
                _twsProcess.CloseMainWindow();
                Thread.Sleep(3000);// let tws process to exit by itself
                UpdateTwsProcess();
                if (_twsProcess != null)
                {
                    // if not exited then try kill process
                    _twsProcess.TryKillProcess();
                    _twsProcess = null;
                }
                SetState(EState.NotStarted);
            }
            catch (Exception exception)
            {
                _log.WriteLine("StopTWS: " + exception.Message);
            }
        }

        private void UpdateTwsProcess()
        {
            if (_twsProcess == null) return;

            try
            {
                _twsProcess = _twsProcess.HasExited ? null : Process.GetProcessById(_twsProcess.Id);
            }
            catch
            {
                _twsProcess = null;
            }

            if (_twsProcess == null)
            {
                _log.WriteLine("TWS application lost");
            }
        }

        private void AttachOrStartTWS()
        {
            _twsProcess = FindTwsProcess();
            if (_twsProcess != null)
            {
                _log.WriteLine("Attached to working TWS application");
                SetState(EState.Working);
            }
            else
                StartTWS();
        }

        private Process FindTwsProcess()
        {
            Process[] processes = Process.GetProcessesByName("tws");
            if (processes.Length == 0) return null;

            Process workingProc =
                processes.FirstOrDefault(proc =>
                    proc.GetMainWindowTitle().IndexOf("Interactive Brokers",
                        StringComparison.OrdinalIgnoreCase) >= 0);
            if (workingProc == null) return null;

            if (processes.Length > 1)
            {
                foreach (var proc in processes)
                {
                    if (proc != workingProc)
                        proc.TryKillProcess();
                }
            }
            return workingProc;
        }

        private bool TradingServerStoppedTimeout(out bool serviceIsStopped)
        {
            serviceIsStopped = GetTradingServiceStatus() == ServiceControllerStatus.Stopped;
            if (!serviceIsStopped)
            {
                _serverStoppedTimeout = DateTime.MaxValue;
                return false;
            }

            if (_serverStoppedTimeout == DateTime.MaxValue)
            {
                _serverStoppedTimeout = DateTime.UtcNow.AddMinutes(DEACTIVATION_TIMEOUT_IN_MINUTES);
                return false;
            }

            return DateTime.UtcNow >= _serverStoppedTimeout;
        }

        private const string TradingServiceName = "TradingService";
        private static ServiceControllerStatus? GetTradingServiceStatus()
        {
            try
            {
                //#if DEBUG
                //                return ServiceControllerStatus.Running;
                //#else
                //#error Remove the forcible result
                //#endif

                using var sc = new ServiceController(TradingServiceName);
                return sc.Status;
            }
            catch
            {
                return null;
            }
        }

        private string _restartFileName;
        private bool MustRestartTws()
        {
            if (_restartFileName == null)
                _restartFileName = Assembly.GetAssembly(typeof(Form1))?.Location + ".RESTART";

            try
            {
                if (!File.Exists(_restartFileName)) return false;
                var info = File.ReadAllText(_restartFileName);
                _log.WriteLine("Restart TWS request detected from TS: " + info);

                return true;
            }
            catch (Exception excp)
            {
                _log.WriteLine("Restart TWS request detection failed: " + excp);
                return true;
            }
            finally
            {
                PathEx.TryDeleteFile(_restartFileName);
            }
        }

        private void KillAllTwsProcesses()
        {
            _log.WriteLine("KillAllTwsProcesses");
            Process[] twsProcesses = Process.GetProcessesByName("tws");
            if (twsProcesses.Length == 0)
            {
                _log.WriteLine("KillAllTwsProcesses ignored, no tws processes found");
            }
            foreach (var p in twsProcesses)
            {
                try
                {
                    _log.WriteLine("kill tws process " + p.Id);
                    p.Kill();
                }
                catch (Exception excp)
                {
                    _log.WriteLine(string.Format("Failed to kill tws process {0}: {1}", p.Id, excp));
                }
            }

            _log.WriteLine("KillAllTwsProcesses done");
        }

    }
}
