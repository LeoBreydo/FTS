using System;
using System.Collections.Generic;
using System.Linq;
using Indicators.Common;
using PluginsInterfaces;

namespace CoreTypes
{
    public class DataStorage : IDataProvider,IDataProvider2
    {
        private readonly bool _ignoreBarFormingPolicies;
        private readonly Dictionary<string, InstrumentData> _instruments;  // key is instrument name in lower case
        public DataStorage(List<InstrumentInfo> instrumentInfos, bool ignoreBarFormingPolicies=false)
        {
            VerifyInstrumInfos(instrumentInfos);
            _ignoreBarFormingPolicies = ignoreBarFormingPolicies;

            _instruments = instrumentInfos.ToDictionary(
                item => item.MktcodeExchange.ToLower(),
                item => new InstrumentData(item));
        }

        private static void VerifyInstrumInfos(List<InstrumentInfo> instrumentInfos)
        {
            var usedNames = new HashSet<string>();
            foreach (var item in instrumentInfos)
            {
                if (string.IsNullOrEmpty(item.MktcodeExchange))
                    throw new Exception("Invalid item, MktcodeExchange is not specified");

                var lwr = item.MktcodeExchange.ToLower();
                if (usedNames.Contains(lwr))
                    throw new Exception("Duplicated instrument description for " + item.MktcodeExchange);

                if (item.MinMove < 0)
                    throw new Exception($"Instrument {item.MinMove} has invalid MinMove, value must be > 0 ");
                if (item.BigPointValue < 0)
                    throw new Exception($"Instrument {item.BigPointValue} has invalid BigPointValue, value must be > 0 ");
            }
        }
        public bool ExistsInstrument(string instrumentName)
        {
            return _instruments.ContainsKey(instrumentName.ToLower());
        }

        public bool GetInstrumentConstant(string instrumentName, string constantName, out double value)
        {
            value = 0;
            return false;
        }

        public bool GetInstrumentProperty(string instrumentName, string propertyName, out TimeFrameData data,
            out int propertyIndex)
        {
            InstrumentData instrum;
            switch (propertyName.ToLower())
            {
                case "minmove":
                    if (_instruments.TryGetValue(instrumentName.ToLower(), out instrum))
                    {
                        data = instrum.MinmoveHolder;
                        propertyIndex = 0;
                        return true;
                    }
                    break;
                case "bpv":
                    if (_instruments.TryGetValue(instrumentName.ToLower(), out instrum))
                    {
                        data = instrum.BpvHolder;
                        propertyIndex = 0;
                        return true;
                    }
                    break;
            }

            data = null;
            propertyIndex = -1;
            return false;

        }
        public TimeFrameData GetAggregatedTimeFrame(string instrumentName, string timeframe)
        {
            return null; // all aggregators works inside the indicator machine, additional logic like RenkoBars is not used
        }

        public TimeFrameData GetTimeGridTimeFrame(string instrumentName, string timeframe)
        {
            if (!_instruments.TryGetValue(instrumentName.ToLower(), out var instrum)) return null;

            if (timeframe.SeparatePrefixFromTimeFrameExpression(out BarFormingPolicy bfPolicy,
                out string timeframeWithoutPrefix))
            {
                if (_ignoreBarFormingPolicies)
                    timeframe = timeframeWithoutPrefix;
                else
                    throw new Exception("Bid,Ask and Middle BarFormingPolicies not supported in this version"); // todo WARNING! What should we do if requested timeframe from Bid,Ask or Middles? : ignore, warning, exception or must do the support?
            }

            return instrum.GetTimeGridTimeFrame(timeframe.GetTimeGridSizeInMinutes());
        }

        //public void ProcessTime(DateTime time)
        //{
        //    foreach (var instruments in _instruments.Values)
        //        instruments.ProcessTime(time);
        //}

        public void AddMinuteBars(DateTime currentTime, List<Tuple<string,Bar,bool>> newBars)
        {
            foreach (Tuple<string, Bar, bool> instrum_bar_newContractStarted in newBars)
            {
                if (!_instruments.TryGetValue(instrum_bar_newContractStarted.Item1.ToLower(), out var instrument)) 
                    continue; // todo ? might be to logout msg about the not-used instrument?
                if (instrum_bar_newContractStarted.Item3)
                    instrument.StartNewContract();
                instrument.AddMinuteBar(instrum_bar_newContractStarted.Item2);
            }

            foreach (var instruments in _instruments.Values)
                instruments.ProcessTime(currentTime);
        }

        public void UpdateSettings(DateTime currentTime, string mktcodeExchange, double minMove, double bpv)
        {
            if (_instruments.TryGetValue(mktcodeExchange.ToLower(), out var instrument))
                instrument.UpdateSettings(currentTime, minMove, bpv);

        }
    }
}
