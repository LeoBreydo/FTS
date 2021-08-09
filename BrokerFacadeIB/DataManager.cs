#define USE_5S_BARS
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CoreTypes;
using IBApi;
using Messages;

namespace BrokerFacadeIB
{
    public class DataManager
    {
        private const int START_TICKER_ID = 10000100;
        private readonly IBClient _client;
        private readonly Dictionary<int, (string symbolExchange, string contractCode)> _registry = new();
        private readonly Dictionary<int, (string symbolExchange, string contractCode)> _5sBars = new();
        private readonly Dictionary<string, Contract> _symbolAndExchangeToContract;

        private readonly BlockingCollection<TickInfo> _quoteQueue = new();
        private readonly BlockingCollection<ContractDetails> _contractQueue = new();
        private readonly BlockingCollection<Bar5s> _barsQueue = new();
        private readonly BlockingCollection<Tuple<string, string>> _textMessageQueue;
        private readonly BlockingCollection<BaseMessage> _baseMessageQueue;

        private void AddMessage(string tag, string message)
        {
            _textMessageQueue.Add(new Tuple<string, string>(tag, message));
        }

        private int _currentTickerId;
        private int GetNextTickerId() { return ++_currentTickerId; }

        public DataManager(IBClient client, 
            Dictionary<string, Contract> symbolAndExchangeToContract,
            BlockingCollection<Tuple<string, string>> textMessageQueue,
            BlockingCollection<BaseMessage> baseMessageQueue)
        {
            _client = client;
            _textMessageQueue = textMessageQueue;
            _baseMessageQueue = baseMessageQueue;
            _symbolAndExchangeToContract = symbolAndExchangeToContract;
            _currentTickerId = START_TICKER_ID;
            _client.ContractDetails += _client_ContractDetails;
            _client.TickPrice += _client_TickPrice;
            _client.TickSize += _client_TickSize;
#if USE_5S_BARS
            _client.RealtimeBar += _client_RealtimeBar;
#endif
        }


        public (List<TickInfo>, List<ContractInfo>, List<Bar5s>) 
            GetState()
        {
            var cnt = _quoteQueue.Count;
            var consumed = 0;
            List<TickInfo> qtList = new();
            if (cnt > 0)
                foreach (var ti in _quoteQueue.GetConsumingEnumerable())
                {
                    qtList.Add(ti);
                    if (++consumed == cnt) break;
                }

            cnt = _contractQueue.Count;
            List<ContractInfo> ciList = new();
            if (cnt > 0)
            {
                consumed = 0;
                foreach (var ci in _contractQueue.GetConsumingEnumerable())
                {
                    ciList.Add(new ContractInfo(ci));
                    if (++consumed == cnt) break;
                }
            }

            cnt = _barsQueue.Count;
            List<Bar5s> barsList = new();
            if (cnt > 0)
            {
                consumed = 0;
                foreach (var qu in _barsQueue.GetConsumingEnumerable())
                {
                    barsList.Add(qu);
                    if (++consumed == cnt) break;
                }
            }

            return (qtList, ciList, barsList);
        }

        // we should call this method at start of new day in timezone of the contracts of interest.
        public void PlaceRequest(List<(string,string)> contractCodesAndExchanges)
        {
            foreach (var (contractCode, exchange) in contractCodesAndExchanges)
                RequestContractDetails(contractCode, exchange);
        }

        public bool GetMarketCode(int id, out string marketCode)
        {
            marketCode = null;
            if (!_registry.ContainsKey(id)) return false;
            marketCode = _registry[id].symbolExchange;
            return true;
        }

        public bool GetContractCode(int id, out string contractCode)
        {
            contractCode = null;
            if (!_registry.ContainsKey(id)) return false;
            contractCode = _registry[id].contractCode;
            return true;
        }

        private readonly object _lock = new();
        private void _client_ContractDetails(ContractDetailsMessage contractDetails)
        {
            lock (_lock)
            {
                var key = contractDetails.Details.Contract.Symbol + contractDetails.Details.Contract.Exchange;
                var cntr = new Contract
                {
                    SecType = "FUT",
                    Exchange = contractDetails.Details.Contract.Exchange,
                    Currency = contractDetails.Details.Contract.Currency,
                    LocalSymbol = contractDetails.Details.Contract.LocalSymbol,
                    Symbol = contractDetails.Details.Contract.Symbol
                };
                // if instrument is registered
                if (_symbolAndExchangeToContract.ContainsKey(key))
                {
                    var previousCode = _symbolAndExchangeToContract[key].LocalSymbol;
                    // if contract for a new delivery date detected
                    if (previousCode != contractDetails.Details.Contract.LocalSymbol)
                    {
                        _symbolAndExchangeToContract[key] = cntr;
                        Unsubscribe(previousCode);
                        Subscribe(key);
                    }
                }
                else // new instrument detected
                {
                    _symbolAndExchangeToContract.Add(key, cntr);
                    Subscribe(key);
                }
            }

            _contractQueue.Add(contractDetails.Details);
        }
        private void _client_TickPrice(TickPriceMessage tickPrice)
        {
            //if (tickPrice.Field > 4) return;
            if (!_registry.ContainsKey(tickPrice.RequestId)) return;
            var symbolExchange = _registry[tickPrice.RequestId].symbolExchange;
            _quoteQueue.Add(new TickInfo(symbolExchange, tickPrice.Field, tickPrice.Price));
        }
        private void _client_TickSize(TickSizeMessage tickSize)
        {
            //if (tickSize.Field > 5) return;
            if (!_registry.ContainsKey(tickSize.RequestId)) return;
            string symbolExchange = _registry[tickSize.RequestId].symbolExchange;
            _quoteQueue.Add(new TickInfo(symbolExchange, tickSize.Field, tickSize.Size));
        }

