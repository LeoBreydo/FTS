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


        private int _curMinute = -1;

        public IndicatorsFacade(TradingConfiguration cfg)
        {
            _dataStorage = new DataStorage(cfg);
            _indicatorsContainer = new CommonIndicatorsContainer(_dataStorage);
        }

        public bool ProcessCurrentState(StateObject so, InfoCollector ic)
        {
            DateTime currentTime = GetBeginOfSecond(so.CurrentUtcTime);

            List<(string instrum, int bpv, double mm)> newBpvMms = ic.NewBpvMms;
            if (newBpvMms.Count>0)
                foreach (var mxBpvMM in newBpvMms)
                    _dataStorage.UpdateSettings(currentTime, mxBpvMM.Item1, mxBpvMM.Item2, mxBpvMM.Item3);

            List<(string mktExch, string contrCode, List<Bar> historicalBars)> historicalBars = so.HistoricalData;
            if (historicalBars.Count>0)
                _dataStorage.AddHistoricalBars(historicalBars);

            var barValues = ic.BarsInfo;
            if (ic.BarsInfo.Count > 0)
                _dataStorage.AddMinuteBars(ic.BarsInfo);


            _dataStorage.ProcessTime(currentTime, so.IsConnectionEstablished);

            // in current version we update indicator values at end of minute when in working state
            bool endOfMinute = false;
            if (currentTime.Minute != _curMinute)
            {
                _curMinute = currentTime.Minute;
                endOfMinute = true;
            }


            if (endOfMinute && _dataStorage.State== DataStorageState.Working)
            {
                _indicatorsContainer.RefreshIndicators(currentTime);
                return true;
            }
            return false;
        }
        private static DateTime GetBeginOfSecond(DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, t.Kind);
        }


        public string CreateIndicators(string instrument, string timeframe, IEnumerable<string> indicatorExpressions,
            out List<Indicator> indicators)
        {
            return _indicatorsContainer.CreateIndicators(instrument, timeframe, indicatorExpressions.ToList(), out indicators);
        }
    }
}