namespace Messages
{
    public static class MessageNumbersEx
    {
        public const int Common_Base = 1;

        public const int Connection_Base = 50;
        public const int Connection_End = 60;
        public static bool IsConnectionMessage(this int messageNumber)
        {
            return messageNumber >= Connection_Base && messageNumber < Connection_End;
        }

        public const int OrderReports_Base = 100;
        public const int OrderReports_End = 180; // the value OrderReports_End used for ManualOrderFills which is not OrderReport
        public static bool IsOrderReport(this int messageNumber)
        {
            return messageNumber >= OrderReports_Base && messageNumber < OrderReports_End;
        }

        public const int DataFeed_Base = 210;
        public const int LogicalInstrument_Base = 220;
        public const int ReconnectionBase = 250;
        public const int OrdStorage_Base = 300;


        public const int ClientCommandBase = 500;
        public const int ClientCommandEnd = 550;

        public const int StrategyNotificationsBase = 550;
        public const int StrategyNotificationsBase2 = 800;

        public const int ExecutionServiceBase = 590;

        public const int MarketFiltersBase = 700;
        public const int MiscBase = 900;

    }
    public enum MessageNumbers
    {
        TRADING___SERVICE___ACTIVATED = 0,
        TRADING___SERVICE___DEACTIVATED = 9,

        //public const int Common_Base = 1;
        TextMessage = MessageNumbersEx.Common_Base,
        FixMessage = MessageNumbersEx.Common_Base + 1,
        Cmd_ActivateConnections = MessageNumbersEx.Common_Base + 2,
        Cmd_DeactivateConnections = MessageNumbersEx.Common_Base + 3,
        ClientMsg = MessageNumbersEx.Common_Base + 4,

        //public const int Connection_Base = 50;
        //public const int Connection_End = 60;
        BrokerConnectionStatus = MessageNumbersEx.Connection_Base,
        SessionReadiness = MessageNumbersEx.Connection_Base + 1,

        PenaltyAddedNotification = 60,
        PenaltiesResetNotification = 61,

        ObsoletteMessage = 98,
        CantRestoreMessage = 99,
        //public const int OrderReports_Base = 100;
        //public const int OrderReports_End = 180; // the value OrderReports_End used for ManualOrderFills which is not OrderReport

        OrderPosting = MessageNumbersEx.OrderReports_Base,
        OrderPostRejection = MessageNumbersEx.OrderReports_Base + 1,

        PendingNewReport = MessageNumbersEx.OrderReports_Base + 2,
        AcknowledgementReport = MessageNumbersEx.OrderReports_Base + 3,
        RejectionReport = MessageNumbersEx.OrderReports_Base + 4,
        OrderFillReport = MessageNumbersEx.OrderReports_Base + 5,
        OrderCancelPosted = MessageNumbersEx.OrderReports_Base + 6,
        OrderCancelPostRejection = MessageNumbersEx.OrderReports_Base + 7,
        PendingCancelReport = MessageNumbersEx.OrderReports_Base + 8,
        OrderCancelledReport = MessageNumbersEx.OrderReports_Base + 9,
        OrderCancelRejectionReport = MessageNumbersEx.OrderReports_Base + 10,
        OrderReplacePosted = MessageNumbersEx.OrderReports_Base + 11,
        OrderReplacePostRejection = MessageNumbersEx.OrderReports_Base + 12,
        PendingReplaceReport = MessageNumbersEx.OrderReports_Base + 13,
        OrderReplaceRejectionReport = MessageNumbersEx.OrderReports_Base + 14,
        OrderStoppedReport = MessageNumbersEx.OrderReports_Base + 15,
        OrderExpiredReport = MessageNumbersEx.OrderReports_Base + 16,
        OrderPosted = MessageNumbersEx.OrderReports_Base + 17,
        OrderReplacedReport = MessageNumbersEx.OrderReports_Base + 18,
        OrderStatusReport = MessageNumbersEx.OrderReports_Base + 19,

        ManualOrderFills = MessageNumbersEx.OrderReports_End,
        CancelAllOrdersPosting = MessageNumbersEx.OrderReports_End + 1,
        CancelAllOrdersPosted = MessageNumbersEx.OrderReports_End + 2,

        Cmd_Subscribe = 200,
        Cmd_Unsubscribe = 201,


        //public const int DataFeed_Base = 210;
        DataFeed_SubscriptionPosted = MessageNumbersEx.DataFeed_Base,
        DataFeed_SubscriptionRejection = MessageNumbersEx.DataFeed_Base + 1,

