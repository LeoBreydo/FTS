using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json.Converters;

namespace CoreTypes
{
    /// <summary>
    /// Общие настройки по исполнению ордеров и обновлению информации о конфигурации
    /// </summary>
    public class GeneralSettings
    {
        #region main settings

        public readonly int DecisionComputationFrequency = 60;
        /// <summary>
        /// Update the cofiguration info each N seconds
        /// </summary>
        public readonly int RefreshStateFrequency = 3;
        /// <summary>
        /// CloseAll timeout when TradingService is shutting down in seconds
        /// </summary>
        public int CloseAllTimeout = 30;

        #endregion
        #region order execution settings
        /// <summary>
        /// The maximum number of attempts to send the order to the same broker
        /// </summary>
        public int MaxAttemptsForEachBroker = 4;
        // /// <summary>
        // /// Total maximum number of attempts to send strategy order to any broker
        // /// </summary>
        // public int TotalMaxAttempts = 10;
        /// <summary>
        /// The timeout of the order execution report from broker in seconds
        /// </summary>
        public readonly int ResponseTimeoutInSeconds = 12;
        /// <summary>
        /// To post the TooLongExecutionWarning message every N seconds of the order execution
        /// </summary>
        public readonly int PostFrequencyFor_WarningTooLongExecution = 15;
        /// <summary>
        /// Сonsider the quotation received from the broker obsolete after N seconds
        /// </summary>
        public readonly int VirtualBooksDataRelevanceInSeconds = 60;

        /// <summary>
        /// To stop provider if specified number of order execution violations detected during a day (invalid fills, order execution timeouts)
        /// </summary>
        public int ProviderPenaltiesThreshold = 3;
        #endregion

        public GeneralSettings() { }
        public GeneralSettings(GeneralSettings from)
        {
            MaxAttemptsForEachBroker = from.MaxAttemptsForEachBroker;
            ResponseTimeoutInSeconds = from.ResponseTimeoutInSeconds;
            PostFrequencyFor_WarningTooLongExecution = from.PostFrequencyFor_WarningTooLongExecution;
            VirtualBooksDataRelevanceInSeconds = from.VirtualBooksDataRelevanceInSeconds;
            ProviderPenaltiesThreshold = from.ProviderPenaltiesThreshold;
            DecisionComputationFrequency = from.DecisionComputationFrequency;
            RefreshStateFrequency = from.RefreshStateFrequency;
            CloseAllTimeout = from.CloseAllTimeout;
        }
        public string Verify()
        {
            if (MaxAttemptsForEachBroker <= 0) return "MaxAttemptsForEachBroker must be >0";
            if (ResponseTimeoutInSeconds <= 0) return "ResponseTimeoutInSeconds must be >0";
            if (PostFrequencyFor_WarningTooLongExecution <= 0) return "WarningTooLongExecutionEachSeconds must be >0";
            if (VirtualBooksDataRelevanceInSeconds <= 0) return "VirtualBooksDataRelevanceInSeconds must be >0";
            if (ProviderPenaltiesThreshold <= 0) return "ProviderPenaltiesThreshold must be >0";

            string err;
            if (null != (err = ValidateDecisionComputationFrequency(DecisionComputationFrequency)))
                return err;

            if (RefreshStateFrequency <= 0) return "UpdateStatePeriodicity must be >0";
            if (CloseAllTimeout <= 0) return "ShutdownStrategiesTimeout must be >0";

            return null;
        }

        public static string ValidateDecisionComputationFrequency(int decisionComputationFrequency)
        {
            if (decisionComputationFrequency <= 0 ||
                decisionComputationFrequency > 60 ||
                60 % decisionComputationFrequency != 0)
                return
                    "Invalid DecisionComputationFrequency value (set the value from the next values list: 1,2,3,4,5,6,10,12,15,20,30,60)";
            return null;
        }
        public bool IsValid()
        {
            return Verify() == null;
        }
    }

