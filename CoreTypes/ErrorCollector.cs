namespace CoreTypes
{
    public class ErrorCollector
    {
        public int MaxErrorsPerDay { get; }
        private int _levelAtForgetMoment;
        public bool ForgetErrors;

        public ErrorCollector(int maxErrorsPerDay)
        {
            if (maxErrorsPerDay < 0) maxErrorsPerDay = 0;
            MaxErrorsPerDay = maxErrorsPerDay;
            Errors = 0;
            _levelAtForgetMoment = 0;
        }

        public void SetErrorsAndEvaluateState(int allErrors)
        {
            Errors = allErrors;
            if (ForgetErrors)
            {
                ForgetErrors = false;
                _levelAtForgetMoment = Errors;
            }
            IsStopped = Errors - _levelAtForgetMoment > MaxErrorsPerDay;
        }

        public void AddErrors(int newErrors)
        {
            Errors += newErrors;
        }

        public bool IsStopped { get; private set; }
        public void StartNewDay()
        {
            _levelAtForgetMoment = 0;
            Errors = 0;
            ForgetErrors = false;
        }
        public int Errors { get; private set; }
    }
}
