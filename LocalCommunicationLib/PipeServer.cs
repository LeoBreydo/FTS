using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace LocalCommunicationLib
{
    public class PipeServer
    {
        private readonly string _statePipeName;
        private readonly string _commandPipeName;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly IDictionary<string, PipeServerWorker> _servers;
        private readonly IServerStateObjectProvider _stateProvider;
        private const int MaxNumberOfServerInstances = 10;

        public PipeServer(IServerStateObjectProvider stateProvider)
            : this("Fts.state", "Fts.command", stateProvider)
        {
        }

        public PipeServer(string statePipeName, string commandPipeName, IServerStateObjectProvider stateProvider)
        {
            _statePipeName = statePipeName;
            _commandPipeName = commandPipeName;
            _synchronizationContext = AsyncOperationManager.SynchronizationContext;
            _servers = new ConcurrentDictionary<string, PipeServerWorker>();
            _stateProvider = stateProvider;
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
        public event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;

        public void Start()
        {
            StartStatePipeServer();
            StartCommandPipeServer();
        }

        public void Stop()
        {
            foreach (var server in _servers.Values)
            {
                try
                {
                    UnregisterFromServerEvents(server);
                    server.Stop();
                }
                catch
                {
                }
            }

            _servers.Clear();
        }

        private void StartStatePipeServer()
        {
            var server = new PipeServerWorker(_statePipeName, MaxNumberOfServerInstances, _stateProvider);
            _servers.Add(server.ServerId, server);

            server.ClientConnectedEvent += ClientConnectedHandler;
            server.ClientDisconnectedEvent += ClientDisconnectedHandler;
            server.MessageReceivedEvent += MessageReceivedHandler;

            server.Start();
        }

        private void StartCommandPipeServer()
        {
            var server = new PipeServerWorker(_commandPipeName, MaxNumberOfServerInstances, null);
            _servers.Add(server.ServerId, server);

            server.ClientConnectedEvent += ClientConnectedHandler;
            server.ClientDisconnectedEvent += ClientDisconnectedHandler;
            server.MessageReceivedEvent += MessageReceivedHandler;

            server.Start();
        }

        private void StopNamedPipeServer(string id)
        {
            UnregisterFromServerEvents(_servers[id]);
            _servers[id].Stop();
            _servers.Remove(id);
        }

        private void UnregisterFromServerEvents(PipeServerWorker server)
        {
            server.ClientConnectedEvent -= ClientConnectedHandler;
            server.ClientDisconnectedEvent -= ClientDisconnectedHandler;
            server.MessageReceivedEvent -= MessageReceivedHandler;
        }

        private void OnMessageReceived(MessageReceivedEventArgs eventArgs)
        {
            _synchronizationContext.Post(e => MessageReceivedEvent.SafeInvoke(this, (MessageReceivedEventArgs)e),
                eventArgs);
        }

        private void OnClientConnected(ClientConnectedEventArgs eventArgs)
        {
            _synchronizationContext.Post(e => ClientConnectedEvent.SafeInvoke(this, (ClientConnectedEventArgs)e),
                eventArgs);
        }

        private void OnClientDisconnected(ClientDisconnectedEventArgs eventArgs)
        {
            _synchronizationContext.Post(
                e => ClientDisconnectedEvent.SafeInvoke(this, (ClientDisconnectedEventArgs)e), eventArgs);
        }

        private void ClientConnectedHandler(object sender, ClientConnectedEventArgs eventArgs)
        {
            bool isStateProvider = eventArgs.IsStateProvider;
            OnClientConnected(eventArgs);
            // Create a additional server as a preparation for new connection
            if (isStateProvider) StartStatePipeServer();
            else StartCommandPipeServer();
        }

        private void ClientDisconnectedHandler(object sender, ClientDisconnectedEventArgs eventArgs)
        {
            OnClientDisconnected(eventArgs);

            StopNamedPipeServer(eventArgs.ClientId);
        }

        private void MessageReceivedHandler(object sender, MessageReceivedEventArgs eventArgs)
        {
            OnMessageReceived(eventArgs);
        }
    }
}
