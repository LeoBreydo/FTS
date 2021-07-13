using System;
using System.Collections.Generic;
using System.Linq;
using CommonStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messages
{
    /// <summary>
    /// Command called before tradingsever shutdown to forcible close all 
    /// </summary>
    public class Cmd_StopTrading : BaseMessage
    {
        public Cmd_StopTrading() : base(MessageNumbers.Cmd_StopTrading) { }
    }

    public class Cmd_AtSystemLevel : BaseMessage
    {
        public string UserName;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExternalCommands Command;
        public Cmd_AtSystemLevel() : base(MessageNumbers.Cmd_AtSystemLevel) { }
        public Cmd_AtSystemLevel(string userName, ExternalCommands command)
            : base(MessageNumbers.Cmd_AtSystemLevel)
        {
            UserName = userName;
            Command = command;
        }
        //public override string ToString()
        //{
        //    return string.Format("Cmd_AtSystemLevel, UserName={0}, Command={1}", UserName,Command );
        //}
    }
    public class Cmd_AtAccountLevel : BaseMessage
    {
        public string UserName;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExternalCommands Command;
        public string AccountName;
        public Cmd_AtAccountLevel() : base(MessageNumbers.Cmd_AtAccountLevel) { }
        public Cmd_AtAccountLevel(string userName, ExternalCommands command, string accountName)
            : base(MessageNumbers.Cmd_AtAccountLevel)
        {
            UserName = userName;
            Command = command;
            AccountName = string.IsNullOrEmpty(accountName) ? null : accountName;
        }
        //public override string ToString()
        //{
        //    return string.Format("Cmd_AtAccountLevel, UserName={0}, Command={1}, AccountName='{2}'", UserName,
        //                         Command, AccountName);
        //}
    }
    public class Cmd_AtAccountCurrencyPairLevel : BaseMessage
    {
        public string UserName;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExternalCommands Command;
        public string AccountName;
        public string CurrencyPair;
        public Cmd_AtAccountCurrencyPairLevel() : base(MessageNumbers.Cmd_AtAccountCurrencyPairLevel) { }
        public Cmd_AtAccountCurrencyPairLevel(string userName, ExternalCommands command, string accountName, string currencyPair)
            : base(MessageNumbers.Cmd_AtAccountCurrencyPairLevel)
        {
            UserName = userName;
            Command = command;
            AccountName = string.IsNullOrEmpty(accountName) ? null : accountName;
            CurrencyPair = string.IsNullOrEmpty(currencyPair) ? null : currencyPair;
        }
        //public override string ToString()
        //{
        //    return string.Format("Cmd_AtAccountCurrencyPairLevel, UserName={0}, Command={1}, AccountName='{2}', CurrencyPair='{3}'", UserName,
        //                         Command, AccountName, CurrencyPair);
        //}
    }
    public class Cmd_AtCurrencyPairLevel : BaseMessage
    {
        public string UserName;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExternalCommands Command;
        public string CurrencyPair;
        public Cmd_AtCurrencyPairLevel() : base(MessageNumbers.Cmd_AtCurrencyPairLevel) { }
        public Cmd_AtCurrencyPairLevel(string userName, ExternalCommands command, string currencyPair)
            : base(MessageNumbers.Cmd_AtCurrencyPairLevel)
        {
            UserName = userName;
            Command = command;
            CurrencyPair = string.IsNullOrEmpty(currencyPair) ? null : currencyPair;
        }
        //public override string ToString()
        //{
        //    return string.Format("Cmd_AtCurrencyPairLevel, UserName={0}, Command={1}, CurrencyPair='{2}'", UserName,
        //                         Command, CurrencyPair);
        //}
    }

    public class Cmd_AtAccountPortfolioLevel : BaseMessage
    {
        public string UserName;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExternalCommands Command;
        public string AccountName;
        public string PortfolioName;

        public Cmd_AtAccountPortfolioLevel() : base(MessageNumbers.Cmd_AtAccountPortfolioLevel) { }

        public Cmd_AtAccountPortfolioLevel(string userName, ExternalCommands command, string accountName, string portfolioName)
            : base(MessageNumbers.Cmd_AtAccountPortfolioLevel)
        {
            UserName = userName;
            Command = command;
            AccountName = string.IsNullOrEmpty(accountName) ? null : accountName;
            PortfolioName = string.IsNullOrEmpty(portfolioName) ? null : portfolioName;
        }
    }


    public class Cmd_StrategyCommand:BaseMessage
    {
        public string UserName;
        [JsonConverter(typeof(StringEnumConverter))]
        public ExternalCommands Command;
        public long  StrategyId;
        public Cmd_StrategyCommand() : base(MessageNumbers.Cmd_StrategyCommand) { }
        public Cmd_StrategyCommand(string userName, ExternalCommands command, long strategyId)
            : base(MessageNumbers.Cmd_StrategyCommand)
        {
            UserName = userName;
            Command = command;
            StrategyId = strategyId;
        }
        //public override string ToString()
        //{
        //    return string.Format("Cmd_StrategyCommand, UserName={0}, Command={1}, StrategyId={2}", UserName, Command, StrategyId);
        //}
    }
    public class Cmd_SetProviderState:BaseMessage
    {
        public string UserName;
        public long ProviderId;
        public bool SetEnabled;
        public Cmd_SetProviderState() : base(MessageNumbers.Cmd_SetProviderState) { }
        public Cmd_SetProviderState(string userName, long providerId, bool setEnabled)
            : base(MessageNumbers.Cmd_SetProviderState)
        {
            UserName = userName;
            ProviderId = providerId;
            SetEnabled = setEnabled;
        }
        //public override string ToString()
        //{
        //    return string.Format("Cmd_SetProviderState, UserName={0}, ProviderId={1}, SetEnabled={2}", UserName, ProviderId, SetEnabled);
        //}
    }
    /// <summary>
    /// The command from the web client to apply fill to the strategy having order with lost fill
    /// </summary>
    public class Cmd_ApplyFill : BaseMessage 
    {
        public string UserName;
        public long StrategyID;
        public long ExecAmount;
        public double ExecQuote;
        //public DateTime ExecTime;
        public TimeStamp ExecTime;
        public string ClOrderId;
        public long RestOrderedAmountBeforeApplyFill;
        public Cmd_ApplyFill() : base(MessageNumbers.Cmd_ApplyFill) { }
        public Cmd_ApplyFill(string userName, long strategyID, long execAmount, double execQuote, TimeStamp execTime, string clOrderId, long restOrderedAmountBeforeApplyFill)
            : base(MessageNumbers.Cmd_ApplyFill)
        {
            UserName = userName;
            StrategyID = strategyID;
            ExecAmount = execAmount;
            ExecQuote = execQuote;
            ExecTime = execTime;
            ClOrderId = clOrderId;
            RestOrderedAmountBeforeApplyFill = restOrderedAmountBeforeApplyFill;
        }

        //public override string ToString()
        //{
        //    return string.Format("Cmd_ApplyFill, UserName={0}, StrategyID={1}, ExecAmount={2}, ExecQuote={3}, ExecTime={4}, ClOrderId={5}, RestOrderedAmountBeforeApplyFill={6}",
        //        UserName, StrategyID, ExecAmount, ExecQuote, ExecTime, ClOrderId, RestOrderedAmountBeforeApplyFill);
        //}
    }

    /// <summary>
    /// The command from client to change TradingAmount and/or stop/target protections for specified strategy
    /// </summary>
    public class Cmd_ApplyStrategyParameters : BaseMessage
    {
        public string UserName;
        public long StrategyID;
        public long TradingAmount;
        public double TakeProfitDelta;
        public double InitialStopDelta;
        public double TrailingDelta;
        public double ActivationProfit;
        [JsonConverter(typeof(StringEnumConverter))]
        public DynamicalRestrictionState DynamicalTargetState;
        [JsonConverter(typeof(StringEnumConverter))]
        public DynamicalRestrictionState DynamicalStopState;

        public bool DisableStrategyByUser;

        public Cmd_ApplyStrategyParameters() : base(MessageNumbers.Cmd_ApplyStrategyParameters) { }
        public Cmd_ApplyStrategyParameters(string userName, long strategyID, long tradingAmount, double takeProfitDelta, double initialStopDelta,
                            double trailingDelta, double activationProfit,
            DynamicalRestrictionState dynamicalTargetState, DynamicalRestrictionState dynamicalStopState, bool disableStrategyByUser)
            : base(MessageNumbers.Cmd_ApplyStrategyParameters)
        {
            UserName = userName;
            StrategyID = strategyID;
            TradingAmount = tradingAmount;
            TakeProfitDelta = takeProfitDelta;
            InitialStopDelta = initialStopDelta;
            TrailingDelta = trailingDelta;
            ActivationProfit = activationProfit;
            DynamicalTargetState = dynamicalTargetState;
            DynamicalStopState = dynamicalStopState;
            DisableStrategyByUser = disableStrategyByUser;
        }
        //public override string ToString()
        //{
        //    return
        //        string.Format(
        //            "Cmd_ApplyStrategyParameters, UserName={0}, StrategyID={1}, TradingAmount={2}, TakeProfitDelta={3}, InitialStopDelta={4}, TrailingDelta={5}, ActivationProfit={6}, DynamicalTargetState={7}, DynamicalStopState={8}",
        //            UserName, StrategyID, TradingAmount, TakeProfitDelta, InitialStopDelta, TrailingDelta, ActivationProfit, DynamicalTargetState, DynamicalStopState);
        //}
    }


   
    /// <summary>
    /// The command from client to load and apply the updated schedule file
    /// </summary>
    public class Cmd_ApplyNewSchedule:BaseMessage
    {
        public string UserName;

        public Cmd_ApplyNewSchedule() : base(MessageNumbers.Cmd_ApplyNewSchedule) { }
        public Cmd_ApplyNewSchedule(string userName)
            : base(MessageNumbers.Cmd_ApplyNewSchedule)
        {
            UserName = userName;
        }
    }

    public class Cmd_StartStopRepublishService : BaseMessage
    {
        public string UserName;
        public bool Start;
        public Cmd_StartStopRepublishService() : base(MessageNumbers.Cmd_StartStopRepublishService) { }
        public Cmd_StartStopRepublishService(string userName,bool start)
            : base(MessageNumbers.Cmd_StartStopRepublishService)
        {
            UserName = userName;
            Start = start;
        }
    }

    /// <summary>
    /// The command from client to apply trading restrictions on the fly
    /// </summary>
    public class Cmd_SetTradingRestrictions : BaseMessage
    {
        public string UserName;
        public string TradingRestrictions;
        public Cmd_SetTradingRestrictions() : base(MessageNumbers.Cmd_SetTradingRestrictions) { }
        public Cmd_SetTradingRestrictions(string userName,string jsonTradingRestrictions)
            : base(MessageNumbers.Cmd_SetTradingRestrictions)
        {
            UserName = userName;
            TradingRestrictions = jsonTradingRestrictions;
        }
    }
    /// <summary>
    /// The command from client to apply TimeGrid on the fly
    /// </summary>
    public class Cmd_SetTimeGrid : BaseMessage
    {
        public string UserName;
        public string JsonSetializedTimeGrid;
        public Cmd_SetTimeGrid() : base(MessageNumbers.Cmd_SetTimeGrid) { }
        public Cmd_SetTimeGrid(string userName, string jsonSetializedTimeGrid)
            : base(MessageNumbers.Cmd_SetTimeGrid)
        {
            UserName = userName;
            JsonSetializedTimeGrid = jsonSetializedTimeGrid;
        }
    }

    public class Cmd_UpdateGroup : BaseMessage
    {
        public string UserName;
        public long GroupId;
        public List<int> Fstates_Item1;
        public List<bool> Fstates_Item2;
        public int UserLongState;
        public int UserShortState;

        public Cmd_UpdateGroup() : base(MessageNumbers.Cmd_UpdateGroup)
        {
            Fstates_Item1=new List<int>();
            Fstates_Item2=new List<bool>();
        }

        public Cmd_UpdateGroup(string userName, long groupId, List<Tuple<int, bool>> fstates, int userLongState,
            int userShortState)
            : base(MessageNumbers.Cmd_UpdateGroup)
        {
            UserName = userName;
            GroupId = groupId;
            Fstates_Item1 = fstates.Select(t => t.Item1).ToList();
            Fstates_Item2 = fstates.Select(t => t.Item2).ToList();
            UserLongState = userLongState;
            UserShortState = userShortState;
        }

        public List<Tuple<int, bool>> FStates
        {
            get
            {
                if (Fstates_Item1.Count != Fstates_Item2.Count)
                    throw new Exception(string.Format("Message Cmd_UpdateGroup contains invalid data ({0}!={1})",
                        Fstates_Item1.Count , Fstates_Item2.Count));

                return Enumerable.Range(0, Fstates_Item1.Count)
                    .Select(i => new Tuple<int, bool>(Fstates_Item1[i], Fstates_Item2[i]))
                    .ToList();
            }
        }
    }

    public class Cmd_UpdateManualResettingStates : BaseMessage
    {
        public string UserName;
        public List<int> ManualResettingStatesForFilters_Item1;
        public List<bool> ManualResettingStatesForFilters_Item2;
        public List<int> FiltersToReset;

        public Cmd_UpdateManualResettingStates() : base(MessageNumbers.Cmd_UpdateManualResettingStates)
        {
            ManualResettingStatesForFilters_Item1 = new List<int>();
            ManualResettingStatesForFilters_Item2 = new List<bool>();
            FiltersToReset = new List<int>();
        }

        public Cmd_UpdateManualResettingStates(string userName, List<Tuple<int, bool>> manualResettingStatesForFilters,
            List<int> filtersToReset)
            : base(MessageNumbers.Cmd_UpdateManualResettingStates)
        {
            UserName = userName;
            ManualResettingStatesForFilters_Item1 = manualResettingStatesForFilters.Select(t => t.Item1).ToList();
            ManualResettingStatesForFilters_Item2 = manualResettingStatesForFilters.Select(t => t.Item2).ToList();
            FiltersToReset = filtersToReset;
        }

        public List<Tuple<int, bool>> ManualResettingStatesForFilters
        {
            get
            {
                if (ManualResettingStatesForFilters_Item1.Count != ManualResettingStatesForFilters_Item2.Count)
                    throw new Exception(string.Format("Message Cmd_UpdateManualResettingStates contains invalid data ({0}!={1})",
                        ManualResettingStatesForFilters_Item1.Count, ManualResettingStatesForFilters_Item2.Count));

                return Enumerable.Range(0, ManualResettingStatesForFilters_Item1.Count)
                    .Select(i =>
                        new Tuple<int, bool>(
                            ManualResettingStatesForFilters_Item1[i],
                            ManualResettingStatesForFilters_Item2[i]))
                    .ToList();
            }
        }
        
    }
}
