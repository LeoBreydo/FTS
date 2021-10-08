using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Configurator.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TimeZoneConverter;

namespace Configurator
{

    public enum ScheduledIntervalState
    {
        NotStarted,
        SoftStopping,
        Stopped,
        Finished
    }
    public class ScheduledIntervalDescription
    {
        public int Id { get; set; }
        public string Info { get; set; }
        public string EnterTimeZoneName { get; set; }
        public string ExitTimeZoneName { get; set; }
        public DateTime? SoftStopTime { get; set; }
        public DateTime HardStopTime { get; set; }
        public DateTime NoRestrictionTime { get; set; }

        public DateTime? SoftStopUtc { get; set; }
        public DateTime HardStopUtc { get; set; }
        public DateTime NoRestrictionUtc { get; set; }
        public DateTime StartUtc => SoftStopUtc ?? HardStopUtc;

        public ScheduledIntervalDescription()
        {
            Id = -1;
            EnterTimeZoneName = null;
            ExitTimeZoneName = null;
            SoftStopTime = null;
            HardStopTime = DateTime.MinValue;
            NoRestrictionTime = DateTime.MinValue;
        }

        public bool EqualsTo(ScheduledIntervalDescription other)
        {
            return Id == other.Id &&
                   EnterTimeZoneName == other.EnterTimeZoneName &&
                   ExitTimeZoneName == other.ExitTimeZoneName &&
                   SoftStopTime == other.SoftStopTime &&
                   HardStopTime == other.HardStopTime &&
                   NoRestrictionTime == other.NoRestrictionTime;
        }

        public void AssignFrom(ScheduledIntervalDescription other)
        {
            Id = other.Id;
            EnterTimeZoneName = other.EnterTimeZoneName;
            ExitTimeZoneName = other.ExitTimeZoneName;
            SoftStopTime = other.SoftStopTime;
            HardStopTime = other.HardStopTime;
            NoRestrictionTime = other.NoRestrictionTime;

            SoftStopUtc = other.SoftStopUtc;
            HardStopUtc = other.HardStopUtc;
            NoRestrictionUtc = other.NoRestrictionUtc;

            Info = other.Info;
        }

        public string VerifyAndUpdateUtc(int timeStepInMinutes)
        {
            if (timeStepInMinutes < 1 || timeStepInMinutes > 30)
                return "Invalid timeStepInMinutes, value must be in range [1..30]: " + timeStepInMinutes;

            if (Id < 0) return "id is not valid";
            if (HardStopTime == DateTime.MinValue) return "HardStopTime is not defined";
            if (NoRestrictionTime == DateTime.MinValue) return "NoRestrictionTime is not defined";

            // try to find TimeZones
            if (string.IsNullOrEmpty(EnterTimeZoneName)) return "Name of EnterTimeZone is undefined";
            if (string.IsNullOrEmpty(ExitTimeZoneName)) return "Name of ExitTimeZone is undefined";
            var enterTzi = TZConvert.GetTimeZoneInfo(EnterTimeZoneName);
            if (enterTzi == null) return "EnterTimeZone info not found";
            var exitTzi = TZConvert.GetTimeZoneInfo(ExitTimeZoneName);
            if (exitTzi == null) return "ExitTimeZone info not found";

            // convert to utc and round to SchedulerTimeStepInMinutes

            HardStopUtc = HardStopTime.TimeToUtc(EnterTimeZoneName);//
            NoRestrictionUtc = NoRestrictionTime.TimeToUtc(ExitTimeZoneName);//
            if (HardStopUtc>=NoRestrictionUtc)
                return "HardStopTime must precede NoRestrictionTime";

            HardStopUtc= HardStopUtc.RoundDateTime(timeStepInMinutes, true);
            NoRestrictionUtc= NoRestrictionUtc.RoundDateTime(timeStepInMinutes, false);

            DateTime? softStopUtc=null;
            if (SoftStopTime != null)
            {
                if (SoftStopTime.Value >= HardStopTime)
                    return "SoftStopTime must precede HardStopTime";

                softStopUtc =
                    SoftStopTime.Value.TimeToUtc(EnterTimeZoneName).RoundDateTime(timeStepInMinutes, true);

                if (softStopUtc == HardStopUtc)
                    softStopUtc = null;
            }
            SoftStopUtc = softStopUtc;

            return null;
        }

