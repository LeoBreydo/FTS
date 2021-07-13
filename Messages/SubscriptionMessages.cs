namespace Messages
{
    /// <summary>
    /// Notifies that the subscription request was posted to the provider for specified symbol
    /// </summary>
    public class DataFeed_SubscriptionPosted : BaseMessage
    {
        public long BrokerID;
        public string Symbol;
        public long Volume;
        public DataFeed_SubscriptionPosted(): base(MessageNumbers.DataFeed_SubscriptionPosted) { }

        public DataFeed_SubscriptionPosted(long brokerID, string symbol, long volume)
            : base(MessageNumbers.DataFeed_SubscriptionPosted)
        {
            BrokerID = brokerID;
            Symbol = symbol;
            Volume = volume;
        }
    }
    /// <summary>
    /// Notifies that provider rejected subscripition request
    /// </summary>
    public class DataFeed_SubscriptionRejection : BaseMessage
    {
        public long BrokerID;
        public string Symbol;
        public long Volume;

        public string Error;
        public DataFeed_SubscriptionRejection() : base(MessageNumbers.DataFeed_SubscriptionRejection) { }

        public DataFeed_SubscriptionRejection(long brokerID, string symbol,long volume, string error)
            : base(MessageNumbers.DataFeed_SubscriptionRejection)
        {
            BrokerID = brokerID;
            Symbol = symbol;
            Volume = volume;
            Error = error;
        }
    }
    /// <summary>
    /// A centralized command for all quote stream users to request subscription for required symbols from specified provider 
    /// via connection to provider is established
    /// </summary>
    public class Cmd_Subscribe : BaseMessage
    {
        public long BrokerID;
        public Cmd_Subscribe() : base(MessageNumbers.Cmd_Subscribe) { }
        public Cmd_Subscribe(long brokerID)
            : base(MessageNumbers.Cmd_Subscribe)
        {
            BrokerID = brokerID;
        }
    }
    /// <summary>
    /// A centralized notification for all quote stream users that connection to provider is lost, the all subscriptions are cancelled
    /// </summary>
    /// <remarks>
    /// Ввиду особенностей фикс (отсутствие возможности отписаться) это событие нужно интепретировать как сообщение "Подписка прервана"
    /// </remarks>
    public class Cmd_Unsubscribe : BaseMessage
    {
        public long BrokerID;
        public Cmd_Unsubscribe() : base(MessageNumbers.Cmd_Unsubscribe) { }
        public Cmd_Unsubscribe(long brokerID)
            : base(MessageNumbers.Cmd_Unsubscribe)
        {
            BrokerID = brokerID;
        }
    }

}
