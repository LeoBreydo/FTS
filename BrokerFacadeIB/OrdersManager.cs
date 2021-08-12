using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTypes;
using IBApi;


namespace BrokerFacadeIB
{
    public class OrdersManager
    {
        private const int MAX_ORDERS_PER_SECOND = 45;
        private const int CLEAN_ORDERS_FREQUENCY_IN_SECONDS = 300;
        private const int CLEAN_ORDERS_SENT_N_SECONDS_AGO = 300; // for filled orders (5 minutes)
        private const int CLEAN_NOTFILLED_ORDERS_SENT_N_SECONDS_AGO = 4*60*60; // for not-filled orders  (4 hours)

        private readonly IBClient _client;

        private readonly BlockingCollection<OrderStateMessage> _orderReportQueue = new();
        private readonly BlockingCollection<Tuple<string, string>> _textMessageQueue;
        private void AddMessage(string tag, string message) => 
            _textMessageQueue.Add(new Tuple<string, string>(tag, message));

        private int _sentCount;
        private readonly Queue<MarketOrderDescription> _waitingOrders = new ();
        private bool _forceUpdateNextIdTime = true;

        private readonly Dictionary<string, Contract> _symbolAndExchangeToContract;

        class OrderInfo
        {
            public readonly int OrderId;
            public readonly int ClOrderId;
            public readonly string Symbol;
            public readonly string Action;
            public readonly long OrderQty;
            public readonly List<string> ExecIds = new(30);
            public long CreationSecond;
            public long LeaveQty;

            public OrderInfo(int orderId,int clOrderId, long creationSecond, long orderQty,string symbol,string action)
            {
                OrderId = orderId;
                ClOrderId = clOrderId;
                CreationSecond = creationSecond;
                OrderQty = LeaveQty= orderQty;
                Symbol=symbol;
                Action = action;
            }
        }

        private readonly List<OrderInfo> _activeOrders = new();
        private OrderInfo[] _activeOrdersR = Array.Empty<OrderInfo>(); // for threadsafe read of the _activeOrders
        private long _secondsCount;
        private int _nextOrderId;

        public OrdersManager(IBClient client, 
            Dictionary<string, Contract> symbolAndExchangeToContract,
            BlockingCollection<Tuple<string, string>> textMessageQueue)
        {
            _client = client;
            _symbolAndExchangeToContract = symbolAndExchangeToContract;
            _textMessageQueue = textMessageQueue;
            _client.OrderStatus += handle_OrderStatus;
            _client.ExecDetails += handle_ExecDetails;
            _client.ExecDetailsEnd += ProcessEndExecReports;
        }

        public List<OrderStateMessage> GetState()
        {
            SecondPulse();
            var cnt = _orderReportQueue.Count;
            var consumed = 0;
            List<OrderStateMessage> oReports = new();
            if (cnt > 0)
                foreach (var or in _orderReportQueue.GetConsumingEnumerable())
                {
                    oReports.Add(or);
                    if (++consumed == cnt) break;
                }

            return oReports;
        }

        public void PlaceRequest(
            List<MarketOrderDescription> orders)
        {
            foreach(var o in orders) SendOrder(o);
        }

        ~OrdersManager()
        {
            _activeOrders.Clear();
            _waitingOrders.Clear();
            _orderReportQueue.CompleteAdding();
            _orderReportQueue.Dispose();
        }

        private void handle_ExecDetails(ExecutionMessage msg)
        {
            ProcessExecReport(msg, FindOrder(msg.Execution.OrderId, out var oi)?oi:null);
        }

        private bool ExistsOrder(int orderId)
        {
            return _activeOrdersR.Any(oi => oi.OrderId == orderId);
        }
        private bool FindOrder(int orderId, out OrderInfo oi)
        {
            oi= _activeOrdersR.FirstOrDefault(item => item.OrderId == orderId);
            return oi != null;
        }

