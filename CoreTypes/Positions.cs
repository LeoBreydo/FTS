using System;
using System.Collections.Generic;
using static System.Math;
using static CoreTypes.MessageStringProducer;

namespace CoreTypes
{
    public class CurrencyPosition
    {
        public List<ExchangePosition> ExchangePositions { get; } = new();
        public decimal UnrealizedResult { get; private set; }
        public decimal RealizedResult { get; private set; }

        public int Update(bool startNewDay)
        {
            UnrealizedResult = 0;
            RealizedResult = 0;
            var en = 0;
            foreach (var ep in ExchangePositions)
            {
                en += ep.Update(startNewDay);
                UnrealizedResult += ep.UnrealizedResult;
                RealizedResult += ep.RealizedResult;
            }

            return en;
        }

        public void RegisterExchangePosition(ExchangeTrader et) => ExchangePositions.Add(et.Position);
    }

    public class ExchangePosition
    {
        public ExchangeTrader Owner;
        public List<MarketPosition> MarketPositions { get; } = new();

        public decimal UnrealizedResult { get; private set; }
        public decimal RealizedResult { get; private set; }
        public int DealNbr { get; private set; }

        public ExchangePosition(ExchangeTrader owner) => Owner = owner;

        public int Update(bool startNewDay)
        {
            UnrealizedResult = 0;
            RealizedResult = 0;
            DealNbr = 0;
            var en = 0;
            foreach (var ip in MarketPositions)
            {
                en = ip.Update(startNewDay);
                UnrealizedResult += ip.UnrealizedResult;
                RealizedResult += ip.RealizedResult;
                DealNbr += ip.DealNbr;
            }
            Owner.ErrorTracker.SetExternalTrackingValue(en);
            Owner.ErrorTracker.CalculateState();
            if (startNewDay) Owner.ErrorTracker.StartNewTrackingPeriod();
            Owner.RestrictionsManager.SetErrorsNbrRestriction(Owner.ErrorTracker.State == WorkingState.Stopped
                ? TradingRestriction.HardStop
                : TradingRestriction.NoRestrictions);
            return en;
        }

        public void RegisterMarketPosition(MarketTrader mp) => MarketPositions.Add(mp.Position);
    }

    public class MarketPosition
    {
        public List<StrategyPosition> StrategyPositions { get; } = new();
        public IValueTracker<decimal, WorkingState> LossManager { get; }
        public int LongSize { get; private set; }
        public int ShortSize { get; private set; }
        public decimal UnrealizedResult { get; private set; }
        public decimal RealizedResult { get; private set; }
        public int DealNbr { get; private set; }

        public PriceProvider PriceProvider { get; }

        private readonly MarketTrader _owner;
        private bool _newSessionStarted;

        public MarketPosition(MarketTrader owner, decimal criticalLoss = decimal.MinValue)
        {
            LossManager = new CriticalLossManager(criticalLoss);
            PriceProvider = new PriceProvider();
            _owner = owner;
        }

        public int Update(bool startNewDay)
        {
            if (PriceProvider.LastPrice == -1) return 0;
            var currentPrice = PriceProvider.LastPrice;
            LongSize = 0;
            ShortSize = 0;
            UnrealizedResult = 0;
            RealizedResult = 0;
            DealNbr = 0;
            foreach (var sp in StrategyPositions)
            {
                sp.UpdatePosition(currentPrice);
                if (sp.Size > 0) LongSize += sp.Size;
                else ShortSize += sp.Size;
                UnrealizedResult += sp.UnrealizedResult;
                RealizedResult += sp.RealizedResult;
                DealNbr += sp.DealNbr;
            }
            LossManager.SetExternalTrackingValue(UnrealizedResult+RealizedResult);
            LossManager.CalculateState();
            if (_newSessionStarted)
            {
                LossManager.StartNewTrackingPeriod();
                _newSessionStarted = false;
            }
            _owner.RestrictionsManager.SetCriticalLossRestriction(LossManager.State == WorkingState.Stopped
                ? TradingRestriction.HardStop
                : TradingRestriction.NoRestrictions);

            _owner.ErrorTracker.CalculateState();
            if (startNewDay) _owner.ErrorTracker.StartNewTrackingPeriod();
            _owner.RestrictionsManager.SetErrorsNbrRestriction(_owner.ErrorTracker.State == WorkingState.Stopped
                ? TradingRestriction.HardStop
                : TradingRestriction.NoRestrictions);
            return _owner.ErrorTracker.TotalValue;
        }

        public void RegisterStrategyPosition(StrategyTrader strategy) => StrategyPositions.Add(strategy.Position);

