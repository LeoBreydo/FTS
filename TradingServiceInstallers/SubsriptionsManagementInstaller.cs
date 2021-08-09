using BrokerInterfaces;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonBrokerClasses;
using ProductInterfaces;

namespace TradingServiceInstallers
{
    public static class SubsriptionsManagementInstaller
    {
        /// <summary>
        /// Регистрация менеджера централизованных подписок (один на всех поставщиков)
        /// </summary>
        public static void RegisterSubsriptionsManagement(
            WindsorContainer container,
            string channel_Commands)
        {
            container
                .Register(Component.For<ISubsriptionsManager>().ImplementedBy<SubsriptionsManager>().DependsOn(
// ReSharper disable PossiblyMistakenUseOfParamsMethod
                    Dependency.OnComponent("commandChannel", channel_Commands)))
// ReSharper restore PossiblyMistakenUseOfParamsMethod
                .Register(Component.For<ISecondPulseRoutine>().ImplementedBy<SubscriptionsManagerRoutine>());
        }
    }
}