        // public const int LogicalInstrument_Base = 220;
        LogicalInstrumentCreated = MessageNumbersEx.LogicalInstrument_Base,
        LogicalInstrumentProviderEvent = MessageNumbersEx.LogicalInstrument_Base + 1,
        LogicalInstrumentGapEvent = MessageNumbersEx.LogicalInstrument_Base + 2,
        StrategyBindedToLogicalInstrument = MessageNumbersEx.LogicalInstrument_Base + 3,

        //public const int ReconnectionBase = 250;
        ReconnectionManager_Activated = MessageNumbersEx.ReconnectionBase,
        ReconnectionManager_Deactivated = MessageNumbersEx.ReconnectionBase + 1,
        ReconnectionManager_CallQuoteSessionConnect = MessageNumbersEx.ReconnectionBase + 6,
        ReconnectionManager_CallTradeSessionConnect = MessageNumbersEx.ReconnectionBase + 7,
        ReconnectionManager_CallQuoteSessionDisconnect = MessageNumbersEx.ReconnectionBase + 8,
        ReconnectionManager_CallTradeSessionDisconnect = MessageNumbersEx.ReconnectionBase + 9,
        ReconnectionManager_ScheduleEvent = MessageNumbersEx.ReconnectionBase + 10,

        // this messages group is used in the conformance test only
        //public const int OrdStorage_Base = 300;
        OrdStorage_OrderAddedMsg = MessageNumbersEx.OrdStorage_Base,
        OrdStorage_OrderUpdatedMsg = MessageNumbersEx.OrdStorage_Base + 1,
        OrdStorage_OrderFinishedMsg = MessageNumbersEx.OrdStorage_Base + 2,
        OrdStorage_OrderFillsMsg = MessageNumbersEx.OrdStorage_Base + 3,
        OrdStorage_OrderTimeoutMsg = MessageNumbersEx.OrdStorage_Base + 4,


        //public const int ClientCommandBase = 500;
        //public const int ClientCommandEnd = 550;
        Cmd_StopTrading = MessageNumbersEx.ClientCommandBase,
        Cmd_StrategyCommand = MessageNumbersEx.ClientCommandBase + 2,
        Cmd_SetProviderState = MessageNumbersEx.ClientCommandBase + 3,
#if OLD
        Cmd_MassCommand = MessageNumbersEx.ClientCommandBase + 1,
#endif
        Cmd_ClientCommand_Obsolette = MessageNumbersEx.ClientCommandBase + 4,

        Cmd_ApplyNewSchedule = MessageNumbersEx.ClientCommandBase + 5,
        Cmd_ApplyStrategyParameters = MessageNumbersEx.ClientCommandBase + 10,
        Cmd_AtSystemLevel = MessageNumbersEx.ClientCommandBase + 11,
        Cmd_AtAccountLevel = MessageNumbersEx.ClientCommandBase + 12,
        Cmd_AtAccountCurrencyPairLevel = MessageNumbersEx.ClientCommandBase + 13,
        Cmd_AtCurrencyPairLevel = MessageNumbersEx.ClientCommandBase + 14,
        Cmd_StartStopRepublishService = MessageNumbersEx.ClientCommandBase + 15,
        Cmd_SetTradingRestrictions = MessageNumbersEx.ClientCommandBase + 16,

        Cmd_UpdateGroup = MessageNumbersEx.ClientCommandBase + 17,
        Cmd_UpdateManualResettingStates = MessageNumbersEx.ClientCommandBase + 18,

        Cmd_ApplyFill = MessageNumbersEx.ClientCommandBase + 20,

        Cmd_AtAccountPortfolioLevel = MessageNumbersEx.ClientCommandBase + 21,
        Cmd_SetTimeGrid = MessageNumbersEx.ClientCommandBase + 22,


        //public const int StrategyNotificationsBase = 550;
        StrategyErrorMsg = MessageNumbersEx.StrategyNotificationsBase,
        StrategyWarningMsg = MessageNumbersEx.StrategyNotificationsBase + 1,

        StrategyNewInputsCalculated = MessageNumbersEx.StrategyNotificationsBase + 2, // OBSOLETTE message
        InputIndicatorValues = MessageNumbersEx.StrategyNotificationsBase2,
        SignalGeneratorValue = MessageNumbersEx.StrategyNotificationsBase2 + 1,
        DelayedTargetPosition = MessageNumbersEx.StrategyNotificationsBase2 + 2,


        StrategySendOrderToBroker = MessageNumbersEx.StrategyNotificationsBase + 4,
        FillAcceptedByExecutor = MessageNumbersEx.StrategyNotificationsBase + 5,
        StrategySentOrderWasNotFilled = MessageNumbersEx.StrategyNotificationsBase + 6,
        StrategyOrderExecutionSuspendedViaAttemptsLeft = MessageNumbersEx.StrategyNotificationsBase + 7,
        StrategyOrderExecutionSuspendedViaResponseTimeout = MessageNumbersEx.StrategyNotificationsBase + 8,
        StrategyRestartSuspendedOrder = MessageNumbersEx.StrategyNotificationsBase + 9,
        StrategyResetOrderExecution = MessageNumbersEx.StrategyNotificationsBase + 10,

