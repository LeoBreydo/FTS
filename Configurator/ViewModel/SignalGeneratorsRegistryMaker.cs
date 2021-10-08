using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Binosoft.TraderLib.Indicators;
using SignalGenerators;

namespace Configurator.ViewModel
{
    public static class SignalGeneratorsRegistryMaker
    {
#if notUsed
        public static List<SignalGeneratorDescription> LoadDllsFromFolder(string folderName, out List<string> errorsList)
        {
            errorsList = new List<string>();
            var modelIDs = new List<string>();
            var result = new List<SignalGeneratorDescription>();
            if (!Directory.Exists(folderName))
            {
                errorsList.Add("Can't load strategies from folder. Folder not found: " + folderName);
                return null;
            }

            foreach (string dllName in Directory.GetFiles(folderName, "*.dll"))
            {
                SignalGeneratorDescription description;
                string err = GetDescription(dllName, out description);
                if (err != null)
                    errorsList.Add(err);
                else if (modelIDs.Contains(description.ModelID))
                {
                    errorsList.Add(string.Format("Strategy dll rejected'{0}': The specified ModelID = {1} already exists",
                                                   dllName, description.ModelID));
                }
                else
                    result.Add(description);
            }
            return result;
        }
#endif
        public static string GetDescription(string strategyDll, out SignalGeneratorDescription sd)
        {
            try
            {
                sd = null;

                if (string.IsNullOrEmpty(strategyDll)) return "Strategy dll is not specified";
                if (!File.Exists(strategyDll)) return "Strategy dll not found " + strategyDll;

                //string strategyXml = Path.ChangeExtension(strategyDll, ".xml");
                //if (!File.Exists(strategyXml)) return "Strategy xml file not found " + strategyDll;

                IByMarketStrategyFactory factory;
                string error;
                if (!StrategyFactoryActivator.LoadStrategyDll(strategyDll, out factory, out error))
                    return string.Format("Can not load strategy dll {0}\n{1}", strategyDll, error);

                var sample = factory.Create(factory.GetDefaultParameters());
                string[] indicatorExpressions = sample.GetIndicatorExpressions();
                bool containsTradingZones = sample.GetTradingZoneIndicators().Length > 0;

                sd = new SignalGeneratorDescription(
                    Path.GetFileName(strategyDll),
                    factory.ModelID,
                    factory.NativeInstrument,
                    TimeGridHelper.TimeFrameWithoutBarFormingPolicy(factory.NativeTimeFrame),
                    indicatorExpressions, containsTradingZones,
                    ConvertStrategyParameters(factory.GetDefaultParameters()),
                    ExtractMaxVotes(factory),
                    ExtractDefaultMarketFilters(strategyDll)
                    );
                //UpdateSignalGeneratorDescription_IS66(sd);
                return null;
            }
            catch (Exception exception)
            {
                sd = null;
                return string.Format("Can not process \"{0}\": {1}", strategyDll, exception.Message);
            }
        }

        //public static SignalGeneratorDescription UpdateSignalGeneratorDescription_IS66(SignalGeneratorDescription description)
        //{
        //    if (description != null)
        //        description.SearchTimeFrame = Converter_IS66.TryCorrect_IS66(description.SearchTimeFrame);
        //    return description;
        //}
        private static StrategyParameters ConvertStrategyParameters(IEnumerable<Parameter> strategyParams)
        {
            return new StrategyParameters(strategyParams.Select(p => new StrategyParameter(p.Name, p.Value)));
        }
        private static int ExtractMaxVotes(IByMarketStrategyFactory factory)
        {
            ParamInfo piMinVotes = factory.GetParameterDescriptions().FirstOrDefault(d => d.Name == "MinVotes");
            if (piMinVotes == null || string.IsNullOrEmpty(piMinVotes.Description))
                return 0;
            var description = piMinVotes.Description;
            int pos = piMinVotes.Description.LastIndexOf("<=", StringComparison.OrdinalIgnoreCase);
            if (pos < 0) return 0;
            int maxVotes;
            return int.TryParse(description.Substring(pos + 2), out maxVotes) ? maxVotes : 0;
        }

        private static DefaultMarketFilters ExtractDefaultMarketFilters(string strategyDll)
        {
            MarketFilterDescriptions dllMfDescription;
            MarketFilterDescriptionExtractor.TryGet(strategyDll, out dllMfDescription);
            return dllMfDescription == null ? null : CastMfDescriptions(dllMfDescription);
        }

        private static DefaultMarketFilters CastMfDescriptions(MarketFilterDescriptions dllMfDescription)
        {
            try
            {
                if (dllMfDescription.Filters == null || dllMfDescription.Filters.Count == 0) return null;
                return new DefaultMarketFilters
                {
                    BarsToKeepMarketFilterRestriction = dllMfDescription.BarsToKeepMarketFilterRestriction,
                    Filters = RemoveDuplicates(
                        dllMfDescription.Filters.Select(f => new MarketFilterDescription
                        {
                            Name = f.Name,
                            Instrument = f.Symbol ?? f.InstrumentName,
                            TimeFrame = f.TimeFrame,
                            Expression = f.Expression,
                            TargetState = CastTargetState(f.TargetState)
                        }))
                };

            }
            catch
            {
                return null;
            }
        }

        private static List<MarketFilterDescription> RemoveDuplicates(IEnumerable<MarketFilterDescription> filters)
        {
            return filters.GroupBy(Key).Select(g => g.First()).ToList();
        }

        private static string Key(MarketFilterDescription filter)
        {
            return string.Format("{0}|{1}|{2}|{3}",
                filter.Instrument,
                filter.TimeFrame.NormalizeIndicatorExpression(),
                filter.Expression.NormalizeIndicatorExpression(),
                filter.TargetState);
        }
        private static MarketFilterTargetState CastTargetState(SignalGenerators.MarketFilterTargetState state)
        {
            var intVal = (int)state;
            if (!Enum.IsDefined(typeof(MarketFilterTargetState), intVal))
                throw new Exception("Unknown TargetState value " + state);
            return (MarketFilterTargetState)intVal;
        }
        public static bool IsAdditionalTimeFrame(this string paramName)
        {
            return paramName.StartsWith("Timeframe", StringComparison.OrdinalIgnoreCase);
        }


    }
}