        private const int UNKNOWN_ClOrderID = -1000000;
        private void handle_OrderStatus(OrderStatusMessage obj)
        {
            var oid = obj.OrderId;
            var status = obj.Status;
            int clOrdID = FindOrder(oid, out var oi) ? oi.ClOrderId : UNKNOWN_ClOrderID;
            switch (status)
            {
                case "Cancelled":
                case "Inactive":
                    _orderReportQueue.Add(new OrderCancelMessage(clOrdID, DateTime.UtcNow, oid, "Rejection: "+status));
                    break;
            }
        }
        public void handle_Error(int oid, int errorCode, string str)
        {
            // from documentation  https://interactivebrokers.github.io/tws-api/error_handling.html
            // Error messages sent by the TWS are handled by the IBApi.EWrapper.error method. 
            // The IBApi.EWrapper.error event contains the originating request Id(or the orderId in case the error was raised when placing an order)...
            // During the tests it was detected that rejection report can be returned using this message with id:=orderId

            if (FindOrder(oid, out var orderInfo))
                _orderReportQueue.Add(new OrderCancelMessage(orderInfo.ClOrderId, DateTime.UtcNow, oid, $"errorCode={errorCode}; {str}"));
        }
        
        public void SecondPulse()
        {
            ++_secondsCount;
            funQueryMissedExecReports?.Invoke();

            if (_secondsCount % 60 == 0)
            {
                // shift counter a bit forward to have next long-time sync procedure not at the minute border but earlier (about 15 sec to minute border).
                _forceUpdateNextIdTime = true; // every minute
                var curSecond = DateTime.UtcNow.Second;
                if (curSecond < 2 || curSecond >= 58)
                    _secondsCount += 15; 
            }

            if (_secondsCount % CLEAN_ORDERS_FREQUENCY_IN_SECONDS == 0)
                RemoveOldOrders();

            if (_forceUpdateNextIdTime && _client.ClientSocket.IsConnected())
                UpdateNextOrderId();

            SendWaitingOrders();
        }
        
        public void OnConnectionRestored()
        {
            InitiateQueryMissedExecReports(4, false); // arg=number of seconds to let trading server initiate quote stream and prepare OrderBooks used in execution subsystem.

            // do not remove orders first 5 minutes after the connection restored event
            foreach (var oi in _activeOrders) oi.CreationSecond = _secondsCount;
        }

        private void RemoveOldOrders()
        {
            var removeBefore = _secondsCount - CLEAN_ORDERS_SENT_N_SECONDS_AGO;
            var removeNotFilledBefore = _secondsCount - CLEAN_NOTFILLED_ORDERS_SENT_N_SECONDS_AGO; 
            // remember removed orders
            var removedItems = _activeOrders.Where(oi =>
                oi.LeaveQty <= 0
                    ? oi.CreationSecond <= removeBefore
                    : oi.CreationSecond <= removeNotFilledBefore).ToList();
            // remove them
            _activeOrders.RemoveAll(oi =>
                oi.LeaveQty <= 0
                    ? oi.CreationSecond <= removeBefore
                    : oi.CreationSecond <= removeNotFilledBefore);
            _activeOrdersR = _activeOrders.ToArray();
            // make report messages
            if (removedItems.Count > 0)
            {
                AddMessage("INFO","Orders with following OrderID cleaned out from ActiveOrders list: "
                                                                 + string.Join(",", removedItems.Select(item => item.OrderId)));

                foreach (var oi in removedItems.Where(item=>item.LeaveQty>0))
                {
                    var msg =
                        $"Not-filled Order {oi.Action} {oi.OrderQty} {oi.Symbol}, OrderId={oi.OrderId}, ClOrderId={oi.ClOrderId} removed by timeout from ActiveOrders list. NOT FILLED AMOUNT={oi.LeaveQty}";
                    AddMessage("CLIENT", msg);
                }
            }
        }
        private void UpdateNextOrderId()
        {
            AddMessage("INFO","re-request nextOrderId");
            _client.NextOrderId = 0;
            _client.ClientSocket.reqIds(-1);
            var t = Task.Run(() =>
            {
                while (_client.NextOrderId <= 0)
                {
                }
                _forceUpdateNextIdTime = false;
            });
            t.Wait(200);
            var cl_nextOrderId = _client.NextOrderId;
            if (cl_nextOrderId > 0 && cl_nextOrderId > _nextOrderId)
            {
                _nextOrderId = cl_nextOrderId;
                AddMessage("INFO","nextOrderId updated to " + _nextOrderId);
            }
        }

