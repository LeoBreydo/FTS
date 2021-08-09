using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ProductClasses;
using ProductInterfaces;

namespace TradingServiceInstallers
{
    public static class SecondPulseThreadInstaller
    {
        /// <summary>
        /// Регистрация фонового потока (хостит и ежесекундно активирует все зарегистрированные ISecondPulseRoutine)
        /// </summary>
        public static void RegisterSecondPulseThread(WindsorContainer container)
        {
            container.Register(Component.For<IProcessWorkingThreadException>().ImplementedBy(typeof(ProcessWorkingThreadException)));
            container.Register(Component.For<IService>().ImplementedBy(typeof(SecondPulseThread)));            
        }
    }
}