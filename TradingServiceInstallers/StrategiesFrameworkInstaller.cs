using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Binosoft.TraderLib.Indicators;
using BrokerInterfaces;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CfgDescription;
using CommonBrokerClasses;
using CommonInterfaces;
using CommonStructures;
using DataSupport;
using ExecutionFramework;
using ExecutionFramework.ExposuresAsAmounts;
using Logs;
using MarketStateFilters;
using ProductInterfaces;
using RTIndicatorsFacade;
using TrendMonitors;
using StateIndicators;
using TimeGridIndicators;


namespace TradingServiceInstallers
{

    public static class StrategiesFrameworkInstaller
    {
        // ReSharper disable PossiblyMistakenUseOfParamsMethod
        public static void Register(WindsorContainer container,
            string strategiesFolderName, string systemIndicatorsFolder,string userIndicatorsFolder, int maxIndicatorSize, string cfgFolderName, string resultsFolderName,string dataHistoryFolder,
            TradingConfiguration tradingConfiguration, ProviderSetups providerSetups,
#if SEPACC
            AccountSetups accountSetups,
#endif
            string channel_Commands, string channel_ExecutionServiceReports, string channel_MainBrokerReports, string channel_LogInstrumEvents, string channel_Logs)
        {
            container.Register(Component.For<IClientMessagesCollector>().ImplementedBy<ClientMessagesCollector>());
            
            container.Register(Component.For<IConfigurationMembersMap>().ImplementedBy<ConfigurationMembersMap>());
            container
                .Register(Component.For<ILogout,IStrategiesLogout>().ImplementedBy<LogoutToChannel>().DependsOn(
                     Dependency.OnComponent("channel", channel_Logs)
                    ));


            //container.Register(Component.For<IMarketFilterLog>().ImplementedBy<MarketFilterLog>());
            container.Register(Component.For<IMarketFilterLog>()
                .ImplementedBy<MarketFilterContainerLog_WithDebugLog>()
                .DependsOn(
                    Property.ForKey("resultsFolderName").Eq(resultsFolderName)
                ));


            container.Register(Component.For<IStateIndicatorsLog>().ImplementedBy<StateIndicatorsLog>().DependsOn(
                Property.ForKey("resultsFolderName").Eq(resultsFolderName)));

            RegisterSignalGeneratorsFactory(container, strategiesFolderName);
            RegisterDataSupport(container, channel_Commands, channel_Logs, channel_LogInstrumEvents, dataHistoryFolder, providerSetups);
            RegisterHistorySupport(container, dataHistoryFolder);
            RegisterIndicatorsFacade(container, systemIndicatorsFolder, userIndicatorsFolder, maxIndicatorSize);

#if SEPACC
            RegisterStrategiesFramework(container, tradingConfiguration, providerSetups, accountSetups, cfgFolderName, resultsFolderName, dataHistoryFolder,
                channel_Commands, channel_ExecutionServiceReports, channel_LogInstrumEvents);
#else
            RegisterStrategiesFramework(container, tradingConfiguration, providerSetups, tradingConfiguration.AccountSetups, cfgFolderName, resultsFolderName, dataHistoryFolder,
                channel_Commands, channel_ExecutionServiceReports, channel_LogInstrumEvents);
#endif

            container.Register(Component.For<ISecondPulseRoutine>().ImplementedBy<TransmitErrorMessagesToUser_Routine>());
            // omit sinleton IClientMessagesCollector
        }
        private static void RegisterSignalGeneratorsFactory(WindsorContainer container, string strategiesFolderName)
        {
            container
                .Register(Component.For<ISignalGeneratorsFactory>().ImplementedBy<SignalGeneratorsFactory>().DependsOn(
                    Property.ForKey("folderName").Eq(strategiesFolderName)
                //omit singleton IStrategiesLogout
                              ));
        }
        private static void RegisterDataSupport(WindsorContainer container, string channel_Commands, string channel_Logs, string channel_LogInstrumEvents, string dataHistoryFolder, ProviderSetups providerSetups)
        {
            container.Register(Component.For<IAllDataFeeds>().ImplementedBy<AllDataFeeds>().DependsOn(
                Property.ForKey("logoutChannelName").Eq(channel_Logs)
                // omit singleton IResolver
                ));
            container.Register(Component.For<ILogicalInstrumentsRepository>().ImplementedBy<LogicalInstrumentsRepository>());
            container.Register(Component.For<ILogicalInstrumentEventsOutput>().ImplementedBy<LogicalInstrumentEventsOutputToChannel>().DependsOn(
                Dependency.OnComponent("channel", channel_LogInstrumEvents)
                ));

            //container.Register(Component.For<IGetQuotePolicy>().ImplementedBy<GetQuotePolicy_Bid>());

            container.Register(Component.For<ILogicalInstrumentsDataProvider>().ImplementedBy<LogicalInstrumentsDataProvider>());
            // omit singletons ILogicalInstrumentsRepository,IAllDataFeeds,ILogicalInstrumentEventHandler,IDisabledProvidersInfo,IGetQuotePolicy

            container.Register(Component.For<ITickSaverMetaInfoMaker>().ImplementedBy<TickSaverMetaInfoMaker>());

            container.Register(Component.For<ITickSaver>().ImplementedBy<TickSaver>().DependsOn(
                Property.ForKey("saveAllInstruments").Eq(true),
                Property.ForKey("dataHistoryFolder").Eq(dataHistoryFolder),
                Property.ForKey("providerSetups").Eq(providerSetups)
                ));

            container.Register(Component.For<ISecondPulseRoutine>().ImplementedBy<DataProviderRoutine>().DependsOn(
                Dependency.OnComponent("commandChannel", channel_Commands)
                // omit singletons ILogicalInstrumentsDataProvider , IAllDataFeeds
                ));
        }
        private static void RegisterHistorySupport(WindsorContainer container, string dataHistoryFolder)
        {
            container.Register(Component.For<ILastBarsGetter>().ImplementedBy<LastBarsGetter>());
            container.Register(Component.For<IBarsHistoryCollector>().ImplementedBy<BarsHistoryCollector>().DependsOn(
                              Property.ForKey("dataHistoryFolder").Eq(dataHistoryFolder)
                // omit singleton IGetQuotePolicy,ILastBarsGetter
                              ));

            container.Register(Component.For<IDataHistoryReader>().ImplementedBy<DataHistoryReader>().DependsOn(
                              Property.ForKey("dataHistoryFolder").Eq(dataHistoryFolder)
                // omit singleton IBarsHistoryCollector
                              ));

            //container.Register(Component.For<IBarsHistoryProvider>().ImplementedBy<BarsHistoryProvider_NoHistory>());
            container.Register(Component.For<IBarsHistoryProvider>().ImplementedBy<BarsHistoryProvider>());
            // omit singletons ILogicalInstrumentsRepository, IDataHistoryReader

            container.Register(Component.For<IStrategyBarsLog>().ImplementedBy<StrategyBarsLog>().DependsOn(
                Property.ForKey("dataHistoryFolder").Eq(dataHistoryFolder)
                ));

            container
                .Register(Component.For<IBarsHistoryRoutine>().ImplementedBy<BarsHistoryCollectorRoutine>());
            // omit singletons IBarsHistoryCollector,ILastBarsGetter,IAllDataFeeds
        }