        public ScheduledIntervalState GetState(DateTime utcNow)
        {
            if (utcNow < StartUtc) return ScheduledIntervalState.NotStarted;
            if (utcNow > NoRestrictionUtc) return ScheduledIntervalState.Finished;
            if (SoftStopUtc!=null && utcNow<HardStopUtc)
                return ScheduledIntervalState.SoftStopping;
            return ScheduledIntervalState.Stopped;
        }
    }

    public class ScheduledInterval
    {
        public int TargetId;
        public DateTime? SoftStopTime;
        public DateTime HardStopTime;
        public DateTime NoRestrictionTime;
        public ScheduledInterval()
        {
        }
        public ScheduledInterval(int targetId,DateTime? softStopTime,DateTime hardStopTime, DateTime noRestrictionTime)
        {
            TargetId = targetId;
            SoftStopTime = softStopTime;
            HardStopTime = hardStopTime;
            NoRestrictionTime = noRestrictionTime;
        }
    }


    public class TradingConfiguration
    {
        public int Id = -1;
        public DateTime StartedAt { get; set; }
        public int NextId { get; set; }
        //public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();
        public List<ExchangeConfiguration> Exchanges { get; set; } = new List<ExchangeConfiguration>();
        public int MaxErrorsPerDay = 0;

        public int SchedulerTimeStepInMinutes = 5;
        public List<ScheduledInterval> ScheduledIntervals { get; set; } = new List<ScheduledInterval>();