        public void StartNewSession() => _newSessionStarted = true;
    }

    public enum StrategyPositionStateEnum
    {
        Short = -1,
        Flat = 0,
        Long = 1
    }

    public class StrategyPosition
    {
        public readonly int StrategyId;
        public readonly string StrategyName;
        // exec, size
        private readonly List<(Execution, int)> _openDeals = new();
        private decimal _bpv;
        private bool _newSessionStarted;

        public IValueTracker<decimal, WorkingState> LossManager { get; }
        public int Size { get; private set; }
        private decimal _refQuote = -1;

        public decimal UnrealizedResult { get; private set; }
        public decimal RealizedResult { get; private set; }
        public int DealNbr => _openDeals.Count;
        public double WeightedOpenQuote { get; set; }

        public StrategyTrader Owner { get; set; }

        // bigPointValue and minMove <- market trader configuration
        public StrategyPosition(int strategyId, string strategyName, decimal criticalLoss = decimal.MinValue)
        {
            if (strategyId < 0) throw new Exception("strategyId < 0");
            if (string.IsNullOrWhiteSpace(strategyName)) throw new Exception("strategy name is null or empty");
            StrategyId = strategyId;
            StrategyName = strategyName;
            _bpv = 1;
            LossManager = new CriticalLossManager(criticalLoss);
        }

        public void UpdatePosition(decimal currentPrice)
        {
            UnrealizedResult = (Size == 0 ? 0 : Size * (currentPrice - _refQuote)) * _bpv;
            LossManager.SetNewValue(UnrealizedResult+RealizedResult);
            LossManager.CalculateState();
            if (_newSessionStarted)
            {
                LossManager.StartNewTrackingPeriod();
                _newSessionStarted = false;
            }
            Owner.RestrictionsManager.SetCriticalLossRestriction(LossManager.State == WorkingState.Stopped
                ? TradingRestriction.HardStop
                : TradingRestriction.NoRestrictions);

        }

        public void ClearStopLossRestrictions() => Owner.PositionValidator.ClearStopLossRestriction();

        public void ProcessNewDeals(IEnumerable<(Execution, int)> deals, InfoCollector ic)
        {
            // Call condition : toReduce != 0 && reduceBy != 0
            static (int, int)? ReduceDeals(int toReduce, int reduceBy)
            {
                if (Sign(toReduce) == Sign(reduceBy))
                    return null; // null means that we must add new deal to the end of _openDeals
                var res = toReduce + reduceBy;
                return res == 0 ? (0, 0) : Sign(res) == Sign(toReduce) ? (res, 0) : (0, res);
            }

            List<string> trades = new();
            int io = 0, total = _openDeals.Count;
            foreach (var (e, s) in deals)
            {
                var cnt = s;
                while (cnt != 0)
                {
                    if (io == total)
                    {
                        _openDeals.Add((e, cnt));
                        ++total;
                        cnt = 0;
                    }
                    else
                    {
                        var (ee, ss) = _openDeals[io];
                        if (ss == 0)
                        {
                            ++io;
                            continue;
                        }

                        var t = ReduceDeals(ss, cnt);
                        if (t == null)
                        {
                            // special case - signs of new deal size and open deal size are the same,
                            // -> to add new open deal to the end of _openDeals 
                            _openDeals.Add((e, cnt));
                            ++total;
                            cnt = 0;
                        }
                        else
                        {
                            var (opDealRemainder, newDealRemainder) = t.Value;
                            _openDeals[io] = (ee, opDealRemainder);
                            if (opDealRemainder == 0)
                            {
                                trades.Add(TradeString(ee, e, ss, StrategyName));
                                RealizedResult += _bpv * (e.Price - ee.Price) * ss;
                                ++io;
                            }
                            else
                            {
                                trades.Add(TradeString(ee, e, cnt, StrategyName));
                                RealizedResult += _bpv * (e.Price - ee.Price) * cnt;
                            }
                            cnt = newDealRemainder;
                        }
                    }
                }
            }
            _openDeals.RemoveAll(t => t.Item2 == 0);

            Size = 0;
            _refQuote = 0;
            foreach (var (e, s) in _openDeals)
            {
                Size += s;
                _refQuote += s * e.Price;
            }

            _refQuote /= Size;
            WeightedOpenQuote = (double) _refQuote;

            Owner.PositionValidator.UpdateGuards();
            if(trades.Count > 0) ic.Accept(trades);
        }

        public void SetBigPointValue(int bpv) => _bpv = bpv;

        public void StartNewSession() => _newSessionStarted = true;
    }
}
