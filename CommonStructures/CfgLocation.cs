namespace CommonStructures
{
    public class CfgLocation
    {
        public string FtpRootPath;
        //public string IndicatorsPath;
        public string SystemIndicatorsFolder;
        public string UserIndicatorsFolder;
        public int MaxIndicatorHistorySize = 1000;

        public string SignalTransformerLogicPath;
        public string MarketFiltersPath;
        public string DirectionalFiltersPath;
        public string TrendMonitorsPath;
    }
}
