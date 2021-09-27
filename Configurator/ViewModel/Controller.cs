using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Configurator.ViewModel
{
    public class Controller
    {
        public const string STR_Configuration = "Configuration";
        public static Controller Instance { get; private set; }
        public readonly ListOfCurrencies ListOfCurrencies;
        private readonly string _strategiesFolder;
        private readonly string _localCfgFileName;

        private TradingConfiguration _tradingConfiguration;

        private List<ExchangeRow> _exchanges;
        private List<MarketRow> _markets;
        private List<StrategyRow> _strategies;
        public string LoadError { get; private set; }
        public Controller(string strategiesFolder)
        {
            Instance = this;
            _strategiesFolder = strategiesFolder;
            ReloadStrategyDlls();
            ListOfCurrencies = new ListOfCurrencies(Path.Combine(strategiesFolder, "Currencies.txt"));
            _localCfgFileName = Path.Combine(strategiesFolder, "LocalCfg.xml");
        }

        public void Save()
        {
            //Serializer<TradingConfiguration>.Save(_localCfgFileName,)
        }

        public void ReloadStrategyDlls()
        {
            _exchanges = new List<ExchangeRow>();
            _markets = new List<MarketRow>();
            _strategies = new List<StrategyRow>();
            LoadError = null;
            try
            {
                if (string.IsNullOrEmpty(_strategiesFolder))
                    return;
                if (!Directory.Exists(_strategiesFolder))
                {
                    LoadError = "Strategies folder not exists: " + _strategiesFolder;
                    return;
                }
                foreach (string exchDir in Directory.GetDirectories(_strategiesFolder))
                {
                    string exchangeName = Path.GetFileName(exchDir);
                    if (!exchangeName.IsIdentifier()) continue;

                    ExchangeConfiguration xcfg;
                    _exchanges.Add(new ExchangeRow
                    {
                        Cfg = xcfg=new ExchangeConfiguration
                        {
                            ExchangeName = exchangeName
                        }
                    });

                    foreach (string mktDir in Directory.GetDirectories(exchDir))
                    {
                        string marketName = Path.GetFileName(mktDir);
                        if (!marketName.IsIdentifier()) continue;

                        MarketConfiguration mcfg;
                        _markets.Add(new MarketRow
                        {
                            Cfg = mcfg=new MarketConfiguration
                            {
                                Exchange = exchangeName,
                                MarketName= marketName,
                            }
                        });
                        xcfg.Markets.Add(mcfg);

                        foreach (var dllFile in Directory.GetFiles(mktDir))
                        {
                            if (SignalGeneratorsRegistryMaker.GetDescription(dllFile,
                                out SignalGeneratorDescription sd) != null)
                                continue;

                            var sr = new StrategyRow(exchangeName, marketName, sd)
                            {
                                Properties = PropertyValue.LoadFromFile(Path.ChangeExtension(dllFile, ".txt"))
                            };
                            _strategies.Add(sr);
                            mcfg.Strategies.Add(sr.Cfg);
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                LoadError = exception.Message;
            }
        }


        public void SetTradingConfiguration(TradingConfiguration tcfg)
        {
            _tradingConfiguration = tcfg;
            // todo reattach cfg to loaded strategies and saved settings?
            // todo to check strategy parameters compatibility to default parameters
        }

        public List<ExchangeRow> GetExchanges(bool onlyWorkingSet)
        {
            return onlyWorkingSet
                ? _exchanges.Where(x => x.IsInWorkingSet()).ToList()
                : _exchanges;
        }

        public List<MarketRow> GetMarkets(bool onlyWorkingSet) 
        {
            return onlyWorkingSet
                ? _markets.Where(x => x.IsInWorkingSet()).ToList()
                : _markets;
        }
        public List<StrategyRow> GetStrategies(bool onlyWorkingSet)
        {
            return onlyWorkingSet
                ? _strategies.Where(x => x.Cfg.NbrOfContracts > 0).ToList()
                : _strategies;
        }
        public IRow GetGroupRow(List<string> groupPath)
        {
            if (groupPath.Count == 0 || groupPath.Last() != STR_Configuration) return null;

            switch (groupPath.Count)
            {
                default:
                    return null;
                case 1:
                    return new TCfgRow(_tradingConfiguration);
                case 2:
                    return _exchanges.FirstOrDefault(item => item.Exchange == groupPath[0]);
                case 3:
                    return _markets.FirstOrDefault(item =>
                        item.MarketName == groupPath[0] && item.Exchange == groupPath[1]);
            }
        }

    }
}
