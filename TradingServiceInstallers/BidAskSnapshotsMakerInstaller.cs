using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonInterfaces;
using DataSupport;
using ProductInterfaces;

namespace TradingServiceInstallers
{
    public static class BidAskSnapshotsMakerInstaller
    {
        public static void Register(WindsorContainer container)
        {
            container.Register(Component.For<IBidAskSnapshots>().ImplementedBy<BidAskSnapshots>());
            container.Register(Component.For<ISecondPulseRoutine>().ImplementedBy<BidAskSnapshotsMakerRoutine>());
            // omit singletons IBidAskSnapshots, IAllDataFeeds
        }
    }
}
