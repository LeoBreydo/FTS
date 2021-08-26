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
        public bool ProcessCurrentState(DateTime currentTime, List<(string, int, double)> listOf_mxBpvMM, List<Tuple<Bar, string, bool>> barValues)
        {
            foreach (var mxBpvMM in listOf_mxBpvMM)
                _dataStorage.UpdateSettings(currentTime, mxBpvMM.Item1, mxBpvMM.Item2, mxBpvMM.Item3);

            if (barValues.Count > 0)
                _dataStorage.AddMinuteBars(barValues);

            if (currentTime.Minute == _curMinute)
            {
                if (listOf_mxBpvMM.Count > 0)
                    _indicatorsContainer.RefreshIndicators(currentTime);
                return false;
            }
            // new minute started
            _curMinute = currentTime.Minute;
            _dataStorage.ProcessEndOfMinute(currentTime);
            _indicatorsContainer.RefreshIndicators(currentTime);
            return true;
        }


        public string CreateIndicators(string instrument, string timeframe, IEnumerable<string> indicatorExpressions,
            out List<Indicator> indicators)
        {
            return _indicatorsContainer.CreateIndicators(instrument, timeframe, indicatorExpressions.ToList(), out indicators);
        }
    }
}