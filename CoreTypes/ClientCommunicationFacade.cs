using System.Collections.Concurrent;
using System.Collections.Generic;
using LocalCommunicationLib;

namespace CoreTypes
{
    public class ClientCommunicationFacade : IServerStateObjectProvider
    {
        private TradingServiceState _currentState = null;
        private readonly PipeServer _ps;
        private readonly BlockingCollection<ICommand> _commands;

        public ClientCommunicationFacade()
        {
            _commands = new();
            _ps = new PipeServer(this);
            _ps.MessageReceivedEvent += (sender, args) =>
            {
                _commands.Add(new RestrictionCommand((CommandDestination)args.Destination,
                    CommandSource.User, args.DestinationId, (TradingRestriction)args.RestrictionCode));
            };
            _ps.Start();
        }

        ~ClientCommunicationFacade()
        {
            if (_ps != null) _ps.Stop();
            _commands.CompleteAdding();
            _commands.Dispose();
        }

        
        public void PushInfo(TradingServiceState state)
        {
            _currentState = state;
        }

        public List<ICommand> GetCommands()
        {
            List<ICommand> ret = new();
            var consumed = 0;
            var cnt = _commands.Count;
            if (cnt > 0)
                foreach (var cmd in _commands.GetConsumingEnumerable())
                {
                    ret.Add(cmd);
                    if (++consumed == cnt) break;
                }
            return ret;
        }

        public ServerStateObject GetState => _currentState?.ComposePayload();
    }
}
