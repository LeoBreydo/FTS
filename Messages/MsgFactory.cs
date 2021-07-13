using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonStructures;

namespace Messages
{
    public interface IMsgFactory
    {
        IMsg CreateMsg(int msgNumber);
        Type GetTypeOfTheMessage(int msgNumber);
    }
    public class MsgFactory:IMsgFactory
    {
        private readonly Dictionary<int, Type> messageTypes = new Dictionary<int, Type>();
        public MsgFactory()
        {
            foreach (Type t in FindPublicTypesOfInterface(Assembly.GetAssembly(typeof(BaseMessage)), typeof(IMsg)))
            {
                var msg = Activator.CreateInstance(t) as IMsg;
                messageTypes.Add(msg.MessageNumber, t);
            }
        }

        public Type GetTypeOfTheMessage(int msgNumber)
        {
            return messageTypes[msgNumber];
        }
        public IMsg CreateMsg(int msgNumber)
        {
            var t = messageTypes[msgNumber];
            return Activator.CreateInstance(t) as IMsg;
        }

        private static IEnumerable<Type> FindPublicTypesOfInterface(Assembly assembly, Type interfaceType)
        {
            var types = assembly.GetTypes();
            return types.Where(t => t.IsPublic && !t.IsAbstract && t.GetInterface(interfaceType.FullName) != null).ToArray();
        }
    }
}
