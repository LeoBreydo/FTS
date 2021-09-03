using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Binosoft.TraderLib.Indicators;
using CoreTypes.SignalServiceClasses;
using SignalGenerators;

namespace CoreTypes
{
    public class SignalService
    {
        private IndicatorsFacade _indicatorsFacade;
        private string _strategiesFolder;

        private readonly Dictionary<int, StrategyInfoHolder> _strategies = new();
        private static readonly Dictionary<string, LastPriceHolder> _lastPriceHolders = new();
        private StrategyScheduleRestrictors _scheduleRestrictors;

        public SignalService(TradingConfiguration cfg, string strategiesFolder)
        {
            string error = Init(cfg, strategiesFolder);
            if (error != null) throw new Exception(error);// todo!!! modify behavior, save to log instead of exception!!!
        }
        #region Init
        private string Init(TradingConfiguration cfg, string strategiesFolder)
        {
            _indicatorsFacade = new IndicatorsFacade(cfg);
            _strategiesFolder = strategiesFolder;
            
            foreach (MarketConfiguration mc in cfg.Exchanges.SelectMany(g => g.Markets))
            {
                var mktcodeExchange = mc.MCX();
                foreach (var sc in mc.Strategies)
                {
                    string error = InitStrategy(mktcodeExchange, sc);
                    if (error != null)
                    {
                        DebugLog.AddMsg("StrategyInit failed: " + error,true);
                        return string.Format("Failed to create strategy {0}: {1}", sc.Id, error);
                    }
                }
            }

            _scheduleRestrictors = new StrategyScheduleRestrictors(GetStrategyWithUsedInstruments(cfg));
            return null;
        }

        private static List<(int, List<string>)> GetStrategyWithUsedInstruments(TradingConfiguration cfg)
        {
            return cfg.Exchanges.SelectMany(x => x.Markets)
                .SelectMany(mkt => mkt.Strategies.Select(s => (s.Id, GetMarketsUsedByStrategy(mkt, s))))
                .ToList();
        }
        // ReSharper disable once UnusedParameter.Local
        private static List<string> GetMarketsUsedByStrategy(MarketConfiguration mkt, StrategyConfiguration str)
        {
            // in cur version we suppose that strategy uses the only main market (no additional instruments)
            return new() {mkt.MCX()};
        }
        #endregion
        public void ProcessCurrentState(DateTime currentTime, List<(string, int, double)> newBpvMms, 
            List<Tuple<Bar, string, bool, string>> barValues,
            List<(string mktExch, PriceProviderInfo ppi)> tickInfos)
        {
            foreach (var tickInfo in tickInfos)
                SetLastPrice(tickInfo.mktExch, tickInfo.ppi.Last);

            if (_indicatorsFacade.ProcessCurrentState(currentTime,newBpvMms, barValues))
            {
                foreach (StrategyInfoHolder strategy in _strategies.Values)
                    strategy.UpdateDecision();
            }

        }
        public Signal GetSignal(int strategyID)
        {
            var ret= _strategies.TryGetValue(strategyID, out var strategy)
                ? strategy.GetResetLastDecision()
                : Signal.TO_FLAT;
            if (ret != Signal.NO_SIGNAL)
                DebugLog.AddMsg(string.Format("Strategy {0} returns new signal {1}", strategyID, ret));
            return ret;
        }

        public void SetLastPrice(string mktcodeExchange, decimal price)
        {
            if (_lastPriceHolders.TryGetValue(mktcodeExchange, out LastPriceHolder holder))
                holder.LastPrice = (double)price;
        }
        public (bool closePositionByStop, bool closePositionByTarget) MustClosePositionByDynamicGuard(int strategyId, int position, double weightedOpenPrice)
        {
            return _strategies.TryGetValue(strategyId, out var strategy)
                ? strategy.GetMustClosePositionByDynamicGuard(position, weightedOpenPrice)
                : (false, false);
        }

        public void ApplyNewMarketRestrictions(List<(string, TradingRestriction)> tCommands)
        {
            if (tCommands?.Count > 0)
                DebugLog.AddMsg("ApplyNewMarketRestrictions: " +
                                string.Join(";", tCommands.Select(t => string.Format("{0} {1}", t.Item1, t.Item2))));
            _scheduleRestrictors.ApplyNewMarketRestrictions(tCommands);
        }

        public List<ICommand> GetCommands()
        {
            var ret= _scheduleRestrictors.GetCommands();
            if (ret?.Count > 0)
                DebugLog.AddMsg("SignalService.GetCommands returns: " + string.Join("; ",
                    ret.Cast<RestrictionCommand>()
                        .Select(r => string.Format("({0},{1})", r.DestinationId, r.Restriction))));
            return ret;
        }

