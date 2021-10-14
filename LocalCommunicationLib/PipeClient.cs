using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace LocalCommunicationLib
{
    public class PipeClient
    {
        private NamedPipeClientStream _pipeClient;
        private readonly string _statePipeName;
        private readonly string _commandPipeName;

        public PipeClient()
            : this("Fts.state", "Fts.command")
        {
        }

        public PipeClient(string statePipeName, string commandPipeName)
        {
            _statePipeName = statePipeName;
            _commandPipeName = commandPipeName;
        }

        public Task<TaskResult> SendCommand(UserCommand uc, out string error)
        {
            error = null;
            var taskCompletionSource = new TaskCompletionSource<TaskResult>();
            _pipeClient = new NamedPipeClientStream(".", _commandPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            bool res = Start();
            if (res)
            {
                if (_pipeClient.IsConnected) Serializer.Serialize(_pipeClient, uc);
                else error = "pipe is not connected";
                Thread.Sleep(100);
            }
            else error = "Can't connect";

            return taskCompletionSource.Task;
        }

        public Task<TaskResult> GetServerState(out ServerStateObject state)
        {
            state = null;
            var taskCompletionSource = new TaskCompletionSource<TaskResult>();
            _pipeClient = new NamedPipeClientStream(".", _statePipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            bool res = Start();
            if (res)
            {
                if (_pipeClient.IsConnected)
                {
                    state = Serializer.Deserialize<ServerStateObject>(_pipeClient);
                }
            }
            return taskCompletionSource.Task;
        }

        public bool Start()
        {
            const int tryConnectTimeout = 100;
            try
            {
                _pipeClient.Connect(tryConnectTimeout);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void Stop()
        {
            _pipeClient.Close();
            _pipeClient.Dispose();
        }
    }
}
