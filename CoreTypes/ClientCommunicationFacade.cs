using System.Collections.Generic;

namespace CoreTypes
{
    public class ClientCommunicationFacade
    {
        public void PushInfo(List<string> msgList, TradingServiceState state)
        {

        }

        public List<Command> GetCommands()
        {
            return new();
        }
    }
}
