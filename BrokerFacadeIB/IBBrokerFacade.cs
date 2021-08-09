using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreTypes;

namespace BrokerFacadeIB
{
    public class IBBrokerFacade
    {
        private readonly IBEngine _engine;
        enum EStates
        {
            Inactive,
            PauseBeforeStartEngine,
            EngineStarted
        }

        private EStates _state;
        private long _counter;
        private DateTime _stateAssignedTime;
        private int _attempt;

        public IBBrokerFacade(IBEngine engine)
        {
            _engine = engine;
            _state = EStates.Inactive;
        }

        public void Stop()
        {
            if (_engine.IsStarted)
                _engine.Stop();
            SetState(EStates.Inactive);
            _attempt = 0;
        }
        public StateObject GetState(DateTime currentUtc)
        {
            _engine.SecondPulse();
            switch (_state)
            {
                case EStates.Inactive:
                    SetState(EStates.PauseBeforeStartEngine, 60); // let TWS to complete the initial loading
                    return null;

                case EStates.PauseBeforeStartEngine:
                    if (--_counter <= 0)
                        StartEngine();
                    return null;

                case EStates.EngineStarted:
                    if (_engine.IsStarted)
                        return _engine.GetState(currentUtc);
                    // connection lost detected
                    if ((DateTime.UtcNow - _stateAssignedTime).TotalMinutes > 5)
                        _attempt = 0; // reset attempts counter after continuous established connection
                    NextAttempt();
                    return null;
            }

            return null;
        }

        public bool PlaceRequest(List<(string, string)> contractCodesAndExchanges,
            List<MarketOrderDescription> orders)
        {
            switch (_state)
            {
                case EStates.Inactive:
                    SetState(EStates.PauseBeforeStartEngine, 60); // let TWS to complete the initial loading
                    return false;

                case EStates.PauseBeforeStartEngine:
                    if (--_counter <= 0)
                        StartEngine();
                    return false;

                case EStates.EngineStarted:
                    if (_engine.IsStarted)
                    {
                        _engine.PlaceRequest(contractCodesAndExchanges,orders);
                        return true;
                    }
                    // connection lost detected
                    if ((DateTime.UtcNow - _stateAssignedTime).TotalMinutes > 5)
                        _attempt = 0; // reset attempts counter after continuous established connection
                    NextAttempt();
                    return false;
            }

            return false;
        }

        private static bool TWSIsWorking()
        {
            return Process.GetProcessesByName("tws").FirstOrDefault() != null;
        }

        private void StartEngine()
        {
            if (!TWSIsWorking())
            {
                _engine.AddMessage("TWS", "TWS application is not started");
                //_engine.RestartTws(); // start tws from here? (console mode)
                SetState(EStates.PauseBeforeStartEngine, 60);
                return;
            }

            if (_engine.Start())
                SetState(EStates.EngineStarted);
            else
                NextAttempt();
        }

        private void SetState(EStates newState,int counter=0)
        {
            _state = newState;
            _counter = counter;
            _stateAssignedTime = DateTime.UtcNow;

            if (_state==EStates.PauseBeforeStartEngine)
                _engine.AddMessage("TWS",
                    $"IBEngineActivator: state = {_state}, counter={_counter}");
            else
                _engine.AddMessage("TWS", "IBEngineActivator: state = " + _state);
        }

        private void NextAttempt()
        {
            ++_attempt;

            int pause;
            if (_attempt < 5)
                pause = 60;
            else if (_attempt < 8)
                pause = 60 * 5;
            else
                pause = 60 * 60;

            SetState(EStates.PauseBeforeStartEngine, pause);
        }
    }
}