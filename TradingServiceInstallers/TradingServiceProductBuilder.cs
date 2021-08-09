using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.MicroKernel.Registration;
using CfgDescription;
using BrokerInterfaces;
using CommonBrokerClasses;
using CommonInterfaces;
using CommonStructures;
using DataSupport;
using ExecutionFramework;
using FillsLogDirectUse;
using Messages;
using ProductInterfaces;
using ProductClasses;
using Utilities;

namespace TradingServiceInstallers
{
    public class TradingServiceProductBuilder : ProductBuilderBase
    {
        private const string ProvidersSubFolder = "Providers";
        private const string ResultsSubFolder = "Results";

        public const string channel_MainBrokerReports = "MainBrokerReports";  //main broker reporst (connectivity, subscription reports, administrative fix rejection messages)
        //order reports channel from broker created personally for each broker
        public const string channel_Commands = "Commands"; // commands channel
        public const string channel_OrdersExecutionReports = "OrdersExecutionReports";// публикуются сообщения об исполнении ордеров БРОКЕРАМИ(получатель-ExecutionService)
        public const string channel_ExecutionServiceReports = "ExecutionServiceReports"; // ExecutionServicе передает стратегиям результат исполнения OrderedAmount
        public const string channel_LogInstrumEvents = "LogInstrumEvents"; // публикуются сообщения о состоянии логических инструментов
        public const string channel_Logs = "LogChannel";// the channel with messages intended for user only

        public const string channel_Quotes= "QuoteChannel";// the channel with messages intended for user only
        public const string channel_BadQuotes = "BadQuoteChannel";// the channel with messages intended for user only

        public const string name_FixEngineActivator = "EngineActivator";
        public const string name_reconnectionServicesActivator = "reconnectionServicesActivator";
        public const string name_logServiceActivator = "logServiceActivator";

        private readonly ProviderSetups _providerSetups;
        private readonly TradingConfiguration _tradingConfiguration;
#if SEPACC
        private readonly AccountSetups _accountSetups;
#endif
        //public TradingConfiguration TradingConfiguration { get { return _tradingConfiguration; } }

        public TradingServiceProductBuilder(CfgLocation cfgLocation, string explicitProvidersCfgFileName)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            string coreFolderName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cfgLocation.FtpRootPath);

            string systemIndicatorsFolder = cfgLocation.SystemIndicatorsFolder;
            string userIndicatorsFolder = cfgLocation.UserIndicatorsFolder;

            string marketFiltersPath = cfgLocation.MarketFiltersPath;
            string directionalFiltersPath = cfgLocation.DirectionalFiltersPath;
            string trendMonitorsPath = cfgLocation.TrendMonitorsPath;
            string signalTransformerLogicPath = cfgLocation.SignalTransformerLogicPath;

            string providersFolderName = Path.Combine(coreFolderName, ProvidersSubFolder);
            string disabledProvidersFileName = Path.Combine(providersFolderName, "State\\DisabledProviders");
            //string stateFolderName = Path.Combine(providersFolderName, "State");
            string resFolderName = Path.Combine(coreFolderName, ResultsSubFolder);
            string strategiesFolderName = Path.Combine(coreFolderName, "Strategies");
            string configurationFolderName = Path.Combine(coreFolderName, "Configuration");
            string disabledStrategiesFileName = Path.Combine(configurationFolderName, "DisabledStrategies");

            string dataHistoryFolder = Path.Combine(resFolderName, "DataHistory");

            string quickFixCfgFileName = Path.Combine(providersFolderName, "QuickFixSessions.cfg");

            _providerSetups = LoadProvidersSetup(providersFolderName);
#if SEPACC
            _accountSetups = LoadAccountSetups(providersFolderName);
#endif

            if (!Directory.Exists(resFolderName))
                Directory.CreateDirectory(resFolderName);
            else
            {
                CloseVirtualStrategiesPosition.Run(resFolderName);
                if (new FillsLogFiles(resFolderName).GetCurrentPosition().HasOpenPosition())
                {
                    throw new Exception("Current position is not flat");
                }
            }

            List<string> currencyPairs = _providerSetups.GetAllCurrencyPairs();
            Container.Register(Component.For<IConfigurationLoader>().ImplementedBy<ConfigurationLoader>().DependsOn(
// ReSharper disable PossiblyMistakenUseOfParamsMethod
                Property.ForKey("cfgFolderName").Eq(configurationFolderName),
                Property.ForKey("currencyPairs").Eq(currencyPairs)
// ReSharper restore PossiblyMistakenUseOfParamsMethod
                ));




