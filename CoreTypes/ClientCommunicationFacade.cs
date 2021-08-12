using System.Collections.Generic;

namespace CoreTypes
{
    public class ClientCommunicationFacade
    {
        public void PushInfo(TradingServiceState state)
        {

        }

        public List<ICommand> GetCommands()
        {
            return new();
        }
    }
}