    public class TradingConfiguration
    {
        public int Id=-1;
        public DateTime StartedAt { get; set; }
        public int NextId { get; set; }
        public GeneralSettings GeneralSettings { get; set; } = new();
        public List<ExchangeConfiguration> Exchanges { get; set; } = new();
        public int MaxErrorsPerDay = 0;

        public string Verify()
        {
            var IDs = new List<int>();
            var strategyNames = new List<string>();
            var currencies = new List<string>();
            var exchanges = new List<string>();
            var mktExs = new List<string>();

            if (Id < 0) return "TradingConfiguration.Id must be non-negative";
            IDs.Add(Id);
            if (GeneralSettings == null) return "General Settings is not specified";
            var error = GeneralSettings.Verify();
            if (error != null) return "Invalid General Settings: " + error;
            if (Exchanges == null || Exchanges.Count == 0) return "Exchanges are undefined";
            if (MaxErrorsPerDay < 0) return "MaxErrorsPerDay must be non-negative";

            var idx = 0;
            foreach (var errString in Exchanges.Select(cgc => cgc.Verify(IDs,strategyNames,exchanges, currencies, mktExs)))
            {
                if (errString != null) return $"Exchange configuration at index {idx} : {errString}";
                ++idx;
            }

            return null;
        }

        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(TradingConfiguration));
        public static TradingConfiguration Restore(string fileName)
        {
            try
            {
                if (!File.Exists(fileName)) return null;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return (TradingConfiguration)serializer.Deserialize(fs);
                }
            }
            catch
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
        public string Currency { get; set; } = "UNK";
        public string ExchangeName { get; set; } = "UNK";
        public int MaxErrorsPerDay = 0;
        public List<MarketConfiguration> Markets { get; set; } = new ();
        public string Verify(List<int> IDs, List<string> strategyNames,List<string> exchanges,  List<string> currencies, List<string> mktExs)
        {
            if (Id < 0) return "Id must be non-negative";
            if (IDs.Contains(Id)) return $"Id duplication detected - {Id}";
            IDs.Add(Id);
            if (string.IsNullOrWhiteSpace(Currency) || Currency.Length != 3 || Currency == "UNK")
                return "Currency is undefined";
            if (currencies.Contains(Currency)) return $"Currency duplication detected - {Currency}";
            currencies.Add(Currency);
            
            if (string.IsNullOrWhiteSpace(ExchangeName) || Currency == "UNK")
                return "Name of exchange is undefined";
            if (exchanges.Contains(ExchangeName)) return $"Exchange name duplication detected - {ExchangeName}";
            exchanges.Add(ExchangeName);

            if (MaxErrorsPerDay < 0) return "MaxErrorsPerDay must be non-negative";

            if (Markets == null || Markets.Count == 0) return "Markets are undefined";
            var idx = 0;
            foreach (var errString in Markets.Select(market => market.Verify(IDs, strategyNames, mktExs)))
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
        public string MarketName = "UNK";
        public string Exchange = "UNK";
        public int BigPointValue = -1;
        public double MinMove = -1;
        public decimal SessionCriticalLoss { get; set; } = decimal.MinValue;
        public int MaxErrorsPerDay = 0;
        public List<StrategyConfiguration> Strategies { get; set; } = new ();

        public string Verify(List<int> IDs, List<string> strategyNames, List<string> mktExs)
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
            //if (Strategies == null || Strategies.Count == 0) return "Strategies are undefined"; // restriction excluded to allow subscribe for additional instruments
            if (Strategies == null) return "Strategies are undefined";
            var idx = 0;
            foreach (var errString in Strategies.Select(sc => sc.Verify(IDs, strategyNames)))
            {
                if (errString != null) return $"Strategy configuration at index {idx} : {errString}";
                ++idx;
            }

            return null;
        }
    }
    public class StrategyParameters
    {
        public readonly List<StrategyParameter> Parameters = new();
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
            Parameters.Clear();
            foreach (StrategyParameter param in from.Parameters)
                Parameters.Add(new StrategyParameter(param));

        }

        public static StrategyParameters UpdateOldStyleValues(StrategyParameters sps)
        {
            if (sps is {Parameters: { }})
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

        public bool ActiveTargetGuard = true;
        public bool ActiveStopGuard = true;

        public DynamicGuard() { }
        public DynamicGuard(DynamicGuard from)
        {
            TargetMode = from.TargetMode;
            TargetGuardLongExpression = from.TargetGuardLongExpression;
            TargetGuardShortExpression = from.TargetGuardShortExpression;

            StopMode = from.StopMode;
            StopGuardLongExpression = from.StopGuardLongExpression;
            StopGuardShortExpression = from.StopGuardShortExpression;

            ActiveTargetGuard = from.ActiveTargetGuard;
            ActiveStopGuard = from.ActiveStopGuard;
        }

        public void Normalize(Func<string, string> normalizeIndicator, bool restoreActiveFlag = true)
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
            if (restoreActiveFlag)
                ActiveTargetGuard = ActiveStopGuard = true; // this setting is used in the TradingServer only for the temporary switching off
        }

        public bool EqualsTo(DynamicGuard other, Func<string, string> normalizeIndicator)
        {
            if (TargetMode != other.TargetMode || StopMode != other.StopMode) return false;
            if (ActiveTargetGuard != other.ActiveTargetGuard || ActiveStopGuard != other.ActiveStopGuard) return false;
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
        public string StrategyName { get; set; } = "UNK";
        public decimal SessionCriticalLoss { get; set; } = decimal.MinValue;
        public string Timeframe { get; set; } = "UNK";
        public int NbrOfContracts { get; set; } = 1; // must be > 0
        public string ModelID { get; set; } = "UNK";
        public string StrategyDll { get; set; } = "UNK";
        public bool IgnoreTimeZones { get; set; } = false;

        // It's assumed to be >= zero. Zero is a special value - only hard stop by scheduler command, no preparation at all.
        // If value is > 0, special processing must be applied - scheduler will modify command sequence as is :
        // if command is 'SchedulerCommands.Stop', a new command 'SchedulerCommands.PreStop' will be inserted before that command
        // with time of new command firing = time of stop command firing - _preparationIntervalInMinutes. After strategy received this new
        // command, strategy start to mimic execution in 'SoftStopping' state, but strategy state will be 'StoppingByScheduler'. 
        // If current position is close before time of stop-command,
        // strategy pass to 'StoppedByScheduler' state immediately after that, if not - position will be closed at time of stop-command 
        // and strategy pass to 'StoppedByScheduler' state immediately after that.
        public int PreparationToStoppingBySchedulerInterval { get; set; } = 0;

        // strategy parameters
        private StrategyParameters _strategyParameters = new();
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
        public int GapTimeoutInSeconds { get; set; } = 60 * 60 * 3;

        /// <summary>
        /// Must be in range[0..1439];
        /// specified timegrid calculation start point,
        /// also the usage in aaggregators is possible
        /// </summary>
        public int SynchronizationMinute { get; set; } = 0;

        /// <summary>
        /// Specified history data size in days to reload
        /// when indicators calculation restarts
        /// (when trading server starts or at the end of the GAP)
        /// </summary>
        public int HistorySizeToLoadInDays { get; set; } = 7;

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

        public DynamicGuard DynamicGuardDescription = new ();

        public int StoplossRestriction_MaxBarsToWaitForOppositeSignal = 1000;
        public bool StoplossRestriction_GoToFlatMustLiftRestriction = true;

        public string Verify(List<int> IDs, List<string> strategyNames)
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

            if (PreparationToStoppingBySchedulerInterval < 0)
                return "PreparationToStoppingBySchedulerInterval must be non-negative";

            // strategy patameters verification to be done!!

            if (GapTimeoutInSeconds <= 0) return "GapTimeoutInSeconds must be non-negative";
            if (SynchronizationMinute < 0 || SynchronizationMinute > 1439)
                return "SynchronizationMinute must be in [0;1439]";
            if (HistorySizeToLoadInDays < 0) return "HistorySizeToLoadInDays must be non-negative";

            return null;
        }
    }
}
