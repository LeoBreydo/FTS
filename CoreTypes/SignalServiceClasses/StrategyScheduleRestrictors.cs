using System.Collections.Generic;
using System.Linq;

namespace CoreTypes.SignalServiceClasses
{
    public class StrategyScheduleRestrictors
    {
        class InstrumentStateHolder
        {
            public readonly List<StrategyRestrictor> ReferredStrategies;
            public TradingRestriction Restriction;

            public InstrumentStateHolder()
            {
                ReferredStrategies = new();
            }
            public void ApplyRestriction(TradingRestriction restriction)
            {
                if (restriction != Restriction)
                {
                    Restriction = restriction;
                    ReferredStrategies.ForEach(s => s.SetCheckFlag());
                }
            }
        }
        class StrategyRestrictor
        {
            private readonly int _strategyId;
            private bool _checkFlag=true;
            private TradingRestriction _lastRestriction = TradingRestriction.SoftStop;
            public readonly List<InstrumentStateHolder> UsedMarkets = new();
            public StrategyRestrictor(int strategyId)
            {
                _strategyId = strategyId;
            }

            public void SetCheckFlag()
            {
                _checkFlag = true;
            }

            public ICommand GetCommand()
            {
                if (!_checkFlag) return null;

                _checkFlag = false;
                TradingRestriction newRestriction = UsedMarkets.Max(mkt => mkt.Restriction);
                if (newRestriction == _lastRestriction) return null;

                _lastRestriction = newRestriction;
                return new RestrictionCommand(CommandDestination.Strategy, CommandSource.Scheduler, _strategyId, newRestriction);
            }
        }

        private readonly Dictionary<string, InstrumentStateHolder> _instrumentsMap;
        private readonly List<StrategyRestrictor> _strategyRestrictors;
        public StrategyScheduleRestrictors(List<(int, List<string>)> listOf_strategyWithUsedInstruments)
        {
            _instrumentsMap = new Dictionary<string, InstrumentStateHolder>();
            _strategyRestrictors = new List<StrategyRestrictor>();
            foreach (var strategyId_usedMarkets in listOf_strategyWithUsedInstruments)
            {
                StrategyRestrictor sr = new StrategyRestrictor(strategyId_usedMarkets.Item1);
                _strategyRestrictors.Add(sr);

                foreach (string mkt in strategyId_usedMarkets.Item2)
                {
                    if (!_instrumentsMap.TryGetValue(mkt, out var instrumentHolder))
                        _instrumentsMap.Add(mkt.ToUpper(), instrumentHolder = new InstrumentStateHolder());

                    instrumentHolder.ReferredStrategies.Add(sr);
                    sr.UsedMarkets.Add(instrumentHolder);
                }
            }

            _toCheckFlag = true;
        }

        private bool _toCheckFlag;
        public void ApplyNewMarketRestrictions(List<(string, TradingRestriction)> tCommands)
        {
            foreach(var item in tCommands)
                if (_instrumentsMap.TryGetValue(item.Item1.ToUpper(), out var instrumHolder))
                    instrumHolder.ApplyRestriction(item.Item2);
            //throw new System.NotImplementedException();
        }
        public List<ICommand> GetCommands()
        {
            if (!_toCheckFlag) return null;

            _toCheckFlag = false;
            return _strategyRestrictors.Select(s => s.GetCommand()).Where(item => item != null).ToList();
        }
    }
}
