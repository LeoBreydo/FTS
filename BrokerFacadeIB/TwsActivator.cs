#if disabled // unable to launch TWS application from windows service
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BrokerInterfaces;
using Messages;
using ProductInterfaces;

namespace BrokerFacadeIB
{
    /// <summary>
    /// Warning! Do not use this class with trading service called as Windows Service, WindowsService is not able to start properly the GUI applications! 
    /// Instead of that please use  class IBEngineActivator in join with standalone utility Tws_Activator (this solution/UtilApps/Activator).
    /// This utility is started from HostServiceManager together with TS and stopped from Tws_Activator itself by timeout when TS is switched off.
    /// </summary>
    public class TwsActivator : ISecondPulseRoutine
    {
        private readonly string _app;
        private readonly string _login;
        private readonly string _password;
        private readonly IIBEngine _engine;
        private readonly IMsgChannel _logChannel;

        enum EStates
        {
            Inactive, TWSStarting, TWSStarted, RestartTws, EngineStarted, RestartEngine
        }

        private EStates _state;
        private long _counter;

        private Process _twsProcess;
        private bool IsTwsExited()
        {
            if (_twsProcess == null) return true;
            if (_twsProcess.HasExited)
            {
                _twsProcess = null;
                return true;
            }

            return false;
        }

        public TwsActivator(string pathToApplication,string login,string password,IIBEngine engine, IBrokerFacadeChannels brokerFacadeChannels)
        {
            _app = pathToApplication;
            _login = login;
            _password = password;
            _engine = engine;
            _logChannel = brokerFacadeChannels.LogChannel;

            if (!File.Exists(_app))
                throw new Exception("Invalid path to application 'Trader Workstation'");

            _state = EStates.Inactive;
        }

        public void Start() { }
        public void Stop()
        {
            if (_engine.IsStarted)
                _engine.Stop();

            if (_twsProcess != null && !_twsProcess.HasExited)
            {
                _twsProcess.CloseMainWindow();
                _twsProcess = null;
            }
            SetState(EStates.Inactive);
        }


        public void Call()
        {
            switch (_state)
            {
                case EStates.Inactive:
                    FindOrStartTWS(true);
                    break;

                case EStates.TWSStarting:
                    if (--_counter <= 0)
                        SetState(IsTwsExited() ? EStates.RestartTws : EStates.TWSStarted);
                    break;

                case EStates.RestartTws:
                    if (--_counter <= 0)
                        FindOrStartTWS(false);
                    break;

                case EStates.TWSStarted:
                    if (IsTwsExited())
                        SetState(EStates.RestartTws);
                    else
                    {
                        if (_engine.Start())
                            SetState(EStates.EngineStarted);
                        else
                            Stop();
                    }
                    break;

                case EStates.EngineStarted:
                    if (_engine.IsStarted)
                        _engine.SecondPulse();
                    else 
                        OnEngineStopped();
                    break;

                case EStates.RestartEngine:
                    if (IsTwsExited())
                        SetState(EStates.RestartTws);
                    else if (--_counter <= 0)
                    {
                        if (_engine.Start())
                            SetState(EStates.EngineStarted);
                        else
                            SetState(EStates.RestartEngine);
                    }
                    break;

               
            }
        }
        private void OnEngineStopped()
        {
            if (IsTwsExited())
                SetState(EStates.RestartTws);
            else
                SetState(EStates.RestartEngine);
        }
        private void SetState(EStates newState)
        {
            _state = newState;
            _logChannel.Publish(new TextMessage(TextMessageTypes.Info, "TwsActivator: state = "+ _state));
            switch (newState)
            {
                case EStates.TWSStarting:
                    _counter = 45; // nbr of second to let application to complete instantiation
                    break;
                case EStates.RestartTws:
                    _counter = 60; // nbr of second before restart TWS
                    break;
                case EStates.RestartEngine:
                    _counter = 60; // nbr of second before restart engine
                    break;
                default:
                    _counter = 0;
                    break;
            }
        }
        private void FindOrStartTWS(bool fromInactiveState)
        {
            Process proc = Process.GetProcessesByName("tws").FirstOrDefault();
            if (proc == null || proc.HasExited)
            {
                _logChannel.Publish(new TextMessage(TextMessageTypes.Debug, "TWS: Starting"));
                StartTws(fromInactiveState);
            }
            else
            {
                _logChannel.Publish(new TextMessage(TextMessageTypes.Debug,
                    "TWS: working app detected: " + proc.MainWindowTitle));
                _twsProcess = proc;
                SetState(EStates.TWSStarted);
            }
        }

        private void StartTws(bool fromInactiveState)
        {
            try
            {
                var startInfo =
                    new ProcessStartInfo(_app, string.Format("username={0} password={1}", _login, _password))
                    {
                        WorkingDirectory = Path.GetDirectoryName(_app)
                    };
                _twsProcess = new Process
                {
                    StartInfo = startInfo
                };
                ;
                if (!_twsProcess.Start())
                {
                    _logChannel.Publish(new TextMessage(TextMessageTypes.ALARM,
                        "Cannot start Trader Workstation"));
                    Stop();
                }


        }
            catch (Exception e)
            {
                _logChannel.Publish(new TextMessage(TextMessageTypes.ALARM,
                    "Failed to start Trader Workstation, exception: " + e));
                if (fromInactiveState)
                    Stop();
                else
                    SetState(EStates.RestartTws);
            }
            SetState(EStates.TWSStarting);
        }
    }
}
#endif