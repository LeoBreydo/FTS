using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes
{
    public enum TradingRestriction
    {
        NoRestrictions = 0,
        SoftStop = 1,
        HardStop = 2
    }

    public static class SRestrictionEx
    {
        public static int ToInt(this TradingRestriction r)
        {
            return (int)r;
        }

        public static string AsString(this TradingRestriction r)
        {
            return r switch
            {
                TradingRestriction.HardStop => "HardStop",
                TradingRestriction.SoftStop => "SoftStop",
                _ => "NoRestriction"
            };
        }
    }

    public class TradingRestrictionManager
    {
        private readonly TradingRestriction[] _restrictions
            = Enumerable.Repeat(TradingRestriction.NoRestrictions, 
                Enum.GetNames(typeof(CommandSource)).Length).ToArray();

        protected void SetRestriction(TradingRestriction r, CommandSource s) =>
            _restrictions[s.ToInt()] = r;

        public TradingRestriction GetCurrentRestriction() =>
            _restrictions.Max();

        public List<(CommandSource, TradingRestriction)> GetDetails() =>
            _restrictions
                .Select((r, i) => ((CommandSource) i, r))
                .Where(t => t.r != TradingRestriction.NoRestrictions)
                .ToList();

        public string GetRestrictions()
        {
            return string.Join(",", _restrictions.Select((tr, i) => tr == TradingRestriction.NoRestrictions
                ? string.Empty
                : $"{tr.AsString()} by {((CommandSource) i).AsString()}")
                .Where(s=>s != string.Empty)
                .ToList());
        }

        public TradingRestriction GetRestriction(CommandSource cs)
        {
            return _restrictions[cs.ToInt()];
        }
    }

    public class ServiceRestrictionsManager : TradingRestrictionManager
    {
        public void SetSchedulerRestriction(TradingRestriction r) => 
            SetRestriction(r,CommandSource.Scheduler);

        public void SetUserRestriction(TradingRestriction r) => 
            SetRestriction(r, CommandSource.User);

        public void SetErrorsNbrRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.ErrorsNbr);
    }

    public class ExchangeRestrictionsManager : TradingRestrictionManager
    {
        public void SetParentRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Parent);

        public void SetUserRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.User);

        public void SetErrorsNbrRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.ErrorsNbr);

        public void SetSchedulerRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Scheduler);
    }

    public class MarketRestrictionsManager : TradingRestrictionManager
    {
        public void SetParentRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Parent);

        public void SetUserRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.User);

        public void SetCriticalLossRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.CriticalLoss);

        public void SetEndOfContractRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.EndOfContract);

        public void SetEndOfSessionRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.EndOfSession);

        public void SetErrorsNbrRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.ErrorsNbr);

        public void SetOutOfMarketRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.OutOfMarket);

        public void SetSchedulerRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Scheduler);
    }

    public class StrategyRestrictionsManager : TradingRestrictionManager
    {
        public void SetParentRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Parent);

        public void SetUserRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.User);

        public void SetCriticalLossRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.CriticalLoss);

        public void SetErrorRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Error);

        public void SetOutOfMarketRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.OutOfMarket);
    }
}
