using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreTypes
{
    public class ErrorCollector
    {
        public int MaxErrorsPerDay { get; }
        private int _errors;

        public ErrorCollector(int maxErrorsPerDay)
        {
            if (maxErrorsPerDay < 0) maxErrorsPerDay = 0;
            MaxErrorsPerDay = maxErrorsPerDay;
            _errors = 0;
        }

        public void ApplyErrors(int errorNbr)
        {
            _errors += errorNbr;
            if (_errors > MaxErrorsPerDay) IsStopped = true;
        }

        public bool IsStopped { get; private set; }
        public void Reset()
        {
            _errors = 0;
            IsStopped = false;
        }
        public void StartNewDay()
        {
            _errors = 0;
        }
    }
}