        private readonly DateTime _baseUnixTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private void _client_RealtimeBar(RealTimeBarMessage msg)
        {
            if (!_5sBars.TryGetValue(msg.ReqId, out var t)) return;
            var (symbolExchange, contractCode) = t;

            bool ignoreBar;
            var barOpenTime = _baseUnixTime.AddSeconds(msg.Time);
            switch (barOpenTime.Second)
            {
                case 0:
                case 55:
                    return;
                default:
                    ignoreBar = DateTime.UtcNow.AddSeconds(2).Minute != barOpenTime.Minute; // bar is late
                    break;
            }

            if (ignoreBar) return;
            _barsQueue.Add(new Bar5s(symbolExchange, contractCode,msg.Open,
                msg.High, msg.Low, msg.Close, barOpenTime));
        }
        private void RequestContractDetails(string contractCode, string exchange)
        {
            var ticker = GetNextTickerId();
            var cnt = new Contract
            {
                Symbol = contractCode.ToUpper(),
                SecType = "CONTFUT",
                Exchange = exchange.ToUpper()
            };
            _client.ClientSocket.reqContractDetails(ticker, cnt);
        }

        private void Subscribe(string symbolExchange)
        {
            lock (_lock)
            {
                var cntr = _symbolAndExchangeToContract.ContainsKey(symbolExchange)
                    ? _symbolAndExchangeToContract[symbolExchange]
                    : null;
                if (cntr == null)
                {
                    AddMessage("WARNING", 
                        $"Invalid contract info used in subscribtion request ({symbolExchange})");
                    return;
                }

                // if (!_client.ConnectionEstablished)
                // {
                //     AddMessage("DEBUG",
                //         $"SubscriptionRequest to {cntr.LocalSymbol} ignored, connection to IB is not established");
                //     return;
                // }

                var tickerId = GetNextTickerId();
                _registry.Add(tickerId,(symbolExchange, cntr.LocalSymbol));

                _client.ClientSocket.reqMarketDataType(3);
                _client.ClientSocket.reqMktData(tickerId, cntr, string.Empty, false, false, null);
                tickerId = GetNextTickerId();
                _5sBars.Add(tickerId, (symbolExchange, cntr.LocalSymbol));
                _client.ClientSocket.reqMarketDataType(3);
                _client.ClientSocket.reqRealTimeBars(tickerId, cntr, 5, "TRADES", true, null);
            }
        }

        public void handle_Error(int oid, int errorCode, string str)
        {
            lock (_lock)
            {
                if (!_registry.ContainsKey(oid)) return;
                var symbolExchange = _registry[oid].symbolExchange;
                AddMessage("ERROR",
                    $"Subsription to IB marketCode {symbolExchange} failed, ErrorCode={errorCode}, Msg={str}");
                _baseMessageQueue.Add(new DataFeed_SubscriptionRejection(
                    1, symbolExchange, 0, $"ErrorCode={errorCode}, Msg={str}"));
            }
        }

        private void Unsubscribe(string contractCode)
        {
            lock (_lock)
            {
                contractCode = contractCode.Trim().ToUpper();
                var ticker = -1;
                foreach (var kvp in _registry.Where(kvp => kvp.Value.contractCode == contractCode))
                {
                    ticker = kvp.Key;
                    break;
                }
                if (ticker == -1) return;
                _registry.Remove(ticker);
                _client.ClientSocket.cancelMktData(ticker);


                var tlst = new List<int>();
                var foundSoFar = 0;
                foreach (var kvp in _5sBars.Where(kvp => kvp.Value.contractCode == contractCode))
                {
                    tlst.Add(kvp.Key);
                    if (++foundSoFar == 3) break;
                }
                foreach (var t in tlst)
                {
                    _5sBars.Remove(t);
                    _client.ClientSocket.cancelRealTimeBars(t);
                }

            }
        }

        public void UnsubscribeAll()
        {
            lock (_lock)
            {
                foreach (var ticker in _registry.Select(kvp => kvp.Key))
                    _client.ClientSocket.cancelMktData(ticker);

                foreach (var t in _5sBars.Keys) _client.ClientSocket.cancelRealTimeBars(t);
                ClearState();
            }
        }

        public void ClearState()
        {
            _symbolAndExchangeToContract.Clear();
            _registry.Clear();
            _5sBars.Clear();
        }

        ~DataManager()
        {
            UnsubscribeAll();
            lock (_lock)
            {
                _quoteQueue?.CompleteAdding();
                _quoteQueue?.Dispose();

                _contractQueue?.CompleteAdding();
                _contractQueue?.Dispose();

                _barsQueue?.CompleteAdding();
                _barsQueue?.Dispose();
            }
        }
    }

}
