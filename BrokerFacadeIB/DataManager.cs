#define USE_5S_BARS
#define WORKWITH_DELAYED_DATA
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CoreTypes;
using CoreTypes.SignalServiceClasses;
using IBApi;

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

        private void AddMessage(string tag, string message)
        {
            _textMessageQueue.Add(new Tuple<string, string>(tag, message));
        }

        private int _currentTickerId;
        private int GetNextTickerId() { return ++_currentTickerId; }

        public DataManager(IBClient client, 
            Dictionary<string, Contract> symbolAndExchangeToContract,
            BlockingCollection<Tuple<string, string>> textMessageQueue)
        {
            _client = client;
            _textMessageQueue = textMessageQueue;
            _symbolAndExchangeToContract = symbolAndExchangeToContract;
            _currentTickerId = START_TICKER_ID;
            _client.ContractDetails += _client_ContractDetails;
            _client.TickPrice += _client_TickPrice;
            _client.TickSize += _client_TickSize;
#if USE_5S_BARS
            _client.RealtimeBar += _client_RealtimeBar;
#endif
        }

#if WORKWITH_DELAYED_DATA
        class Bar5sBuilder
        {
            public readonly string SymbolExchange;
            public string ContractCode { get; private set; }

            private bool IsBarStarted;
            private DateTime Begin;
            private double O, H, L, C;

            public Bar5sBuilder(string symbolExchange)
            {
                SymbolExchange = symbolExchange;
            }
            public void ProcessTime(DateTime nextBarBegin, BlockingCollection<Bar5s> output)
            {
                if (IsBarStarted && nextBarBegin > Begin)
                {
                    output.Add(new Bar5s(SymbolExchange, ContractCode, O, H, L, C, Begin));
                    IsBarStarted = false;
                }
            }
            public void ProcessQuote(double quote, DateTime barBegin,string contractCode)
            {
                ContractCode = contractCode;

                if (!IsBarStarted)
                {
                    IsBarStarted = true;
                    O = H = L = C = quote;
                    Begin = barBegin;
                }
                else
                {
                    H = Math.Max(H, quote);
                    L = Math.Min(L, quote);
                    C = quote;
                }
            }

        }
        private readonly List<Bar5sBuilder> _bar5sBuilders = new List<Bar5sBuilder>();

        private Bar5sBuilder GetBar5sBuilder(TickInfo ti)
        {
            var ret = _bar5sBuilders.FirstOrDefault(item =>
                item.SymbolExchange == ti.SymbolExchange);
            if (ret == null)
                _bar5sBuilders.Add(ret = new Bar5sBuilder(ti.SymbolExchange));
            return ret;
        }

        private static DateTime GetS5BarBegin(DateTime time)
        {
            var sec = (int)Math.Floor(time.TimeOfDay.TotalSeconds / 5) * 5;
            var barBegin = time.Date.AddSeconds(sec);
            return barBegin;
        }

