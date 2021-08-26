using System;
using System.Globalization;
using IBApi;

namespace CoreTypes
{
    public class StrategyOrderInfo
    {
        public readonly int StrategyId;
        public int NbrOfContracts;

        public StrategyOrderInfo(int strategyId, int nbrOfContracts)
        {
            NbrOfContracts = nbrOfContracts;
            StrategyId = strategyId;
        }
    }

    public class ContractInfo
    {
        public string MarketName { get; }
        public string Exchange { get; set; }
        public string LocalSymbol { get; }
        public DateTime ContractMonth { get; }
        public string TimeZoneId { get; set; }
        public DateTime StartLiquidHours { get; set; }
        public DateTime EndLiquidHours { get; set; }
        public DateTime OpenMarket { get; set; }
        public DateTime CloseMarket { get; set; }
        public DateTime ExpirationDate { get; }
        public DateTime LastTradeTime { get; set; }
        public int Multiplier { get; }
        public double MinTick { get; }

        public ContractInfo(ContractDetails cd)
        {
            MarketName = cd.Contract.Symbol;//MarketName = cd.MarketName; // do not use cd.MarketName. Example: subsribed to BAKKT@ICECRYPTO, Symbol='BAKKT'; MarketName='BTM'; LocalSymbol(contract)='BTMU1'
            Exchange = cd.Contract.Exchange;
            LocalSymbol = cd.Contract.LocalSymbol;
            //yyyymmdd
            ContractMonth = DateTime.ParseExact(cd.ContractMonth, "yyyyMM", CultureInfo.InvariantCulture);
            TimeZoneId = cd.TimeZoneId;
            //20180323:0930-20180323:1600; etc
            var parts = cd.LiquidHours.Split(";");
            var fst = parts[0].Split("-");
            (StartLiquidHours, EndLiquidHours) =
                (DateTime.ParseExact(fst[0], "yyyyMMdd:HHmm", CultureInfo.InvariantCulture),
                    DateTime.ParseExact(fst[1], "yyyyMMdd:HHmm", CultureInfo.InvariantCulture));
            parts = cd.TradingHours.Split(";");
            fst = parts[0].Split("-");
            (OpenMarket,CloseMarket) =
                (DateTime.ParseExact(fst[0], "yyyyMMdd:HHmm", CultureInfo.InvariantCulture),
                    DateTime.ParseExact(fst[1], "yyyyMMdd:HHmm", CultureInfo.InvariantCulture));
            //20210831
            ExpirationDate = DateTime.ParseExact(cd.RealExpirationDate, "yyyyMMdd",
                CultureInfo.InvariantCulture);
            parts = cd.LastTradeTime.Split(":");
            var (h, m) = (int.Parse(parts[0]), int.Parse(parts[1]));
            LastTradeTime = ExpirationDate.AddHours(h).AddMinutes(m);
            Multiplier = int.Parse(cd.Contract.Multiplier);
            MinTick = cd.MinTick;
        }

        public override string ToString()
        {
            return $"{MarketName}, {LocalSymbol}, {TimeZoneId}, {StartLiquidHours}, {EndLiquidHours}, {LastTradeTime}";
        }
    }
}
