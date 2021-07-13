using System;
using System.IO;
using ProductInterfaces;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.MicroKernel.Registration;

namespace ProductClasses
{
    public class ProductBuilderBase : IProductBuilder, IDisposable
    {
        /// <summary>
        /// Container to register all services and their sub-entities to use in the product
        /// </summary>
        public WindsorContainer Container { get; private set; }
        public IResolver Resolver { get; private set; }
        public ProductBuilderBase()
        {
            Container = new WindsorContainer();
            InitResolver();
        }
        public ProductBuilderBase(string cfgFileName)
        {
            Container = new WindsorContainer();
            InitResolver();
            Install(cfgFileName);
        }
        private void InitResolver()
        {
            Container.Register(Component.For<IResolver>().ImplementedBy(typeof(Resolver)));
            var resolver = (Resolver)Container.Resolve<IResolver>();
            resolver.Container = Container;
            Resolver = resolver;
        }
        public virtual IProduct Build()
        {
            return new Product(Container.ResolveAll<IService>(),TryResolve<IProductActivator>(),TryResolve<IProductDeactivator>());
        }
        public void Dispose()
        {
            if (Container!=null)
            {
                Container.Dispose();
                Container = null;
            }
        }
        public void Install(string cfgFileName)
        {
            try
            {
                Container.Install(Configuration.FromXmlFile(cfgFileName));
            }
            catch (Exception exception)
            {
                if (string.Equals(Path.GetFileName(cfgFileName),"SaveTicksHistory.config",StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Please remove the file 'SaveTicksHistory.config' from the providers folder. This component is not optional more and is included to the obligatory instantiation");
                throw new Exception("Invalid configuration file " + cfgFileName, exception);
            }
        }
        public T TryResolve<T>()
        {
            try
            {
                return Container.Resolve<T>();
            }
            catch 
            {

                return default(T);
            }
        }

    }
}
