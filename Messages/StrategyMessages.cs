using System;
using CommonStructures;
//using CfgDescription;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace Messages
{
    /// <summary>
    /// An error occured during strategy instantiation or runtime comutation
    /// </summary>
    public class StrategyErrorMsg : BaseMessage
    {
        public long StrategyID;
        public string Text;
        public StrategyErrorMsg() : base(MessageNumbers.StrategyErrorMsg) { }
        public StrategyErrorMsg(long strategyID, string msg)
            : base(MessageNumbers.StrategyErrorMsg)
        {
            StrategyID = strategyID;
            Text = msg;
        }
    }
    /// <summary>
    /// An extraordinary situation detected
    /// </summary>
    public class StrategyWarningMsg : BaseMessage
    {
        public long StrategyID;
        public string Text;
        public StrategyWarningMsg() : base(MessageNumbers.StrategyWarningMsg) { }
        public StrategyWarningMsg(long strategyID, string msg)
            : base(MessageNumbers.StrategyWarningMsg)
        {
            StrategyID = strategyID;
            Text = msg;
        }
    }

    /// <summary>
    /// Saves the obtained new values of the indicators and the theoretical position calculated from this values.
    /// </summary>
    public class StrategyNewInputsCalculated : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TheoreticalPosition; // this field is obsolete in the version 2. But do not delete it for a while to keep working the processing of the existing logs
        public int SignalGeneratorValue;
        public double[] Inputs;

        public StrategyNewInputsCalculated() : base(MessageNumbers.StrategyNewInputsCalculated) { }
        public StrategyNewInputsCalculated(long strategyID, string symbol, int signalGeneratorValue, double[] inputs)
            : base(MessageNumbers.StrategyNewInputsCalculated)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            SignalGeneratorValue = signalGeneratorValue;
            Inputs = inputs;
        }
    }

    public class InputIndicatorValues : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public double[] Inputs;
        public bool IgnoreTradingZones;

        public InputIndicatorValues() : base(MessageNumbers.InputIndicatorValues) { }
        public InputIndicatorValues(long strategyID, string symbol, double[] inputs, bool ignoreTradingZones)
            : base(MessageNumbers.InputIndicatorValues)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            Inputs = inputs;
            IgnoreTradingZones = ignoreTradingZones;
        }
    }
    public class SignalGeneratorValue : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public int Value;

        public SignalGeneratorValue() : base(MessageNumbers.SignalGeneratorValue) { }
        public SignalGeneratorValue(long strategyID, string symbol, int value)
            : base(MessageNumbers.SignalGeneratorValue)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            Value = value;
        }
    }
    public class DelayedTargetPosition : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public int Value;

        public DelayedTargetPosition() : base(MessageNumbers.DelayedTargetPosition) { }
        public DelayedTargetPosition(long strategyID, string symbol, int value)
            : base(MessageNumbers.DelayedTargetPosition)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            Value = value;
        }
    }

    public class NewTheoreticalPosition : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TheoreticalPosition;

        public NewTheoreticalPosition() : base(MessageNumbers.NewTheoreticalPosition) { }

        public NewTheoreticalPosition(long strategyId, string symbol, string theoreticalPosition)
            : base(MessageNumbers.NewTheoreticalPosition)
        {
            StrategyID = strategyId;
            Symbol = symbol;
            TheoreticalPosition = theoreticalPosition;
        }
        
    }

    /// <summary>
    /// New trend monitor value received by strategy
    /// </summary>
    public class NewTrendMonitorValue : BaseMessage
    {
        public long StrategyID;
        public int TrendMonitorValue;

        public NewTrendMonitorValue() : base(MessageNumbers.NewTrendMonitorValue) { }

        public NewTrendMonitorValue(long strategyId, int trendMonitorValue)
            : base(MessageNumbers.NewTrendMonitorValue)
        {
            StrategyID = strategyId;
            TrendMonitorValue = trendMonitorValue;
        }
    }

    public class SignalTransformerZoneChanged : BaseMessage
    {
        public long StrategyID;
        public string BarZoneType;

        public SignalTransformerZoneChanged() : base(MessageNumbers.SignalTransformerZoneChanged) { }

        public SignalTransformerZoneChanged(long strategyId, string barZoneType)
            : base(MessageNumbers.SignalTransformerZoneChanged)
        {
            StrategyID = strategyId;
            BarZoneType = barZoneType;
        }
    }

    /// <summary>
    /// Obsolette message
    /// strategy (its transaction manager) made new decision by the all input data (strategy signal, trend monitor value, last prices etc)
    /// </summary>
    public class NewTransactionManagerDecision : BaseMessage
    {
        public long StrategyID;
        public bool IsRealStrategy;
        public string TransactionID;
        public int TargetPosition;
        public long CurrentPosition;

        public NewTransactionManagerDecision() : base(MessageNumbers.NewTransactionManagerDecision) { }
        public NewTransactionManagerDecision(long strategyId, bool isRealStrategy, string transactionID, int targetPosition, long currentPosition)
            : base(MessageNumbers.NewTransactionManagerDecision)
        {
            StrategyID = strategyId;
            IsRealStrategy = isRealStrategy;
            TransactionID = transactionID;

            TargetPosition = targetPosition;
            CurrentPosition = currentPosition;
        }
    }

    public class NewStrategyTargetPosition : BaseMessage
    {
        public long StrategyID;
        public bool IsRealStrategy;
        public string Symbol;
        public string TransactionID;

        public int TargetPosition; // the sign of the target positions
        public string StrategyState;
        public long CurOpenPosition;
        public string Reason;

        public long CurrentProviderID;
        public double CurrentProviderBid;
        public double CurrentProviderAsk;

        
            
        public NewStrategyTargetPosition() : base(MessageNumbers.NewStrategyTargetPosition) { }
        public NewStrategyTargetPosition(long strategyID, bool isRealStrategy, string symbol, string transactionID, 
            int targetPosition, string strategyState, long curOpenPosition,string reason, 
            long currentProviderID, double currentProviderBid, double currentProviderAsk)
            : base(MessageNumbers.NewStrategyTargetPosition) 
        {
            StrategyID = strategyID;
            IsRealStrategy = isRealStrategy;
            Symbol = symbol;
            TransactionID = transactionID;

            TargetPosition = targetPosition;
            StrategyState = strategyState;
            CurOpenPosition = curOpenPosition;
            Reason = reason;

            CurrentProviderID = currentProviderID;
            CurrentProviderBid = currentProviderBid;
            CurrentProviderAsk = currentProviderAsk;
        }
    }

    /// <summary>
    /// Strategy ordered the new amount 
    /// </summary>
    public class StrategyNewOrderedAmount : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TransactionID;

        public long OrderedAmount;
        public int? TargetPosition;   // new field
        public string StrategyState;
        public long CurOpenPosition;
        public string Reason;

        public long CurrentProviderID;
        public double CurrentProviderBid;
        public double CurrentProviderAsk;
                
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side { get { return OrderedAmount > 0 ? OrderSide.Buy : OrderSide.Sell; } }
        public long AbsOrderedAmount { get { return Math.Abs(OrderedAmount); } }
        public double CurrentProviderPrice { get { return OrderedAmount > 0 ? CurrentProviderAsk : CurrentProviderBid; } }

        public StrategyNewOrderedAmount() : base(MessageNumbers.StrategyNewOrderedAmount) { }
        public StrategyNewOrderedAmount(long strategyID, string symbol, string transactionID,
            long orderedAmount, int targetPosition,string strategyState, long curOpenPosition, 
            string reason,
            long currentProviderID,double currentProviderBid,double currentProviderAsk)
            : base(MessageNumbers.StrategyNewOrderedAmount)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            TransactionID = transactionID;

            OrderedAmount = orderedAmount;
            TargetPosition = targetPosition;
            StrategyState = strategyState;
            CurOpenPosition = curOpenPosition;
            Reason = reason;

            CurrentProviderID = currentProviderID;
            CurrentProviderBid = currentProviderBid;
            CurrentProviderAsk = currentProviderAsk;
        }
    }

    /// <summary>
    /// Transaction manager sent order for execution (the closing order or order was accepted by trading restrictions)
    /// </summary>
    public class OrderExecutionStarted : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TransactionID;

        public long OrderedAmount;
        public int TargetPosition;   
        public string StrategyState;
        public long CurOpenPosition;
        public string Reason;

        public long CurrentProviderID;
        public double CurrentProviderBid;
        public double CurrentProviderAsk;

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side { get { return OrderedAmount > 0 ? OrderSide.Buy : OrderSide.Sell; } }
        public long AbsOrderedAmount { get { return Math.Abs(OrderedAmount); } }
        public double CurrentProviderPrice { get { return OrderedAmount > 0 ? CurrentProviderAsk : CurrentProviderBid; } }

        public OrderExecutionStarted() : base(MessageNumbers.OrderExecutionStarted) { }
        public OrderExecutionStarted(long strategyID, string symbol, string transactionID,
            long orderedAmount, int targetPosition, string strategyState, long curOpenPosition,
            string reason,
            long currentProviderID, double currentProviderBid, double currentProviderAsk)
            : base(MessageNumbers.OrderExecutionStarted)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            TransactionID = transactionID;

            OrderedAmount = orderedAmount;
            TargetPosition = targetPosition;
            StrategyState = strategyState;
            CurOpenPosition = curOpenPosition;
            Reason = reason;

            CurrentProviderID = currentProviderID;
            CurrentProviderBid = currentProviderBid;
            CurrentProviderAsk = currentProviderAsk;
        }
    }

    /// <summary>
    /// Transaction manager postponed the order execution because of  trading restrictions
    /// </summary>
    public class OrderExecutionPostponed : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TransactionID;

        public int TargetPosition;  
        public string StrategyState;
        public long CurOpenPosition;
        public string DelayReason;

        public long CurrentProviderID;
        public double CurrentProviderBid;
        public double CurrentProviderAsk;


        public OrderExecutionPostponed() : base(MessageNumbers.OrderExecutionPostponed) { }
        public OrderExecutionPostponed(long strategyID, string symbol, string transactionID,
            int targetPosition, string strategyState, long curOpenPosition,
            string delayReason,
            long currentProviderID, double currentProviderBid, double currentProviderAsk)
            : base(MessageNumbers.OrderExecutionPostponed)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            TransactionID = transactionID;

            TargetPosition = targetPosition;
            StrategyState = strategyState;
            CurOpenPosition = curOpenPosition;
            DelayReason = delayReason;

            CurrentProviderID = currentProviderID;
            CurrentProviderBid = currentProviderBid;
            CurrentProviderAsk = currentProviderAsk;
        }
    }

    public class TransactionDone : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TransactionID;

        public TransactionDone() : base(MessageNumbers.TransactionDone) { }
        public TransactionDone(long strategyID, string symbol, string transactionID)
            : base(MessageNumbers.TransactionDone)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            TransactionID = transactionID;
        }
    }

    public class TransactionCancelled : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TransactionID;

        public string TransactionCancelReason;

        public int TargetPosition;  // the sign of the target position, which is not reached via cancel of the transaction
        public string StrategyState;
        public long CurOpenPosition;
        public long LostOrderedAmount; // the LeaveOrderedAmount(amount already sent to broker but not filled) at the timepoint when transaction was terminated

        public long CurrentProviderID;
        public double CurrentProviderBid;
        public double CurrentProviderAsk;

        public TransactionCancelled() : base(MessageNumbers.TransactionCancelled) { }
        public TransactionCancelled(long strategyID, string symbol, string transactionID, 
            int targetPosition, string strategyState, long curOpenPosition,string transactionCancelReason, 
            long currentProviderID, double currentProviderBid, double currentProviderAsk)
            : base(MessageNumbers.TransactionCancelled)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            TransactionID = transactionID;

            TargetPosition = targetPosition;
            StrategyState = strategyState;
            CurOpenPosition = curOpenPosition;

            TransactionCancelReason = transactionCancelReason;

            CurrentProviderID = currentProviderID;
            CurrentProviderBid = currentProviderBid;
            CurrentProviderAsk = currentProviderAsk;
        }

    }


    /// <summary>
    /// Indicates to which broker was directed the strategy order
    /// </summary>
    public class StrategySendOrderToBroker : BaseMessage
    {
        public long StrategyID;
        public string TransactionID;
        public long BrokerID;
        public long OrderedAmount;
        public string ClOrdID;
        public string Symbol;
        public double BidWhenSendOrder;
        public double AskWhenSendOrder;

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide Side { get { return OrderedAmount > 0 ? OrderSide.Buy : OrderSide.Sell; } }
        public long AbsOrderedAmount { get { return Math.Abs(OrderedAmount); } }
        public double PriceWhenSendOrder
        {
            get
            {
                return OrderedAmount > 0 ? AskWhenSendOrder : BidWhenSendOrder;
            }
        }

        public StrategySendOrderToBroker() : base(MessageNumbers.StrategySendOrderToBroker) { }
        public StrategySendOrderToBroker(long strategyID, string transactionID, long brokerID, long orderedAmount, string clOrderID, string symbol, double postingBid, double postingAsk)
            : base(MessageNumbers.StrategySendOrderToBroker)
        {
            StrategyID = strategyID;
            TransactionID = transactionID;
            BrokerID = brokerID;
            OrderedAmount = orderedAmount;
            ClOrdID = clOrderID;
            Symbol = symbol;
            BidWhenSendOrder = postingBid;
            AskWhenSendOrder = postingAsk;
        }
    }
    /// <summary>
    /// The strategy order executor accepted the fill from broker (its possible multiple fills messages for each order; RestOrderedAmount=0 indicates that order is done)
    /// </summary>
    public class FillAcceptedByExecutor:BaseMessage
    {
        public long StrategyID;
        public string TransactionID;
        public DateTime OrderStartTime;
        public FillsItem Fill;
        public long RestOrderedAmount;

        public FillAcceptedByExecutor() : base(MessageNumbers.FillAcceptedByExecutor) { }
        public FillAcceptedByExecutor(long strategyID, string transactionID, DateTime orderStartTime, FillsItem fill, long restOrderedAmount)//long brokerID, string clOrderID, string orderId, string execId, long qty, double price, string currencyPair, 
            : base(MessageNumbers.FillAcceptedByExecutor)
        {
            StrategyID = strategyID;
            TransactionID = transactionID;
            OrderStartTime = orderStartTime;
            Fill = fill;
            RestOrderedAmount = restOrderedAmount;
        }
    }

    /// <summary>
    /// The order execution is terminated (rejected, cancelled, expired or lost by broker).
    /// </summary>
    public class StrategySentOrderWasNotFilled:BaseMessage
    {
        public long StrategyID;
        public string TransactionID;
        public long BrokerID;
        public string Symbol;
        public string ClOrdID;
        public DateTime OrderStartTime;
        public string FinishedState;
        public long NonFilledOrderedAmount;

        public StrategySentOrderWasNotFilled() : base(MessageNumbers.StrategySentOrderWasNotFilled) { }
        public StrategySentOrderWasNotFilled(long strategyID, string transactionID, long brokerID, string symbol, string clOrderID, DateTime orderStartTime, string finishedState, long nonFilledOrderedAmount)
            : base(MessageNumbers.StrategySentOrderWasNotFilled)
        {
            StrategyID = strategyID;
            TransactionID = transactionID;
            BrokerID = brokerID;
            Symbol = symbol;
            ClOrdID = clOrderID;
            OrderStartTime = orderStartTime;
            FinishedState = finishedState;
            NonFilledOrderedAmount = nonFilledOrderedAmount;
        }
    }


    public class StrategyOrderExecutionSuspendedViaAttemptsLeft : BaseMessage
    {
        public long StrategyID;
        public string TransactionID;
        public long NonFilledOrderedAmount;

        public StrategyOrderExecutionSuspendedViaAttemptsLeft() : base(MessageNumbers.StrategyOrderExecutionSuspendedViaAttemptsLeft) { }
        public StrategyOrderExecutionSuspendedViaAttemptsLeft(long strategyID, string transactionID, long nonFilledOrderedAmount)
            : base(MessageNumbers.StrategyOrderExecutionSuspendedViaAttemptsLeft)
        {
            StrategyID = strategyID;
            TransactionID = transactionID;
            NonFilledOrderedAmount = nonFilledOrderedAmount;
        }
    }

    public class StrategyOrderExecutionSuspendedViaResponseTimeout : BaseMessage
    {
        public long StrategyID;
        public string TransactionID;

        public long BrokerID;
        public string ClOrdID;
        public DateTime OrderStartTime;
        public long SecondsSinceOrderSentToBroker;
        public long NonFilledOrderedAmount;

        public StrategyOrderExecutionSuspendedViaResponseTimeout() : base(MessageNumbers.StrategyOrderExecutionSuspendedViaResponseTimeout) { }
        public StrategyOrderExecutionSuspendedViaResponseTimeout(long strategyID, string transactionID, long brokerID, string clOrderID, DateTime orderStartTime, long secondsSinceOrderSentToBroker, long nonFilledOrderedAmount)
            : base(MessageNumbers.StrategyOrderExecutionSuspendedViaResponseTimeout)
        {
            StrategyID = strategyID;
            TransactionID = transactionID;
            BrokerID = brokerID;
            ClOrdID = clOrderID;
            OrderStartTime = orderStartTime;
            SecondsSinceOrderSentToBroker = secondsSinceOrderSentToBroker;
            NonFilledOrderedAmount = nonFilledOrderedAmount;
        }
    }

    public class StrategyRestartSuspendedOrder : BaseMessage
    {
        public long StrategyID;
        public string TransactionID;

        public bool AvoidBadBroker;

        public StrategyRestartSuspendedOrder() : base(MessageNumbers.StrategyRestartSuspendedOrder) { }
        public StrategyRestartSuspendedOrder(long strategyID, string transactionID, bool avoidBadBroker)
            : base(MessageNumbers.StrategyRestartSuspendedOrder)
        {
            StrategyID = strategyID;
            TransactionID = transactionID;
            AvoidBadBroker = avoidBadBroker;
        }
    }

    public class StrategyResetOrderExecution : BaseMessage
    {
        public long StrategyID;
        public string TransactionID;

        public StrategyResetOrderExecution() : base(MessageNumbers.StrategyResetOrderExecution) { }
        public StrategyResetOrderExecution(long strategyID, string transactionID)
            : base(MessageNumbers.StrategyResetOrderExecution)
        {
            StrategyID = strategyID;
            TransactionID = transactionID;
        }
    }
    [Obsolete]
    public class StrategyNewScheduleApplied : BaseMessage
    {
        public long StrategyID;
        public string Schedule;

        public StrategyNewScheduleApplied() : base(MessageNumbers.StrategyNewScheduleApplied) { }
        public StrategyNewScheduleApplied(long strategyID, string jsonScheduleRecords)
            : base(MessageNumbers.StrategyNewScheduleApplied)
        {
            StrategyID = strategyID;
            Schedule = jsonScheduleRecords;
        }
    }
    public class StrategyNewScheduledState : BaseMessage
    {
        public long StrategyID;
        public bool IsScheduled;

        public StrategyNewScheduledState() : base(MessageNumbers.StrategyNewScheduledState) { }
        public StrategyNewScheduledState(long strategyID, bool isScheduled)
            : base(MessageNumbers.StrategyNewScheduledState)
        {
            StrategyID = strategyID;
            IsScheduled = isScheduled;
        }
    }
    [Obsolete]
    public class StrategyScheduleCommandFired : BaseMessage
    {
        public long StrategyID;
        public string ScheduleCommand;

        public StrategyScheduleCommandFired() : base(MessageNumbers.StrategyScheduleCommandFired) { }
        public StrategyScheduleCommandFired(long strategyID, string scheduleCommand)
            : base(MessageNumbers.StrategyScheduleCommandFired)
        {
            StrategyID = strategyID;
            ScheduleCommand = scheduleCommand;
        }
    }

    public class StrategySchedulerStateChanged : BaseMessage
    {
        public long StrategyID;
        public string JsonSerializedTimeMomentSpecifications;
        public StrategySchedulerStateChanged() : base(MessageNumbers.StrategySchedulerStateChanged) { }

        public StrategySchedulerStateChanged(long strategyID, string jsonSerializedTimeMomentSpecifications)
            : base(MessageNumbers.StrategySchedulerStateChanged)
        {
            StrategyID = strategyID;
            JsonSerializedTimeMomentSpecifications = jsonSerializedTimeMomentSpecifications;
        }

    }

    /// <summary>
    /// Message from execution service to the strategy to apply the incomming fill
    /// </summary>
    public class ExecutionService_FillsReport : BaseMessage
    {
        public long StrategyID;
        public FillsItem Fill;
        public string ClBasketID;
        public ExecutionService_FillsReport() : base(MessageNumbers.ExecutionService_FillsReport) { }
        public ExecutionService_FillsReport(long strategyID, FillsItem fill, string clBasketID)
            : base(MessageNumbers.ExecutionService_FillsReport)
        {
            StrategyID = strategyID;
            Fill = fill;
            ClBasketID = clBasketID;
        }
    }
    public class ExecutionService_OrderExecutionSuspended:BaseMessage
    {
        public long StrategyID;
        public bool RetryAtAnotherBrokerOnlyIsEnabled;
        public ExecutionService_OrderExecutionSuspended() : base(MessageNumbers.ExecutionService_OrderExecutionSuspended) { }
        public ExecutionService_OrderExecutionSuspended(long strategyID, bool retryAtAnotherBrokerOnlyIsEnabled)
            : base(MessageNumbers.ExecutionService_OrderExecutionSuspended)
        {
            StrategyID = strategyID;
            RetryAtAnotherBrokerOnlyIsEnabled = retryAtAnotherBrokerOnlyIsEnabled;
        }
    }
    public class StrategyInfoMsg : BaseMessage
    {
        public long StrategyID;
        public string Symbol;

        public string State; // StrategyState.ToString()
        public bool IsScheduled;
        public bool IsMarketFilterOn;
        public decimal OrderedAmount;
        public bool StrategyOrderSuspended;
        public bool IsRetryAtAnotherBrokerEnabled;

        public decimal TradingAmount;

        public double TargetLevel;
        public double InitialStopLevel;
        public double TrailingActivationLevel;
        public double TrailingStopLevel;

        public decimal OpenPosition;
        public decimal ClosedResultInQuoteCurrency;
        public bool DynamicStopGuardIsActive;
        public bool DynamicTargetGuardIsActive;
        public bool TrendMonitorIsActive;
        
        public StrategyInfoMsg() : base(MessageNumbers.StrategyInfoMsg) { }
        public StrategyInfoMsg(long strategyID, string symbol,
                             string state, bool isScheduled, decimal orderedAmount, bool strategyOrderSuspended,
                             bool isRetryAtAnotherBrokerEnabled,
                             decimal tradingAmount, double targetLevel, double initialStopLevel,
                             double trailingActivationLevel, double trailingStopLevel,
                             decimal openPosition, decimal closedResultInQuoteCurrency,
                             bool dynamicStopGuardIsActive, bool dynamicTargetGuardIsActive,
            bool trendMonitorIsActive
            )
            : base(MessageNumbers.StrategyInfoMsg)

        {
            StrategyID = strategyID;
            Symbol = symbol;
            State = state;
            IsScheduled = isScheduled;
            OrderedAmount = orderedAmount;
            StrategyOrderSuspended = strategyOrderSuspended;
            IsRetryAtAnotherBrokerEnabled = isRetryAtAnotherBrokerEnabled;
            TradingAmount = tradingAmount;
            TargetLevel = targetLevel;
            InitialStopLevel = initialStopLevel;
            TrailingActivationLevel = trailingActivationLevel;
            TrailingStopLevel = trailingStopLevel;
            OpenPosition = openPosition;
            ClosedResultInQuoteCurrency = closedResultInQuoteCurrency;
            DynamicStopGuardIsActive = dynamicStopGuardIsActive;
            DynamicTargetGuardIsActive = dynamicTargetGuardIsActive;
            TrendMonitorIsActive = false;
        }
    }

    // public class StrategyCfgInfoMsg:BaseMessage
    // {
    //     //private long ID;
    //     //public readonly StrategyDescription StrategyDescription;
    //     public StrategyDescription Description;
    //     public StrategyCfgInfoMsg() : base(MessageNumbers.StrategyCfgInfoMsg) { }
    //     //public StrategyCfgInfoMsg(StrategyDescription strategyDescription)
    //     public StrategyCfgInfoMsg(StrategyDescription sd)
    //         : base(MessageNumbers.StrategyCfgInfoMsg)
    //     {
    //         Description = sd;
    //         //StrategyDescription = strategyDescription;
    //         //ID = strategyDescription.StrategyID;
    //         //txt = strategyDescription.ToJson();
    //     }
    // }


    public class ApplyFillsToStrategy : BaseMessage
    {
        public long StrategyID;
        public string TransactionID;
        public string AccountName;

        public long OrderedAmountBeforeFills;
        public long OrderedAmountAfterFills;
        public long OpenedPositionBeforeFills;
        public long OpenedPositionAfterFills;
        public string ApplyError;

        public FillsItem Fill;

        public ApplyFillsToStrategy() : base(MessageNumbers.ApplyFillsToStrategy) { }
        public ApplyFillsToStrategy(
            long strategyID,
            string transactionID,
            string accountName,
            FillsItem fill,
            long orderedAmountBeforeFills,
            long orderedAmountAfterFills,
            long openedPositionBeforeFills,
            long openedPositionAfterFills,
            string applyError)
            : base(MessageNumbers.ApplyFillsToStrategy)
        {
            StrategyID = strategyID;
            TransactionID = transactionID;
            AccountName = accountName;

            Fill = fill;
            OrderedAmountBeforeFills = orderedAmountBeforeFills;
            OrderedAmountAfterFills = orderedAmountAfterFills;
            OpenedPositionBeforeFills = openedPositionBeforeFills;
            OpenedPositionAfterFills = openedPositionAfterFills;
            ApplyError = applyError;
        }
    }
    /// <summary>
    /// TradingServer ignored fill from broker via the outdated transact time (filled before start of the trading server)
    /// </summary>
    public class OutdatedFillsIgnoredMsg:BaseMessage
    {
        public FillsItem Fill;
        public OutdatedFillsIgnoredMsg() : base(MessageNumbers.OutdatedFillsIgnoredMsg) { }
        public OutdatedFillsIgnoredMsg(FillsItem fill)
            : base(MessageNumbers.OutdatedFillsIgnoredMsg)
        {
            Fill = fill;
        }
    }
    /// <summary>
    /// Received execution report not relates to the active strategy order(s); will be ignored
    /// </summary>
    public class WrongFillIgnoredMsg : BaseMessage
    {
        public long StrategyID;
        public FillsItem Fill;
        public string RejectionReason;
        public WrongFillIgnoredMsg() : base(MessageNumbers.WrongFillIgnoredMsg) { }
        public WrongFillIgnoredMsg(long strategyID, FillsItem fill,string rejectionReason)
            : base(MessageNumbers.WrongFillIgnoredMsg)
        {
            StrategyID = strategyID;
            //FillsReceiveTime = fillsReceiveTime;
            Fill = fill;
            RejectionReason = rejectionReason??"";
        }
    }


    // public class VirtualStrategyCfgInfoMsg : BaseMessage
    // {
    //     public StrategyDescription Description;
    //
    //     public VirtualStrategyCfgInfoMsg() : base(MessageNumbers.VirtualStrategyCfgInfoMsg) { }
    //     public VirtualStrategyCfgInfoMsg(StrategyDescription sd)
    //         : base(MessageNumbers.VirtualStrategyCfgInfoMsg)
    //     {
    //         Description = sd;
    //         //StrategyDescription = strategyDescription;
    //         //ID = strategyDescription.StrategyID;
    //         //txt = strategyDescription.ToJson();
    //     }
    // }
    public class VirtualStrategyInfoMsg : BaseMessage
    {
        public long StrategyID;
        public string Symbol;

        public string State; // StrategyState.ToString()
        public bool IsScheduled;
        public bool IsMarketFilterOn;
        public decimal OrderedAmount;

        public decimal TradingAmount;

        public double TargetLevel;
        public double InitialStopLevel;
        public double TrailingActivationLevel;
        public double TrailingStopLevel;

        public decimal OpenPosition;
        public decimal ClosedResultInQuoteCurrency;
        public bool DynamicStopGuardIsActive;
        public bool DynamicTargetGuardIsActive;
        public bool TrendMonitorIsActive;
        public VirtualStrategyInfoMsg() : base(MessageNumbers.VirtualStrategyInfoMsg) { }
        public VirtualStrategyInfoMsg(long strategyID, string symbol,
                             string state, bool isScheduled, decimal orderedAmount,
                             decimal tradingAmount, double targetLevel, double initialStopLevel,
                             double trailingActivationLevel, double trailingStopLevel,
                             decimal openPosition, decimal closedResultInQuoteCurrency,
                             bool dynamicStopGuardIsActive, bool dynamicTargetGuardIsActive,
            bool trendMonitorIsActive
            )
            : base(MessageNumbers.VirtualStrategyInfoMsg)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            State = state;
            IsScheduled = isScheduled;
            OrderedAmount = orderedAmount;
            TradingAmount = tradingAmount;
            TargetLevel = targetLevel;
            InitialStopLevel = initialStopLevel;
            TrailingActivationLevel = trailingActivationLevel;
            TrailingStopLevel = trailingStopLevel;
            OpenPosition = openPosition;
            ClosedResultInQuoteCurrency = closedResultInQuoteCurrency;
            DynamicStopGuardIsActive = dynamicStopGuardIsActive;
            DynamicTargetGuardIsActive = dynamicTargetGuardIsActive;
            TrendMonitorIsActive = trendMonitorIsActive;
        }
    }

    public class VirtualStrategyOrderExecution : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TransactionID;

        public long BrokerID;
        public long FilledAmount;
        public double FilledPrice;
        public long PositionBeforeFills;
        public long PositionAfterFills;
        public double Bid;
        public double Ask;
        public double Price { get { return FilledAmount > 0 ? Ask : Bid; } }
        public DateTime TransactTime;
        public VirtualStrategyOrderExecution() : base(MessageNumbers.VirtualStrategyOrderExecution) { }
        public VirtualStrategyOrderExecution(long strategyID, string symbol, string transactionID,
                                             long brokerID, long filledAmount, double filledPrice,DateTime transactTime,
                                             long positionBeforeFills, long positionAfterFills,
                                             double bid, double ask)
            : base(MessageNumbers.VirtualStrategyOrderExecution)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            TransactionID = transactionID;
            BrokerID = brokerID;
            FilledAmount = filledAmount;
            FilledPrice = filledPrice;
            PositionBeforeFills = positionBeforeFills;
            PositionAfterFills = positionAfterFills;
            TransactTime = transactTime;
            Bid = bid;
            Ask = ask;
        }
    }

    public class VirtualStrategyNewOrderedAmount : BaseMessage
    {
        public long StrategyID;
        public string Symbol;
        public string TransactionID;

        public long OrderedAmount;
        public string StrategyState;
        public long CurOpenPosition;
        public string Reason;
        public long CurrentProviderID;
        public double CurrentProviderBid;
        public double CurrentProviderAsk;
        public int? TargetPosition;   // new field

        public VirtualStrategyNewOrderedAmount() : base(MessageNumbers.VirtualStrategyNewOrderedAmount) { }
        public VirtualStrategyNewOrderedAmount(long strategyID, string symbol, string transactionID,
            long orderedAmount, int targetPosition, string strategyState, long curOpenPosition,
            string reason,
            long currentProviderID, double currentProviderBid, double currentProviderAsk)
            : base(MessageNumbers.VirtualStrategyNewOrderedAmount)
        {
            StrategyID = strategyID;
            Symbol = symbol;
            TransactionID = transactionID;

            OrderedAmount = orderedAmount;
            TargetPosition = targetPosition;
            StrategyState = strategyState;
            CurOpenPosition = curOpenPosition;
            Reason = reason;

            CurrentProviderID = currentProviderID;
            CurrentProviderBid = currentProviderBid;
            CurrentProviderAsk = currentProviderAsk;
        }
    }

   
    public class MarketFilterStateMsg : BaseMessage
    {
        public long StrategyID;
        public bool IsVirtual;
        public int MF;
        public int DF;

        public MarketFilterStateMsg() : base(MessageNumbers.MarketFilterStateMsg) { }
        public MarketFilterStateMsg(long strategyId, bool isVirtual,int mf,int df)
            : base(MessageNumbers.MarketFilterStateMsg)
        {
            StrategyID = strategyId;
            IsVirtual=isVirtual;
            MF = mf;
            DF = df;
        }
    }

}
