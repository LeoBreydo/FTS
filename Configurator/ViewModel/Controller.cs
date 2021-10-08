using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Policy;

namespace Configurator.ViewModel
{
    public class Controller
    {
        public const string STR_Configuration = "Configuration";
        public static Controller Instance { get; private set; }
        public readonly ListOfCurrencies ListOfCurrencies;
        private readonly string _strategiesFolder;
        private readonly string _localCfgFileName;
        private readonly string _localScheduleFileName;

        private TradingConfiguration _tradingConfiguration;
        private List<ScheduledIntervalDescription> _scheduleItems;
        public BindingList<ScheduledIntervalDescription> ScheduleItemsBList;

        private TCfgRow _cfgRow;
        private List<ExchangeRow> _exchanges;
        private List<MarketRow> _markets;
        private List<StrategyRow> _strategies;
        //public string LoadError { get; private set; }
        public Controller(string strategiesFolder)
        {
            Instance = this;
            _strategiesFolder = strategiesFolder;

            ListOfCurrencies = new ListOfCurrencies(Path.Combine(strategiesFolder, "Currencies.txt"));
            _localCfgFileName = Path.Combine(strategiesFolder, "Cfg.xml");
            _localScheduleFileName = Path.Combine(strategiesFolder, "Schedule.xml");

            InitFromConfiguration(Serializer<TradingConfiguration>.Open(_localCfgFileName) ?? new TradingConfiguration(),
                Serializer<List<ScheduledIntervalDescription>>.Open(_localScheduleFileName) ??
                new List<ScheduledIntervalDescription>());
            RefreshStrategyDlls();
        }

        private void InitFromConfiguration(TradingConfiguration tcgf,
            List<ScheduledIntervalDescription> scheduleItems)
        {
            _tradingConfiguration = tcgf;
            _cfgRow = new TCfgRow(this, _tradingConfiguration);
            _exchanges = new List<ExchangeRow>();
            _markets = new List<MarketRow>();
            _strategies = new List<StrategyRow>();

            foreach (ExchangeConfiguration xcfg in tcgf.Exchanges)
            {
                _exchanges.Add(new ExchangeRow(xcfg));
                foreach (MarketConfiguration mcfg in xcfg.Markets)
                {
                    mcfg.Exchange = xcfg.ExchangeName;
                    _markets.Add(new MarketRow(mcfg));
                    foreach (var scfg in mcfg.Strategies)
                    {
                        _strategies.Add(new StrategyRow(xcfg.ExchangeName,mcfg.MarketName,scfg));
                    }
                }
            }

            var allIds = GetCfgIds().Select(t => t.Item1).ToList();
            scheduleItems.RemoveAll(item => !allIds.Contains(item.Id));
            _scheduleItems = scheduleItems;
            ScheduleItemsBList = new BindingList<ScheduledIntervalDescription>(_scheduleItems);
        }

        public bool IsConfigrationChanged()
        {
            return Serializer<TradingConfiguration>.DiffersFromFile(_tradingConfiguration, _localCfgFileName) ||
                   Serializer<List<ScheduledIntervalDescription>>.DiffersFromFile(_scheduleItems, _localScheduleFileName);
        }
        public string SaveConfiguration()
        {
            return Serializer<TradingConfiguration>.Save(_tradingConfiguration, _localCfgFileName) ??
                   Serializer<List<ScheduledIntervalDescription>>.Save(_scheduleItems, _localScheduleFileName);
        }

