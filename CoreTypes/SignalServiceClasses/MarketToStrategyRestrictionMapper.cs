using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes.SignalServiceClasses
{
    public class MarketToStrategyRestrictionMapper
    {
        #region nested classes
        class InstrumentStateHolder
        {
            public readonly List<StrategyStateHolder> ReferredStrategies;
            public bool IsOutOfMarket = true;

            public InstrumentStateHolder()
            {
                ReferredStrategies = new();
            }

            public bool SetOutOfMarket(bool isOutOfMarket)
            {
                if (IsOutOfMarket != isOutOfMarket)
                {
                    IsOutOfMarket = isOutOfMarket;
                    ReferredStrategies.ForEach(s => s.ToUpdate());
                    return true;
                }
                return false;
            }
        }
        class StrategyStateHolder
        {
            private readonly int _strategyId;
            public readonly List<InstrumentStateHolder> UsedMarkets = new();

            private bool _updateNeeded = true;
            private bool _isOutOfMarket;
            public StrategyStateHolder(int strategyId)
            {
                _strategyId = strategyId;
            }

            public void ToUpdate()
            {
                _updateNeeded = true;
            }

            public ICommand GetCommand()
            {
                if (!_updateNeeded) return null;

                _updateNeeded = false;
                bool newval_IsOutOfMarket = UsedMarkets.Any(mkt => mkt.IsOutOfMarket);
                if (newval_IsOutOfMarket == _isOutOfMarket) return null;

                _isOutOfMarket = newval_IsOutOfMarket;
                return new RestrictionCommand(CommandDestination.Strategy, CommandSource.OutOfMarket, _strategyId,
                    _isOutOfMarket ? TradingRestriction.HardStop : TradingRestriction.NoRestrictions);
            }
            public ICommand GetCommandFirstTime()
            {
                //  when called first time, return command for all strategies
                _updateNeeded = false;
                _isOutOfMarket = UsedMarkets.Any(mkt => mkt.IsOutOfMarket);
                return new RestrictionCommand(CommandDestination.Strategy, CommandSource.OutOfMarket, _strategyId,
                    _isOutOfMarket ? TradingRestriction.HardStop : TradingRestriction.NoRestrictions);
            }
        }
        #endregion
        private readonly Dictionary<string, InstrumentStateHolder> _instrumentsMap;
        private readonly List<StrategyStateHolder> _strategyRestrictors;
        private bool _firstTime = true;
        private bool _anyStateChanged;

        public MarketToStrategyRestrictionMapper(List<(int, List<string>)> listOf_strategyWithUsedInstruments)
        {
            if (listOf_strategyWithUsedInstruments.Count > 0)
                throw new Exception("Usage of additional instruments is disabled"); // please re-activate methods GetCommands() when will remove this restriction

            _instrumentsMap = new Dictionary<string, InstrumentStateHolder>();
            _strategyRestrictors = new List<StrategyStateHolder>();
            foreach (var strategyId_usedMarkets in listOf_strategyWithUsedInstruments)
            {
                StrategyStateHolder sr = new StrategyStateHolder(strategyId_usedMarkets.Item1);
                _strategyRestrictors.Add(sr);

                foreach (string mkt in strategyId_usedMarkets.Item2)
                {
                    if (!_instrumentsMap.TryGetValue(mkt, out var instrumentHolder))
                        _instrumentsMap.Add(mkt.ToUpper(), instrumentHolder = new InstrumentStateHolder());

                    instrumentHolder.ReferredStrategies.Add(sr);
                    sr.UsedMarkets.Add(instrumentHolder);
                }
            }
        }

        public void ApplyNewMarketRestrictions(List<(string, TradingRestriction)> tCommands)
        {
            foreach(var item in tCommands)
                if (_instrumentsMap.TryGetValue(item.Item1.ToUpper(), out var instrumHolder))
                    if (instrumHolder.SetOutOfMarket(item.Item2 != TradingRestriction.NoRestrictions))
                        _anyStateChanged = true;
        }

        public List<ICommand> GetCommands()
        {
            return null; 
            if (_firstTime)
            {
                _firstTime = false;
                _anyStateChanged = false;
                return _strategyRestrictors.Select(s => s.GetCommandFirstTime()).ToList();
            }

            if (!_anyStateChanged) return null;
            _anyStateChanged = false;
            return _strategyRestrictors.Select(s => s.GetCommand()).Where(item => item != null).ToList();
        }
    }
}
