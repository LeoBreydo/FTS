using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CommonStructures
{
    public enum DynamicalRestrictionState
    {
        Undefined = -1,
        Stopped = 0,
        Active = 1,
    }

    public enum SchedulerRestrictionState
    {
        NoRestrictions = 0,
        PreStopping =1,
        Stopped = 2,
        PrePausing = 3,
        Paused = 4
    }

    /// <summary>
    /// The strategy info to transmit to the web clients
    /// </summary> 
    [Serializable]
    [DataContract]
    public class StrategyInfo
    {
        [DataMember]
        public long StrategyID { get; set; }
        [DataMember]
        public string StrategyName { get; set; }

        [DataMember]
        public string AccountName { get; set; }

        [DataMember]
        public string PortfolioName { get; set; }

        [DataMember]
        public string CurrencyPair { get; set; }

        [DataMember]
        public decimal TradingAmount { get; set; }
        [DataMember]
        public double TargetLevel { get; set; }
        [DataMember]
        public double InitialStopLevel { get; set; }
        [DataMember]
        public double TrailingActivationLevel { get; set; }
        [DataMember]
        public double TrailingStopLevel { get; set; }

        [DataMember]
        public int DynamicalTargetState { get; set; }// the int presentation of the DynamicalRestrictionState
        [DataMember]
        public int DynamicalStopState { get; set; }// the int presentation of the DynamicalRestrictionState

        [DataMember]
        public bool IsScheduled { get; set; }
        [DataMember]
        public bool IsMarketFilerDefined { get; set; }
        [DataMember]
        public bool MarketFilter { get; set; }
        [DataMember]
        public bool IsTrendMonitorDefined { get; set; }
        [DataMember]
        public bool TrendMonitor { get; set; }
        [DataMember]
        public bool StoppedByLoss { get; set; }
        [DataMember]
        public bool StoppedBySymbolLoss { get; set; }
        [DataMember]
        public bool DisabledByUser { get; set; }

        [DataMember]
        public int SchedulerRestriction { get; set; } //the int presentation of the SchedulerRestrictionState

        [DataMember]
        public string State { get; set; } // //CommonInterfaces.StrategyState.ToString()

        [DataMember]
        public decimal BaseCurrencyExposure { get; set; }
        [DataMember]
        public decimal QuoteCurrencyExposure { get; set; }

        [DataMember]
        public decimal CurrentOpenResultInQuoteCurrency { get; set; }
        [DataMember]
        public decimal ClosedResultInQuoteCurrency { get; set; }

        [DataMember]
        public decimal OpenedPositionInMainCurrency { get; set; }
        [DataMember]
        public decimal CurrentOpenResultInMainCurrency { get; set; }
        [DataMember]
        public decimal ClosedResultInMainCurrency { get; set; }

        // доступность дополнительных команд для пользователя
        [DataMember]
        public bool HasProblem { get; set; }
        // ордер приостановлен (завис), доступна команда ПОВТОРИТЬ ОРДЕР (помимо Reset и Reset+HardStop) 
    }
    /// <summary>
    /// The market info to transmit to the web clients
    /// </summary>
    [Serializable]
    [DataContract]
    public class MarketInfo
    {
        public MarketInfo()
        {
            StrategyInfos = new List<StrategyInfo>();
        }
        [DataMember]
        public string CurrencyPair { get; set; }
        [DataMember]
        public List<StrategyInfo> StrategyInfos { get; set; }
    }

    /// <summary>
    /// The provider info to transmit to the web clients
    /// </summary>
    [Serializable]
    [DataContract]
    public class ProviderInfo
    {
        [DataMember]
        public long ProviderID { get; set; }
        [DataMember]
        public string ProviderName { get; set; }
        [DataMember]
        public bool IsDisabled { get; set; }
    }
    /// <summary>
    /// The message to transmit to the web clients
    /// </summary>
    [Serializable]
    [DataContract]
    public class ClientMessage
    {
        [DataMember]
        public long Number { get; set; }
        [DataMember]
        public bool IsImportant { get; set; }
        [DataMember]
        public string Text { get; set; }

        public ClientMessage() { }
        // конструктор для наполнителей
        public ClientMessage(bool isImportant, string text, DateTime utcNow, long number)
        {
            IsImportant = isImportant;
            Text = utcNow.ToString("HH:mm:ss ") + text;
            Number = number;
        }
    }


    /// <summary>
    /// Information about order having some execution problems
    /// </summary>
    [Serializable]
    [DataContract]
    public class OrderProblemDescription
    {
        [DataMember]
        public long StrategyID { get; set; }

        [DataMember]
        public string AccountName { get; set; }

        [DataMember]
        public string CurrencyPair { get; set; }
        [DataMember]
        public long OrderedAmount { get; set; }

        [DataMember]
        public string ClOrderID { get; set; }

        [DataMember]
        public string BrokerID { get; set; }
       
        [DataMember]
        public string GenerationTime { get; set; }

        [DataMember]
        public long WaitingTime { get; set; }

        [DataMember]
        public string Status { get; set; }

        public OrderProblemDescription(long strategyID, string currencyPair, string accountName, long orderedAmount, string clOrderID, string brokerID, string generationTime, long waitingTime, string status)
        {
            StrategyID = strategyID;
            CurrencyPair = currencyPair;
            AccountName = accountName;
            OrderedAmount = orderedAmount;
            ClOrderID = clOrderID;
            BrokerID = brokerID;
            GenerationTime = generationTime;
            WaitingTime = waitingTime;
            Status = status;
        }

        private readonly DateTime _utcStartOfWaiting;
        public OrderProblemDescription(long strategyID, string currencyPair, string accountName, long orderedAmount, string clOrderID, string brokerID, string generationTime, DateTime utcStartOfWaiting, string status)
        {
            StrategyID = strategyID;
            CurrencyPair = currencyPair;
            AccountName = accountName;
            OrderedAmount = orderedAmount;
            ClOrderID = clOrderID;
            BrokerID = brokerID;
            GenerationTime = generationTime;
            _utcStartOfWaiting = utcStartOfWaiting;
            Status = status;
        }

        public void UpdateWaitingTime()
        {
            WaitingTime = (long) (DateTime.UtcNow - _utcStartOfWaiting).TotalSeconds;
        }
    }


    [Serializable]
    [DataContract]
    public class AccountInfo
    {
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public List<MarketInfo> MarketInfos { get; set; }
        [DataMember]
        public ClientMessage[] Messages { get; set; }
        [DataMember]
        public List<OrderProblemDescription> OrderProblemDescriptions { get; set; }
    }

    /// <summary>
    /// The state of the trading server configuration with newly collected messages to transmit to the web clients
    /// </summary>
    [Serializable]
    [DataContract]
    public class ConfigurationInfo
    {
        public ConfigurationInfo()
        {
            AccountInfos=new List<AccountInfo>();
            ProviderInfos = new List<ProviderInfo>();
            AllMessages=new ClientMessage[0];
            CurrencyPairMessages = new List<Tuple<string, ClientMessage[]>>();
        }

        [DataMember]
        public string MainCurrency { get; set; }
        [DataMember]
        public List<ProviderInfo> ProviderInfos { get; set; }

        [DataMember]
        public List<AccountInfo> AccountInfos { get; set; }

        [DataMember]
        public MarketStateFiltersInformation MarketStateFiltersInformation { get; set; }

        [DataMember]
        public List<Tuple<string, ClientMessage[]>> CurrencyPairMessages { get; set; }

        [DataMember]
        public ClientMessage[] AllMessages;
    }
}