using System;

namespace CoreTypes
{
    public interface IValueTracker<T, out U> 
        where T:struct 
        where U:Enum
    {
        void ChangeValueBy(T amount);
        void SetNewValue(T newValue);
        void ChangeExternalTrackingValueBy(T amount);
        void SetExternalTrackingValue(T newExternalTrackingValue);
        T ValueForCurrentPeriod { get; }
        T TotalValue { get; }
        void CalculateState();
        void ResetState();
        void StartNewTrackingPeriod();
        U State { get; }
        bool AutoReset { get; set; }
    }

    public enum WorkingState
    {
        Running=0,
        Stopped=1
    }

    public class ErrorTracker : IValueTracker<int, WorkingState>
    {
        private readonly int _maxErrorsPerDay;
        private int _levelAtForgetMoment;
        private bool _forgetErrors;
        private int _selfValue, _externalValue;

        public ErrorTracker(int maxErrorsPerDay)
        {
            _maxErrorsPerDay = maxErrorsPerDay;
            _levelAtForgetMoment = 0;
            _forgetErrors = false;
        }
        public void ChangeValueBy(int amount) => _selfValue += amount;

        [Obsolete("This method is not supported by ErrorTracker", true)]
        public void SetNewValue(int newValue) { }
        
        [Obsolete("This method is not supported by ErrorTracker", true)]
        public void ChangeExternalTrackingValueBy(int amount) { }
        public void SetExternalTrackingValue(int newExternalTrackingValue) => _externalValue = newExternalTrackingValue;
        public int TotalValue => _selfValue + _externalValue;
        public int ValueForCurrentPeriod => TotalValue - _levelAtForgetMoment;
        public void CalculateState()
        {
            if (State == WorkingState.Running && ValueForCurrentPeriod >= _maxErrorsPerDay) State = WorkingState.Stopped;
            if (_forgetErrors)
            {
                _forgetErrors = false;
                _levelAtForgetMoment = TotalValue;
                State = WorkingState.Running;
            }
        }
        public void ResetState()
        {
            _forgetErrors = true;
        }
        public void StartNewTrackingPeriod()
        {
            _levelAtForgetMoment = TotalValue;
            if(AutoReset) State = WorkingState.Running;
        }
        public WorkingState State { get; private set; } = WorkingState.Running;
        public bool AutoReset { get; set; } = false;
    }

    public class CriticalLossManager : IValueTracker<decimal, WorkingState>
    {
        private readonly decimal _criticalLoss;
        private decimal _levelAtForgetMoment;
        private decimal _selfValue, _externalValue;

        public CriticalLossManager(decimal criticalLoss = decimal.MinValue)
        {
            _criticalLoss = criticalLoss;
            _levelAtForgetMoment = 0;
        }

        [Obsolete("This method is not supported by CriticalLossManager", true)]
        public void ChangeValueBy(decimal amount) { }
        public void SetNewValue(decimal newValue)
        {
            _selfValue = newValue;
        }

        [Obsolete("This method is not supported by CriticalLossManager", true)]
        public void ChangeExternalTrackingValueBy(decimal amount) { }
        public void SetExternalTrackingValue(decimal newExternalTrackingValue)
        {
            _externalValue = newExternalTrackingValue;
        }
        public decimal TotalValue => _selfValue + _externalValue;
        public decimal ValueForCurrentPeriod => TotalValue - _levelAtForgetMoment;
        public void CalculateState()
        {
            if (State == WorkingState.Running && ValueForCurrentPeriod <= _criticalLoss) State = WorkingState.Stopped;
        }

        [Obsolete("This method is not supported by CriticalLossManager", true)]
        public void ResetState() { }
        public void StartNewTrackingPeriod()
        {
            _levelAtForgetMoment = TotalValue;
            if (AutoReset) State = WorkingState.Running;
        }
        public WorkingState State { get; private set; } = WorkingState.Running;
        public bool AutoReset { get; set; } = true;
    }
}
