using System;
using System.Collections.Generic;

namespace CoreTypes
{
    public class TradingServiceState
    {
        public List<Tuple<string, string>> MessagesToShow;
        public int DayErrorNbr;
        public string Restrictions;
        public List<CurrencyGroupState> CurrencyGroupStates=new ();

        public TradingServiceState(List<Tuple<string, string>> messagesToShow,
            TradingService ts)
        {
            MessagesToShow = messagesToShow;
            DayErrorNbr = ts.ErrorCollector.Errors;
            Restrictions = ts.RestrictionManager.GetRestrictions();
            foreach (var (key, value) in ts.Positions)
                CurrencyGroupStates.Add(new CurrencyGroupState(key, value));
        }
    }

    public class CurrencyGroupState
    {
        public string Currency;
        public decimal UnrealizedResult;
        public decimal RealizedResult;
        public List<ExchangeState> ExchangeStates=new();

        public CurrencyGroupState(string currency, CurrencyPosition position)
        {
            Currency = currency;
            UnrealizedResult = position.UnrealizedResult;
            RealizedResult = position.RealizedResult;
            foreach (var ep in position.ExchangePositions) ExchangeStates.Add(new ExchangeState(ep.Owner));
        }
    }

    public class ExchangeState
    {
        public int Id;
        public string Exchange;
        public decimal UnrealizedResult;
        public decimal RealizedResult;
        public string Restrictions;
        public List<MarketState> MarketStates = new List<MarketState>();

        public ExchangeState(ExchangeTrader et)
        {
            Id = et.Id;
            Exchange = et.Exchange;
            UnrealizedResult = et.Position.UnrealizedResult;
            RealizedResult = et.Position.RealizedResult;
            Restrictions = et.RestrictionsManager.GetRestrictions();
            foreach (var mt in et.Markets.Values) MarketStates.Add(new MarketState(mt));
        }
    }

    public class MarketState
    {
        public int Id;
        public string MarketName;
        public string ContractCode;
        public decimal UnrealizedResult;
        public decimal RealizedResult;
        public int LongSize;
        public int ShortSize;
        public decimal SessionResult;
        public string Restrictions;
        public List<StrategyState> StrategyStates = new();

        public MarketState(MarketTrader mt)
        {
            Id = mt.Id;
            MarketName = mt.MarketCode;
            ContractCode = mt.ContractCode;
            UnrealizedResult = mt.Position.UnrealizedResult;
            RealizedResult = mt.Position.RealizedResult;
            LongSize = mt.Position.LongSize;
            ShortSize = mt.Position.ShortSize;
            SessionResult = mt.Position.LossManager.SessionResult;
            Restrictions = mt.RestrictionsManager.GetRestrictions();
            foreach (var st in mt.StrategyMap.Values) StrategyStates.Add(new StrategyState(st));
        }
    }

    public class StrategyState
    {
        public int Id;
        public decimal UnrealizedResult;
        public decimal RealizedResult;
        public int Size;
        public decimal SessionResult;
        public string Restrictions;

        public StrategyState(StrategyTrader st)
        {
            Id = st.Id;
            UnrealizedResult = st.Position.UnrealizedResult;
            RealizedResult = st.Position.RealizedResult;
            Size = st.Position.Size;
            SessionResult = st.Position.LossManager.SessionResult;
            Restrictions = st.RestrictionsManager.GetRestrictions();
        }
    }
}
