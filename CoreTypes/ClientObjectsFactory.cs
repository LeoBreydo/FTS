using System;
using System.Collections.Generic;
using WebClientObjects;

namespace CoreTypes
{
    public static class ClientObjectsFactory
    {
        public static ServerStateObject ComposeServerStateObject(this TradingServiceState serviceState)
        {
            return new ServerStateObject
            {
                Summary = serviceState.GetSummary(),
                Details = serviceState.GetExchangeDetails()
            };
        }
        private static TradingServicesSummary GetSummary(this TradingServiceState serviceState)
        {
            var summary = new TradingServicesSummary
            {
                Id = serviceState.Id,
                IsConnected = serviceState.IsConnected,
                RestrictionDetails = serviceState.Restrictions,
                CurrentRestriction = serviceState.CurrentRestriction,
                MessagesToShow = serviceState.MessagesToShow
            };
            foreach (var cgs in serviceState.CurrencyGroupStates)
            { 
                summary.CGSummaries.Add(cgs.GetCGS());
                foreach (var es in cgs.ExchangeStates) summary.ExSummaries.Add(es.GetES(cgs.Currency));
            }
            return summary;
        }

        private static Dictionary<int, ExchangeDetails> GetExchangeDetails(this TradingServiceState serviceState)
        {
            Dictionary<int, ExchangeDetails> ret = new();
            foreach (var cgs in serviceState.CurrencyGroupStates)
            {
                var currency = cgs.Currency;
                foreach (var es in cgs.ExchangeStates)
                {
                    var ed = es.GetED(currency, serviceState.IsConnected, serviceState.MessagesToShow);
                    foreach (var ms in es.MarketStates)
                    {
                        ed.MktOrStrategies.Add(ms.GetMOSD());
                        foreach (var ss in ms.StrategyStates) ed.MktOrStrategies.Add(ss.GetMOSD());
                    }
                    ret.Add(ed.Id, ed);
                }
            }

            return ret;
        }

        private static CurrencyGroupSummary GetCGS(this CurrencyGroupState cgs)
        {
            return new CurrencyGroupSummary
            {
                Currency = cgs.Currency,
                UPL = cgs.UnrealizedResult,
                RPL = cgs.RealizedResult
            };
        }

        private static ExchangeSummary GetES(this ExchangeState es, string currency)
        {
            return new ExchangeSummary
            {
                Id = es.Id,
                Name = es.Exchange,
                Currency = currency,
                UPL = es.UnrealizedResult,
                RPL = es.RealizedResult,
                CurrentRestriction = es.CurrentRestriction,
            };
        }

        private static ExchangeDetails GetED(this ExchangeState es, string currency, bool isConnected,
            List<Tuple<string, string>> msgToShow)
        {
            return new ExchangeDetails
            {
                Id = es.Id,
                Name = es.Exchange,
                Currency = currency,
                IsConnected = isConnected,
                UPL = es.UnrealizedResult,
                RPL = es.RealizedResult,
                Restrictions = es.Restrictions,
                Info = $"Restriction - {es.CurrentRestriction}; Act. mkts. - {es.ActiveMarketsNbr}; " +
                       $"Act. str. - {es.ActiveStrategiesNbr}; Day err. nbr. - {es.DayErrorNbr}",
                MessagesToShow = msgToShow,
                MktOrStrategies = new()
            };
        }

        private static MarketOrStrategyDetails GetMOSD(this MarketState ms)
        {
            return new MarketOrStrategyDetails
            {
                IsMarket = true,
                Id = ms.Id,
                Name = ms.MarketName + " : " + ms.ContractCode,
                UPL = ms.UnrealizedResult,
                RPL = ms.RealizedResult,
                Position = ms.LongSize + ms.ShortSize,
                Restrictions = ms.Restrictions,
                SessionResult = ms.SessionResult,
                Info =
                    $"Restriction - {ms.CurrentRestriction}; Act. str. - {ms.ActiveStrategiesNbr}; Day err. nbr. - {ms.DayErrorNbr}"
            };
        }

        private static MarketOrStrategyDetails GetMOSD(this StrategyState ss)
        {
            return new MarketOrStrategyDetails
            {
                IsMarket = false,
                Id = ss.Id,
                Name = ss.StrategyName,
                UPL = ss.UnrealizedResult,
                RPL = ss.RealizedResult,
                Position = ss.Size,
                Restrictions = ss.Restrictions,
                SessionResult = ss.SessionResult,
                Info = $"Restriction - {ss.CurrentRestriction}",
            };
        }
    }
}