            //Container.Register(Component.For<ITransactionManagerTransformerServer>().ImplementedBy<TransactionManagerTransformerServer>());

            Container.Register(Component.For<IConfigurationVerificator>().ImplementedBy<ConfigurationVerificator>().DependsOn(
                Property.ForKey("providerSetups").Eq(_providerSetups)
                //Property.ForKey("accountSetups").Eq(_accountSetups)
                ));

            var loader = Container.Resolve<IConfigurationLoader>();
            string error;
            _tradingConfiguration = loader.LoadConfiguration(DateTime.UtcNow, out error);
            if (_tradingConfiguration == null)
            {
                throw new Exception(error ?? "Configuration not found");
            }


            IPublicChannelListener publicChannelListener = LogServiceInstaller.RegisterLogService(Container, resFolderName, name_logServiceActivator);

            ChannelsInstaller.RegisterChannels(
                Container,
                publicChannelListener,
                resFolderName,
                new[]
                    {
                        channel_MainBrokerReports,
                        channel_Commands,
                        channel_OrdersExecutionReports,
                        channel_ExecutionServiceReports,
                        channel_LogInstrumEvents,
                        channel_Logs
                    },
                new[]
                    {
                        channel_Quotes, channel_BadQuotes
                    }
                );
            Container.Register(Component.For<IBrokerFacadeChannels>().ImplementedBy<BrokerFacadeChannels>().DependsOn(
                                  Dependency.OnComponent("mainReportsChannel", channel_MainBrokerReports),
                                  Dependency.OnComponent("orderReportsChannel", channel_OrdersExecutionReports),
                                  Dependency.OnComponent("logChannel", channel_Logs),
                                  Dependency.OnComponent("commandsChannel", channel_Commands),
                                  Dependency.OnComponent("quotesChannel", channel_Quotes),
                                  Dependency.OnComponent("badQuotesChannel", channel_BadQuotes)
                                  
                ));

            SecondPulseThreadInstaller.RegisterSecondPulseThread(Container);


            SubsriptionsManagementInstaller.RegisterSubsriptionsManagement(Container, channel_Commands);

            //string providersWindzorCfgFileName = explicitProvidersCfgFileName ?? Path.Combine(providersFolderName, "Providers.config");
            //Install(providersWindzorCfgFileName);
            if (explicitProvidersCfgFileName != null)
                Install(explicitProvidersCfgFileName);
            else
            {
                foreach(string cfgFileName in Directory.GetFiles(providersFolderName,"*.config"))
                    Install(cfgFileName);
            }
            
            Container.Register(Component.For<IActivator>().Named(name_reconnectionServicesActivator).ImplementedBy<AllReconnectionServicesActivator>());

            ProvidersSupervisorInstaller.RegisterProvidersSupervisor(Container,
#if DEBUG
                                                                         50,
#else
                                                                     _tradingConfiguration.GeneralSettings.ProviderPenaltiesThreshold,    
#endif
                _providerSetups, channel_Commands, channel_Logs);

            ExecutionServiceInstaller.Register(Container, _tradingConfiguration.GeneralSettings,
                disabledProvidersFileName, disabledStrategiesFileName,
                channel_Commands, channel_ExecutionServiceReports, channel_MainBrokerReports, channel_Logs);

            StrategiesFrameworkInstaller.Register(Container,
                strategiesFolderName, systemIndicatorsFolder, userIndicatorsFolder, cfgLocation.MaxIndicatorHistorySize, configurationFolderName, resFolderName, dataHistoryFolder,
                _tradingConfiguration, _providerSetups, 
#if SEPACC
                _accountSetups,
#endif
                channel_Commands, channel_ExecutionServiceReports, channel_MainBrokerReports, channel_LogInstrumEvents, channel_Logs);


            FillsLogInstaller.Register(Container, resFolderName, _tradingConfiguration);
            BidAskSnapshotsMakerInstaller.Register(Container);

            Container.Register(Component.For<ISecondPulseRoutine>().ImplementedBy<BadTicksSaverRoutine>().DependsOn(
                Property.ForKey("dataHistoryFolder").Eq(dataHistoryFolder)));