        public string RefreshStrategyDlls()
        {
            if (string.IsNullOrEmpty(_strategiesFolder))
                return "Strategies folder is not set";
            if (!Directory.Exists(_strategiesFolder))
                return "Strategies folder not exists: " + _strategiesFolder;

            foreach (ExchangeRow xr in _exchanges)
                xr.SetAttr(null);

            foreach (MarketRow mr in _markets)
                mr.SetAttr(null);

            try
            {
                var lostStrategies = new List<StrategyRow>(_strategies);
                #region stage 1, add new detected strategies to configuration
                foreach (string exchDir in Directory.GetDirectories(_strategiesFolder))
                {
                    string exchangeName = Path.GetFileName(exchDir);
                    if (!exchangeName.IsIdentifier()) continue;

                    foreach (string mktDir in Directory.GetDirectories(exchDir))
                    {
                        string marketName = Path.GetFileName(mktDir);
                        if (!marketName.IsIdentifier()) continue;

                        MarketRow mrow = null;

                        foreach (var dllFile in Directory.GetFiles(mktDir,"*.dll"))
                        {
                            if (SignalGeneratorsRegistryMaker.GetDescription(dllFile,
                                out SignalGeneratorDescription sd) != null)
                                continue;

                            var existingStrategy=_strategies.FirstOrDefault(item =>
                                string.Equals(item.Exchange, exchangeName, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(item.Market, marketName, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(Path.GetFileName(dllFile), item.Cfg.StrategyDll,
                                    StringComparison.OrdinalIgnoreCase));

                            if (existingStrategy != null)
                            {
                                existingStrategy.SetDescription(sd);
                                existingStrategy.SetAttr(PropertyValue.LoadFromFile(Path.ChangeExtension(dllFile, ".txt")));
                                lostStrategies.Remove(existingStrategy);
                            }
                            else
                            {
                                StrategyRow sr = new StrategyRow(exchangeName, marketName, sd,
                                    PropertyValue.LoadFromFile(Path.ChangeExtension(dllFile, ".txt")));
                                _strategies.Add(sr);
                                GetOrAddMarket(ref mrow, exchangeName, marketName, true).Cfg.Strategies.Add(sr.Cfg);

                            }
                        }

                        var mattr = PropertyValue.LoadFromFile(Path.Combine(
                            mktDir,
                            Path.GetFileNameWithoutExtension(mktDir) + ".txt"));
                        if (mattr != null)
                            GetOrAddMarket(ref mrow, exchangeName, marketName, false)?.SetAttr(mattr);
                    }

                    var xattr = PropertyValue.LoadFromFile(Path.Combine(
                        exchDir,
                        Path.GetFileNameWithoutExtension(exchDir) + ".txt"));
                    if (xattr!=null)
                        FindExchange(exchangeName)?.SetAttr(xattr);

                }
                foreach (var str in lostStrategies)
                {
                    str.SetDescription(null);
                    str.SetAttr(null);
                }

                #endregion
                #region stage 2  Remove from configuration strategies where strategy dll is lost

                foreach (var s in _strategies.Where(s=>s.GetDescription()==null))
                    s.Cfg.StrategyName = null;
                _strategies.RemoveAll(item => item.GetDescription() == null);

                foreach (var m in _markets)
                    m.Cfg.Strategies.RemoveAll(s => s.StrategyName == null);

                _markets.RemoveAll(m => m.Cfg.Strategies.Count == 0);

                foreach (var x in _exchanges)
                    x.Cfg.Markets.RemoveAll(m => m.Strategies.Count == 0);

                _exchanges.RemoveAll(x => x.Cfg.Markets.Count == 0);

                #endregion

                ExcludeNotUsedScheduleRecords();
                InitCfgIdentifiers();
                return null;
            }
            catch (Exception exception)
            {
                return exception.ToString();
            }
        }

        private void InitCfgIdentifiers()
        {
            var allEntities = GetAllEntities();
            if (allEntities.All(item => item.GetId() >= 0)) return;
            if (allEntities.Count == 0) return; // formal check to have no exception below, should be never fired
            var nextId = allEntities.Max(item => item.GetId());
            foreach(var item in allEntities)
                if (item.GetId() < 0)
                    item.SetId(++nextId);
        }

        private void ExcludeNotUsedScheduleRecords()
        {
            var usedIds = new HashSet<int>(GetAllEntities().Select(item => item.GetId()));
            _scheduleItems.RemoveAll(item => !usedIds.Contains(item.Id));
        }

        private ExchangeRow FindExchange(string exchangeName)
        {
            return _exchanges.FirstOrDefault(item =>
                string.Equals(item.Exchange, exchangeName, StringComparison.OrdinalIgnoreCase));

        }
        private MarketRow GetOrAddMarket(ref MarketRow result,string exchangeName,string marketName, bool allowCreate)
        {
            if (result != null) return result;

            result = _markets.FirstOrDefault(item =>
                string.Equals(item.Exchange, exchangeName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.MarketName, marketName, StringComparison.OrdinalIgnoreCase));
            if (result != null) return result;
            if (!allowCreate)
            {
                result = null;
                return result;
            }

            var exchange = FindExchange(exchangeName);
            if (exchange == null)
            {
                _exchanges.Add(exchange = new ExchangeRow(new ExchangeConfiguration
                {
                    ExchangeName = exchangeName
                }));
            }

            var mcfg = new MarketConfiguration
            {
                Exchange = exchangeName,
                MarketName = marketName
            };
            exchange.Cfg.Markets.Add(mcfg);
            _markets.Add(result = new MarketRow(mcfg));
            return result;
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

        public List<IRow> GetAllEntities()
        {
            var ret = new List<IRow> { _cfgRow };
            ret.AddRange(_exchanges);
            ret.AddRange(_markets);
            ret.AddRange(_strategies);
            return ret;
        }

        public List<Tuple<int, string>> GetCfgIds()
        {
            var ret = new List<Tuple<int, string>>{new Tuple<int, string>(_tradingConfiguration.Id, "Configuration") };
            foreach (var x in _tradingConfiguration.Exchanges.OrderBy(item=> item.ExchangeName.ToLower()))
            {
                ret.Add(new Tuple<int, string>(x.Id, $"   {x.ExchangeName}"));
                foreach (var m in x.Markets.OrderBy(item=>item.MarketName.ToLower()))
                {
                    ret.Add(new Tuple<int, string>(m.Id, $"      {m.MarketName}"));
                    foreach (var s in m.Strategies.OrderBy(item => item.StrategyName.ToLower()))
                        ret.Add(new Tuple<int, string>(s.Id, $"      {s.StrategyName}"));
                }
            }

            return ret;
        }

        public bool ExistsEntityWithId(int id)
        {
            if (id < 0) return false;
            return _cfgRow.GetId() == id ||
                   _exchanges.Any(item => item.GetId() == id) ||
                   _markets.Any(item => item.GetId() == id) ||
                   _strategies.Any(item => item.GetId() == id);
        }

        public IRow GetGroupRow(List<string> groupPath)
        {
            if (groupPath.Count == 0 || groupPath.Last() != STR_Configuration) return null;

            switch (groupPath.Count)
            {
                default:
                    return null;
                case 1:
                    return _cfgRow;
                case 2:
                    return _exchanges.FirstOrDefault(item => item.Exchange == groupPath[0]);
                case 3:
                    return _markets.FirstOrDefault(item =>
                        item.MarketName == groupPath[0] && item.Exchange == groupPath[1]);
            }
        }

        public string VerifyScheduleRecord(ScheduledIntervalDescription descr)
        {
            if (!ExistsEntityWithId(descr.Id))
                return "Invalid Target Id: " + descr.Id;

            string err = descr.VerifyAndUpdateUtc(_tradingConfiguration.SchedulerTimeStepInMinutes);
            if (err != null) return err;

            var existingItem = _scheduleItems.FirstOrDefault(item => item.EqualsTo(descr));
            if (existingItem != null)
                return "Specified schedule item already exists";

            return null;
        }
        public Tuple<string, ScheduledIntervalDescription> AddScheduleRecord(ScheduledIntervalDescription descr)
        {
            string err = VerifyScheduleRecord(descr);
            if (err != null) return new Tuple<string, ScheduledIntervalDescription>(err, null);
            ScheduleItemsBList.Add(descr);
            return new Tuple<string, ScheduledIntervalDescription>(null, descr);
        }

       
        public string UpdateEditedScheduleRecord(ScheduledIntervalDescription initial, ScheduledIntervalDescription edited)
        {
            if (initial.EqualsTo(edited))
            {
                initial.AssignFrom(edited);
                initial.VerifyAndUpdateUtc(_tradingConfiguration.SchedulerTimeStepInMinutes);
                return null;
            }

            string err = VerifyScheduleRecord(edited);
            if (err != null) return err;

            initial.AssignFrom(edited);
            return null;
        }

        public int DeleteScheduleRecords(List<ScheduledIntervalDescription> recs)
        {
            int counter = 0;
            foreach (var rec in recs)
            {
                var existingItem = ScheduleItemsBList.FirstOrDefault(item => item.EqualsTo(rec));
                if (existingItem != null)
                {
                    ScheduleItemsBList.Remove(existingItem);
                    ++counter;
                }
            }
            return counter;
        }

        public event Action OnSchedulerTimeStepChanged = delegate { };
        public void SetSchedulerTimeStepInMinutes(int value)
        {
            if (value < 1 || value > 30)
                throw new Exception("SchedulerTimeStepInMinutes is not valid (must be integer and in [1;30] range)");

            _tradingConfiguration.SchedulerTimeStepInMinutes = value;
            foreach (var sid in _scheduleItems)
                sid.VerifyAndUpdateUtc(value);
            OnSchedulerTimeStepChanged();
        }

        public string VerifyConfiguration(out IRow errorLocation,out List<string> usedIndicatorDlls)
        {
            usedIndicatorDlls = null;

            string err = RefreshStrategyDlls();
            if (err != null)
            {
                errorLocation = _cfgRow;
                return err;
            }

            if (_strategies.All(item => !item.InWorkingSet))
            {
                errorLocation = _cfgRow;
                return "TradingConfiguration is EMPTY";
            }

            err = _cfgRow.Verify();
            if (err != null)
            {
                errorLocation = _cfgRow;
                return err;
            }

            List<int> IDs = new List<int> {_cfgRow.GetId()};
            List<string> exchanges = new List<string>();
            List<string> currencies = new List<string>();
            List<string> mktExs = new List<string>();
            List<string> strategyNames = new List<string>();
            foreach (ExchangeRow exchange in _exchanges)
            {
                foreach (var mkt in exchange.Cfg.Markets)
                    mkt.Exchange = exchange.Exchange;
                err=exchange.Verify(IDs, exchanges, currencies);
                if (err != null)
                {
                    errorLocation = exchange;
                    return err;
                }
            }

            foreach (MarketRow mkt in GetMarkets(true))
            {
                err = mkt.Verify(IDs, mktExs);
                if (err != null)
                {
                    errorLocation = mkt;
                    return err;
                }
            }

            var indVerificator = new IndicatorsVerificator();

            foreach (StrategyRow str in GetStrategies(true))
            {
                err = str.Verify(IDs, strategyNames, indVerificator);
                if (err != null)
                {
                    errorLocation = str;
                    return err;
                }
            }

            usedIndicatorDlls = indVerificator.GetUsedIndicatorDlls();
            errorLocation = null;
            return null; 
        }
        
        
    }
}