        private void RefuseWaitingOrders()
        {
            var waitCnt = _waitingOrders.Count;
            for (var i = 0; i < waitCnt; ++i)
            {
                var order = _waitingOrders.Dequeue();
                _orderReportQueue.Add(
                    new OrderCancelMessage(order.ClOrdId, DateTime.UtcNow, -1, "Impossible to send order - no connection"));
            }
        }

        private void SendWaitingOrders()
        {
            _sentCount = 0;

            var waitCnt = _waitingOrders.Count;
            if (waitCnt == 0) return;
            if (waitCnt <= MAX_ORDERS_PER_SECOND)
            {
                for (var i = 0; i < waitCnt; ++i)
                    SendMarketOrder(_waitingOrders.Dequeue());
            }
            else
            {
                for (var i = 0; i < MAX_ORDERS_PER_SECOND; ++i)
                    SendMarketOrder(_waitingOrders.Dequeue());
            }
        }

        private void SendOrder(MarketOrderDescription order)
        {
            if (!_client.ClientSocket.IsConnected() || _nextOrderId <= 0)
                return;

            if (_sentCount < MAX_ORDERS_PER_SECOND)
                SendMarketOrder(order);
            else
                _waitingOrders.Enqueue(order);
        }

        private void SendMarketOrder(MarketOrderDescription order)
        {
            var action = order.SignedContractsNbr  >  0 ? "BUY":"SELL";
            var qty = Math.Abs(order.SignedContractsNbr);
            var clientOrderId = order.ClOrdId;
            var key = order.Symbol + order.Exchange;
            if (!_symbolAndExchangeToContract.ContainsKey(key))
                return;

            var contract = _symbolAndExchangeToContract[key];
            var o = new IBApi.Order
            {
                Action = action,
                OrderType = "MKT",
                TotalQuantity = qty,
            };
            var orderId = _nextOrderId++;
            try
            {
                if (ExistsOrder(orderId))
                {
                    // an emergency check, existing item will lead to exception and financial loss
                    AddMessage("WARNING","Fired the IBOrderManager.SendMarketOrder() emergency check 'duplicated orderID' " + orderId);
                    while (ExistsOrder(orderId)) orderId = _nextOrderId++;
                    AddMessage("INFO","orderId corrected -> " + orderId);
                }

                _activeOrders.Add(new OrderInfo(orderId, clientOrderId, _secondsCount, qty, order.Symbol, action));
                _activeOrdersR = _activeOrders.ToArray();

                _client.ClientSocket.placeOrder(orderId, contract, o);
                ++_sentCount;
            }
            catch (Exception exception)
            {
                _orderReportQueue.Add(
                    new OrderCancelMessage(clientOrderId, DateTime.UtcNow, orderId, "Exception:" + exception.Message));
            }
        }

        public void ClearState()
        {
            RefuseWaitingOrders();
            ResetQueryMissedExecRep();
            _forceUpdateNextIdTime = true;
        }

        private static string ExecToStr(ExecutionMessage msg)
        {
            return
                $"OrderId={msg.Execution.OrderId}, ExecId={msg.Execution.ExecId}, Time={msg.Execution.Time}: {msg.Execution.Side} {msg.Execution.Shares} at {msg.Execution.Price}; ReqId={msg.ReqId}";
        }

        private int _nextReqId = 10000;
        private long _sec_toRequestMissedReports;
        private long _cancelMissedOrdersBeforeSecond;
        private Action funQueryMissedExecReports;


