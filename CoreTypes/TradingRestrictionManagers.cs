using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreTypes
{
    public enum TradingRestriction : int
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
                .Where(t => t.Item2 != TradingRestriction.NoRestrictions)
                .ToList();


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
    }

    public class StrategyRestrictionsManager : TradingRestrictionManager
    {
        public void SetParentRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Parent);

        public void SetUserRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.User);

        public void SetCriticalLossRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.CriticalLoss);

        public void SetSchedulerRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Scheduler);

        public void SetErrorRestriction(TradingRestriction r) =>
            SetRestriction(r, CommandSource.Error);
    }
}
