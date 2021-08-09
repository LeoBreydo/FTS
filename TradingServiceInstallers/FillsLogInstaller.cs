using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CfgDescription;
using System.Linq;
using CommonInterfaces;
using ExecutionFramework;
using Logs;
using ProductInterfaces;

namespace TradingServiceInstallers
{
    // ReSharper disable PossiblyMistakenUseOfParamsMethod
    public static class FillsLogInstaller
    {
        public static void Register(WindsorContainer container, string resultsFolderName, TradingConfiguration tradingConfiguration)
        {
            container.Register(Component.For<ILastFillsGetter>().ImplementedBy<LastFillsGetter>());

            container.Register(Component.For<IFillsLogSaver>().ImplementedBy<FillsLogSaver>().DependsOn(
                Property.ForKey("resultsFolderName").Eq(resultsFolderName)));
            // omit singleton ILastFillsGetter

            List<StrategyShortInfo> strategyInfos = tradingConfiguration.Strategies.Select(sd => new StrategyShortInfo(sd)).ToList();
            List<VirtualStrategyShortInfo> virtualStrategyInfos =
                tradingConfiguration.VirtualStrategies.Select(vsi => new VirtualStrategyShortInfo(vsi)).ToList();
            
            container.Register(Component.For<IFillsLogGenerator>().ImplementedBy<FillsLogGenerator>().DependsOn(
                Property.ForKey("strategyInfos").Eq(strategyInfos),
                Property.ForKey("virtualStrategyInfos").Eq(virtualStrategyInfos)
                                   // omit singleton IFillsLogSaver
                                   ));

            container.Register(Component.For<ISecondPulseRoutine>().ImplementedBy<FillsLogRoutine>());
            // omit singleton IFillsLogGenerator

            //container.Register(Component.For<IActivator>().ImplementedBy<FillsLogActivator>());
            //// omit singleton IFillsLogGenerator

        }
    }
    // ReSharper restore PossiblyMistakenUseOfParamsMethod
}
