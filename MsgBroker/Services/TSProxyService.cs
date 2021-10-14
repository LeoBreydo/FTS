using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LocalCommunicationLib;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace MsgBroker.Services
{
    public class TSProxyService
    {
        // singleton
        private static TSProxyService _instance;
        public static TSProxyService Instance
        {
            get { return _instance ??= new TSProxyService(); }
        }

        //private string _statePipeName = "Fts.state", _commandPipeName = "Fts.command";
        private readonly object _locker = new object();
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(3000);
        private readonly Timer _timer;
        private ServerStateObject _state;
        public IHubClients Clients { get; set; }

        private TSProxyService() => _timer = new Timer(UpdateStates, null, _updateInterval, _updateInterval);

        private void UpdateStates(object state)
        {
            lock (_locker)
            {
                var client = new PipeClient();
                var t = client.GetServerState(out var serverState);
                client.Stop();
                if (serverState == null)
                {
                    if (_state != null)
                    {
                        _state.Summary.MessagesToShow = new List<Message> { new() { Tag = "Attention!", Body = "Service unavailable!" } };
                        _state.Summary.IsConnected = false;
                        foreach (var ed in _state.Details.Values)
                        {
                            ed.MessagesToShow = new List<Message> { new() { Tag = "Attention!", Body = "Service unavailable!" } };
                            ed.IsConnected = false;
                        }
                    }
                }
                else _state = serverState;
                if(_state != null)BroadcastNewModel();
            }
        }
        private async Task BroadcastNewModel()
        {
            foreach (var kvp in _state.Details)
                await Clients.Group(kvp.Key).SendAsync("updateStates", "server",JsonConvert.SerializeObject(kvp.Value));
            await Clients.Group("Global").SendAsync("updateStates", "server", JsonConvert.SerializeObject(_state.Summary));
        }
        public void PostUserCommand(UserCommand uc)
        {
            var client = new PipeClient();
            var t = client.SendCommand(uc, out var error);
            //if (error != null) Console.WriteLine(error);
            client.Stop();
        }
    }
}
