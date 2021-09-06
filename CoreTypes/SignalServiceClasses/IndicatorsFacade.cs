using System;
using System.Collections.Generic;
using System.Linq;
using Binosoft.TraderLib.Indicators;
using Indicators.Common;

namespace CoreTypes.SignalServiceClasses
{
    public class IndicatorsFacade
    {
        private readonly DataStorage _dataStorage;
        private readonly CommonIndicatorsContainer _indicatorsContainer;

        public IndicatorsFacade(TradingConfiguration cfg)
        {
            _dataStorage = new DataStorage(cfg);
            _indicatorsContainer = new CommonIndicatorsContainer(_dataStorage);

        }
        private int _curMinute = -1;

        public bool ProcessCurrentState(DateTime currentTime, List<(string, int, double)> newBpvMms,
            List<Tuple<Bar, string, string>> barValues)
        {
            if (newBpvMms != null)
                foreach (var mxBpvMM in newBpvMms)
                    _dataStorage.UpdateSettings(currentTime, mxBpvMM.Item1, mxBpvMM.Item2, mxBpvMM.Item3);

            if (barValues?.Count > 0)
                _dataStorage.AddMinuteBars(barValues);

            bool endOfMinute = false;
            if (currentTime.Minute == _curMinute)
            {
                _curMinute = currentTime.Minute;
                _dataStorage.ProcessTime(currentTime);
                endOfMinute = true;
            }

            if (newBpvMms?.Count > 0 || barValues?.Count > 0 || endOfMinute)
            {
                _indicatorsContainer.RefreshIndicators(currentTime);
                return true;
            }
            return false;
        }


        public string CreateIndicators(string instrument, string timeframe, IEnumerable<string> indicatorExpressions,
            out List<Indicator> indicators)
        {
            return _indicatorsContainer.CreateIndicators(instrument, timeframe, indicatorExpressions.ToList(), out indicators);
        }
    }
}