using System;
using System.Collections.Generic;
using static System.Math;

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

        public void RegisterExchangePosition(ExchangeTrader et)
        {
            ExchangePositions.Add(et.Position);
        }
    }

    public class ExchangePosition
    {
        public ExchangeTrader Owner;
        public List<MarketPosition> MarketPositions { get; } = new();

        public decimal UnrealizedResult { get; private set; }
        public decimal RealizedResult { get; private set; }
        public int DealNbr { get; private set; }

        public ExchangePosition(ExchangeTrader owner)
        {
            Owner = owner;
        }

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
            Owner.ErrorCollector.SetErrorsAndEvaluateState(en);
            if (startNewDay) Owner.ErrorCollector.StartNewDay();
            Owner.RestrictionsManager.SetErrorsNbrRestriction(Owner.ErrorCollector.IsStopped
                ? TradingRestriction.HardStop
                : TradingRestriction.NoRestrictions);
            return en;
        }

        public void RegisterMarketPosition(MarketTrader mp)
        {
            MarketPositions.Add(mp.Position);
        }
    }

    public class MarketPosition
    {
        public List<StrategyPosition> StrategyPositions { get; } = new();
        public MarketCriticalLossManager LossManager { get; }
        public int LongSize { get; private set; }
        public int ShortSize { get; private set; }
        public int Size => LongSize + ShortSize;
        public decimal UnrealizedResult { get; private set; }
        public decimal RealizedResult { get; private set; }
        public int DealNbr { get; private set; }

        public PriceProvider PriceProvider { get; }

        private readonly MarketTrader _owner;

        public MarketPosition(MarketTrader owner, decimal criticalLoss = decimal.MinValue)
        {
            LossManager = new MarketCriticalLossManager(criticalLoss);
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
            LossManager.SessionResult = 0;
            foreach (var sp in StrategyPositions)
            {
                sp.UpdatePosition(currentPrice);
                if (sp.Size > 0) LongSize += sp.Size;
                else ShortSize += sp.Size;
                UnrealizedResult += sp.UnrealizedResult;
                RealizedResult += sp.RealizedResult;
                DealNbr += sp.DealNbr;
                LossManager.SessionResult += sp.LossManager.SessionResult;
            }
            LossManager.UpdateState();
            _owner.ErrorCollector.SetErrorsAndEvaluateState(_owner.ErrorCollector.Errors);
            if (startNewDay) _owner.ErrorCollector.StartNewDay();
            _owner.RestrictionsManager.SetErrorsNbrRestriction(_owner.ErrorCollector.IsStopped
                ? TradingRestriction.HardStop
                : TradingRestriction.NoRestrictions);
            return _owner.ErrorCollector.Errors;
        }

        public void RegisterStrategyPosition(StrategyTrader strategy)
        {
            StrategyPositions.Add(strategy.Position);
        }
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
        // exec, size, last settlement price (initial -1)
        private readonly List<(Execution, int, decimal)> _openDeals = new();
        private decimal _bpv;

        public StrategyCriticalLossManager LossManager { get; }
        public int Size { get; private set; }
        private decimal _refQuote = -1;

        public decimal UnrealizedResult { get; private set; }
        public decimal RealizedResult { get; private set; }
        public decimal TotalResult => RealizedResult + UnrealizedResult;
        public StrategyPositionStateEnum StrategyPositionState => Size > 0
            ? StrategyPositionStateEnum.Long
            : Size < 0
            ? StrategyPositionStateEnum.Short
            : StrategyPositionStateEnum.Flat;

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
            LossManager = new StrategyCriticalLossManager(this, criticalLoss);
        }

        public void UpdatePosition(decimal currentPrice)
        {
            UnrealizedResult = (Size == 0 ? 0 : Size * (currentPrice - _refQuote)) * _bpv;
            LossManager.UpdateState();
        }

        public void ProcessSettlementPrice(decimal settlementPrice)
        {
            for (var i = 0; i < _openDeals.Count; ++i)
            {
                var (e, size, p) = _openDeals[i];
                RealizedResult += _bpv * (p == -1
                    ? (settlementPrice - e.Price) * size
                    : (settlementPrice - p) * size);
                _openDeals[i] = (e, size, settlementPrice);
            }

            _refQuote = settlementPrice;
            Owner.PositionValidator.ClearStopLossRestriction();
        }

        public List<string> ProcessNewDeals(IEnumerable<(Execution, int)> deals)
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
                        _openDeals.Add((e, cnt, -1));
                        ++total;
                        cnt = 0;
                    }
                    else
                    {
                        var (ee, ss, p) = _openDeals[io];
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
                            _openDeals.Add((e, cnt, -1));
                            ++total;
                            cnt = 0;
                        }
                        else
                        {
                            var (opDealRemainder, newDealRemainder) = t.Value;
                            _openDeals[io] = (ee, opDealRemainder, p);
                            switch (opDealRemainder)
                            {
                                case 0 when newDealRemainder == 0:
                                    // opened deal and new deal are reduced the both
                                    trades.Add(new Trade(ee, e, ss, StrategyName).ToString());
                                    RealizedResult += _bpv * (p == -1
                                        ? (e.Price - ee.Price) * ss
                                        : (e.Price - p) * ss);
                                    ++io;
                                    break;
                                case 0:
                                    // opened deal is reduced
                                    trades.Add(new Trade(ee, e, ss, StrategyName).ToString());
                                    RealizedResult += _bpv * (p == -1
                                        ? (e.Price - ee.Price) * ss
                                        : (e.Price - p) * ss);
                                    ++io;
                                    break;
                                default:
                                    // new deal is reduced
                                    trades.Add(new Trade(ee, e, cnt, StrategyName).ToString());
                                    RealizedResult += _bpv * (p == -1
                                        ? (e.Price - ee.Price) * cnt
                                        : (e.Price - p) * cnt);
                                    break;
                            }
                            cnt = newDealRemainder;
                        }
                    }
                }
            }
            _openDeals.RemoveAll(t => t.Item2 == 0);

            Size = 0;
            _refQuote = 0;
            var woq = 0m;
            foreach (var (e, s, p) in _openDeals)
            {
                Size += s;
                _refQuote += (s * (p == -1 ? e.Price : p));
                woq += s * e.Price;
            }

            _refQuote /= Size;
            woq /= Size;
            WeightedOpenQuote = (double) woq;

            Owner.PositionValidator.UpdateGuards();

            return trades;
        }

        public void StartNewSession() =>
            LossManager.StartNewSession(RealizedResult + UnrealizedResult);

        public void SetBigPointValue(int bpv)
        {
            _bpv = bpv;
        }
    }
}
