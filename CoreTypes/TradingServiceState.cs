using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace CoreTypes
{
    public class TradingServiceState
    {
        public int Id;
        public bool IsConnected;
        public List<Tuple<string, string>> MessagesToShow;
        public int DayErrorNbr;
        public string Restrictions;
        public TradingRestriction CurrentRestriction;
        public List<CurrencyGroupState> CurrencyGroupStates=new ();

        public TradingServiceState(List<Tuple<string, string>> messagesToShow, bool isConnected,
            TradingService ts)
        {
            Id = ts.Id;
            IsConnected = isConnected;
            MessagesToShow = messagesToShow;
            DayErrorNbr = ts.ErrorTracker.ValueForCurrentPeriod;
            Restrictions = ts.RestrictionManager.GetRestrictions();
            CurrentRestriction = ts.RestrictionManager.GetCurrentRestriction();
            foreach (var (key, value) in ts.Positions)
                CurrencyGroupStates.Add(new CurrencyGroupState(key, value));
        }

        public TradingServicesSummary GetSummary => new(this);
        public Dictionary<int,ExchangeDetails> GetExchangeDetails()
        {
            Dictionary<int, ExchangeDetails> ret = new();
            foreach (var cgs in CurrencyGroupStates)
            {
                var currency = cgs.Currency;
                foreach(var es in cgs.ExchangeStates) ret.Add(es.Id,new ExchangeDetails(es, currency, IsConnected, MessagesToShow));
            }

            return ret;
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
        public TradingRestriction CurrentRestriction;
        public int ActiveMarketsNbr;
        public int ActiveStrategiesNbr;
        public List<MarketState> MarketStates = new ();
        public int DayErrorNbr;

        public ExchangeState(ExchangeTrader et)
        {
            Id = et.Id;
            Exchange = et.Exchange;
            UnrealizedResult = et.Position.UnrealizedResult;
            RealizedResult = et.Position.RealizedResult;
            Restrictions = et.RestrictionsManager.GetRestrictions();
            CurrentRestriction = et.RestrictionsManager.GetCurrentRestriction();
            DayErrorNbr = et.ErrorTracker.ValueForCurrentPeriod;

            foreach (var mt in et.Markets.Values)
            {
                var ms = new MarketState(mt);
                if (ms.CurrentRestriction == TradingRestriction.NoRestrictions)
                {
                    ++ActiveMarketsNbr;
                    ActiveStrategiesNbr += ms.ActiveStrategiesNbr;
                }
                MarketStates.Add(ms);
            }
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
        public TradingRestriction CurrentRestriction;
        public List<StrategyState> StrategyStates = new();
        public int DayErrorNbr;
        public int ActiveStrategiesNbr;

        public MarketState(MarketTrader mt)
        {
            Id = mt.Id;
            MarketName = mt.MarketCode;
            ContractCode = mt.ContractCode;
            UnrealizedResult = mt.Position.UnrealizedResult;
            RealizedResult = mt.Position.RealizedResult;
            LongSize = mt.Position.LongSize;
            ShortSize = mt.Position.ShortSize;
            SessionResult = mt.Position.LossManager.ValueForCurrentPeriod;
            Restrictions = mt.RestrictionsManager.GetRestrictions();
            CurrentRestriction = mt.RestrictionsManager.GetCurrentRestriction();
            DayErrorNbr = mt.ErrorTracker.ValueForCurrentPeriod;
            foreach (var st in mt.StrategyMap.Values)
            {
                var ss = new StrategyState(st);
                if (ss.CurrentRestriction == TradingRestriction.NoRestrictions) ++ActiveStrategiesNbr;
                StrategyStates.Add(ss);
            }
        }
    }

    public class StrategyState
    {
        public int Id;
        public string StrategyName;
        public decimal UnrealizedResult;
        public decimal RealizedResult;
        public int Size;
        public decimal SessionResult;
        public string Restrictions;
        public TradingRestriction CurrentRestriction;

        public StrategyState(StrategyTrader st)
        {
            Id = st.Id;
            StrategyName = st.Position.StrategyName;
            UnrealizedResult = st.Position.UnrealizedResult;
            RealizedResult = st.Position.RealizedResult;
            Size = st.Position.Size;
            SessionResult = st.Position.LossManager.ValueForCurrentPeriod;
            Restrictions = st.RestrictionsManager.GetRestrictions();
            CurrentRestriction = st.RestrictionsManager.GetCurrentRestriction();
        }
    }

    public class TradingServicesSummary
    {
        public int Id;
        public bool IsConnected;
        public string RestrictionDetails;
        public string CurrentRestriction;
        public int DayErrorNbr;
        public List<CurrencyGroupSummary> CGSummaries = new();
        public List<ExchangeSummary> ExSummaries = new();
        public List<Tuple<string, string>> MessagesToShow;

        public TradingServicesSummary(TradingServiceState ts)
        {
            Id = ts.Id;
            IsConnected = ts.IsConnected;
            RestrictionDetails = ts.Restrictions;
            CurrentRestriction = ts.CurrentRestriction.AsString();
            foreach (var cgs in ts.CurrencyGroupStates)
            {
                CGSummaries.Add(new CurrencyGroupSummary(cgs));
                foreach (var es in cgs.ExchangeStates) ExSummaries.Add(new ExchangeSummary(es,cgs.Currency));
            }
            MessagesToShow = ts.MessagesToShow;
        }
    }

    public class CurrencyGroupSummary
    {
        public string Currency;
        public decimal UPL;
        public decimal RPL;

        public CurrencyGroupSummary(CurrencyGroupState cs)
        {
            Currency = cs.Currency;
            UPL = cs.UnrealizedResult;
            RPL = cs.RealizedResult;
        }
    }

    public class ExchangeSummary
    {
        public int Id;
        public string Name;
        public string Currency;
        public decimal UPL;
        public decimal RPL;
        public string CurrentRestriction;

        public ExchangeSummary(ExchangeState es, string currency)
        {
            Id = es.Id;
            Name = es.Exchange;
            Currency = currency;
            UPL = es.UnrealizedResult;
            RPL = es.RealizedResult;
            CurrentRestriction = es.CurrentRestriction.AsString();
        }
    }

    public class ExchangeDetails
    {
        public int Id;
        public string Name;
        public string Currency;
        public bool IsConnected;
        public decimal UPL;
        public decimal RPL;
        public string Restrictions;
        private string Info;
        public List<Tuple<string, string>> MessagesToShow;
        public List<MarketOrStrategyDetails> MktOrStrategies = new();

        public ExchangeDetails(ExchangeState es, string currency, bool isConnected, List<Tuple<string, string>> messagesToShow)
        {
            Id = es.Id;
            Name = es.Exchange;
            Currency = currency;
            IsConnected = isConnected;
            UPL = es.UnrealizedResult;
            RPL = es.RealizedResult;
            Restrictions = es.Restrictions;
            Info = $"Restriction - {es.CurrentRestriction.AsString()}; Act. mkts. - {es.ActiveMarketsNbr}; " +
                   $"Act. str. - {es.ActiveStrategiesNbr}; Day err. nbr. - {es.DayErrorNbr}";
            MessagesToShow = messagesToShow;
            foreach (var ms in es.MarketStates)
            {
                MktOrStrategies.Add(new MarketOrStrategyDetails(ms));
                foreach (var ss in ms.StrategyStates) MktOrStrategies.Add(new MarketOrStrategyDetails(ss));
            }
        }
    }

    public class MarketOrStrategyDetails
    {
        public bool IsMarket;
        public int Id;
        public string Name;
        public decimal UPL;
        public decimal RPL;
        public int Position;
        public string Restrictions;
        public decimal SessionResult;
        public string Info;

        public MarketOrStrategyDetails(MarketState ms)
        {
            IsMarket = true;
            Id = ms.Id;
            Name = ms.MarketName + " : " + ms.ContractCode;
            UPL = ms.UnrealizedResult;
            RPL = ms.RealizedResult;
            Position = ms.LongSize + ms.ShortSize;
            Restrictions = ms.Restrictions;
            SessionResult = ms.SessionResult;
            Info = $"Restriction - {ms.CurrentRestriction.AsString()}; Act. str. - {ms.ActiveStrategiesNbr}; Day err. nbr. - {ms.DayErrorNbr}";
        }

        public MarketOrStrategyDetails(StrategyState ss)
        {
            IsMarket = false;
            Id = ss.Id;
            Name = ss.StrategyName;
            UPL = ss.UnrealizedResult;
            RPL = ss.RealizedResult;
            Position = ss.Size;
            Restrictions = ss.Restrictions;
            SessionResult = ss.SessionResult;
            Info = $"Restriction - {ss.CurrentRestriction.AsString()}";
        }
    }
}
