using System;
using System.Collections.Generic;
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

        public IBBrokerFacade(IBCredentials credentials)
        {
            _engine = new IBEngine(credentials);
            _state = EStates.Inactive;
        }

        public void Stop()
        {
            if (_engine.IsStarted)
                _engine.Stop();
            _engine.TwsActivator.Stop();

            SetState(EStates.Inactive);
            _attempt = 0;
        }

        public StateObject GetState(DateTime currentUtc)
        {
            SecondPulse();

            if (_state == EStates.EngineStarted)
                return _engine.GetState(currentUtc);
            return null; // todo to get out and process collected messages even engine is not started
        }
        public bool PlaceRequest(List<(string, string)> contractCodesAndExchanges,
            List<MarketOrderDescription> orders)
        {
            if (_state != EStates.EngineStarted) return false;
            _engine.PlaceRequest(contractCodesAndExchanges, orders);
            return true;
        }

        private void SecondPulse()
        {
            _engine.SecondPulse();
            switch (_state)
            {
                case EStates.Inactive:
                    _engine.TwsActivator.Start();
                    SetState(EStates.PauseBeforeStartEngine, 5); // let TWS to complete the initial loading
                    return;// null;

                case EStates.PauseBeforeStartEngine:
                    if (--_counter <= 0)
                        TryStartEngine();
                    return;// null;

                case EStates.EngineStarted:
                    if (_engine.IsStarted)
                        return;

                    // connection lost detected
                    if ((DateTime.UtcNow - _stateAssignedTime).TotalMinutes > 5)
                        _attempt = 0; // reset attempts counter after continuous established connection
                    NextAttempt();
                    return;
            }
        }
        private void TryStartEngine()
        {
            if (!_engine.TwsActivator.IsReady)
                return;

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