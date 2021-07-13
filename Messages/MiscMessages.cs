using System;
using System.Collections.Generic;
using System.Linq;
using CfgDescription;

namespace Messages
{
    // saves the conversion rates used in TradingRestrictionsManager
    public class ConversionRatesToAccountCurrency : BaseMessage
    {
        public string JsonSerializedConversionRates;

        public ConversionRatesToAccountCurrency() : base(MessageNumbers.ConversionRatesToAccountCurrency) { }
        public ConversionRatesToAccountCurrency(string jsonSerializedConversionRates)
            : base(MessageNumbers.ConversionRatesToAccountCurrency)
        {
            JsonSerializedConversionRates = jsonSerializedConversionRates;
        }
    }

    public class ExposureTriggersList : BaseMessage
    {
        public bool SetTargetStateWhenNoData;
        public string Descriptions;

        public ExposureTriggersList() : base(MessageNumbers.ExposureTriggersList) { }
        public ExposureTriggersList(bool setTargetStateWhenNoData,string descriptions)
            : base(MessageNumbers.ExposureTriggersList)
        {
            SetTargetStateWhenNoData= setTargetStateWhenNoData;
            Descriptions = descriptions;
        }
    }
    public class ExposureTriggerStates : BaseMessage
    {
        public bool FullList;
        public string SignalStateTriggers;
        public string NotSignalStateTriggers;

        public ExposureTriggerStates() : base(MessageNumbers.ExposureTriggerStates) { }
        public ExposureTriggerStates(bool fullList,IEnumerable<int> signalStateIds, IEnumerable<int> notSignalStateIds)
            : base(MessageNumbers.ExposureTriggerStates)
        {
            FullList = fullList;
            SignalStateTriggers = string.Join(",", signalStateIds);
            NotSignalStateTriggers = string.Join(",", notSignalStateIds);
        }
    }

    public class SignalDelayConfigurationWithHandlerIDs : BaseMessage
    {
        public string Info;

        public SignalDelayConfigurationWithHandlerIDs() 
            : base(MessageNumbers.SignalDelayConfigurationWithHandlerIDs)
        { }
        public SignalDelayConfigurationWithHandlerIDs(string jsonSerializedInfo) : base(MessageNumbers.SignalDelayConfigurationWithHandlerIDs)
        {
            Info = jsonSerializedInfo;
        }
    }

    public class ConditionHandlerStates : BaseMessage
    {
        public bool FullList;
        public string SignalStateHandlers;
        public string NotSignalStateHandlers;

        public ConditionHandlerStates() : base(MessageNumbers.ConditionHandlerStates) { }
        public ConditionHandlerStates(bool fullList, IEnumerable<int> signalStateIds, IEnumerable<int> notSignalStateIds)
            : base(MessageNumbers.ConditionHandlerStates)
        {
            FullList = fullList;
            SignalStateHandlers = string.Join(",", signalStateIds);
            NotSignalStateHandlers = string.Join(",", notSignalStateIds);
        }
    }

    public class DelayExecStateChanged : BaseMessage
    {
        public string ChangedConditionSets;
        public DelayExecStateChanged() : base(MessageNumbers.DelayExecStateChanged) { }
        public DelayExecStateChanged(IEnumerable<string> changedConditionSets)
            : base(MessageNumbers.DelayExecStateChanged)
        {
            ChangedConditionSets= string.Join(";", changedConditionSets);
        }
    }

    public class CPRestr
    {
        public string CP;
        public decimal Long;
        public decimal Short;
        public decimal Total;
    }

    public class PRestr
    {
        public string Portfolio;
        public decimal Total;
        public List<CPRestr> CPRS;
    }

    public class TriggeredRestrictionsSummary : BaseMessage
    {
        public bool FullList;
        public string Info; // json serialized List<PRestr>

        public TriggeredRestrictionsSummary() : base(MessageNumbers.TriggeredRestrictionsSummary) { }
        public TriggeredRestrictionsSummary(bool fullList, string info)
            : base(MessageNumbers.TriggeredRestrictionsSummary)
        {
            FullList = fullList;
            Info = info;
        }
    }
    public class DefaultExposureRestrictions : BaseMessage
    {
        public bool AmountRestrictions;
        public string Info; // json serialized List<PRestr>

        public DefaultExposureRestrictions() : base(MessageNumbers.DefaultExposureRestrictions) { }
        public DefaultExposureRestrictions(bool amountRestrictions, string info)
            : base(MessageNumbers.DefaultExposureRestrictions)
        {
            AmountRestrictions = amountRestrictions;
            Info = info;
        }
    }




}