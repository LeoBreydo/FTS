using System;
using System.Windows.Forms;
using Binosoft.TraderLib.Indicators;

namespace Configurator.ViewModel
{
    public enum EDynamicStopModes
    {
        No, ExitByQuote, SizeOfLoss
    }
    public enum EDynamicTargetModes
    {
        No, ExitByQuote, SizeOfProfit
    }

    public static class TargetModesHelper
    {
        public static EDynamicStopModes DynamicStopToUser(this DynamicGuardMode mode)
        {
            switch (mode)
            {
                case DynamicGuardMode.NotUse:
                    return EDynamicStopModes.No;
                case DynamicGuardMode.OrderPrice:
                    return EDynamicStopModes.ExitByQuote;
                case DynamicGuardMode.Delta:
                    return EDynamicStopModes.SizeOfLoss;
                default:
                    throw new Exception("Unknown value " + mode);
            }
        }

        public static DynamicGuardMode DynamicStopFromUser(this EDynamicStopModes mode)
        {
            switch (mode)
            {
                case EDynamicStopModes.No:
                    return DynamicGuardMode.NotUse;
                case EDynamicStopModes.ExitByQuote:
                    return DynamicGuardMode.OrderPrice;
                case EDynamicStopModes.SizeOfLoss:
                    return DynamicGuardMode.Delta;
                default:
                    throw new Exception("Unknown value " + mode);
            }
        }

        public static EDynamicTargetModes DynamicTargetToUser(this DynamicGuardMode mode)
        {
            switch (mode)
            {
                case DynamicGuardMode.NotUse:
                    return EDynamicTargetModes.No;
                case DynamicGuardMode.OrderPrice:
                    return EDynamicTargetModes.ExitByQuote;
                case DynamicGuardMode.Delta:
                    return EDynamicTargetModes.SizeOfProfit;
                default:
                    throw new Exception("Unknown value " + mode);
            }
        }

        public static DynamicGuardMode DynamicTargetFromUser(this EDynamicTargetModes mode)
        {
            switch (mode)
            {
                case EDynamicTargetModes.No:
                    return DynamicGuardMode.NotUse;
                case EDynamicTargetModes.ExitByQuote:
                    return DynamicGuardMode.OrderPrice;
                case EDynamicTargetModes.SizeOfProfit:
                    return DynamicGuardMode.Delta;
                default:
                    throw new Exception("Unknown value " + mode);
            }
        }

    }

    public class StrategyPGEditor : BaseController
    {
        private const string CAT_MAIN = "1. Instrument";

        private const string CAT_SIGNALGENERATOR_PARAMS = "2. Signal Generator parameters";
        private const string CAT_GUARD_SL = "3. StopLoss Guard";
        private const string CAT_GUARD_TP = "4. TakeProfit Guard";
        private const string CAT_GUARD_DSL = "5. DynamicStop Guard";
        private const string CAT_GUARD_DTP = "6. DynamicTarget Guard";
        private const string CAT_SLRestriction = "7. Restriction to reenter position after stoploss";


        private readonly PropertyGrid _propertyGrid;
        private readonly StrategyRow _info; 
        private readonly DynamicGuard _dynGuard; // just for compact call, assigned in ctor but also can be wrapped as property _info.Cfg.DynamicGuardDescription
        private readonly SignalGeneratorDescription _sgd;

        private readonly PropertySpec
            psTakeProfitDelta,
            psFixedStopLossDelta,
            psTrailedStopLossInitialDelta,
            psTrailingDelta,
            psActivationProfit;

        private readonly PropertySpec psDGT_longExpr, psDGT_shortExpr; 
        private readonly PropertySpec psDGS_longExpr, psDGS_shortExpr;
        private readonly PropertySpec psGoToFlatMustLiftRestriction;