        private LastPriceHolder GetOrCreateLastPriceHolder(string mktcodeExchange)
        {
            if (!_lastPriceHolders.TryGetValue(mktcodeExchange, out LastPriceHolder holder))
                _lastPriceHolders.Add(mktcodeExchange, holder = new LastPriceHolder());
            return holder;
        }

        private string InitStrategy(string mktcodeExchange, StrategyConfiguration strConfig)
        {
            string error = CreateStrategy(strConfig, out IByMarketStrategy str);
            if (error != null) return error;

            List<string> indicatorExpressions = str.GetIndicatorExpressions().ToList();

            int ixCloseIndicator = indicatorExpressions.Count;
            indicatorExpressions.Add("Close");

            IDynamicGuard stopGuard=null;
            IDynamicGuard targetGuard = null;
            var guardDescr = strConfig.DynamicGuardDescription;
            if (guardDescr != null)
            {
                switch (guardDescr.StopMode)
                {
                    case DynamicGuardMode.Delta:
                    case DynamicGuardMode.OrderPrice:
                        stopGuard = new DynamicStopGuard(
                            guardDescr.StopMode,
                            indicatorExpressions.Count,
                            indicatorExpressions.Count + 1,
                            GetOrCreateLastPriceHolder(mktcodeExchange));
                        indicatorExpressions.Add(guardDescr.StopGuardLongExpression);
                        indicatorExpressions.Add(guardDescr.StopGuardShortExpression);
                        break;
                }

                switch (guardDescr.TargetMode)
                {
                    case DynamicGuardMode.Delta:
                    case DynamicGuardMode.OrderPrice:
                        targetGuard = new DynamicTargetGuard(
                            guardDescr.TargetMode,
                            indicatorExpressions.Count,
                            indicatorExpressions.Count + 1,
                            GetOrCreateLastPriceHolder(mktcodeExchange));
                        indicatorExpressions.Add(guardDescr.TargetGuardLongExpression);
                        indicatorExpressions.Add(guardDescr.TargetGuardShortExpression);
                        break;
                }
            }

            StrategyDynamicGuards dynamicGuards = stopGuard == null && targetGuard == null
                ? null
                : new StrategyDynamicGuards(stopGuard ?? new NullDynmaicGuard(), targetGuard ?? new NullDynmaicGuard());

            error = _indicatorsFacade.CreateIndicators(mktcodeExchange, strConfig.Timeframe,
                indicatorExpressions, out List<Indicator> indicators);
            if (error != null) return error;


            _strategies.Add(strConfig.Id,
                new StrategyInfoHolder(strConfig.Id, str, indicators, ixCloseIndicator, strConfig.IgnoreTimeZones, dynamicGuards));
            return null;
        }

        private string CreateStrategy(StrategyConfiguration sc, out IByMarketStrategy signalGenerator)
        {
            signalGenerator = null;
            try
            {
                string dllName = Path.Combine(_strategiesFolder, sc.StrategyDll);
                if (!File.Exists(dllName)) return "Strategy dll not found " + dllName;

                if (!StrategyFactoryActivator.LoadStrategyDll(dllName, out var factory, out string error))
                    return error;

                if (factory.ModelID != sc.ModelID)
                    return string.Format("ModelID specified in configuration ('{0}') does not match ModelID specified in the strategy xml file ('{1}')",
                        sc.ModelID, factory.ModelID);

                List<Parameter> sgParameters = ConvertParameters(sc.StrategyParameters);
                error = factory.VerifyParameters(sgParameters);
                if (error != null)
                    return error;

//                sgParameters=factory.GetDefaultParameters(); 
//#if RELEASE
//#error remove previous assignment!
//#endif

                signalGenerator = factory.Create(sgParameters);
                return null;
            }
            catch(Exception exception)
            {
                return exception.ToString();
            }
            
        }
        private static List<Parameter> ConvertParameters(StrategyParameters parameters)
        {
            var res = new List<Parameter>();
            foreach (var p in parameters.Parameters)
            {
                switch (p.Name.ToLower())
                {
                    case "flatpositionallowed":
                    case "onlyalteratedpositionsallowed":
                        switch (p.Value)
                        {
                            case "0":
                                res.Add(new Parameter(p.Name, "false"));
                                break;
                            case "1":
                                res.Add(new Parameter(p.Name, "true"));
                                break;
                            default:
                                res.Add(new Parameter(p.Name, p.Value));
                                break;
                        }
                        break;
                    default:
                        res.Add(new Parameter(p.Name, p.Value));
                        break;
                }
            }
            return res;
        }
    }

}

