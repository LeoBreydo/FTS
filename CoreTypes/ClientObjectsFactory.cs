using System;
using System.Collections.Generic;
using System.Linq;
using LocalCommunicationLib;

namespace CoreTypes
{
    public static class ClientObjectsFactory
    {
        public static ServerStateObject ComposePayload(this TradingServiceState serviceState)
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
                DayErrorNbr = serviceState.DayErrorNbr,
                RestrictionDetails = serviceState.CurrentRestrictions,
                MessagesToShow = serviceState.MessagesToShow.Select(m=>new Message{Tag = m.Item1, Body = m.Item2}).ToList()
            };
            foreach (var cgs in serviceState.CurrencyGroupStates)
            {
                summary.CGSummaries.Add(cgs.GetCGS());
                foreach (var es in cgs.ExchangeStates) summary.ExSummaries.Add(es.GetES(cgs.Currency));
            }
            return summary;
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
                RestrictionDetails = es.CurrentRestrictions
            };
        }


        private static Dictionary<string, ExchangeDetails> GetExchangeDetails(this TradingServiceState serviceState)
        {
            Dictionary<string, ExchangeDetails> ret = new();
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
                    ret.Add(ed.Name, ed);
                }
            }

            return ret;
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
                RestrictionDetails = es.CurrentRestrictions,
                Info = $"Act. mkts. - {es.ActiveMarketsNbr}; Act. str. - {es.ActiveStrategiesNbr}; ",
                MessagesToShow = msgToShow.Select(m => new Message { Tag = m.Item1, Body = m.Item2 }).ToList(),
                DayErrorNbr = es.DayErrorNbr,
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
                RestrictionDetails = ms.CurrentRestrictions,
                SessionResult = ms.SessionResult,
                Info = $"Act. str. - {ms.ActiveStrategiesNbr}; Day err. nbr. - {ms.DayErrorNbr}"
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
                RestrictionDetails = ss.CurrentRestrictions,
                SessionResult = ss.SessionResult,
                Info = ""
            };
        }




        // old



        

        

        

        
    }
}