        private static void RegisterIndicatorsFacade(WindsorContainer container, string systemIndicatorsFolder,string userIndicatorsFolder, int maxIndicatorSize)
        {
            IndicatorsServer.Init(systemIndicatorsFolder, userIndicatorsFolder);
            string externalIndicatorsDll = Assembly.GetAssembly(typeof(Rpl)).Location;
            IndicatorsServer.AddDll(externalIndicatorsDll);

            string timeGridIndicatorsDll= Assembly.GetAssembly(typeof(TimeGridFactory)).Location;
            IndicatorsServer.AddDll(timeGridIndicatorsDll);

            Indicator.BackupSize = maxIndicatorSize;
            IrRoutines.MaxPossibleHistorySize = maxIndicatorSize;
            container.Register(Component.For<IDataStorage>().ImplementedBy<DataStorage>());

            container.Register(Component.For<IIndicatorsFacade>().ImplementedBy<IndicatorsFacade>());

            container.Register(Component.For<ISignalContainer>().ImplementedBy<SignalContainer>());
        }

        private static void RegisterStrategiesFramework(WindsorContainer container, 
            TradingConfiguration tradingConfiguration, 
            ProviderSetups providerSetups,AccountSetups accountSetups,
            string cfgFolderName, string resultsFolderName,string dataHistoryFolder, 
            string channel_Commands,string channel_ExecutionServiceReports, string channel_LogInstrumEvents)
        {
            container.Register(Component.For<IEndOfGapObservers>().ImplementedBy<EndOfGapObservers>().DependsOn(
                Dependency.OnComponent("logicalInstrumentEventsChannel", channel_LogInstrumEvents)
                                   // omit singleton IStrategiesLogout
                                   ));

            container.Register(Component.For<IRatesTable>().ImplementedBy<RatesTableHolder>().DependsOn(
                Property.ForKey("providerSetups").Eq(providerSetups),
                Dependency.OnComponent("commandChannel", channel_Commands)
                // omit singletons IAllDataFeeds
                ));

            //container.Register(Component.For<ILocalFilterFactory>().ImplementedBy<LocalFilterFactory>());
            container.Register(Component.For<IVirtualResultLog>().ImplementedBy<VirtualResultLog>().DependsOn(
                Property.ForKey("resultsFolderName").Eq(resultsFolderName)
                ));
            container.Register(Component.For<ISynteticBarsSaver>().ImplementedBy<SynteticBarsSaver>().DependsOn(
                Property.ForKey("dataHistoryFolder").Eq(dataHistoryFolder)
                ));



            container.Register(Component.For<IRenkoLogSaver>().ImplementedBy<RenkoLogSaver>().DependsOn(
                Property.ForKey("folder").Eq(dataHistoryFolder)
                ));
            container.Register(Component.For<ITrendMonitorLogSaver>().ImplementedBy<TrendMonitorLogSaver>().DependsOn(
                Property.ForKey("folder").Eq(dataHistoryFolder)
                ));
            container.Register(Component.For<IIndicatorsFacadeForBarSequenceFactory>().ImplementedBy<IndicatorsFacadeForBarSequenceFactory>());
                
            //container.Register(Component.For<ITrendMonitorsManager>().ImplementedBy<TrendMonitorsManager>());
            // omit singletons ILogicalInstrumentsRepository,ILogicalInstrumentsDataProvider
            
            //container.Register(Component.For<IExposureRestrictionsManager>().ImplementedBy<ExposureRestrictionsAsAmountsManager>());

            container.Register(Component.For<ITradesSaver>().ImplementedBy<TradesSaver>().DependsOn(
                Property.ForKey("resultsFolderName").Eq(resultsFolderName)
                ))
                .Register(Component.For<IStrategiesFramework>().ImplementedBy<StrategiesFramework>().DependsOn(
                    Property.ForKey("tradingConfiguration").Eq(tradingConfiguration),
                    Property.ForKey("providerSetups").Eq(providerSetups)
                // omit singletons IStrategiesLogout,ISignalGeneratorsFactory,ILogicalInstrumentsRepository,
                // IIndicatorsFacade,IAllVirtualBooks,IExecutionService,IProvidersSupervisor,IDisabledProvidersInfo,IDisabledStrategies,ILogicalInstrumentsDataProvider,
                // IConfigurationLoader,IEndOfGapObservers,
                // IRatesTable,ILocalFilterFactory,IVirtualResultLog,ISynteticBarsSaver
                              ));

            container.Register(Component.For<IConfigurationInfoMaker>().ImplementedBy<ConfigurationInfoMaker>().DependsOn(
                Property.ForKey("providerSetups").Eq(providerSetups),
                Property.ForKey("accountSetups").Eq(accountSetups),
                Property.ForKey("mainCurrency").Eq(tradingConfiguration.GeneralSettings.MainCurrency)
                // omit singletons IDisabledProvidersInfo, IClientMessagesCollector, IConfigurationMembersMap
                ));

            container.Register(Component.For<ISecondPulseRoutine>().ImplementedBy<StrategiesFrameworkRoutine>().DependsOn(
                 Dependency.OnComponent("commandChannel", channel_Commands),
                 Dependency.OnComponent("channelExecutionServiceReports", channel_ExecutionServiceReports),
                 Property.ForKey("updateStatePeriodicity").Eq(tradingConfiguration.GeneralSettings.RefreshStateFrequency)
                //omit singletons IAllVirtualBooks, IRatesTable, IExecutionService,IConfigurationInfoMaker
                ));

            string strategyLivetimesFileName = Path.Combine(cfgFolderName, "StrategyLivetimes");
            container.Register(Component.For<IStrategyLiveTimes>().ImplementedBy<StrategyLiveTimes>().
                                   DependsOn(Property.ForKey("fileName").Eq(strategyLivetimesFileName)));

            string virtualStrategyLivetimesFileName = Path.Combine(cfgFolderName, "VirtualStrategyLivetimes");
            string bindingLivetimesFileName = Path.Combine(cfgFolderName, "BindingLivetimes");
            container.Register(Component.For<IVirtualStrategyLiveTimes>().ImplementedBy<VirtualStrategyLiveTimes>().
                                   DependsOn(
                                   Property.ForKey("liveTimeFileName").Eq(virtualStrategyLivetimesFileName),
                                   Property.ForKey("bindingsFileName").Eq(bindingLivetimesFileName)));

            string fileNameStopped = Path.Combine(resultsFolderName, "LastStartStopTimes.xml");
            container.Register(Component.For<ITradingServerLastStartStopTimes>().ImplementedBy<TradingServerLastStartStopTimes>().
                                   DependsOn(Property.ForKey("fileName").Eq(fileNameStopped)));

        }
        // ReSharper restore PossiblyMistakenUseOfParamsMethod
    }
}

