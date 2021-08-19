using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Binosoft.TraderLib.Indicators;
using SignalGenerators;
namespace CoreTypes
{
    class StrategyInfoHolder
    {
        private readonly IByMarketStrategy _strategy;
        private readonly List<Indicator> _indicators;
        private readonly double[] _inputsBuf;
        private readonly bool _ignoreTradingZones;

        private Signal _decision = Signal.NO_SIGNAL;
        private string CalculationError;

        private readonly StrategyDynamicGuards _dynamicGuards;
        public StrategyInfoHolder(IByMarketStrategy strategy, List<Indicator> strategyIndicators,bool ignoreTradingZones, StrategyDynamicGuards dynamicGuards)
        {
            _strategy = strategy;
            _indicators = strategyIndicators;
            _inputsBuf = new double[_indicators.Count];
            _ignoreTradingZones = ignoreTradingZones;

            _dynamicGuards = dynamicGuards;
        }
        public Signal GetResetLastDecision()
        {
            var ret = _decision;
            _decision = Signal.NO_SIGNAL;
            return ret;
        }

        public void UpdateDecision(DateTime currentTime)
        {
            if (CalculationError!=null)
            {
                _decision = Signal.TO_FLAT; 
                return;
            }

            int i = -1;
            bool allIndicatorsHasNewValues = true;
            foreach (var indicator in _indicators)
            {
                ++i;
                var exceptionInfo = indicator.CalculationExceptionInfo; 
                if (exceptionInfo != null)
                {
                    CalculationError = exceptionInfo.ToString(); // !!+ todo to output msg about occurred problem
                    _decision = Signal.TO_FLAT;
                    return;
                }

                if (indicator.Count == 0 || indicator.CalculationTime(0) != currentTime)
                    allIndicatorsHasNewValues = false;
                else
                    _inputsBuf[i] = indicator[0];
            }
            if (!allIndicatorsHasNewValues) return;

            switch (Math.Sign(_strategy.GenerateSignal(_inputsBuf, _ignoreTradingZones)))
            {
                case 1:
                    _decision = Signal.TO_LONG;
                    break;
                case -1:
                    _decision = Signal.TO_SHORT;
                    break;
                case 0:
                    _decision = Signal.TO_FLAT;
                    break;
            }
            _dynamicGuards?.UpdateValues(_inputsBuf);
        }

        public (bool, bool) GetMustClosePositionByDynamicGuard(int position, double weightedOpenPrice)
        {
            return _dynamicGuards?.GetMustClosePosition(position, weightedOpenPrice)
                   ?? (false, false);
        }
    }

    public class SignalService
    {
        private IndicatorsFacade _indicatorsFacade;
        private string _folderName;

        private readonly Dictionary<int, StrategyInfoHolder> _strategies = new();
        private static readonly Dictionary<string, LastPriceHolder> _lastPriceHolders = new();
        public string Init(TradingConfiguration cfg, string strategiesFolder, IndicatorsFacade indicatorsFacade)
        {
            _folderName = strategiesFolder;
            _indicatorsFacade = indicatorsFacade;
            
            foreach (MarketConfiguration mc in cfg.Exchanges.SelectMany(g => g.Markets))
            {
                var mktcodeExchange = GetMktcodeExchange(mc);
                foreach (var sc in mc.Strategies)
                {
                    string error = InitStrategy(mktcodeExchange, sc);
                    if (error != null)
                        return string.Format("Failed to create strategy {0}: {1}", sc.Id, error);
                }
            }
            return null;
        }
        public void ProcessMinuteBars(DateTime currentTime, List<Tuple<string, Bar, bool>> barValues)
        {
            _indicatorsFacade.ProcessMinuteBars(currentTime, barValues);
            foreach (StrategyInfoHolder strategy in _strategies.Values)
                strategy.UpdateDecision(currentTime);
        }
        public Signal GetSignal(int strategyID)
        {
            return _strategies.TryGetValue(strategyID, out var strategy)
                ? strategy.GetResetLastDecision()
                : Signal.TO_FLAT;
        }

        public void SetLastPrice(string mktcodeExchange, double price)
        {
            if (_lastPriceHolders.TryGetValue(mktcodeExchange, out LastPriceHolder holder))
                holder.LastPrice = price;
        }
        public (bool, bool) MustClosePosition(int strategyId, int position, double weightedOpenPrice)
        {
            return _strategies.TryGetValue(strategyId, out var strategy)
                ? strategy.GetMustClosePositionByDynamicGuard(position, weightedOpenPrice)
                : (false, false);
        }
        public List<ICommand> GetCommands()
        {
            return new List<ICommand>();
            //throw new System.NotImplementedException();
        }

        public void ApplyNewMarketRestrictions(List<(string, TradingRestriction)> tCommands)
        {
            //throw new System.NotImplementedException();
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

            var indicatorExpressions = str.GetIndicatorExpressions().ToList();

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
                new StrategyInfoHolder(str, indicators, strConfig.IgnoreTimeZones, dynamicGuards));
            return null;
        }

        private string CreateStrategy(StrategyConfiguration sc, out IByMarketStrategy signalGenerator)
        {
            signalGenerator = null;
            try
            {
                string dllName = Path.Combine(_folderName, sc.StrategyDll);
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

        private static string GetMktcodeExchange(MarketConfiguration mktCfg)
        {
            return mktCfg.MarketName + mktCfg.Exchange;
        }

    }

}

