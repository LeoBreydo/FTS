#define WORKWITH_DELAYED_DATA // a patch to have 5s bars while have no real access to market data: produces 5s bars from "quotes"

using System;
using System.Globalization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CoreTypes;
using CoreTypes.SignalServiceClasses;
using IBApi;
using Bar = CoreTypes.Bar;

namespace BrokerFacadeIB
{
    public class DataManager
    {
        private const int START_TICKER_ID = 10000100;
        private readonly IBClient _client;

        private readonly ConcurrentDictionary<int, (string symbolExchange, string contractCode)> _registry = new();
        private readonly ConcurrentDictionary<int, (string symbolExchange, string contractCode, List<Bar> history)> _regHistoryRequests = new();
        private readonly ConcurrentDictionary<string, Contract> _symbolAndExchangeToContract;

        private readonly BlockingCollection<TickInfo> _quoteQueue = new();
        private readonly BlockingCollection<ContractInfo> _contractQueue = new();

        private readonly BlockingCollection<Tuple<string, string>> _textMessageQueue;
        private readonly BlockingCollection<(string mktExch, string contrCode,List<Bar> history)> _loadedHistory=new ();
        

        private void AddMessage(string tag, string message)
        {
            _textMessageQueue.Add(new Tuple<string, string>(tag, message));
            DebugLog.AddMsg("DataManager: "+tag + "\t" + message);

        }

        private int _currentTickerId;
        private int GetNextTickerId() { return Interlocked.Increment(ref _currentTickerId); }

        public DataManager(IBClient client,
            ConcurrentDictionary<string, Contract> symbolAndExchangeToContract,
            BlockingCollection<Tuple<string, string>> textMessageQueue)
        {
            _client = client;
            _textMessageQueue = textMessageQueue;
            _symbolAndExchangeToContract = symbolAndExchangeToContract;
            _currentTickerId = START_TICKER_ID;
            _client.ContractDetails += _client_ContractDetails;
            _client.TickPrice += _client_TickPrice;
            _client.TickSize += _client_TickSize;
            _client.HistoricalData += _client_HistoricalData;
        }

        public (List<TickInfo>, List<ContractInfo>, List<(string mktExch, string contrCode, List<Bar> historicalBars)>) 
            GetState()
        {
            return (GetValues(_quoteQueue), GetValues(_contractQueue), GetValues(_loadedHistory));
        }
        static List<T> GetValues<T>(BlockingCollection<T> queue)
        {
            var ret = new List<T>();
            int cnt = queue.Count;
            if (cnt == 0) return ret;
            int consumed = 0;
            foreach (var v in queue.GetConsumingEnumerable())
            {
                ret.Add(v);
                if (++consumed == cnt) break;
            }

            return ret;
        }


        // we should call this method at start of new day in timezone of the contracts of interest.
        private readonly List<(string, string)> _waitingReqContractDetails = new ();
        private const int MAX_REQUESTS_PER_SECOND = 50;
        public void PlaceRequest(List<(string,string)> contractCodesAndExchanges)
        {
            // NB _waitingReqContractDetails is used from main thread only, so thread safety is not needed here
            foreach (var item in contractCodesAndExchanges)
                if (!_waitingReqContractDetails.Contains(item))
                    _waitingReqContractDetails.Add(item);

            if (_waitingReqContractDetails.Count==0 || !_client.ConnectionEstablished) return;

            int limit = Math.Min(_waitingReqContractDetails.Count, MAX_REQUESTS_PER_SECOND);
            int i;
            for (i = 0; i < limit && _client.ConnectionEstablished; ++i)
            {
                var (contractCode, exchange) = _waitingReqContractDetails[i];
                RequestContractDetails(contractCode, exchange);
            }
            _waitingReqContractDetails.RemoveRange(0, i);
        }

        public bool GetMarketCode(int id, out string marketCode)
        {
            if (!_registry.TryGetValue(id, out var symbExch))
            {
                marketCode = null;
                return false;
            }
            marketCode = symbExch.symbolExchange;
            return true;
        }

        private void _client_ContractDetails(ContractDetailsMessage contractDetails)
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
            if (_symbolAndExchangeToContract.TryGetValue(key, out var prevContract))
            {
                var previousCode = prevContract.LocalSymbol;
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
                _symbolAndExchangeToContract[key] = cntr;
                Subscribe(key);
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


            _contractQueue.Add(new ContractInfo(contractDetails.Details));
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
            _quoteQueue.Add(new TickInfo(info.symbolExchange, info.contractCode, tickPrice.Field, tickPrice.Price));
        }
        private void _client_TickSize(TickSizeMessage tickSize)
        {
            if (tickSize.Field > 5) return; // ignore delayed data
            if (!_registry.TryGetValue(tickSize.RequestId, out var info)) return;
            string symbolExchange = _registry[tickSize.RequestId].symbolExchange;
            _quoteQueue.Add(new TickInfo(symbolExchange, info.contractCode, tickSize.Field, tickSize.Size));
        }
#endif

