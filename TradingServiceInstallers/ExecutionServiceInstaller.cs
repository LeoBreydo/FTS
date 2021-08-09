using BrokerInterfaces;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CfgDescription;
using CommonBrokerClasses;
using CommonInterfaces;
using CommonStructures;
using ExecutionFramework;
using ProductInterfaces;

namespace TradingServiceInstallers
{
    public static class ExecutionServiceInstaller
    {
        // ReSharper disable PossiblyMistakenUseOfParamsMethod
        public static void Register(WindsorContainer container, GeneralSettings orderExecutionSettings,
                                    string disabledProvidersFileName, string disabledStrategiesFileName,
                                    string channel_Commands, string channel_ExecutionServiceReports, string channel_MainBrokerReports, string channel_Logs)
        {
            container.Register(Component.For<IClOrdIDGenerator>().ImplementedBy<ClOrdIDGenerator>());
            container.Register(Component.For<IDisabledProvidersInfo>().ImplementedBy<DisabledProvidersInfo>().DependsOn(
                Property.ForKey("disabledProvidersFileName").Eq(disabledProvidersFileName)
                ));
            container.Register(Component.For<IDisabledStrategies>().ImplementedBy<DisabledStrategies>().DependsOn(
                Property.ForKey("disabledStrategiesFileName").Eq(disabledStrategiesFileName)
                ));

            RegisterExecutionService(container, orderExecutionSettings,
                                     channel_Commands, channel_ExecutionServiceReports, channel_MainBrokerReports, channel_Logs);
        }


        private static void RegisterExecutionService(WindsorContainer container, GeneralSettings orderExecutionSettings,
                                                     string channel_Commands, string channel_ExecutionServiceReports, string channel_MainBrokerReports, string channel_Logs)
        {
            container.Register(Component.For<IReconnectionTime>().ImplementedBy<ReconnectionTime_EST_1659_1705>());
            container.Register(Component.For<IAllVirtualBooks>().ImplementedBy<AllVirtualBooks>().DependsOn(
                Property.ForKey("dataRelevanceInSeconds").Eq(orderExecutionSettings.VirtualBooksDataRelevanceInSeconds),
                Dependency.OnComponent("commandChannel", channel_Commands),
                Dependency.OnComponent("mainBrokerReports", channel_MainBrokerReports)
                                   // omit singletons IAllDataFeeds, IAllowSendOrderToBrokers, IDisabledProvidersInfo
                                   ));

            container.Register(Component.For<IAllTradingProviders>().ImplementedBy<AllTradingProviders>().DependsOn(
                Property.ForKey("logoutChannelName").Eq(channel_Logs)
                                   // omit singleton IResolver
                                   ));

            //container.Register(Component.For<IFilterObsoleteFills>().ImplementedBy<FilterObsoleteFillsByServerStartTime>());
            container.Register(
                Component.For<IFilterObsoleteFills, ISecondPulseRoutine>().ImplementedBy
                    <FilterObsoleteFillsByProviderFirstTickTime>()
                // omit singleton IAllDataFeeds
                );


            container.Register(Component.For<IExecutionServiceReport>().ImplementedBy<ExecutionServiceReport_ToChannel>().DependsOn(
                Dependency.OnComponent("channel", channel_ExecutionServiceReports)
                                   ));
            container.Register(Component.For<IAllowSendOrderToBrokers>().ImplementedBy<AllowSendOrderToBrokers>());

            container.Register(Component.For<IExecutionService>().ImplementedBy<ExecutionService>().DependsOn(
                Property.ForKey("orderExecutionSettings").Eq(orderExecutionSettings)

                                   // omit singletons IAllTradingProviders, IClOrdIDGenerator, IFilterObsoleteFills, IExecutionServiceReport,IStrategiesLogout,IProvidersSupervisor
                                   ));


        }
        // ReSharper restore PossiblyMistakenUseOfParamsMethod
    }
}