        public string AddScheduledInterval(ScheduledIntervalDescription sid)
        {
            var err = sid.VerifyAndUpdateUtc(SchedulerTimeStepInMinutes);
            if (err != null) return err;

            ScheduledIntervals.Add(
                new ScheduledInterval(sid.Id, sid.SoftStopUtc, sid.HardStopUtc, sid.NoRestrictionUtc));
            return null;
        }

#if tmp
        public List<(StrategyConfiguration, List<string>)> GetAdditionalInstruments()
        {
            var ret = new List<(StrategyConfiguration, List<string>)>();
            foreach (var xcfg in Exchanges)
                foreach (var mcfg in xcfg.Markets)
                    foreach (var scfg in mcfg.Strategies)
                    {
                        var additionalInstrums = scfg.GetAdditionalInstruments(mcfg.MarketName);
                        if (additionalInstrums.Count > 0)
                            ret.Add(new(scfg, additionalInstrums));
                    }

            return ret;
        }
#endif
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(TradingConfiguration));
        public static TradingConfiguration Restore(string fileName)
        {
            try
            {
                if (!File.Exists(fileName)) return null;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return (TradingConfiguration) serializer.Deserialize(fs);
                }
            }
            catch (Exception exception)
            {
                return null;
            }

        }
        public string Save(string fileName)
        {
            try
            {
                string folder = Path.GetDirectoryName(Path.GetFullPath(fileName));
                if (folder != null && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                using (TextWriter writer = new StreamWriter(fileName))
                {
                    serializer.Serialize(writer, this);
                }

                return null;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

    }
    public class ExchangeConfiguration
    {
        public int Id = -1;
        public string Currency;// { get; set; } = "UNK";
        public string ExchangeName;// { get; set; } = "UNK";
        public int MaxErrorsPerDay = 0;
        public List<MarketConfiguration> Markets { get; set; } = new List<MarketConfiguration>();

        public string VerifyMe(List<int> IDs, List<string> exchanges, List<string> currencies)
        {
            if (Id < 0) return "Id must be non-negative";
            if (IDs.Contains(Id)) return $"Id duplication detected - {Id}";
            IDs.Add(Id);
            if (string.IsNullOrWhiteSpace(Currency) || Currency.Length != 3 || Currency == "UNK")
                return "Currency is undefined";

            //if (currencies.Contains(Currency)) return $"Currency duplication detected - {Currency}";
            //currencies.Add(Currency);
            if (!currencies.Contains(Currency))
                currencies.Add(Currency);

            if (string.IsNullOrWhiteSpace(ExchangeName) || Currency == "UNK")
                return "Name of exchange is undefined";
            if (exchanges.Contains(ExchangeName)) return $"Exchange name duplication detected - {ExchangeName}";
            exchanges.Add(ExchangeName);

            if (MaxErrorsPerDay < 0) return "MaxErrorsPerDay must be non-negative";

            return null;
        }
        public string Verify(List<int> IDs, List<string> strategyNames, List<string> exchanges, List<string> currencies, List<string> mktExs, IndicatorsVerificator indVerificator)
        {
            string err = VerifyMe(IDs,  exchanges, currencies);

            if (Markets == null || Markets.Count == 0) return "Markets are undefined";
            var idx = 0;
            foreach (var errString in Markets.Select(market => market.Verify(IDs, strategyNames, mktExs, indVerificator)))
            {
                if (errString != null) return $"Market configuration at index {idx} : {errString}";
                ++idx;
            }
            return null;
        }
    }
    public class MarketConfiguration
    {
        public int Id = -1;
        public string MarketName;// = "UNK";
        public string Exchange;// = "UNK";
        public int BigPointValue = -1;
        public double MinMove = -1;

        public decimal SessionCriticalLoss = decimal.MinValue;
        public int MaxErrorsPerDay = 0;
        public int MaxNbrContracts = 0;
        public List<StrategyConfiguration> Strategies { get; set; } = new List<StrategyConfiguration>();

        public string VerifyMe(List<int> IDs, List<string> mktExs)
        {
            if (Id < 0) return "Id must be non-negative";
            if (IDs.Contains(Id)) return $"Id duplication detected - {Id}";
            IDs.Add(Id);
            if (string.IsNullOrWhiteSpace(MarketName) || MarketName == "UNK")
                return "MarketName is undefined";
            if (string.IsNullOrWhiteSpace(Exchange) || Exchange == "UNK")
                return "Exchange is undefined";
            var me = MarketName + Exchange;
            if (mktExs.Contains(me)) return $"Market + Exchange pair duplicate detected - {MarketName} {Exchange}";
            mktExs.Add(me);
            if (SessionCriticalLoss >= 0) return "SessionCriticalLoss must be negative";
            if (MaxErrorsPerDay < 0) return "MaxErrorsPerDay must be non-negative";
            if (BigPointValue <= 0) return "BigPointValue must be positive";
            if (MinMove <= 0) return "MinMove must be positive";
            // We CAN have the situation where there are no strategies traded a given market.
            // it's true if a given market used just for indicator calculation.
            // So, condition 'Strategy.Count == 0' is removed.
            if (Strategies == null) return "Strategies are undefined";

            return null;
        }
        public string Verify(List<int> IDs, List<string> strategyNames, List<string> mktExs, IndicatorsVerificator indVerificator)
        {
            var err = VerifyMe(IDs,  mktExs);
            if (err != null) return err;

            if (Strategies == null) return "Strategies are undefined";
            var idx = 0;
            foreach (var errString in Strategies.Select(sc => sc.Verify(IDs, strategyNames,indVerificator,null))) // todo Last arg can not be null (SGDescription), that lead to constant error!!! 
            {
                if (errString != null) return $"Strategy configuration at index {idx} : {errString}";
                ++idx;
            }

            return null;
        }
    }
    public class StrategyParameters
    {
        public readonly List<StrategyParameter> Parameters = new List<StrategyParameter>();
        public bool TryGetValue(string name, out string value)
        {
            var sp = Parameters.FirstOrDefault(p => p.Name == name);
            if (sp == null)
            {
                value = null;
                return false;
            }
            value = sp.Value;
            return true;
        }

        public bool SetValue(string name, string value)
        {
            var par = Parameters.First(p => p.Name == name);
            par.Value = value;
            return true;
        }

        public StrategyParameters() { }
        public StrategyParameters(StrategyParameters from)
        {
            //Parameters.Clear();
            foreach (StrategyParameter param in from.Parameters)
                Parameters.Add(new StrategyParameter(param));
        }
        public StrategyParameters(IEnumerable<StrategyParameter> pps)
        {
            Parameters.AddRange(pps);
        }

        public static StrategyParameters UpdateOldStyleValues(StrategyParameters sps)
        {
            if (sps != null && sps.Parameters != null)
            {
                foreach (var p in sps.Parameters)
                {
                    switch (p.Name.ToLower())
                    {
                        case "flatpositionallowed":
                        case "onlyalteratedpositionsallowed":
                            switch (p.Value)
                            {
                                case "0":
                                    p.Value = "False";
                                    break;
                                case "1":
                                    p.Value = "True";
                                    break;
                            }
                            break;
                    }
                }
            }
            return sps;
        }
    }
    public class StrategyParameter
    {
        public string Name;
        public string Value;
        public StrategyParameter() { }
        public StrategyParameter(string name, object value)
        {
            Name = name;
            Value = value.ToString();
        }
        public StrategyParameter(StrategyParameter from)
        {
            Name = from.Name;
            Value = from.Value;
        }
    }
    public enum StopLossPositionGuardTypes { No = 0, Fixed = 1, Trailed = 2 }
    public enum DynamicGuardMode { NotUse = 0, OrderPrice = 1, Delta = 2 }
    public class DynamicGuard
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DynamicGuardMode TargetMode = DynamicGuardMode.NotUse;
        public string TargetGuardLongExpression = string.Empty;
        public string TargetGuardShortExpression = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public DynamicGuardMode StopMode = DynamicGuardMode.NotUse;
        public string StopGuardLongExpression = string.Empty;
        public string StopGuardShortExpression = string.Empty;

        //public bool ActiveTargetGuard = true;
        //public bool ActiveStopGuard = true;

        public DynamicGuard() { }
        public DynamicGuard(DynamicGuard from)
        {
            TargetMode = from.TargetMode;
            TargetGuardLongExpression = from.TargetGuardLongExpression;
            TargetGuardShortExpression = from.TargetGuardShortExpression;

            StopMode = from.StopMode;
            StopGuardLongExpression = from.StopGuardLongExpression;
            StopGuardShortExpression = from.StopGuardShortExpression;

            //ActiveTargetGuard = from.ActiveTargetGuard;
            //ActiveStopGuard = from.ActiveStopGuard;
        }

        public void Normalize(Func<string, string> normalizeIndicator)//, bool restoreActiveFlag = true)
        {
            if (TargetMode == DynamicGuardMode.NotUse)
            {
                TargetGuardLongExpression = string.Empty;
                TargetGuardShortExpression = string.Empty;
            }
            else
            {
                TargetGuardLongExpression = normalizeIndicator(TargetGuardLongExpression ?? "");
                TargetGuardShortExpression = normalizeIndicator(TargetGuardShortExpression ?? "");
            }

            if (StopMode == DynamicGuardMode.NotUse)
            {
                StopGuardLongExpression = string.Empty;
                StopGuardShortExpression = string.Empty;
            }
            else
            {
                StopGuardLongExpression = normalizeIndicator(StopGuardLongExpression ?? "");
                StopGuardShortExpression = normalizeIndicator(StopGuardShortExpression ?? "");
            }
            //if (restoreActiveFlag)
            //    ActiveTargetGuard = ActiveStopGuard = true; // this setting is used in the TradingServer only for the temporary switching off
        }

        public bool EqualsTo(DynamicGuard other, Func<string, string> normalizeIndicator)
        {
            if (TargetMode != other.TargetMode || StopMode != other.StopMode) return false;
            //if (ActiveTargetGuard != other.ActiveTargetGuard || ActiveStopGuard != other.ActiveStopGuard) return false;
            if (TargetMode != DynamicGuardMode.NotUse)
            {
                if (normalizeIndicator(TargetGuardLongExpression ?? "") !=
                    normalizeIndicator(other.TargetGuardLongExpression ?? ""))
                    return false;

                if (normalizeIndicator(TargetGuardShortExpression ?? "") !=
                    normalizeIndicator(other.TargetGuardShortExpression ?? ""))
                    return false;
            }

            if (StopMode != DynamicGuardMode.NotUse)
            {
                if (normalizeIndicator(StopGuardLongExpression ?? "") !=
                    normalizeIndicator(other.StopGuardLongExpression ?? ""))
                    return false;

                if (normalizeIndicator(StopGuardShortExpression ?? "") !=
                    normalizeIndicator(other.StopGuardShortExpression ?? ""))
                    return false;
            }
            return true;
        }
    }

    public class StrategyConfiguration
    {
        public int Id = -1;
        public string StrategyName = "UNK";
        public decimal SessionCriticalLoss = decimal.MinValue;
        public string Timeframe = "UNK";
        public int NbrOfContracts;// = 1; // must be > 0  Def value == 0 here, needed for Configurator purpose
        public string ModelID = "UNK";
        public string StrategyDll = "UNK";
        public bool IgnoreTimeZones = false;

        // It's assumed to be >= zero. Zero is a special value - only hard stop by scheduler command, no preparation at all.
        // If value is > 0, special processing must be applied - scheduler will modify command sequence as is :
        // if command is 'SchedulerCommands.Stop', a new command 'SchedulerCommands.PreStop' will be inserted before that command
        // with time of new command firing = time of stop command firing - _preparationIntervalInMinutes. After strategy received this new
        // command, strategy start to mimic execution in 'SoftStopping' state, but strategy state will be 'StoppingByScheduler'. 
        // If current position is close before time of stop-command,
        // strategy pass to 'StoppedByScheduler' state immediately after that, if not - position will be closed at time of stop-command 
        // and strategy pass to 'StoppedByScheduler' state immediately after that.
        //public int PreparationToStoppingBySchedulerInterval { get; set; } = 0; // todo to exclude?

        // strategy parameters
        private StrategyParameters _strategyParameters = new StrategyParameters();
        public StrategyParameters StrategyParameters
        {
            get => _strategyParameters;
            set => _strategyParameters = StrategyParameters.UpdateOldStyleValues(value);
        }

        /// <summary>
        /// Announce the GAP if no quotes received >= than specified number of seconds.
        /// The strategy will be inactive while GAP. With the resumption of the quotes stream
        /// the all the previously collected data will be erased,  data history reloaded,
        /// indicators calculation restarted.
        /// Must be >0; the values here specified in seconds.
        /// The values in GUI can be specified in minutes or hours for more convenience.
        /// </summary>
        //public int GapTimeoutInSeconds { get; set; } = 60 * 60 * 3; // todo to exclude?

        /// <summary>
        /// Must be in range[0..1439];
        /// specified timegrid calculation start point,
        /// also the usage in aaggregators is possible
        /// </summary>
        //public int SynchronizationMinute { get; set; } = 0; // todo to exclude?

        /// <summary>
        /// Specified history data size in days to reload
        /// when indicators calculation restarts
        /// (when trading server starts or at the end of the GAP)
        /// </summary>
        //public int HistorySizeToLoadInDays { get; set; } = 7; // todo to exclude?

        // stop loss/take profit
        public bool UseTakeProfitGuard;
        public double TakeProfitDelta;  // must be >0

        public StopLossPositionGuardTypes StopLossPositionGuardType;
        // for FixedStopLossPositionGuardTypes.Fixed
        public double FixedStopLossDelta;       // must be >0
        // for FixedStopLossPositionGuardTypes.Trailed
        public double TrailedStopLossInitialDelta;  // must be >0
        public double TrailingDelta;                // must be >0
        public double ActivationProfit;             // must be >0

        public DynamicGuard DynamicGuardDescription = new DynamicGuard();

        public int StoplossRestriction_MaxBarsToWaitForOppositeSignal = 0;
        public bool StoplossRestriction_GoToFlatMustLiftRestriction = true;

        public List<string> GetAdditionalInstruments(string mainInstrument)
        {
            return _strategyParameters
                .GetInstrumentsFromStrategyParams()
                .Where(instrum => !string.Equals(instrum, mainInstrument, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private string VerifyGuards()
        {
            if (UseTakeProfitGuard)
            {
                if (TakeProfitDelta <= 0)
                    return "TakeProfitGuard, TakeProfitDelta is invalid";
            }
            switch (StopLossPositionGuardType)
            {
                case StopLossPositionGuardTypes.No:
                    break;
                case StopLossPositionGuardTypes.Fixed:
                    if (FixedStopLossDelta <= 0)
                        return "StopLossGuard, FixedStopLossDelta is invalid";
                    break;
                case StopLossPositionGuardTypes.Trailed:
                    if (TrailedStopLossInitialDelta <= 0)
                        return "StopLossGuard, TrailedStopLossInitialDelta is invalid";
                    if (TrailingDelta <= 0)
                        return "StopLossGuard, TrailingDelta is invalid";
                    if (ActivationProfit <= 0)
                        return "StopLossGuard, ActivationProfit is invalid";
                    break;
            }
            return null;
        }
        private string VerifyDynamicGuard(IndicatorsVerificator indVerificator)
        {
            if (indVerificator == null) return null;

            DynamicGuard dd = DynamicGuardDescription;
            if (dd.TargetMode != DynamicGuardMode.NotUse)
            {
                if (string.IsNullOrEmpty(dd.TargetGuardLongExpression)) return "DynamicTargetGuardLongExpression is not specified";
                var err = indVerificator.VerifyIndicator(Timeframe, dd.TargetGuardLongExpression);
                if (err != null) return "DynamicTargetGuardLongExpression is invalid: " + err;

                if (string.IsNullOrEmpty(dd.TargetGuardShortExpression)) return "DynamicTargetGuardShortExpression is not specified";
                err = indVerificator.VerifyIndicator(Timeframe, dd.TargetGuardShortExpression);
                if (err != null) return "DynamicTargetGuardShortExpression is invalid: " + err;
            }
            if (dd.StopMode != DynamicGuardMode.NotUse)
            {
                if (string.IsNullOrEmpty(dd.StopGuardLongExpression)) return "DynamicStopGuardLongExpression is not specified";
                var err = indVerificator.VerifyIndicator(Timeframe, dd.StopGuardLongExpression);
                if (err != null) return "DynamicStopGuardLongExpression is invalid: " + err;
                 
                if (string.IsNullOrEmpty(dd.StopGuardShortExpression)) return "DynamicStopGuardShortExpression is not specified";
                err = indVerificator.VerifyIndicator(Timeframe, dd.StopGuardShortExpression);
                if (err != null) return "DynamicStopGuardShortExpression is invalid: " + err;
            }
            return null;
        }
        public static string VerifyStrategyParameters(SignalGeneratorDescription descr, StrategyParameters sp)//, Func<string, bool> verifyAdditionalTimeFrame) //string modelID, 
        {
            if (sp == null || sp.Parameters == null) return "Strategy parameters are not specified";

            foreach (var defParam in descr.DefaultParameters.Parameters)
            {
                if (!sp.Parameters.Exists(item => item.Name == defParam.Name))
                    return string.Format("Strategy parameter {0} is not specified ", defParam.Name);
            }
            if (sp.Parameters.Count == 0) return null;

            int maxBarsInPosition = 0;
            bool flatPositionAllowed = true;
            bool onlyAlteratedPositionsAllowed = false;
            foreach (var par in sp.Parameters)
            {
                switch (par.Name)
                {
                    case "MaxBarsInPosition":
                        if (!int.TryParse(par.Value, out maxBarsInPosition) || maxBarsInPosition < 0)
                            return "MaxBarsInPosition must be>=0";
                        break;
                    case "FlatPositionAllowed":
                        if (!ToBool(par.Value, out flatPositionAllowed))
                            return "FlatPositionAllowed must have boolean value";
                        break;
                    case "OnlyAlteratedPositionsAllowed":
                        if (!ToBool(par.Value, out onlyAlteratedPositionsAllowed))
                            return "OnlyAlteratedPositionsAllowed must have boolean value";
                        break;
                    case "MinVotes":
                        if (!int.TryParse(par.Value, out int minVotes) || minVotes < 0)
                            return "Invalid MinVotes value, expected the positive integer value";
                        if (descr.MaxVotes > 0 && minVotes > descr.MaxVotes)
                            return "Invalid MinVotes, value must be <= " + descr.MaxVotes;
                        break;
                    case "Quorum": // by the way-  it is missed in Fx Configurator checking!!! and might be directly in TS
                        if (!int.TryParse(par.Value, out int quorum) || quorum < 0)
                            return "Invalid Quorum value, expected the positive integer value";
                        if (descr.MaxVotes > 0 && quorum > descr.MaxVotes)
                            return "Invalid Quorum, value must be <= " + descr.MaxVotes;
                        break;
                    default:
                        if (par.Name.IsAdditionalTimeFrame())
                        {
                            return
                                "Strategies with additional timeframes not supported in this version (strategy parameter {par.Name})";
#if tmp
                            if (string.IsNullOrWhiteSpace(par.Value))
                                return string.Format("Value is not specified for signal generator parameter '{0}'", par.Name);
                            if (verifyAdditionalTimeFrame != null && !verifyAdditionalTimeFrame(par.Value))
                                return string.Format("Signal generator parameter '{0}' has invalid value '{1}'",
                                    par.Name, par.Value);
#endif
                        }
                        break;
                }
            }
            return CheckParametersConsistency(maxBarsInPosition, flatPositionAllowed, onlyAlteratedPositionsAllowed);
        }
        private static bool ToBool(string str, out bool val)
        {
            if (string.Equals(str, "true", StringComparison.OrdinalIgnoreCase))
            {
                val = true;
                return true;
            }
            if (string.Equals(str, "false", StringComparison.OrdinalIgnoreCase))
            {
                val = false;
                return true;
            }
            val = false;
            return false;
        }
        private static string CheckParametersConsistency(int maxBarsInPosition, bool flatPositionAllowed, bool onlyAlteratedPositionsAllowed)
        {
            if (maxBarsInPosition < 0) return "Invalid MaxBarsInPosition";
            if (!flatPositionAllowed && maxBarsInPosition > 0)
                return "FlatPositionAllowed must be true if the MaxBarsInPosition restriction is specified";

            if (!flatPositionAllowed)
            {
                if (!onlyAlteratedPositionsAllowed)
                    return "If FlatPositionAllowed is false then OnlyAlternatePositionAllowed must be set to true";
            }

            return null;
        }


        public string Verify(List<int> IDs, List<string> strategyNames, IndicatorsVerificator indVerificator,SignalGeneratorDescription sgd)
        {
            if (Id < 0) return "Id must be non-negative";
            if (IDs.Contains(Id)) return $"Id duplication detected - {Id}";
            IDs.Add(Id);
            if (string.IsNullOrWhiteSpace(StrategyName) || StrategyName == "UNK")
                return $"Wrong strategy name detected - {StrategyName}";
            if (strategyNames.Contains(StrategyName)) return $"Strategy name duplication detected - {StrategyName}";
            strategyNames.Add(StrategyName);

            if (SessionCriticalLoss >= 0) return "SessionCriticalLoss must be negative";
            if (string.IsNullOrWhiteSpace(Timeframe) || Timeframe == "UNK")
                return $"Wrong timeframe detected - {Timeframe}";

            if (NbrOfContracts < 1) return "NbrOfContracts must be positive";
            if (string.IsNullOrWhiteSpace(ModelID) || ModelID == "UNK")
                return $"Wrong model ID detected - {ModelID}";

            if (string.IsNullOrWhiteSpace(StrategyDll) || StrategyDll == "UNK")
                return $"Wrong strategy dll detected - {StrategyDll}";

            //if (PreparationToStoppingBySchedulerInterval < 0)
            //    return "PreparationToStoppingBySchedulerInterval must be non-negative";

            //// strategy parameters verification to be done!!

            //if (GapTimeoutInSeconds <= 0) return "GapTimeoutInSeconds must be non-negative";
            //if (SynchronizationMinute < 0 || SynchronizationMinute > 1439)
            //    return "SynchronizationMinute must be in [0;1439]";
            //if (HistorySizeToLoadInDays < 0) return "HistorySizeToLoadInDays must be non-negative";


            if (sgd != null)
            {
                var err = VerifyStrategyParameters(sgd, StrategyParameters);
                if (err != null) return err;
                foreach (string expr in sgd.IndicatorExpressions)
                {
                    err = indVerificator.VerifyIndicator(Timeframe, expr);
                    if (err != null)
                        return $"Cannot create indicator used by strategy\nExpression:{expr}\nError:{err}";
                }
            }
            return VerifyGuards()?? VerifyDynamicGuard(indVerificator);
        }

    }
}