#endif
        public (List<TickInfo>, List<ContractInfo>, List<Bar5s>) 
            GetState(DateTime currentUtc)
        {
#if WORKWITH_DELAYED_DATA
            DateTime tBegin5S = GetS5BarBegin(currentUtc);
            // when working with delayed market data we have no subscription to 5sec bars. Emulate it using collected quotes
            foreach (var bb in _bar5sBuilders)
                bb.ProcessTime(currentUtc, _barsQueue);
#endif
            var cnt = _quoteQueue.Count;
            var consumed = 0;
            List<TickInfo> qtList = new();
            if (cnt > 0) 
                foreach (TickInfo ti in _quoteQueue.GetConsumingEnumerable())
                {
                    qtList.Add(ti);
                    if (ti.Tag == 4) // last price
                    {
                        GetBar5sBuilder(ti).ProcessQuote(ti.Value, tBegin5S, ti.ContractCode);
                        //DebugLog.AddMsg(string.Format("Quote: {0} {1} {2} {3}", ti.SymbolExchange, ti.ContractCode, ti.Tag, ti.Value));
                    }

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
                    //DebugLog.AddMsg(string.Format("5sBar: {0} {1} {2} {3} {4} {5} {6}", qu.SymbolExchange, qu.ContractCode, qu.BarOpenTime.ToString("yyyyMMdd-HHmmss"),qu.Open,qu.High,qu.Low,qu.Close));
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

            var cd = contractDetails.Details;
            var cdc = contractDetails.Details.Contract;
            DebugLog.AddMsg(string.Format("_client_ContractDetails {0},{1},{2},{3},{4},{5},{6},{7}", 
                contractDetails.ReqId,
                cdc.Exchange,
                cdc.Currency,
                cdc.LocalSymbol,
                cdc.Symbol,
                cd.Category,
                cd.MinTick,
                cdc.Multiplier
                ));


            _contractQueue.Add(contractDetails.Details);
        }
#if WORKWITH_DELAYED_DATA
        int TransformDelayedTagToMainTag(int field)
        {
            switch (field)
            {
                case 66: // delayed bid
                    return 1; // bid price
                case 67: // delayed ask
                    return 2; // ask price
                case 68: // delayed last
                    return 4; // last price

                case 69: // delayed bid size
                    return 0; // bid size
                case 70: // delayed ask size
                    return 3; // ask size
                case 71: // delayed last size
                    return 5; // last size

                    
                default:
                        return field;
            }

        }

        private void _client_TickPrice(TickPriceMessage tickPrice)
        {
#if RELEASE
#error This code is a patch to have the futures data stream from delayed quotation. Can not be used in production!!! Toggle off the conditional compilation key '#WORKWITH_DELAYED_DATA'
#endif

            var field = TransformDelayedTagToMainTag(tickPrice.Field);
            if (!_registry.TryGetValue(tickPrice.RequestId, out var info)) return;
            _quoteQueue.Add(new TickInfo(info.symbolExchange, info.contractCode, field, tickPrice.Price));
        }

        private void _client_TickSize(TickSizeMessage tickSize)
        {
            var field = TransformDelayedTagToMainTag(tickSize.Field);
            if (!_registry.TryGetValue(tickSize.RequestId,out var info)) return;
            _quoteQueue.Add(new TickInfo(info.symbolExchange, info.contractCode, field, tickSize.Size));
        }

#else
            private void _client_TickPrice(TickPriceMessage tickPrice)
        {
            if (tickPrice.Field > 4) return; // ignore delayed data
            if (!_registry.TryGetValue(tickPrice.RequestId, out var info)) return;
            _quoteQueue.Add(new TickInfo(info.symbolExchange, tickPrice.Field, tickPrice.Price));
        }
        private void _client_TickSize(TickSizeMessage tickSize)
        {
            if (tickSize.Field > 5) return; // ignore delayed data
            if (!_registry.ContainsKey(tickSize.RequestId)) return;
            string symbolExchange = _registry[tickSize.RequestId].symbolExchange;
            _quoteQueue.Add(new TickInfo(symbolExchange, tickSize.Field, tickSize.Size));
        }
#endif

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
            DebugLog.AddMsg(string.Format("DM.RequestContractDetails, {0},{1}; tickerId={2}", contractCode, exchange, ticker));
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
                var contract = _symbolAndExchangeToContract.ContainsKey(symbolExchange)
                    ? _symbolAndExchangeToContract[symbolExchange]
                    : null;
                if (contract == null)
                {
                    DebugLog.AddMsg(string.Format("DM.Subscribe to {0} failed - invalid contract info", symbolExchange));
                    AddMessage("WARNING", 
                        $"Invalid contract info used in subscription request ({symbolExchange})");
                    return;
                }

                var tickerId = GetNextTickerId();
                DebugLog.AddMsg(string.Format("DM.Subscribe, {0}; tickerId={1}", symbolExchange, tickerId));
                _registry.Add(tickerId,(symbolExchange, contract.LocalSymbol));

                _client.ClientSocket.reqMarketDataType(3);
                _client.ClientSocket.reqMktData(tickerId, contract, string.Empty, false, false, null);

                tickerId = GetNextTickerId();
                _5sBars.Add(tickerId, (symbolExchange, contract.LocalSymbol));
                _client.ClientSocket.reqMarketDataType(3);
                _client.ClientSocket.reqRealTimeBars(tickerId, contract, 5, "TRADES", true, null);
            }
        }

        public void handle_Error(int tickerId, int errorCode, string str)
        {
            DebugLog.AddMsg(string.Format("handle_Error {0},{1}; tickerId={2}", errorCode, str, tickerId));
            lock (_lock)
            {
                if (_registry.TryGetValue(tickerId,out var symbolExchange))
                {
                    AddMessage("ERROR",
                        $"Subscription to IB marketCode {symbolExchange} failed, ErrorCode={errorCode}, Msg={str}");
                }
                else if (_5sBars.TryGetValue(tickerId, out symbolExchange))
                {
                    AddMessage("ERROR",
                        $"Subscription to IB 5Sec_Bars marketCode {symbolExchange} failed, ErrorCode={errorCode}, Msg={str}");
                }
#if !tmp_Fulloutput // to save all error messages while application is not tested carefully
                else
                {
                    AddMessage("IBDEBUG", $"common msg handle_Error: tickerId={tickerId}, errorCode={errorCode}, str={str}");
                }
            }
#endif
        }

        private void Unsubscribe(string contractCode)
        {
            DebugLog.AddMsg("DM.Unsubscribe, " + contractCode);
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


                var tickList = new List<int>();
                var foundSoFar = 0;
                foreach (var kvp in _5sBars.Where(kvp => kvp.Value.contractCode == contractCode))
                {
                    tickList.Add(kvp.Key);
                    if (++foundSoFar == 3) break;
                }
                foreach (var t in tickList)
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
