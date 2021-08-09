using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ProductClasses;
using ProductInterfaces;

namespace TradingServiceInstallers
{
    public static class LogServiceInstaller
    {
        public static IPublicChannelListener RegisterLogService(WindsorContainer container, string resultsFolderName, string name_logServiceActivator)
        {
            // ReSharper disable PossiblyMistakenUseOfParamsMethod
            container
                .Register(Component.For<IGeneralLog>().ImplementedBy<GeneralLog>().DependsOn(
                        Property.ForKey("resultsFolderName").Eq(resultsFolderName)
                        ))
                .Register(Component.For<ILogServiceWorker, IPublicChannelListener>().ImplementedBy<LogServiceWorker>())
                // omit singleton ILogFileSelector
                .Register(Component.For<IActivator>().Named(name_logServiceActivator).ImplementedBy<LogServiceActivator>());
            // ReSharper restore PossiblyMistakenUseOfParamsMethod

            return container.Resolve<IPublicChannelListener>();
        }
    }
}