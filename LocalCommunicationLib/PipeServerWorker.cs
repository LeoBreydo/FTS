using System;
using System.IO.Pipes;
using System.Threading;
using ProtoBuf;

namespace LocalCommunicationLib
{
    public class PipeServerWorker
    {
        private readonly NamedPipeServerStream _worker;
        private bool _isStopping;
        private readonly IServerStateObjectProvider _stateProvider;
        private readonly object _locker = new object();
        private readonly bool _isStateProvider;

        public PipeServerWorker(string pipeName, int maxNumberOfServerInstances,
            IServerStateObjectProvider stateProvider)
        {
            _stateProvider = stateProvider;
            _isStateProvider = _stateProvider != null;
            _worker = new NamedPipeServerStream(pipeName, PipeDirection.InOut, maxNumberOfServerInstances,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            ServerId = Guid.NewGuid().ToString();
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnectedEvent;
        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnectedEvent;
        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;

        public string ServerId { get; }

        // can throw exception !!!
        public void Start()
        {
            _worker.BeginWaitForConnection(WaitForConnectionCallBack, null);
        }

        // can throw exception
        public void Stop()
        {
            _isStopping = true;

            try
            {
                if (_worker.IsConnected) _worker.Disconnect();
            }
            finally
            {
                _worker.Close();
                _worker.Dispose();
            }
        }

        private void WaitForConnectionCallBack(IAsyncResult result)
        {
            if (!_isStopping)
            {
                lock (_locker)
                {
                    if (!_isStopping)
                    {
                        // Call EndWaitForConnection to complete the connection operation
                        _worker.EndWaitForConnection(result);
                        OnConnected();
                        if (_isStateProvider)
                            Serializer.Serialize(_worker, _stateProvider.GetState);
                        else
                        {
                            var res = Serializer.Deserialize<UserCommand>(_worker);
                            OnMessageReceived(res);
                        }
                        Thread.Sleep(100);
                        OnDisconnected();
                        Stop();
                    }
                }
            }
        }

        private void OnMessageReceived(UserCommand uc) =>
            MessageReceivedEvent?.Invoke(this,
                new MessageReceivedEventArgs
                {
                    Destination = uc.Destination,
                    DestinationId = uc.DestinationId,
                    RestrictionCode = uc.RestrictionCode
                });

        private void OnConnected() =>
            ClientConnectedEvent?.Invoke(this, new ClientConnectedEventArgs { ClientId = ServerId, IsStateProvider = _isStateProvider });

        private void OnDisconnected() =>
            ClientDisconnectedEvent?.Invoke(this, new ClientDisconnectedEventArgs { ClientId = ServerId });
    }
}
