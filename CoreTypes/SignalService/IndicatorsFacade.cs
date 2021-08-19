using System;
using System.Collections.Generic;
using System.Linq;
using Binosoft.TraderLib.Indicators;
using Indicators.Common;

namespace CoreTypes
{
    public class InstrumentInfo
    {
        public string MktcodeExchange;
        public double MinMove;
        public double BigPointValue;
    }
    public class IndicatorsFacade
    {
        private readonly DataStorage _dataStorage;
        private readonly CommonIndicatorsContainer _indicatorsContainer;

        public IndicatorsFacade(List<InstrumentInfo> instrumInfos)
        {
            _dataStorage = new DataStorage(instrumInfos);
            _indicatorsContainer = new CommonIndicatorsContainer(_dataStorage);

        }
        public void ProcessMinuteBars(DateTime currentTime, List<Tuple<string, Bar, bool>> barValues)
        {
            _dataStorage.AddMinuteBars(currentTime, barValues);
            _indicatorsContainer.RefreshIndicators(currentTime);
        }

        public void UpdateSettings(DateTime currentTime, string mktcodeExchange, double minMove, double bpv)
        {
            _dataStorage.UpdateSettings(currentTime, mktcodeExchange, minMove, bpv);
        }

        public string CreateIndicators(string instrument, string timeframe, IEnumerable<string> indicatorExpressions,
            out List<Indicator> indicators)
        {
            return _indicatorsContainer.CreateIndicators(instrument, timeframe, indicatorExpressions.ToList(), out indicators);
        }
    }
}