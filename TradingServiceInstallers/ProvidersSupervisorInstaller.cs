using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonInterfaces;
using CommonStructures;
using ExecutionFramework;

namespace TradingServiceInstallers
{
    public static class ProvidersSupervisorInstaller
    {
        public static void RegisterProvidersSupervisor(
            WindsorContainer container,
            int threshold, ProviderSetups providerSetups,
            string channel_Commands,
            string channel_Logs)
        {
            container
                .Register(Component.For<IProvidersSupervisor>().ImplementedBy<ProvidersSupervisor>().DependsOn(
                    Property.ForKey("threshold").Eq(threshold),
                    Property.ForKey("providerSetups").Eq(providerSetups),
                    Dependency.OnComponent("cmdChannel", channel_Commands),
                    Dependency.OnComponent("logChannel", channel_Logs)));
        }
    }
}