        private void InitiateQueryMissedExecReports(int postponeInSeconds,bool cancelMissedOrders)
        {
            var orders = _activeOrdersR;
            if (orders.Length == 0 || orders.All(oi => oi.LeaveQty == 0))
                return;  // no active orders or

            AddMessage("INFO",
                $"InitiateQueryMissedExecReports postpone={postponeInSeconds}, cancelMissed={cancelMissedOrders}");


            _sec_toRequestMissedReports = _secondsCount + postponeInSeconds + 1;
            _cancelMissedOrdersBeforeSecond = cancelMissedOrders ? 0 : _secondsCount;

            funQueryMissedExecReports = WaitThenQueryMissedExecReports;
        }

        private void WaitThenQueryMissedExecReports()
        {
            if (_secondsCount >= _sec_toRequestMissedReports)
            {
                funQueryMissedExecReports = null;
                QueryMissedExecReports();
            }
        }

        private void ResetQueryMissedExecRep()
        {
            funQueryMissedExecReports = null;
        }
        private void QueryMissedExecReports()
        {
            var reqId = ++_nextReqId;
            AddMessage("INFO","QueryMissedExecReports, reqId=" + reqId);
            _client.ClientSocket.reqExecutions(reqId, new ExecutionFilter());
        }
        private void ProcessEndExecReports(int reqId)
        {
            ResetQueryMissedExecRep();
            if (reqId != _nextReqId) return;

            if (_cancelMissedOrdersBeforeSecond>0)
                CancelMissedReports();
            else
            {
                // postponed re-query exec reports after the  2min stable work to solve not-completed orders
                if (_activeOrdersR.Any(oi => oi.LeaveQty > 0))
                    InitiateQueryMissedExecReports(120, true);
            } 
        }

        private void CancelMissedReports()
        {
            var ordersToCancel = _activeOrdersR
                .Where(oi => oi.CreationSecond <= _cancelMissedOrdersBeforeSecond && oi.LeaveQty > 0).ToArray();
            foreach (var oi in ordersToCancel)
                _orderReportQueue.Add(new OrderCancelMessage(oi.ClOrderId, DateTime.UtcNow, oi.OrderId, "Order lost"));
        }

        private void ProcessExecReport(ExecutionMessage msg, OrderInfo oi)
        {
            AddMessage("INFO", "ProcessTrade " + ExecToStr(msg));
            if (oi == null)
            {
                if (msg.ReqId > 0) // historical exec report
                    AddMessage("INFO","Ignore obsolete historical ExecReport: " + ExecToStr(msg));
                else
                {
                    _orderReportQueue.Add(new OrderExecutionMessage(UNKNOWN_ClOrderID, DateTime.UtcNow,
                        msg.Execution.OrderId, msg.Execution.ExecId,
                        msg.Contract.Symbol, msg.Contract.LocalSymbol, msg.Contract.Exchange,
                        (int) msg.Execution.Shares * (msg.Execution.Side == "BOT" ? 1 : -1),
                        (int) msg.Execution.CumQty, (decimal) msg.Execution.Price,
                        msg.Execution.Time.ParseIBDateTime()));
                }
                return;
            }
            if (oi.ExecIds.Contains(msg.Execution.ExecId))
            {
                AddMessage("INFO","Ignore duplicated ExecReport: " + ExecToStr(msg));
                return;
            }
            oi.ExecIds.Add(msg.Execution.ExecId);
            var qty = (long)msg.Execution.Shares;
            oi.LeaveQty -= qty;
            _orderReportQueue.Add(new OrderExecutionMessage(oi.ClOrderId, DateTime.UtcNow,
                msg.Execution.OrderId, msg.Execution.ExecId,
                msg.Contract.Symbol, msg.Contract.LocalSymbol, msg.Contract.Exchange,
                (int)msg.Execution.Shares * (msg.Execution.Side == "BOT" ? 1 : -1),
                (int)msg.Execution.CumQty, (decimal)msg.Execution.Price,
                msg.Execution.Time.ParseIBDateTime()));
        }
    }
}