        private void _client_HistoricalData(int reqId, IBApi.Bar bar)
        {
            if (!_regHistoryRequests.TryGetValue(reqId, out var symbolExchange_contractcode_bars)) return;

            if (bar == null) // end of history
            {
                _regHistoryRequests.Remove(reqId, out _);
                _loadedHistory.Add(symbolExchange_contractcode_bars);
            }
            else
                symbolExchange_contractcode_bars.history.Add(ConvertMinuteBar(bar));
        }
        private static Bar ConvertMinuteBar(IBApi.Bar bar)
        {
            if (bar == null) return null;

            var openTime = DateTime.ParseExact(bar.Time, "yyyyMMdd  HH:mm:ss", null,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);


            return new Bar(bar.Open, bar.High, bar.Low, bar.Close, openTime, openTime.AddMinutes(1));
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
            if (!_symbolAndExchangeToContract.TryGetValue(symbolExchange, out var contract))
            {
                AddMessage("WARNING", $"Invalid symbolExchange used in subscription request ({symbolExchange})");
                return;
            }

            var tickerId = GetNextTickerId();
            DebugLog.AddMsg(string.Format("DM.Subscribe, {0}; tickerId={1}", symbolExchange, tickerId));
            _registry[tickerId] = (symbolExchange, contract.LocalSymbol);

            _client.ClientSocket.reqMarketDataType(3); // todo!!! to check what for we use this call, it looks useless
            _client.ClientSocket.reqMktData(tickerId, contract, string.Empty, false, false, null);

            QueryHistoricalData(symbolExchange);
        }

        public void QueryHistoricalData(string symbolExchange)
        {
            if (!_symbolAndExchangeToContract.TryGetValue(symbolExchange, out var contract))
            {
                AddMessage("WARNING", $"Invalid symbolExchange used in historical data request ({symbolExchange})");
                return;
            }

            var tickerId = GetNextTickerId();
            _regHistoryRequests[tickerId] = (symbolExchange, contract.LocalSymbol,new List<Bar>());
            var end = GetBeginOfMinute(DateTime.UtcNow).ToString("yyyyMMdd HH:mm:ss"); // nearest border of the requested data converted to used format 
            _client.ClientSocket.reqHistoricalData(tickerId, contract, end,
                "1 W", "1 min", "TRADES", 1, 1, false, null);
        }

        private static DateTime GetBeginOfMinute(DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0, t.Kind);
        }

        public void handle_Error(int tickerId, int errorCode, string str)
        {
            DebugLog.AddMsg(string.Format("handle_Error {0},{1}; tickerId={2}", errorCode, str, tickerId));

            if (_registry.Remove(tickerId, out var symbolExchange_contractcode))
            {
                AddMessage("ERROR",
                    $"Subscription to {symbolExchange_contractcode.symbolExchange} contract {symbolExchange_contractcode} failed, ErrorCode={errorCode}, Msg={str}");
            }
            else if (_regHistoryRequests.Remove(tickerId, out var symbolExchange_contractcode_bars))
            {
                _loadedHistory.Add(symbolExchange_contractcode_bars);

                AddMessage("ERROR",
                    $"Failed to load historical market data for {symbolExchange_contractcode.symbolExchange}, Contract={symbolExchange_contractcode.contractCode}. Msg={str}");
            }
        }

        private void Unsubscribe(string contractCode)
        {
            DebugLog.AddMsg("DM.Unsubscribe, " + contractCode);

            contractCode = contractCode.Trim().ToUpper();
            var ticker = -1;
            foreach (var kvp in _registry.Where(kvp => kvp.Value.contractCode == contractCode))
            {
                ticker = kvp.Key;
                break;
            }
            if (ticker == -1) return;

            _registry.Remove(ticker, out _);
            _client.ClientSocket.cancelMktData(ticker);
        }

        public void UnsubscribeAll()
        {
            foreach (var ticker in _registry.Select(kvp => kvp.Key).ToArray())
                _client.ClientSocket.cancelMktData(ticker);

            ClearState();
        }

        public void ClearState()
        {
            _symbolAndExchangeToContract.Clear();
            _registry.Clear();
        }

        ~DataManager()
        {
            UnsubscribeAll();

            _quoteQueue?.CompleteAdding();
            _quoteQueue?.Dispose();

            _contractQueue?.CompleteAdding();
            _contractQueue?.Dispose();
        }
    }

}
