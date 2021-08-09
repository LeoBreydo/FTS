using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ProductClasses;
using ProductInterfaces;

namespace TradingServiceInstallers
{
    public static class ActivatorsInstaller
    {
        /// <summary>
        /// Регистрация активаторов и деактиваторов
        /// </summary>
        public static void RegisterProductActivators(WindsorContainer container,
                                                     List<string> activationOrderNames,
                                                     List<string> deactivationOrderNames)
        {
            // ReSharper disable PossiblyMistakenUseOfParamsMethod
            container
                .Register(Component.For<IProductActivator>().ImplementedBy<ProductActivator>().DependsOn(
                    Property.ForKey("memberIds").Eq(activationOrderNames)))
                .Register(Component.For<IProductDeactivator>().ImplementedBy<ProductDeactivator>().DependsOn(
                    Property.ForKey("memberIds").Eq(deactivationOrderNames)));
            // ReSharper restore PossiblyMistakenUseOfParamsMethod
        }
        public static void RegisterProductActivators(WindsorContainer container, string logServiceActivator, string[] lastOnes)
        {
            List<IActivator> allActivators = container.ResolveAll<IActivator>().ToList();
            var aLog = container.Resolve<IActivator>(logServiceActivator);
            List<IActivator> _lastOnes = lastOnes.Select(id => container.Resolve<IActivator>(id)).ToList();
            allActivators.Remove(aLog);
            allActivators.RemoveAll(_lastOnes.Contains);
            allActivators.AddRange(_lastOnes);
            allActivators.Insert(0, aLog);

            var deactivationOrder = new List<IActivator>(allActivators);
            deactivationOrder.Reverse();

            container
                .Register(Component.For<IProductActivator>().ImplementedBy<ProductActivator>().DependsOn(
                    Property.ForKey("members").Eq(allActivators)))
                .Register(Component.For<IProductDeactivator>().ImplementedBy<ProductDeactivator>().DependsOn(
                    Property.ForKey("members").Eq(deactivationOrder)));

            container.Resolve<IProductActivator>();
            container.Resolve<IProductDeactivator>();
        }
    }
}
