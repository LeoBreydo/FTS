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

    public enum DataStorageState
    {
        NoConnection,
        WaitForHistory,
        Working
    };

    public class DataStorage : IDataProvider,IDataProvider2
    {
        private const int MaxIgnoredGapSizeInSeconds = 60;
        private const int WaitHistoryTimeoutInSeconds = 60;
        private readonly Dictionary<string, InstrumentData> _instruments;  // todo should be optimized: casting of the instrument name to upper case should be in the BrokerFacade only but not here

        public DataStorageState State { get; private set; }
        private DateTime _stateStartedTime;

        public DataStorage(TradingConfiguration cfg)
        {
            VerifyInstrumInfos(cfg);

            _instruments = cfg.Exchanges.SelectMany(x => x.Markets).ToDictionary(
                item=>item.MCX(),
                item => new InstrumentData(item));

            State = DataStorageState.NoConnection;
            _stateStartedTime = DateTime.MinValue;
        }

        private void SetState(DataStorageState state, DateTime now)
        {
            if (State == state) return; // formal check, should never fire
            State = state;
            _stateStartedTime = now;

            bool workingMode = state == DataStorageState.Working;
            foreach (var instrum in _instruments.Values)
                instrum.WorkingMode = workingMode;
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
                throw new Exception("Bid,Ask and Middle BarFormingPolicies not supported in this version, the next timeframe prefix was detected: "+ timeframeWithoutPrefix); // todo WARNING! What should we do if requested timeframe from Bid,Ask or Middles? : ignore, warning, exception or must do the support?
            }

            return instrum.GetTimeGridTimeFrame(timeframe.GetTimeGridSizeInMinutes());
        }

        public void UpdateSettings(DateTime currentTime, string mktcodeExchange, int bpv, double minMove)
        {
            if (_instruments.TryGetValue(mktcodeExchange.ToUpper(), out var instrument))
                instrument.UpdateSettings(currentTime, bpv, minMove);
        }

        public void AddHistoricalBars(List<(string mktExch, string contrCode, List<Bar> historicalBars)> historicalBars)
        {
            foreach (var instrum_cc_history in historicalBars)
            {
                if (!_instruments.TryGetValue(instrum_cc_history.Item1.ToUpper(), out var instrument))
                {
                    if (DebugLog.IsWorking)
                        DebugLog.AddMsg($"Ignored historical data for unknown instrument {instrum_cc_history.Item1}");

                    continue;
                }

                instrument.AddHistoricalBars(instrum_cc_history.contrCode, instrum_cc_history.historicalBars);
            }
        }

        public void AddMinuteBars(List<Tuple<Bar, string, string>> newBars)
        {
            foreach (Tuple<Bar, string, string> bar_instrum_cc in newBars)
            {
                if (!_instruments.TryGetValue(bar_instrum_cc.Item2.ToUpper(), out var instrument)) 
                {
                    if (DebugLog.IsWorking)
                        DebugLog.AddMsg($"Ignored minute bar for unknown instrument {bar_instrum_cc.Item2}");

                    continue; 
                }
                instrument.AddMinuteBar(bar_instrum_cc.Item3, bar_instrum_cc.Item1);
            }
        }


        public void ProcessTime(DateTime currentTime, bool isConnectionEstablished)
        {
            switch (State)
            {
                case DataStorageState.NoConnection:
                    if (isConnectionEstablished)
                    {

                        SetState((currentTime - _stateStartedTime).TotalSeconds <= MaxIgnoredGapSizeInSeconds
                            ? DataStorageState.Working
                            : DataStorageState.WaitForHistory, currentTime);
                    }
                    break;
                case DataStorageState.WaitForHistory:
                    if (!isConnectionEstablished)
                        SetState(DataStorageState.NoConnection, currentTime);
                    else if (_instruments.Values.All(item => item.WorkingMode) ||
                             (currentTime - _stateStartedTime).TotalSeconds >= WaitHistoryTimeoutInSeconds) 
                        SetState(DataStorageState.Working, currentTime); // toggle to working mode if load of history is done or  if timeout of the history wait
                    break;
                case DataStorageState.Working:
                    if (!isConnectionEstablished)
                        SetState(DataStorageState.NoConnection, currentTime);

                    break;
            }

            if (State== DataStorageState.Working)
                foreach (var instrum in _instruments.Values)
                    instrum.ProcessTime(currentTime);
        }


    }
}
