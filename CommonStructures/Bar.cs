using System;
using System.Globalization;

namespace CommonStructures
{
    /// <summary>
    /// класс используется в построителе индикаторов и при наполнении/ закачке истории 
    /// </summary>
    public class Bar
    {
        public DateTime OpenTime;
        public DateTime CloseTime;
        public double[] OHLC;

        public Bar() { }
        public Bar(Bar from)
        {
            OpenTime = from.OpenTime;
            CloseTime = from.CloseTime;
            OHLC = (double[])from.OHLC.Clone();
        }
        public Bar(DateTime openTime, DateTime closeTime, double open, double high, double low, double close)
        {
            OpenTime = openTime;
            CloseTime = closeTime;
            OHLC = new[] { open, high, low, close };
        }

    }

    public static class BamBarFileFormat
    {
        private const string barTimeFormat = "yyyyMMdd HHmm";

        public const string Title =
            "BarOpenTime\tProvider\tSymbol\tBidOpen\tBidHigh\tBidLow\tBidClose\tAskOpen\tAskHigh\tAskLow\tAskClose\tMiddleOpen\tMiddleHigh\tMiddleLow\tMiddleClose";

        public const string FileExtension = ".bambar";
        public const string FileMask = "*.bambar";
        public static string BarOpenTimeToString(DateTime barOpenTime)
        {
            return barOpenTime.ToString(barTimeFormat);
        }
        public static string ToTextTableRowBidAskMiddle(long providerID, string currencyPair, Bar[] bamBars)
        {
            return string.Join("\t",

                BarOpenTimeToString(bamBars[0].OpenTime),
                providerID.ToString(CultureInfo.InvariantCulture),
                currencyPair,

                bamBars[0].OHLC[0].ToString(CultureInfo.InvariantCulture),
                bamBars[0].OHLC[1].ToString(CultureInfo.InvariantCulture),
                bamBars[0].OHLC[2].ToString(CultureInfo.InvariantCulture),
                bamBars[0].OHLC[3].ToString(CultureInfo.InvariantCulture),

                bamBars[1].OHLC[0].ToString(CultureInfo.InvariantCulture),
                bamBars[1].OHLC[1].ToString(CultureInfo.InvariantCulture),
                bamBars[1].OHLC[2].ToString(CultureInfo.InvariantCulture),
                bamBars[1].OHLC[3].ToString(CultureInfo.InvariantCulture),

                bamBars[2].OHLC[0].ToString(CultureInfo.InvariantCulture),
                bamBars[2].OHLC[1].ToString(CultureInfo.InvariantCulture),
                bamBars[2].OHLC[2].ToString(CultureInfo.InvariantCulture),
                bamBars[2].OHLC[3].ToString(CultureInfo.InvariantCulture));

        }
        public static Tuple<long, string, Bar[]> GetBidAskMiddleBarsFromTextTableRow(this string row)
        {
            string[] cells = GetProviderCurrencyPairFromTableRow(row, out long providerID, out string symbol);
            if (cells == null) return null; //  invalid row

            Bar[] bamBars = cells.GetBarsFromTableRow();
            if (bamBars == null) return null;
            return new Tuple<long, string, Bar[]>(providerID, symbol, bamBars);
        }
        public static string[] GetProviderCurrencyPairFromTableRow(string row, out long providerID, out string currencyPair)
        {
            string[] cells = row.Split('\t');
            if (cells.Length != 15)
            {
                providerID = 0;
                currencyPair = null;
                return null;
            }
            currencyPair = cells[2];
            if (!long.TryParse(cells[1], out providerID) || providerID <= 0 || string.IsNullOrEmpty(currencyPair))
                return null;

            return cells;
        }

        private static Bar[] GetBarsFromTableRow(this string[] cells)
        {
            DateTime dt;
            if (!DateTime.TryParseExact(cells[0], barTimeFormat, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
                return null;

            var ohlc = new double[12];
            for (int i = 0; i < 12; ++i)
                if (!double.TryParse(cells[i + 3], out ohlc[i])) return null;
            DateTime closeTime = dt.AddMinutes(1);
            return new[]
            {
                new Bar
                {
                    OpenTime = dt,
                    CloseTime = closeTime,
                    OHLC = new[] {ohlc[0], ohlc[1], ohlc[2], ohlc[3]}
                },
                new Bar
                {
                    OpenTime = dt,
                    CloseTime = closeTime,
                    OHLC = new[] {ohlc[4], ohlc[5], ohlc[6], ohlc[7]}
                },
                new Bar
                {
                    OpenTime = dt,
                    CloseTime = closeTime,
                    OHLC = new[] {ohlc[8], ohlc[9], ohlc[10], ohlc[11]}
                }
            };
        }
        
    }
    public static class BarEx
    {
        private const string barTimeFormat = "yyyyMMdd HHmm";
        public static string BarOpenTimeToString(DateTime barOpenTime)
        {
            return barOpenTime.ToString(barTimeFormat);
        }
        public static string ToTextTableRow(long providerID,string currencyPair,Bar bar)
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                BarOpenTimeToString(bar.OpenTime),
                providerID,
                currencyPair,
                bar.OHLC[0].ToString(CultureInfo.InvariantCulture),
                bar.OHLC[1].ToString(CultureInfo.InvariantCulture),
                bar.OHLC[2].ToString(CultureInfo.InvariantCulture),
                bar.OHLC[3].ToString(CultureInfo.InvariantCulture));
        }
        public static string[] SplitStrBar(string strBar)
        {
            string[] cells = strBar.Split('\t');
            return (cells.Length == 7) ? cells : null;
        }


        public const int ColTime = 0;
        public const int ColProvider = 1;
        public const int ColSymbol = 2;
        public const int ColOHLC = 3;

        public static Tuple<long, string, Bar> GetBarFromTextTableRow(this string row)
        {
            long providerID;
            string symbol;
            string[] cells = GetProviderCurrencyPairFromTableRow(row, out providerID, out symbol);
            if (cells == null) return null; //  invalid row
            Bar bar = cells.GetBarFromTableRow();
            if (bar==null) return null;
            return new Tuple<long, string, Bar>(providerID, symbol, bar);
        }
   
        public static string[] GetProviderCurrencyPairFromTableRow(string row,out long providerID,out string currencyPair)
        {
            string[] cells = row.Split('\t');
            if (cells.Length!=7)
            {
                providerID = 0;
                currencyPair = null;
                return null;
            }
            currencyPair = cells[2];
            if (!long.TryParse(cells[1],out providerID)|| providerID<=0 || string.IsNullOrEmpty(currencyPair))
                return null;

            return cells;
        }
        private static Bar GetBarFromTableRow(this string[] cells)
        {
            DateTime dt;
            if (!DateTime.TryParseExact(cells[0], barTimeFormat, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
                return null;
            var ohlc = new double[4];
            for(int i=0;i<4;++i)
                if (!double.TryParse(cells[i + 3], out ohlc[i])) return null;
            return new Bar
                       {
                           OpenTime = dt,
                           CloseTime = dt.AddMinutes(1),
                           OHLC = ohlc
                       };
        }
    }
}
