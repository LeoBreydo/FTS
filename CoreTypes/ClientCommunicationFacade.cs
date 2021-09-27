using System.Collections.Generic;

namespace CoreTypes
{
    public class ClientCommunicationFacade
    {
        public void PushInfo(TradingServiceState state)
        {
            var stateForClients = state.ComposeServerStateObject();
            // push to hub
        }

        public List<ICommand> GetCommands()
        {
            return new();
        }
    }
}
