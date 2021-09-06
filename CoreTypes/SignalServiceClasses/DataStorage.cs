using System;
using System.Collections.Generic;
using System.Linq;
using Indicators.Common;
using PluginsInterfaces;

namespace CoreTypes.SignalServiceClasses
{
    public static class MarketConfigurationEx
    {
        public static string MCX(this MarketConfiguration mkt) => (mkt.MarketName + mkt.Exchange).ToUpper();
    }
    public class DataStorage : IDataProvider,IDataProvider2
    {
        private readonly bool _ignoreBarFormingPolicies;
        private readonly Dictionary<string, InstrumentData> _instruments;  // key is instrument name in lower case
        public DataStorage(TradingConfiguration cfg, bool ignoreBarFormingPolicies=false)
        {
            VerifyInstrumInfos(cfg);
            _ignoreBarFormingPolicies = ignoreBarFormingPolicies;

            _instruments = cfg.Exchanges.SelectMany(x => x.Markets).ToDictionary(
                item=>item.MCX(),
                item => new InstrumentData(item));
        }

        private static void VerifyInstrumInfos(TradingConfiguration cfg)
        {
            var usedNames = new HashSet<string>();
            foreach (var mkt in cfg.Exchanges.SelectMany(x => x.Markets))
            {
                if (string.IsNullOrWhiteSpace(mkt.Exchange))
                    throw new Exception("Invalid ExchangeConfiguration, ExchangeName is not defined");
                if (string.IsNullOrEmpty(mkt.MarketName))
                    throw new Exception("Invalid MarketConfiguration, MarketName is not defined");

                string mktcodeExchange = mkt.MCX();
                if (usedNames.Contains(mktcodeExchange))
                    throw new Exception("Invalid MarketConfiguration, MarketName duplication for " + mktcodeExchange);
                usedNames.Add(mktcodeExchange);

                if (mkt.MinMove < 0)
                    throw new Exception($"Instrument {mktcodeExchange} has invalid MinMove {mkt.MinMove}, value must be > 0 ");
                if (mkt.BigPointValue < 0)
                    throw new Exception($"Instrument {mktcodeExchange} has invalid BigPointValue {mkt.BigPointValue} , value must be > 0 ");
            }
        }
        public bool ExistsInstrument(string instrumentName)
        {
            return _instruments.ContainsKey(instrumentName.ToUpper());
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
            switch (propertyName.ToUpper())
            {
                case "MINMOVE":
                    if (_instruments.TryGetValue(instrumentName.ToUpper(), out instrum))
                    {
                        data = instrum.MinmoveHolder;
                        propertyIndex = 0;
                        return true;
                    }
                    break;
                case "BPV":
                    if (_instruments.TryGetValue(instrumentName.ToUpper(), out instrum))
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
            if (!_instruments.TryGetValue(instrumentName.ToUpper(), out var instrum)) return null;

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

        public void UpdateSettings(DateTime currentTime, string mktcodeExchange, int bpv, double minMove)
        {
            if (_instruments.TryGetValue(mktcodeExchange.ToUpper(), out var instrument))
                instrument.UpdateSettings(currentTime, bpv, minMove);
        }

        public void AddMinuteBars(List<Tuple<Bar, string, string>> newBars)
        {
            if (DebugLog.IsWorking)
            {
                DebugLog.AddMsg("AddMinuteBars " +
                                string.Join("|", newBars.Select(t => string.Format("({0},{1},{2},{3},{4},{5},{6})",
                                    t.Item2,
                                    t.Item3,
                                    t.Item1.O,
                                    t.Item1.H,
                                    t.Item1.L,
                                    t.Item1.C,
                                    t.Item1.End.ToString("yyyyMMdd-HHmmss")
                                ))));
            }

            foreach (Tuple<Bar, string, string> tuple in newBars)
            {
                if (!_instruments.TryGetValue(tuple.Item2.ToUpper(), out var instrument)) 
                {
                    if (DebugLog.IsWorking)
                        DebugLog.AddMsg($"Ignored minute bar for unknown instrument {tuple.Item2} {tuple.Item1.End.ToString("yyyyMMdd-HHmmss")} ");

                    continue; 

                }
                instrument.AddMinuteBar(tuple.Item1, tuple.Item3);
            }
        }

        public void ProcessTime(DateTime currentTime)
        {
            const int maxPossibleDataDelayInSeconds = 60;
            var reachedTime = currentTime.AddSeconds(-maxPossibleDataDelayInSeconds);
            foreach (var instruments in _instruments.Values)
                instruments.ProcessTime(reachedTime);
        }

    }
}
