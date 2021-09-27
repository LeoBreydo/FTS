using System;
using System.Collections.Generic;

namespace Configurator.ViewModel
{
    public enum MarketFilterTargetState
    {
        HardStop = 0,
        HardStopLong = 1,
        HardStopShort = 2,
        SoftStop = 3,
        SoftStopLong = 4,
        SoftStopShort = 5,
        Warning = 6
    }

    public class MarketFilterDescription
    {
        public string Name; // unique within the configuration
        public string Instrument; // the instrument name described in MarketFiltersConfiguration.MarketFilterDescriptions
        public string TimeFrame;
        public string Expression;

        public bool HasActivationTrigger => !string.IsNullOrWhiteSpace(TriggerExpression);
        public string TriggerInstrument;
        public string TriggerTimeFrame;
        public string TriggerExpression;

        public MarketFilterTargetState TargetState;
        public bool SetTargetStateWhenNoData = true;

        // STATE MEMBER
        public bool ManualMode;
    }

    public class DefaultMarketFilters
    {
        public List<MarketFilterDescription> Filters = new List<MarketFilterDescription>();
        public uint BarsToKeepMarketFilterRestriction = 0;
    }
    public class SignalGeneratorDescription
    {
        public string ShortDllName { get; set; }
        public string ModelID;
        public string SearchInstrument;
        public string SearchTimeFrame;
        public StrategyParameters DefaultParameters;
        public double MaxVotes;
        public string[] IndicatorExpressions;
        public bool ContainsTradingZones = true;
        public DefaultMarketFilters DefaultMarketFilters;

        public SignalGeneratorDescription() { }
        public SignalGeneratorDescription(string shortDllName, string modelID, string searchInstrument, string searchTimeFrame, string[] indicatorExpressions, bool containsTradingZones,
            StrategyParameters defaltParameters, double maxVotes = 0, DefaultMarketFilters defaultMarketFilters = null)
        {
            ShortDllName = shortDllName;
            ModelID = modelID;
            SearchInstrument = searchInstrument;
            SearchTimeFrame = searchTimeFrame;
            IndicatorExpressions = indicatorExpressions;
            ContainsTradingZones = containsTradingZones;
            DefaultParameters = defaltParameters;
            MaxVotes = maxVotes;
            DefaultMarketFilters = defaultMarketFilters;
        }

        public StrategyParameters CloneDefautParameters()
        {
            return new StrategyParameters(DefaultParameters);
        }

        public bool IsAdditionalTimeFrame(string paramName)
        {
            return paramName.StartsWith("Timeframe", StringComparison.OrdinalIgnoreCase);
        }
        public Type GetParamType(string paramName)
        {
            switch (paramName)
            {
                default:   // expected the additional timeframe
                    return typeof(string);

                case "MaxBarsInPosition": 
                    return typeof(int);
                case "OnlyAlteratedPositionsAllowed":
                case "FlatPositionAllowed":
                    return typeof(bool);

                case "Quorum":
                case "MinVotes":
                    return typeof(int);
            }

        }

        public string GetParamDescription(string paramName)
        {
            switch (paramName)
            {
                default:
                    return paramName;
                case "MaxBarsInPosition": 
                    return "Max num bars in position (zero value means no restriction)";
                case "FlatPositionAllowed":
                    return "Specifies if flat position is allowed or not";
                case "OnlyAlteratedPositionsAllowed":
                    return "OnlyAlteratedPositionsAllowed";
                case "MinVotes":
                    if (MaxVotes <= 0) return ""; // unexpected use, this is not a committee
                    return "Committee MinVotes threshold. Must have positive numeric value <= " + MaxVotes;
                case "QuorumQuorum":
                    if (MaxVotes <= 0) return ""; // unexpected use, this is not a committee
                    return "Committee Quorum threshold. Must have positive numeric value <= " + MaxVotes;
            }
        }
        public string VerifyParameterValue(string paramName, object value)//, Func<string, bool> verifyAdditionalTimeFrame)
        {
            switch (paramName)
            {
                case "MaxBarsInPosition":
                    try
                    {
                        var intVal = (int)value;
                        if (intVal < 0)
                            return "Value must be >=0";
                        return null;
                    }
                    catch
                    {
                        return "The integer not negative value expected";
                    }
                case "FlatPositionAllowed":
                case "OnlyAlteratedPositionsAllowed":
                    return (value is bool) ? null : "The boolean value expected";

                case "MinVotes":
                case "Quorum":
                    if (MaxVotes <= 0) return null; // ignore
                    var val = (int)value;
                    return (val > 0 && val < MaxVotes)
                        ? null
                        : "Expected the positive numeric value <= " + MaxVotes;
                default:
                    return null;
                    //if (!paramName.IsAdditionalTimeFrame())
                        

                    //if (string.IsNullOrWhiteSpace(value.ToString()))
                    //    return string.Format("Value is not specified for signal generator parameter '{0}'", paramName);
                    //if (verifyAdditionalTimeFrame != null && !verifyAdditionalTimeFrame(value.ToString()))
                    //    return string.Format("Signal generator parameter '{0}' has invalid value '{1}'", paramName,
                    //        value);
                    //return null;
            }
        }


    }
}
