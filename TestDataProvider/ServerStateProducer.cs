using System;
using System.Collections.Generic;
using LocalCommunicationLib;

namespace TestDataProvider
{
    public class ServerStateProducer : IServerStateObjectProvider
    {
        private static int _idx = 0;
        private static Random Rand = new(12345);
        public ServerStateObject GetState => ComposePayload();

        private static ServerStateObject ComposePayload()
        {
            return new ServerStateObject
            {
                Summary = GetSummary(),
                Details = GetExchangeDetails()
            };
        }

        private static TradingServicesSummary GetSummary()
        {
            var summary = new TradingServicesSummary
            {
                Id = 0,
                IsConnected = true,
                DayErrorNbr = Rand.Next(4),
                RestrictionDetails = new Restrictions { userStyle = Rand.Next(3), sessStyle = Rand.Next(3), parStyle = Rand.Next(3) },
                MessagesToShow = new List<Message>
                {
                    new() {Tag = ++_idx+"", Body = "2"},
                    new() {Tag = ++_idx+"", Body = "4"}
                }
            };
            summary.CGSummaries.Add(new CurrencyGroupSummary
            {
                Currency = "USD",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000 - 500, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000 - 500, 2)
            });
            summary.ExSummaries.Add(new ExchangeSummary
            {
                Id = 1,
                Name = "exchange 1",
                Currency = "USD",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),

                RestrictionDetails = new Restrictions {userStyle=1,sessStyle=2}
            });
            summary.ExSummaries.Add(new ExchangeSummary
            {
                Id = 2,
                Name = "exchange 2",
                Currency = "EUR",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RestrictionDetails = new()
            });

            summary.CGSummaries.Add(new CurrencyGroupSummary
            {
                Currency = "EUR",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000 - 500, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000 - 500, 2)
            });
            summary.ExSummaries.Add(new ExchangeSummary
            {
                Id = 100,
                Name = "exchange 100",
                Currency = "USD",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),

                RestrictionDetails = new Restrictions { userStyle = 1, sessStyle = 2 }
            });
            summary.ExSummaries.Add(new ExchangeSummary
            {
                Id = 200,
                Name = "exchange 200",
                Currency = "EUR",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RestrictionDetails = new()
            });

            return summary;
        }

        private static Dictionary<string, ExchangeDetails> GetExchangeDetails()
        {
            Dictionary<string, ExchangeDetails> ret = new();
            var ed = new ExchangeDetails
            {
                Id = 1,
                Name = "exchange 1",
                Currency = "USD",
                IsConnected = true,
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RestrictionDetails = new(),
                Info = "Act. mkts. - 1; Act. str. - 1;",
                MessagesToShow = new List<Message>
                {
                    new() {Tag = "1", Body = "2"},
                    new() {Tag = "3", Body = "4"}
                },
                MktOrStrategies = new(),
                DayErrorNbr = 3
            };
            ed.MktOrStrategies.Add(new MarketOrStrategyDetails
            {
                IsMarket = true,
                Id = 5,
                Name = "mname1 : mname1u7",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Position = 3,
                RestrictionDetails = new(),
                SessionResult = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Info = $"Act. str. - 1; Day err. nbr. - 0"
            });
            ed.MktOrStrategies.Add(new MarketOrStrategyDetails
            {
                IsMarket = false,
                Id = 6,
                Name = "str1",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Position = 3,
                RestrictionDetails = new(),
                SessionResult = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Info = "",
            });
            ret.Add(ed.Name, ed);

            ed = new ExchangeDetails
            {
                Id = 2,
                Name = "exchange 2",
                Currency = "EUR",
                IsConnected = true,
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RestrictionDetails = new(),
                Info = "Act. mkts. - 1; Act. str. - 1;",
                MessagesToShow = new List<Message>
                {
                    new() {Tag = "1", Body = "2"},
                    new() {Tag = "3", Body = "4"}
                },
                MktOrStrategies = new(),
                DayErrorNbr = 0
            };
            ed.MktOrStrategies.Add(new MarketOrStrategyDetails
            {
                IsMarket = true,
                Id = 15,
                Name = "mname2 : mname2u9",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Position = 2,
                RestrictionDetails = new(),
                SessionResult = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Info = $" Act. str. - 0; Day err. nbr. - 0"
            });
            ed.MktOrStrategies.Add(new MarketOrStrategyDetails
            {
                IsMarket = false,
                Id = 16,
                Name = "str2",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Position = 2,
               RestrictionDetails = new(),
                SessionResult = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Info = "",
            });
            ret.Add(ed.Name, ed);

            ed = new ExchangeDetails
            {
                Id = 100,
                Name = "exchange 100",
                Currency = "USD",
                IsConnected = true,
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RestrictionDetails = new(),
                Info = "Act. mkts. - 1; Act. str. - 1;",
                MessagesToShow = new List<Message>
                {
                    new() {Tag = "1", Body = "2"},
                    new() {Tag = "3", Body = "4"}
                },
                MktOrStrategies = new(),
                DayErrorNbr = 3
            };
            ed.MktOrStrategies.Add(new MarketOrStrategyDetails
            {
                IsMarket = true,
                Id = 50,
                Name = "mname1 : mname1u7",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Position = 3,
                RestrictionDetails = new(),
                SessionResult = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Info = $"Act. str. - 1; Day err. nbr. - 0"
            });
            ed.MktOrStrategies.Add(new MarketOrStrategyDetails
            {
                IsMarket = false,
                Id = 60,
                Name = "str1",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Position = 3,
                RestrictionDetails = new(),
                SessionResult = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Info = "",
            });
            ret.Add(ed.Name, ed);

            ed = new ExchangeDetails
            {
                Id = 200,
                Name = "exchange 200",
                Currency = "EUR",
                IsConnected = true,
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RestrictionDetails = new(),
                Info = "Act. mkts. - 1; Act. str. - 1;",
                MessagesToShow = new List<Message>
                {
                    new() {Tag = "1", Body = "2"},
                    new() {Tag = "3", Body = "4"}
                },
                MktOrStrategies = new(),
                DayErrorNbr = 0
            };
            ed.MktOrStrategies.Add(new MarketOrStrategyDetails
            {
                IsMarket = true,
                Id = 150,
                Name = "mname2 : mname2u9",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Position = 2,
                RestrictionDetails = new(),
                SessionResult = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Info = $" Act. str. - 0; Day err. nbr. - 0"
            });
            ed.MktOrStrategies.Add(new MarketOrStrategyDetails
            {
                IsMarket = false,
                Id = 160,
                Name = "str2",
                UPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                RPL = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Position = 2,
                RestrictionDetails = new(),
                SessionResult = (decimal)Math.Round(Rand.NextDouble() * 1000, 2),
                Info = "",
            });
            ret.Add(ed.Name, ed);

            return ret;
        }
    }
}
