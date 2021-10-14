using System;
using System.Collections.Generic;
using LocalCommunicationLib;

namespace CoreTypes
{
    public class TradingServiceState
    {
        public int Id;
        public bool IsConnected;
        public List<Tuple<string, string>> MessagesToShow;
        public int DayErrorNbr;
        public Restrictions CurrentRestrictions;
        public List<CurrencyGroupState> CurrencyGroupStates=new ();

        public TradingServiceState(List<Tuple<string, string>> messagesToShow, bool isConnected,
            TradingService ts)
        {
            Id = ts.Id;
            IsConnected = isConnected;
            MessagesToShow = messagesToShow;
            DayErrorNbr = ts.ErrorTracker.ValueForCurrentPeriod;
            CurrentRestrictions = ts.RestrictionManager.GetRestrictionsAsObject();
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
        public Restrictions CurrentRestrictions;
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
            CurrentRestrictions = et.RestrictionsManager.GetRestrictionsAsObject();
            DayErrorNbr = et.ErrorTracker.ValueForCurrentPeriod;

            foreach (var mt in et.Markets.Values)
            {
                var ms = new MarketState(mt);
                if (ms.IsActive)
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
        public Restrictions CurrentRestrictions;
        public List<StrategyState> StrategyStates = new();
        public int DayErrorNbr;
        public int ActiveStrategiesNbr;
        public bool IsActive;

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
            CurrentRestrictions = mt.RestrictionsManager.GetRestrictionsAsObject();
            DayErrorNbr = mt.ErrorTracker.ValueForCurrentPeriod;
            IsActive = mt.RestrictionsManager.GetCurrentRestriction() == TradingRestriction.NoRestrictions;
            foreach (var st in mt.StrategyMap.Values)
            {
                var ss = new StrategyState(st);
                if (ss.IsActive) ++ActiveStrategiesNbr;
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
        public Restrictions CurrentRestrictions;
        public bool IsActive;

        public StrategyState(StrategyTrader st)
        {
            Id = st.Id;
            StrategyName = st.Position.StrategyName;
            UnrealizedResult = st.Position.UnrealizedResult;
            RealizedResult = st.Position.RealizedResult;
            Size = st.Position.Size;
            SessionResult = st.Position.LossManager.ValueForCurrentPeriod;
            CurrentRestrictions = st.RestrictionsManager.GetRestrictionsAsObject();
            IsActive = st.RestrictionsManager.GetCurrentRestriction() == TradingRestriction.NoRestrictions;
        }
    }
}
