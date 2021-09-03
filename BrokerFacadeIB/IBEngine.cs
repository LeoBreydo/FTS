using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CoreTypes;
using IBApi;

namespace BrokerFacadeIB
{
    public class IBEngine
    {
        public enum SessionConnectionStatus { ByUser, ByCounterParty, ConnectionLost, StoppedByIBEngine }//todo to remove this enum, to use plain text instead of

        private readonly EReaderMonitorSignal signal = new();
        private readonly IBClient _client;

        private readonly int _port;
        private readonly string _host;
        private readonly int _clientId;
        public TwsActivator TwsActivator { get; }

        private OrdersManager OrderFeed { get; }
        private DataManager DataFeed { get; }

        private Thread _listenerThread;
        private CancellationTokenSource _lcts;

        private readonly BlockingCollection<Tuple<string, string>> _textMsgQueue = new();
        public void AddMessage(string tag, string message)
        {
            _textMsgQueue.Add(new Tuple<string, string>(tag,message));
        }

        public IBEngine(IBCredentials credentials)
        {
            _port = credentials.Port;
            _host = credentials.Hostname;
            _clientId = credentials.ClientId;
            TwsActivator = new TwsActivator(credentials, AddMessage);

            _client = new IBClient(signal)
            {
                ClientId = _clientId
            };

            _client.Error += handle_Error;
            _client.ConnectionClosed += handle_ConnectionClosed;

            ConcurrentDictionary<string, Contract> symbolAndExchangeToContract = new();

            DataFeed = new DataManager(_client, symbolAndExchangeToContract, _textMsgQueue);
            OrderFeed = new OrdersManager(_client, symbolAndExchangeToContract, _textMsgQueue);
        }

        ~IBEngine()
        {
            _textMsgQueue?.CompleteAdding();
            _textMsgQueue?.Dispose();
        }

        public void PlaceRequest(List<(string, string)> contractCodesAndExchanges,
            List<MarketOrderDescription> orders)
        {
            DataFeed.PlaceRequest(contractCodesAndExchanges);
            OrderFeed.PlaceRequest(orders);
        }

        public StateObject GetState(DateTime currentUtc)
        {
            var cnt = _textMsgQueue.Count;
            List<Tuple<string, string>> tmList = new ();
            if (cnt > 0)
            {
                var consumed = 0;
                foreach (var t in _textMsgQueue.GetConsumingEnumerable())
                {
                    tmList.Add(t);
                    if (++consumed == cnt) break;
                }
            }

            var (tickInfos, contractInfos, barUpdates, historicalData) = DataFeed.GetState(currentUtc); // todo to transfer outside the historical data
            var orderReports = OrderFeed.GetState();

            return new StateObject(currentUtc,tickInfos,contractInfos,barUpdates,orderReports,tmList);
        }

        public bool IsConnectionEstablished
        {
            get => _client.ConnectionEstablished;
            private set => _client.ConnectionEstablished = value;
        }
        public bool IsStarted { get; private set; }
        private bool _subscriptionError;
        public bool Start()
        {
            if (IsStarted)
            {
                AddMessage("INFO", "Attempt to start of the already working client was detected. No action was performed.");
                AddMessage("CLIENT", "Attempt to start of the already working client was detected");
                return false;
            }

            AddMessage("INFO", "Start");
            IsStarted = true;

            string errmsg = null;
            var suceeded = false;

            _lcts = new CancellationTokenSource();
            var ltcs = _lcts;

            try
            {
                AddMessage("INFO", "Run Client");

                // wait 10 seconds for any farmConnection notification
                _anyNotification_received = false;
                _farmConnectionNtf_restWaitSeconds = 60;

                _client.ClientSocket.eConnect(_host, _port, _clientId);
                var reader = new EReader(_client.ClientSocket, signal);
                reader.Start();

                _listenerThread = new Thread(() =>
                {
                    while (_client.ClientSocket.IsConnected())
                    {
                        signal.waitForSignal(); // <-- HERE was detected a hang; Because of that was set the timeout when wait for join this thread
                        reader.processMsgs();
                        if (ltcs.Token.IsCancellationRequested || _listenerThread!=Thread.CurrentThread)
                            break;
                    }

                    ltcs.Dispose();

                })
                { IsBackground = true };
                _listenerThread.Start();

                suceeded = true;
            }
            catch (Exception exception)
            {
                errmsg = "StartClient failed by exception: " + exception.Message;
            }

            if (suceeded)
            {
                AddMessage("INFO", "StartClient passed successfully");
                return true;
            }

            AddMessage("INFO", errmsg);
            AddMessage("CLIENT", errmsg);
            Disconnection(SessionConnectionStatus.ConnectionLost, errmsg);
            return false;
        }

        public void Stop(SessionConnectionStatus reason= SessionConnectionStatus.ByUser)
        {
            DataFeed.UnsubscribeAll();
            Disconnection(reason, null); 
        }

        private int _farmConnectionNtf_restWaitSeconds;
        private bool _anyNotification_received;