        public StrategyPGEditor(StrategyRow info,PropertyGrid propertyGrid)
        {
            _propertyGrid = propertyGrid;
            _info = info;
            _dynGuard = _info.Cfg.DynamicGuardDescription;
            Properties.Add(new PropertySpec(" Exchange", typeof(string), CAT_MAIN){ReadOnly = true});
            Properties.Add(new PropertySpec(" Market", typeof(string), CAT_MAIN) { ReadOnly = true });
            Properties.Add(new PropertySpec(" TimeFrame", typeof(string), CAT_MAIN) { ReadOnly = true });
            Properties.Add(new PropertySpec("InWorkingSet", typeof(bool), CAT_MAIN));
            Properties.Add(new PropertySpec("NbrOfContracts", typeof(int?), CAT_MAIN));
            Properties.Add(new PropertySpec("StrategyID", typeof(int?), CAT_MAIN) { ReadOnly = true });
            Properties.Add(new PropertySpec("StrategyName", typeof(string), CAT_MAIN));

            _sgd = _info.GetDescription();
            if (_sgd != null)
            {
                if (_sgd.ContainsTradingZones)
                    Properties.Add(new PropertySpec(" IgnoreTradingZones", typeof(bool), CAT_SIGNALGENERATOR_PARAMS));
                foreach (StrategyParameter par in _info.Cfg.StrategyParameters.Parameters)
                {
                    Properties.Add(new PropertySpec(
                        par.Name, _sgd.GetParamType(par.Name), CAT_SIGNALGENERATOR_PARAMS,
                        _sgd.GetParamDescription(par.Name)));
                }
            }

            Properties.Add(new PropertySpec(" Use", typeof(StopLossPositionGuardTypes), CAT_GUARD_SL, "Use StopLoss Guard"));
            psFixedStopLossDelta = new PropertySpec("FixedStopLossDelta", typeof(double), CAT_GUARD_SL,
                                                        "FixedStopLossDelta in quotes");
            psTrailedStopLossInitialDelta = new PropertySpec("InitialStopLossDelta", typeof(double), CAT_GUARD_SL,
                                                        "InitialStopLossDelta in quotes");
            psTrailingDelta = new PropertySpec("TrailingDelta", typeof(double), CAT_GUARD_SL,
                                                        "TrailingDelta in quotes");
            psActivationProfit = new PropertySpec("ActivationProfit", typeof(double), CAT_GUARD_SL,
                                                        "ActivationProfit in quotes");

            Properties.Add(new PropertySpec(" Use", typeof(bool), CAT_GUARD_TP, " Use TakeProfit Guard"));
            psTakeProfitDelta = new PropertySpec("TakeProfitDelta", typeof(double), CAT_GUARD_TP, "TakeProfitDelta in quotes");



            Properties.Add(new PropertySpec(" Use", typeof(EDynamicTargetModes), CAT_GUARD_DTP,
                                            "Usage of the target order based on the dynamically calculated indicator expression"));
            Type indeditor = typeof(IndicatorEditor);

            psDGT_longExpr = new PropertySpec("LongPos Expression", typeof(string), CAT_GUARD_DTP,
                    "ExitByQuote: exit long if bid>=value; SizeOfProfit: exit long if profit >= value", indeditor);
            psDGT_shortExpr = new PropertySpec("ShortPos Expression", typeof(string), CAT_GUARD_DTP,
                    "ExitByQuote: exit short if ask<=value; SizeOfProfit: exit short if profit >= value", indeditor);


            Properties.Add(new PropertySpec(" Use", typeof(EDynamicStopModes), CAT_GUARD_DSL,
                                             "Usage of the stop order based on the dynamically calculated indicator expression"));

            psDGS_longExpr = new PropertySpec("LongPos Expression", typeof(string), CAT_GUARD_DSL,
                    "ExitByQuote: exit long if bid<=value; SizeOfLoss: exit long if Abs(Lose) >= value", indeditor);
            psDGS_shortExpr = new PropertySpec("ShortPos Expression", typeof(string), CAT_GUARD_DSL,
                    "ExitByQuote: exit short if ask>=value; SizeOfLoss: exit short if Abs(Lose) >= value", indeditor);

            Properties.Add(new PropertySpec("MaxBarsToWaitForOppositeSignal", typeof(int), CAT_SLRestriction,
                @"Disallow to re-enter the same direction position after exit by stoploss for specified number of bars.
After the maxBarsToWaitForOppositeSignal bars this restriction is lifted.
Restriction is not used if maxBarsToWaitForOppositeSignal is zero"));
            psGoToFlatMustLiftRestriction = new PropertySpec("GoToFlatMustLiftRestriction", typeof(bool),
                                                             CAT_SLRestriction,
                                                             @"'True' means 'to lift restriction when strategy decides to go to flat or opposite direction'. 
'False' means 'to lift restriction when strategy decides to go to opposite direction only'");


            SetGuardSettingsVisibility(false);
            SetDynamicGuardSettingsVisibility(false);
            SetStoplossRestrictionVisibility(false);


            GetValue += _GetValue;
            SetValue += _SetValue;
        }


        private void _GetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Category)
            {
                case CAT_SIGNALGENERATOR_PARAMS:
                    if (e.Property.Name == " IgnoreTradingZones")
                        e.Value = _info.Cfg.IgnoreTimeZones;
                    else if (_info.Cfg.StrategyParameters.TryGetValue(e.Property.Name, out string val))
                        e.Value = val;
                    return;

                case CAT_GUARD_SL:
                    switch (e.Property.Name)
                    {
                        case " Use":
                            e.Value = _info.Cfg.StopLossPositionGuardType;
                            break;

                        case "FixedStopLossDelta":
                            e.Value = _info.Cfg.FixedStopLossDelta;
                            break;

                        case "InitialStopLossDelta":
                            e.Value = _info.Cfg.TrailedStopLossInitialDelta;
                            break;
                        case "ActivationProfit":
                            e.Value = _info.Cfg.ActivationProfit;
                            break;
                        case "TrailingDelta":
                            e.Value = _info.Cfg.TrailingDelta;
                            break;

                    }
                    return;
                case CAT_GUARD_TP:
                    switch (e.Property.Name)
                    {
                        case " Use":
                            e.Value = _info.Cfg.UseTakeProfitGuard;
                            break;
                        case "TakeProfitDelta":
                            e.Value = _info.Cfg.TakeProfitDelta;
                            break;
                    }
                    return;
                case CAT_GUARD_DTP:
                    switch (e.Property.Name)
                    {
                        case " Use":
                            e.Value = _dynGuard.TargetMode.DynamicTargetToUser();
                            break;
                        case "LongPos Expression":
                            e.Value = MacroSubstServer.ToShort(_dynGuard.TargetGuardLongExpression);
                            break;
                        case "ShortPos Expression":
                            e.Value = MacroSubstServer.ToShort(_dynGuard.TargetGuardShortExpression);
                            break;
                    }
                    return;
                case CAT_GUARD_DSL:
                    switch (e.Property.Name)
                    {
                        case " Use":
                            e.Value = _dynGuard.StopMode.DynamicStopToUser();
                            break;
                        case "LongPos Expression":
                            e.Value = MacroSubstServer.ToShort(_dynGuard.StopGuardLongExpression);
                            break;
                        case "ShortPos Expression":
                            e.Value = MacroSubstServer.ToShort(_dynGuard.StopGuardShortExpression);
                            break;
                    }
                    return;

                default:
                    switch (e.Property.Name)
                    {
                        case "StrategyName":
                            e.Value = _info.StrategyName;
                            break;
                        case " Exchange":
                            e.Value = _info.Exchange;
                            break;
                        case " Market":
                            e.Value = _info.Market;
                            break;
                        case " TimeFrame":
                            e.Value = _info.TimeFrame;
                            break;

                        case "MaxBarsToWaitForOppositeSignal":
                            e.Value = _info.Cfg.StoplossRestriction_MaxBarsToWaitForOppositeSignal;
                            break;
                        case "GoToFlatMustLiftRestriction":
                            e.Value = _info.Cfg.StoplossRestriction_GoToFlatMustLiftRestriction;
                            break;

                        case "NbrOfContracts":
                            e.Value = _info.NbrOfContracts;
                            break;
                        case "InWorkingSet":
                            e.Value = _info.InWorkingSet;
                            break;
                    }
                    break;
            }


        }

        private void _SetValue(object sender, PropertySpecEventArgs e)
        {
            string strValue;
            double dbl;

            switch (e.Property.Category)
            {
                case CAT_SIGNALGENERATOR_PARAMS:
                    if (e.Value == null) return;

                    if (e.Property.Name == " IgnoreTradingZones")
                        _info.Cfg.IgnoreTimeZones = (bool) e.Value;
                    else
                    {
                        string err = _sgd.VerifyParameterValue(e.Property.Name, e.Value);
                        if (err != null) throw new Exception(err);
                        _info.Cfg.StrategyParameters.SetValue(e.Property.Name, e.Value.ToString());
                    }
                    break;
                case CAT_GUARD_SL:
                    switch (e.Property.Name)
                    {
                        case " Use":
                            _info.Cfg.StopLossPositionGuardType = (StopLossPositionGuardTypes)e.Value;
                            _info.UpdateSummaryCells();
                            SetGuardSettingsVisibility();
                            break;

                        case "FixedStopLossDelta":
                            _info.SetFixedStopLossDelta((double)e.Value);
                            break;

                        case "InitialStopLossDelta":
                            _info.SetInitialStopLossDelta((double)e.Value);
                            break;
                        case "ActivationProfit":
                            _info.SetActivationProfit((double) e.Value);
                            break;
                        case "TrailingDelta":
                            _info.SetTrailingDelta((double) e.Value);
                            break;
                    }
                    break;
                case CAT_GUARD_TP:
                    switch (e.Property.Name)
                    {
                        case " Use":
                            _info.Cfg.UseTakeProfitGuard = (bool)e.Value;
                            _info.UpdateSummaryCells();
                            SetGuardSettingsVisibility();
                            break;
                        case "TakeProfitDelta":
                            _info.SetTakeProfitDeltaInPips((double) e.Value);
                            break;
                    }
                    break;
                case CAT_GUARD_DSL:
                    switch (e.Property.Name)
                    {
                        case " Use":
                            _dynGuard.StopMode = ((EDynamicStopModes)e.Value).DynamicStopFromUser();
                            _info.UpdateSummaryCells();
                            SetDynamicGuardSettingsVisibility();
                            break;
                        case "LongPos Expression":
                            strValue = MacroSubstServer.ToFull(((string)e.Value).Trim());
                            if (double.TryParse(strValue, out dbl))
                            {
                                if (dbl <= 0)
                                    throw new Exception("Invalid value");
                                if (_dynGuard.StopMode == DynamicGuardMode.OrderPrice)
                                    throw new Exception("Invalid indicator expression, can not be constant for specified guard mode");
                            }
                            else if (!string.IsNullOrEmpty(strValue))
                            {
                                if (!IndicatorsVerificator.IsValidIndicatorExpression(_info.Cfg.Timeframe, strValue))
                                    throw new Exception("Invalid indicator expression");
                            }
                            _dynGuard.StopGuardLongExpression = strValue;
                            _info.UpdateSummaryCells();
                            break;
                        case "ShortPos Expression":
                            strValue = MacroSubstServer.ToFull(((string)e.Value).Trim());
                            if (double.TryParse(strValue, out dbl))
                            {
                                if (dbl <= 0)
                                    throw new Exception("Invalid value");
                                if (_dynGuard.StopMode == DynamicGuardMode.OrderPrice)
                                    throw new Exception("Invalid indicator expression, can not be constant for specified guard mode");
                            }
                            else if (!string.IsNullOrEmpty(strValue))
                            {
                                if (!IndicatorsVerificator.IsValidIndicatorExpression(_info.Cfg.Timeframe, strValue))
                                    throw new Exception("Invalid indicator expression");
                            }
                            _dynGuard.StopGuardShortExpression = strValue;
                            _info.UpdateSummaryCells();
                            break;
                    }
                    break;
                case CAT_GUARD_DTP:
                    switch (e.Property.Name)
                    {
                        case " Use":
                            _dynGuard.TargetMode = ((EDynamicTargetModes)e.Value).DynamicTargetFromUser();
                            _info.UpdateSummaryCells();
                            SetDynamicGuardSettingsVisibility();
                            break;
                        case "LongPos Expression":
                            strValue = MacroSubstServer.ToFull(((string)e.Value).Trim());
                            if (double.TryParse(strValue, out dbl))
                            {
                                if (dbl <= 0)
                                    throw new Exception("Invalid value");
                                if (_dynGuard.StopMode == DynamicGuardMode.OrderPrice)
                                    throw new Exception("Invalid indicator expression, can not be constant for specified guard mode");
                            }
                            else if (!string.IsNullOrEmpty(strValue))
                            {
                                if (!IndicatorsVerificator.IsValidIndicatorExpression(_info.Cfg.Timeframe, strValue))
                                    throw new Exception("Invalid indicator expression");
                            }
                            _dynGuard.TargetGuardLongExpression = strValue;
                            _info.UpdateSummaryCells();
                            break;
                        case "ShortPos Expression":
                            strValue = MacroSubstServer.ToFull(((string)e.Value).Trim());
                            if (double.TryParse(strValue, out dbl))
                            {
                                if (dbl <= 0)
                                    throw new Exception("Invalid value");
                                if (_dynGuard.StopMode == DynamicGuardMode.OrderPrice)
                                    throw new Exception("Invalid indicator expression, can not be constant for specified guard mode");
                            }
                            else if (!string.IsNullOrEmpty(strValue))
                            {
                                if (!IndicatorsVerificator.IsValidIndicatorExpression(_info.Cfg.Timeframe, strValue))
                                    throw new Exception("Invalid indicator expression");
                            }
                            _dynGuard.TargetGuardShortExpression = strValue;
                            _info.UpdateSummaryCells();
                            break;
                    }
                    break;
                default:
                {
                    switch (e.Property.Name)
                    {
                        case "InWorkingSet":
                            _info.InWorkingSet = (bool)e.Value;
                            _propertyGrid.Refresh();
                            break;
                        case "NbrOfContracts":
                            _info.NbrOfContracts = (int?)e.Value;
                            _propertyGrid.Refresh();
                            break;
                        case "StrategyName":
                            _info.StrategyName= (string)e.Value;
                            break;
                            
                        case "MaxBarsToWaitForOppositeSignal":
                            var intValue = (int)e.Value;
                            if (intValue < 0) intValue = 0;
                            _info.Cfg.StoplossRestriction_MaxBarsToWaitForOppositeSignal = intValue;
                            _info.UpdateSummaryCells();
                            SetStoplossRestrictionVisibility();
                            break;
                        case "GoToFlatMustLiftRestriction":
                            _info.Cfg.StoplossRestriction_GoToFlatMustLiftRestriction = (bool)e.Value;
                            _info.UpdateSummaryCells();
                            break;


                        }
                    }
                    break;
            }
        }

        private void SetStoplossRestrictionVisibility(bool updatePropertyGrid = true)
        {
            AddRemovePropertySpec(_info.Cfg.StoplossRestriction_MaxBarsToWaitForOppositeSignal > 0, psGoToFlatMustLiftRestriction);

            if (updatePropertyGrid)
                _propertyGrid.Refresh();
        }
        private void SetGuardSettingsVisibility(bool updatePropertyGrid = true)
        {
            AddRemovePropertySpec(_info.Cfg.UseTakeProfitGuard, psTakeProfitDelta);
            AddRemovePropertySpec(_info.Cfg.StopLossPositionGuardType == StopLossPositionGuardTypes.Fixed, psFixedStopLossDelta);

            AddRemovePropertySpec(_info.Cfg.StopLossPositionGuardType == StopLossPositionGuardTypes.Trailed,
                psTrailedStopLossInitialDelta, psTrailingDelta, psActivationProfit);

            if (updatePropertyGrid)
                _propertyGrid.Refresh();
        }
        private void SetDynamicGuardSettingsVisibility(bool updatePropertyGrid = true)
        {
            switch (_dynGuard.StopMode)
            {
                default://case DynamicGuardMode.NotUse:
                    AddRemovePropertySpec(false, psDGS_longExpr, psDGS_shortExpr);
                    break;
                case DynamicGuardMode.OrderPrice:
                    AddRemovePropertySpec(true, psDGS_longExpr, psDGS_shortExpr);
                    psDGS_longExpr.Description = "Indicator expression to exit LONG at <expression> STOP";
                    psDGS_shortExpr.Description = "Indicator expression to exit SHORT at <expression> STOP";
                    break;
                case DynamicGuardMode.Delta:
                    AddRemovePropertySpec(true, psDGS_longExpr, psDGS_shortExpr);
                    psDGS_longExpr.Description = "Indicator expression to exit LONG at openPrice - <expression> STOP";
                    psDGS_shortExpr.Description = "Indicator expression to exit SHORT at openPrice + <expression> STOP";
                    break;
            }
            switch (_dynGuard.TargetMode)
            {
                default://case DynamicGuardMode.NotUse:
                    AddRemovePropertySpec(false, psDGT_longExpr, psDGT_shortExpr);
                    break;
                case DynamicGuardMode.OrderPrice:
                    AddRemovePropertySpec(true, psDGT_longExpr, psDGT_shortExpr);
                    psDGT_longExpr.Description = "Indicator expression to exit LONG at <expression> LIMIT";
                    psDGT_shortExpr.Description = "Indicator expression to exit SHORT at <expression> LIMIT";
                    break;
                case DynamicGuardMode.Delta:
                    AddRemovePropertySpec(true, psDGT_longExpr, psDGT_shortExpr);
                    psDGT_longExpr.Description = "Indicator expression to exit LONG at openPrice + <expression> LIMIT";
                    psDGT_shortExpr.Description = "Indicator expression to exit SHORT at openPrice - <expression> LIMIT";
                    break;

            }
            if (updatePropertyGrid)
                _propertyGrid.Refresh();
        }

    }
}