using System;
using Castle.Windsor;
using ProductInterfaces;

namespace ProductClasses
{
    public class Resolver:IResolver
    {
        public IWindsorContainer Container;
        public T[] ResolveAll<T>()
        {
            return Container.ResolveAll<T>();
        }
        public T Resolve<T>(bool returnNullWhenNotRegistered = false)
        {
            try
            {
                return Container.Resolve<T>();
            }
            catch(Castle.MicroKernel.ComponentNotFoundException exception)
            {
                if (returnNullWhenNotRegistered && exception.Message.IndexOf("it is not possible to instansiate it as implementation of service",StringComparison.OrdinalIgnoreCase)>=0)
                    return default(T);
                throw;
            }
            
        }
        public T ResolveById<T>(string id, bool returnNullWhenUnknownId=false)
        {
            if (string.IsNullOrEmpty(id)) return default(T);
            try
            {
                return Container.Resolve<T>(id);
            }
            catch (Castle.MicroKernel.ComponentNotFoundException)
            {
                if (returnNullWhenUnknownId) return default(T);
                throw;
            }
        }
    }
}