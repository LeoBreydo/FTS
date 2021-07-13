using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Utilities
{
    public static class AssemblyHelper
    {
        public static Assembly Load(string dllName)
        {
            if (dllName == null) return null;

            dllName = Path.GetFullPath(dllName);
            if (!File.Exists(dllName)) return null;
            try
            {
                // uses already loaded assembly if try to load twice
                return Assembly.LoadFile(dllName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Загрузить сборку
        /// </summary>
        /// <param name="dllName">путь до dll</param>
        /// <param name="errorMsg">сообщение об ошибке при сбое</param>
        /// <param name="nonBlockingLoad">nonBlockingLoad: true=файл dll не блокируется, но недоступна ее отладка и нельзя узнать ее местоположение на диске</param>
        /// <returns></returns>
        public static Assembly Load(string dllName, out string errorMsg, bool nonBlockingLoad = false)
        {
            if (dllName == null)
            {
                errorMsg = "Dll not specified";
                return null;
            }
            dllName = Path.GetFullPath(dllName);
            if (!File.Exists(dllName))
            {
                errorMsg = "File not found " + dllName;
                return null;
            }
            try
            {
                // uses already loaded assembly if try to load twice
                errorMsg = null;
                if (!nonBlockingLoad)
                    return Assembly.LoadFile(dllName);

                byte[] data = File.ReadAllBytes(dllName);
                return Assembly.Load(data);
            }
            catch (ReflectionTypeLoadException loadException)
            {
                errorMsg = (loadException.LoaderExceptions.Length > 0)
                    ? loadException.LoaderExceptions[0].ToString()
                    : loadException.Message;
                return null;
            }
            catch (Exception exception)
            {
                errorMsg = exception.Message;
                return null;
            }
        }
        public static Type[] FindPublicTypesOfInterface(this Assembly assembly, Type interfaceType)
        {
            var types = assembly.GetTypes();
            return types.Where(t => t.IsPublic && !t.IsAbstract && t.GetInterface(interfaceType.FullName) != null).ToArray();
        }
        public static TInterface CreateInstanceOfInterface<TInterface>(string dllName)
            where TInterface : class
        {
            return CreateInstanceOfInterface<TInterface>(Load(dllName));
        }
        public static TInterface CreateInstanceOfInterface<TInterface>(this Assembly assembly)
            where TInterface : class
        {
            if (assembly == null) return null;
            Type[] assemblyTypes = assembly.FindPublicTypesOfInterface(typeof(TInterface));
            if (assemblyTypes.Length == 0) return null;
            try
            {
                return Activator.CreateInstance(assemblyTypes[0]) as TInterface;
            }
            catch
            {
                return null;
            }
        }
    }
}
