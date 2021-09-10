using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrokerFacadeIB
{
    public class TwsActivator
    {
        private const int LIMIT_IN_SEC__FOR_MAINWNDTITLE__When_Initializing = 60;
        private const int NUM_SEC__Wait_While_ProcessLost = 60;

        private readonly string _location;
        private readonly string _login;
        private readonly string _password;

        public TwsActivator(IBCredentials credentials, Action<string, string> actionAddMessage)
        {
            if (!File.Exists(credentials.Location))
            {
                throw new ArgumentException("TWS Application not found: " + credentials.Location, nameof(credentials.Location));
            }

            _location = credentials.Location;
            _login = credentials.Login;
            _password = credentials.Password;

            InitLogout(actionAddMessage);
            DebugLog("=====================");
        }


        private Action<string, string> _actionAddMessage = (_, _) => { };
        private void InitLogout(Action<string, string> actionAddMessage)
        {
            _actionAddMessage = actionAddMessage ?? ((_, _) => { });
        }
        private void Logout(string txt)
        {
            _actionAddMessage("CLIENT", txt); // send message to client
            DebugLog(txt); // output to log file
        }

        private static readonly object _Lock_LogFile = new object();
        public static void DebugLog(string txt)
        {
#if DEBUG
            lock (_Lock_LogFile)
            {
                File.AppendAllText("Logs/TwsActivatorLogout.txt",
                    DateTime.UtcNow.ToString("yyyyMMdd-HH:mm:ss.fff ") + txt + "\r\n");
            }
#endif
        }


        public void Start()
        {
            if (!IsStarted)
                StartTask();
        }
        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _workingTask?.Wait();
        }

        public void Restart()
        {
            if (!IsStarted)
                StartTask();
            else
                KillAllTwsProcesses();

        }
        public bool IsStarted => _cancellationTokenSource != null;
        public bool IsReady => _state == State.Working;


        enum State { Inactive, Starting, Working, ProcessLost }
        private CancellationTokenSource _cancellationTokenSource;

        private volatile State _state;
        private Process _twsProcess;

        private int _counter;
        private string _lastTitle;
        private void StartTask()
        {
            SetState(State.Inactive);
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew((_) =>
            {
                try
                {
                    DebugLog("Started");
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        SecondPulse();
                        Thread.Sleep(1000);
                    }

                    if (_state == State.Starting)
                        CloseStartingApplication();
                }
                catch (Exception e)
                {
                    DebugLog("TWS Activator MainLoop failed by exception: " + e);
                }
                finally
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                    SetState(State.Inactive);
                    DebugLog("Finished");
                    _workingTask = null;
                }

            }, TaskCreationOptions.LongRunning
                , _cancellationTokenSource.Token);
        }

        private void CloseStartingApplication()
        {
            if (_twsProcess == null || _twsProcess.HasExited)
                return;

            //DebugLog(string.Format("Stop(3A): _state={0}, hasProc={1}, title={2}", _state, _twsProcess == null ? "n" : "y",
            //    _lastTitle));

            try
            {
                while ((DateTime.UtcNow- _stateStartedTime).TotalSeconds< LIMIT_IN_SEC__FOR_MAINWNDTITLE__When_Initializing)
                {
                    if (_twsProcess.HasExited) return;

                    var title = _twsProcess.GetMainWindowTitle();
                    if (title?.IndexOf("Interactive Brokers", StringComparison.OrdinalIgnoreCase) >= 0)
                        break;
                    
                    Thread.Sleep(1000);
                }
                _twsProcess.CloseMainWindow();
            }
            catch 
            {
            }
        }

        private void SecondPulse()
        {
            switch (_state)
            {
                case State.Inactive:
                    Process_InactiveState();
                    break;
                case State.Starting:
                    Process_StartingState();
                    break;
                case State.ProcessLost:
                    Process_ProcessLostState();
                    break;
                case State.Working:
                    Process_WorkingState();
                    break;
            }
        }

        private DateTime _stateStartedTime;
        private void SetState(State state)
        {
            _state = state;
            _stateStartedTime=DateTime.UtcNow;

            DebugLog("State " + state);
            switch (_state)
            {
                //case State.Inactive:
                //    break;
                //case State.Working:
                //    break;

                case State.ProcessLost:
                    Init_ProcessLostState();
                    break;

                case State.Starting:
                    Init_StartingState();
                    break;
            }
        }
        private void Process_InactiveState()
        {
            if (AttachToWorkingTws())
                SetState(State.Working);
            else
            {
                LaunchTws();
            }
        }

        private void Init_StartingState()
        {
            _counter = 0;
            _lastTitle = ""; // any impossible title
        }

        private void Process_StartingState()
        {
            UpdateTwsProcess();
            if (_twsProcess==null) return;
            
            var title = _twsProcess.GetMainWindowTitle();
            if (title != _lastTitle)
            {
                _lastTitle = title;
                DebugLog("Title changed to: "+ _lastTitle);
                _counter = 0;
                return;
            }

            ++_counter;
            if (_lastTitle.IndexOf("Interactive Brokers", StringComparison.OrdinalIgnoreCase) >= 0 && _counter >= 5)
            {
                SetState(State.Working);
                return;
            }

            if (_counter >= LIMIT_IN_SEC__FOR_MAINWNDTITLE__When_Initializing) // if initialization hangs at any stage 
            {
                _twsProcess.TryKillProcess();
                Logout(string.Format("TWS hang detected at initialization stage (title='{0}')", _lastTitle));
                SetState(State.Inactive);
            }
        }

        private void Init_ProcessLostState()
        {
            _counter = 0;
        }

        private void Process_ProcessLostState()
        {
            Process[] processes = Process.GetProcessesByName("tws");
            if (processes.Length == 1)
            {
                _twsProcess = processes[0];
                SetState(State.Starting);
                return;
            }

            ++_counter;
            if (_counter >= NUM_SEC__Wait_While_ProcessLost)
            {
                foreach (var proc in processes)
                    proc.TryKillProcess();
                LaunchTws();
            }

        }

        private void Process_WorkingState()
        {
            UpdateTwsProcess(); // if app is lost then toggle to Inactive state
        }



        private bool AttachToWorkingTws()
        {
            _twsProcess = FindSingleTwsProcess();
            if (_twsProcess != null) return true;

            KillAllTwsProcesses();
            return false;
        }
        private static Process FindSingleTwsProcess()
        {
            Process[] processes = Process.GetProcessesByName("tws");
            if (processes.Length == 0) return null;

            Process workingProc =
                processes.FirstOrDefault(proc =>
                    proc.GetMainWindowTitle().IndexOf("Interactive Brokers", StringComparison.OrdinalIgnoreCase) >= 0);
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
        private void LaunchTws()
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested) return;
                
            DebugLog("LaunchTws");
            try
            {
                var startInfo =
                    new ProcessStartInfo(_location,
                        string.Format("username={0} password={1}", _login, _password))
                    {
                        WorkingDirectory = Path.GetDirectoryName(_location)
                    };

                _twsProcess = new Process
                {
                    StartInfo = startInfo
                };

                _twsProcess.Start();
                SetState(State.Starting);
            }
            catch (Exception e)
            {
                Logout("Failed to start TWS!!! : " + e);
                SetState(State.Inactive);
                Stop();
            }
        }

        private static void KillAllTwsProcesses()
        {
            foreach (var proc in Process.GetProcessesByName("tws"))
                proc.TryKillProcess();
        }
        private void UpdateTwsProcess()
        {
            if (_twsProcess == null) return;

            try
            {
                if (_twsProcess.HasExited)
                    _twsProcess = null;
            }
            catch
            {
                _twsProcess = null;
            }

            if (_twsProcess == null)
            {
                SetState(State.ProcessLost);
            }
        }

    }
}