        private void handle_Error(int id, int errorCode, string str, Exception ex)
        {
            _anyNotification_received = true;

            if (errorCode == 2137) // ignore message 2137 "The closing order quantity is greater than your current position"
                return;     // It can be also disabled in TWS: Global Configuration -> Messages -> disable the "Cross Side Warning"

            var txt =
                $"Id = {id}, ErrorCode = {errorCode}, Message = {str}{(ex == null ? "" : ", Exception: " + ex.Message)}";

            AddMessage("INFO",txt);
            AddMessage("DEBUG",txt);

            if (errorCode == -1) // socket problem - need to disconnect!!
            {
                Disconnection(SessionConnectionStatus.ConnectionLost, str);
                return;
            }


            if (id == 0 || errorCode == 0 || ex != null)
            {
                // Id = 0, ErrorCode = 0 with exception is called when TWS application is closed or when TWS is not working when engine is starting
                // usually occured when TWS application is closed
                string msg = ex?.Message ?? str;
                Disconnection(SessionConnectionStatus.ConnectionLost, msg);
                return;
            }
            DataFeed.handle_Error(id, errorCode, str);
            OrderFeed.handle_Error(id, errorCode, str);

            switch (errorCode)
            {
                case 1100:  // Connectivity between IB and the TWS has been lost
                    IsConnectionEstablished = false;
                    SendDisconnectionMessages(SessionConnectionStatus.ByCounterParty, str);
                    break;
                case 2110:  // Connectivity between TWS and server is broken. It will be restored automatically.
                    IsConnectionEstablished = false;
                    SendDisconnectionMessages(SessionConnectionStatus.ByCounterParty, str);
                    break;
                case 1101: // Connectivity between IB and TWS has been restored- data lost.
                    DataFeed.ClearState();
                    OnConnectionEstablished();
                    break;
                case 1102: // Connectivity between IB and TWS has been restored- data maintained.
                    OnConnectionEstablished();
                    break;
                case 1300: //Unexpected case. The port number in the TWS/IBG settings has been changed during an active API connection.
                    AddMessage("CLIENT",str);
                    break;

                case 200: // Subscription failed  (No security definition has been found for the request)
                     if (!DataFeed.GetMarketCode(id, out var symbolExchange)) return;
                     AddMessage("CLIENT",
                        $"Warning! Failed to subscribe IB currency pair '{symbolExchange}'");
                    _subscriptionError = true;
                    break;
            }
        }
        private void handle_ConnectionClosed()
        {
            DataFeed.ClearState();
            OrderFeed.ClearState();
            AddMessage("INFO","ConnectionClosed");
        }

        private string _conn_deconn_time;
        private void OnConnectionEstablished()
        {
            IsConnectionEstablished = true;
            _conn_deconn_time = DateTime.UtcNow.ToString("yyyyMMdd:HHmmss");
            _textMsgQueue.Add(new Tuple<string, string>("ConnectionStatus",$"Connected at {_conn_deconn_time}"));
        }
        private void SendDisconnectionMessages(SessionConnectionStatus disconnectionReason, string disconnectionDetails)
        {
            //_baseMsgQueue.Add(BrokerConnectionStatus.MakeDisconnected(1, false, disconnectionReason, disconnectionDetails));
            _textMsgQueue.Add(new Tuple<string, string>("ConnectionStatus", 
                $"Disconnected at {DateTime.UtcNow:yyyyMMdd:HHmmss} by reason {disconnectionReason}. Details: {disconnectionDetails}"));
        }
        private void Disconnection(SessionConnectionStatus disconnectionReason, string disconnectionDetails)
        {
            _client.ClientSocket.eDisconnect();

            var thread = _listenerThread;
            try
            {
                if (thread != null)
                {
                    _lcts?.Cancel();
                    thread.Join(15000);   // <-- HERE was detected a hang because of hang in listener thread when called out of params
                }
            }
            catch  // avoid the exception "Thread is not started"
            {
            }
            _listenerThread = null;
            _lcts?.Dispose();
            _lcts = null;

            IsStarted = false;
            IsConnectionEstablished = false;
            _conn_deconn_time = DateTime.UtcNow.ToString("yyyyMMdd:HHmmss");

            _anyNotification_received = false;
            _farmConnectionNtf_restWaitSeconds = 0;
            _subscriptionError = false;

            SendDisconnectionMessages(disconnectionReason, disconnectionDetails);
           AddMessage("INFO","Stopped by reason " + disconnectionReason);
        }

        public void SecondPulse()
        {
            if (IsStarted)
            {
                if (_subscriptionError)
                {
                    const string msg =
                        "Cannot subscribe for one or more currency pairs, its possible connection problem or symbol is not supported (check file 'FTPRoot/Providers/ProvidersCfg.xml'). TWS will be restarted";
                    AddMessage("INFO",msg);
                    AddMessage("CLIENT","TWS will be forcibly restarted via subscription problem");
                    Stop(SessionConnectionStatus.StoppedByIBEngine);
                    RestartTws();
                    return;
                }

                if (_farmConnectionNtf_restWaitSeconds > 0)
                {
                    if (_anyNotification_received)
                    {
                        AddMessage("INFO","Farm connection notification received. Set is ready");
                        _farmConnectionNtf_restWaitSeconds = 0;
                    }
                    else if (--_farmConnectionNtf_restWaitSeconds == 0)
                    {
                        AddMessage("INFO","Farm connection notification timeout. TWS will be restarted");
                        Stop(SessionConnectionStatus.StoppedByIBEngine);
                        AddMessage("CLIENT","TWS will be restarted via no qt messages comes from");
                        RestartTws();
                        return;
                    }
                }
            }

            if (!IsConnectionEstablished && _client.ClientSocket.IsConnected())
            {
                OnConnectionEstablished();
                OrderFeed.OnConnectionRestored();
            }

            if (IsConnectionEstablished)
                OrderFeed.SecondPulse();
        }
        public void RestartTws()
        {
            TwsActivator.Restart();
        }
    }
}