           ActivatorsInstaller.RegisterProductActivators(Container, name_logServiceActivator,
                                                              new[]
                                                                  {
                                                                      name_reconnectionServicesActivator
                                                                  });
        }
        public override IProduct Build()
        {
            IActivator logServiceActivator=null;
            ILogServiceWorker logServiceWorker = null;
            try
            {
                var product = base.Build();
                // в данной версии инстанцирование/запуск торговой конфигурации осуществляется только при запуске торгового сервера:
                // загружаемая конфигурация верифицируется и инстанцируется создается класс контейнер стратегий (strategiesFramework)
                // В случае ошибок (отсутствие плагина стратегии/индикаторов и т д) в лог выводятся соответствующие сообщения об ошибках
                // И затем выбрасывается исключение.
                // (!В следующей версии я предполагал инстанцирование strategiesFramework делать явной командной после активации торгового сервиса
                //  это потребует определенной доработки: 
                //      - деинстанцировать работающую конфигурацию, если она уже есть (дописать код- ве закрыть, от всего отписаться...)
                //      - инстанцировать новую конфигурацию (выделить инстанцирование конфигурации в отдельную сущность из конструктора strategiesFramework)
                //
                // В текущей же версии класс отвечающий за вывод ошибок в лог (LogService)
                //  к моменту инстанцирования strategiesFramework уже должен прослушивать каналы, чтобы сохранить в лог сообщения об ошибках
                // В связи с этим такое временное решение:
                //    Активация LogService выносится из активации продукта и вызывается явно перед конструктором strategiesFramework
                logServiceActivator = Container.Resolve<IActivator>(name_logServiceActivator);
                logServiceActivator.Activate();
                logServiceWorker = Container.Resolve<ILogServiceWorker>();
                Container.Resolve<ISignalGeneratorsFactory>().LoadDlls();

                string error = VerifyProvidersInstantiation();
                if (error != null)
                    throw new Exception("Invalid provider settings. " + error);

                var cmdChannel = Resolve<IMsgChannel>(channel_Commands);
                var orderReportsChannel = Resolve<IMsgChannel>(channel_OrdersExecutionReports);
                var logChannel = Resolve<IMsgChannel>(channel_Logs);

                var ret= new TradingService(product, cmdChannel, orderReportsChannel, logChannel, Resolver, 
                                          _tradingConfiguration.GeneralSettings.CloseAllTimeout, this);
                logServiceWorker.Flush();
                return ret;
            }
            catch (Exception exception)
            {
                if (logServiceWorker != null)
                    logServiceWorker.DirectMessageOutput(new TextMessage(TextMessageTypes.ERROR, exception.ToString()));
                if (logServiceActivator != null)
                    logServiceActivator.Deactivate();

                throw;
            }
        }

        public T Resolve<T>()
        {
            return Container.Resolve<T>();
        }
        public T Resolve<T>(string byName)
        {
            return Container.Resolve<T>(byName);
        }


        private static ProviderSetups LoadProvidersSetup(string providersFolderName)
        {
            string providersFileName = Path.Combine(providersFolderName, "ProvidersCfg.xml");
            var providerSetups = Serializer<ProviderSetups>.Open(providersFileName, true);
            string error = providerSetups.Verify();
            if (error != null)
                throw new Exception(string.Format("Invalid Providers configuration '{0}': {1}", providersFileName, error));
            return providerSetups;
        }
#if SEPACC
        private static AccountSetups LoadAccountSetups(string providersFolderName)
        {
            string accountSetupsFileName = Path.Combine(providersFolderName, "AccountsCfg.xml");
            var accountSetups = Serializer<AccountSetups>.Open(accountSetupsFileName, true);
            string error = accountSetups.Verify();
            if (error != null)
                throw new Exception(string.Format("Invalid Accounts configuration '{0}': {1}", accountSetupsFileName, error));

            return accountSetups;
        }
#endif
        private string VerifyProvidersInstantiation()
        {
            IBrokerFacade[] allImplementations_BrokerFacades=Container.ResolveAll<IBrokerFacade>();

            foreach (ProviderSetup providerSetup in _providerSetups.Providers)
            {
                long brokerID = providerSetup.ID;
                if (allImplementations_BrokerFacades.All(b => b.BrokerID != brokerID))
                    return
                        $"'IBrokerFacade' implementation not specified for provider {providerSetup.Name}(ID={brokerID})";
            }
            return null;
        }
    }

    
}