        NewTrendMonitorValue = MessageNumbersEx.StrategyNotificationsBase + 12,
        NewTheoreticalPosition = MessageNumbersEx.StrategyNotificationsBase + 13,
        SignalTransformerZoneChanged = MessageNumbersEx.StrategyNotificationsBase + 15,

        // transaction manager events
        NewStrategyTargetPosition = MessageNumbersEx.StrategyNotificationsBase + 11, // (called from GoLong,GoSHort,GoFlat) this is the point where the new transaction starts
        NewTransactionManagerDecision = MessageNumbersEx.StrategyNotificationsBase + 14,  // OBSOLETTE message!!!
        StrategyNewOrderedAmount = MessageNumbersEx.StrategyNotificationsBase + 3, // see VirtualStrategyNewOrderedAmount for virtual strategies
        OrderExecutionPostponed = MessageNumbersEx.StrategyNotificationsBase+16,
        OrderExecutionStarted = MessageNumbersEx.StrategyNotificationsBase + 17,
        ApplyFillsToStrategy = MessageNumbersEx.StrategyNotificationsBase + 26,
        TransactionDone = MessageNumbersEx.StrategyNotificationsBase + 18,
        TransactionCancelled = MessageNumbersEx.StrategyNotificationsBase + 19,

        StrategyNewScheduleApplied = MessageNumbersEx.StrategyNotificationsBase + 20, // obsolette message, keep alive for historical logs only
        StrategyNewScheduledState = MessageNumbersEx.StrategyNotificationsBase + 21,
        StrategyScheduleCommandFired = MessageNumbersEx.StrategyNotificationsBase + 22,
        StrategySchedulerStateChanged = MessageNumbersEx.StrategyNotificationsBase + 23,

        StrategyInfoMsg = MessageNumbersEx.StrategyNotificationsBase + 25,
        OutdatedFillsIgnoredMsg = MessageNumbersEx.StrategyNotificationsBase + 27,
        WrongFillIgnoredMsg = MessageNumbersEx.StrategyNotificationsBase + 28,

        StrategyCfgInfoMsg = MessageNumbersEx.StrategyNotificationsBase + 30,
        MarketFilterStateMsg = MessageNumbersEx.StrategyNotificationsBase + 31,

        VirtualStrategyCfgInfoMsg = MessageNumbersEx.StrategyNotificationsBase + 35,
        VirtualStrategyInfoMsg = MessageNumbersEx.StrategyNotificationsBase + 36,
        VirtualStrategyNewOrderedAmount = MessageNumbersEx.StrategyNotificationsBase + 38,
        VirtualStrategyOrderExecution = MessageNumbersEx.StrategyNotificationsBase + 37,
        //NewVirtualStrategyTargetPosition = MessageNumbersEx.StrategyNotificationsBase + 39,

        // must be <MessageNumbersEx.StrategyNotificationsBase +40 !!!;  MessageNumbersEx.StrategyNotificationsBase + 40 is the ExecutionService_FillsReport

        //public const int ExecutionServiceBase = 590;
        ExecutionService_FillsReport = MessageNumbersEx.ExecutionServiceBase,
        ExecutionService_OrderExecutionSuspended = MessageNumbersEx.ExecutionServiceBase + 1,


        MarketFilterStates = MessageNumbersEx.MarketFiltersBase,
        FilterCurrentResultChanged = MessageNumbersEx.MarketFiltersBase + 1,
        FilterLockedResultChanged = MessageNumbersEx.MarketFiltersBase + 2,
        FilterGroupStateChanged = MessageNumbersEx.MarketFiltersBase + 3,
        TransactionCancelledByMarketFilter = MessageNumbersEx.MarketFiltersBase + 4,

        ConversionRatesToAccountCurrency = MessageNumbersEx.MiscBase,

        DefaultExposureRestrictions = MessageNumbersEx.MiscBase + 1,
        ExposureTriggersList = MessageNumbersEx.MiscBase + 2,
        ExposureTriggerStates = MessageNumbersEx.MiscBase + 3,
        TriggeredRestrictionsSummary = MessageNumbersEx.MiscBase + 4,
        SignalDelayConfigurationWithHandlerIDs = MessageNumbersEx.MiscBase + 5,
        ConditionHandlerStates = MessageNumbersEx.MiscBase + 6,
        DelayExecStateChanged = MessageNumbersEx.MiscBase + 7,
    }
}
