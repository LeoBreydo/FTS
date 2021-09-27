using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Binosoft.TraderLib.Indicators;

namespace Configurator.ViewModel
{
    public enum RowType
    {
        Unknown=-1,
        //TCfg=0,
        Exchanges=0,
        Markets=1,
        Strategies=2
    };
    public interface IRow
    {
        RowType GetRowType();
        object GetPropertyGridEditor(PropertyGrid pg);
        string GetTitle();
    }

    static class ConvertHelper
    {
        public static int? MaxErrorsToUser(int maxErrorsPerDay)
        {
            if (maxErrorsPerDay == int.MaxValue) return null;
            return maxErrorsPerDay;
        }
        public static int AcceptMaxErrors(int? value)//maxErrorsPerDay_or_null
        {
            if (!value.HasValue)
                return Int32.MaxValue;

            int v = value.Value;
            if (v < 0) throw new Exception("MaxErrorsPerDay must be not-negative or empty");
            return v;
        }
    }

    public class TCfgRow : IRow
    {
        private readonly TradingConfiguration _tcfg;

        public TCfgRow(TradingConfiguration tcfg)
        {
            _tcfg = tcfg;
        }
        public object GetPropertyGridEditor(PropertyGrid pg)
        {
            return new TCfgPGEditor(_tcfg);
        }

        public RowType GetRowType()
        {
            return RowType.Unknown;
        }

        public string GetTitle() => "Trading Configuration";
    }
    public class ExchangeRow : BaseNotifyPropertyChanged, IRow
    {
        public RowType GetRowType() => RowType.Exchanges;
        public object GetPropertyGridEditor(PropertyGrid pg) => new ExchangePGEditor(this);
        public string GetTitle() => "Exchange " + Cfg.ExchangeName;

        public ExchangeConfiguration Cfg;
        public string Configuration => Controller.STR_Configuration;
        public string Exchange => Cfg.ExchangeName;

        public string Currency
        {
            get => Cfg.Currency;
            set => SetField(ref Cfg.Currency, Controller.Instance.ListOfCurrencies.AcceptCurrency(value));
        }


        public int? MaxErrorsPerDay
        {
            get => ConvertHelper.MaxErrorsToUser(Cfg.MaxErrorsPerDay);
            set => SetField(ref Cfg.MaxErrorsPerDay, ConvertHelper.AcceptMaxErrors(value));
        }
        public bool IsInWorkingSet()
        {
            return Cfg.Markets.Any(m => m.Strategies.Any(s => s.NbrOfContracts > 0));
        }
    }
    public class MarketRow : BaseNotifyPropertyChanged, IRow
    {
        public RowType GetRowType() => RowType.Markets;
        public object GetPropertyGridEditor(PropertyGrid pg) => new MarketPGEditor(this);
        public string GetTitle() => $"Market {Cfg.MarketName} at {Cfg.Exchange}";

        public MarketConfiguration Cfg; // is not null if market is included to TradingConfiguration
        public List<PropertyValue> Properties;
        public string Configuration => Controller.STR_Configuration;
        public string Exchange => Cfg.Exchange;
        public string MarketName => Cfg.MarketName;

        public int BigPointValue
        {
            get => Cfg.BigPointValue;
            set
            {
                if (value <= 0) throw new Exception("BigPointValue must be positive");
                SetField(ref Cfg.BigPointValue, value);
            }
        }
        public double MinMove
        {
            get => Cfg.MinMove;
            set
            {
                if (value <= 0) throw new Exception("MinMove must be positive");
                SetField(ref Cfg.MinMove, value);
            }
        }

        public decimal? SessionCriticalLoss
        {
            get => Cfg.SessionCriticalLoss == decimal.MinValue ? (decimal?) null : Cfg.SessionCriticalLoss;
            set
            {
                if (!value.HasValue || value.Value==0)
                    SetField(ref Cfg.SessionCriticalLoss, decimal.MinValue);
                else
                {
                    SetField(ref Cfg.SessionCriticalLoss, -Math.Abs(value.Value));
                }
            }
        }
        public int? MaxErrorsPerDay
        {
            get => ConvertHelper.MaxErrorsToUser(Cfg.MaxErrorsPerDay);
            set => SetField(ref Cfg.MaxErrorsPerDay, ConvertHelper.AcceptMaxErrors(value));
        }
        public int? MaxNbrContracts
        {
            get => Cfg.MaxNbrContracts == 0 ? (int?)null : Cfg.MaxNbrContracts;
            set
            {
                if (!value.HasValue)
                    SetField(ref Cfg.MaxNbrContracts, 0);
                else
                {
                    int v = value.Value;
                    if (v < 0) throw new Exception("MaxNbrContracts must be positive or empty");
                    SetField(ref Cfg.MaxNbrContracts, v);
                }
            }
        }
        public int SumOfStrategyContracts => Cfg.Strategies.Sum(str => str.NbrOfContracts);
        public bool IsInWorkingSet()
        {
            return Cfg.Strategies.Any(s => s.NbrOfContracts > 0);
        }

    }


    public class StrategyRow: BaseNotifyPropertyChanged,IRow
    {
        public RowType GetRowType() => RowType.Strategies;
        public object GetPropertyGridEditor(PropertyGrid pg) => new StrategyPGEditor(this, pg);
        public string GetTitle() => $"Strategy {Cfg.StrategyName} at  {Exchange}/{MarketName}";

        public readonly SignalGeneratorDescription SGDescription;
        public StrategyConfiguration Cfg;
        public List<PropertyValue> Properties;
        // grouping keys
        public string Configuration => Controller.STR_Configuration;
        public string Exchange { get; }
        public string MarketName { get; }

        private int _lastPositiveNumLots = 1;
        public bool InWorkingSet
        {
            get => Cfg.NbrOfContracts>0;
            set
            {
                if (value != Cfg.NbrOfContracts > 0)
                    NbrOfContracts = value ? _lastPositiveNumLots : 0;
            }
        }
        public int? NbrOfContracts
        {
            get
            {
                if (Cfg.NbrOfContracts<=0) return null;
                return Cfg.NbrOfContracts;
            }
            set
            {
                int newCfgVal = (!value.HasValue || value <= 0) ? 0 : value.Value;
                int oldCfgVal = Cfg.NbrOfContracts;
                SetField(ref Cfg.NbrOfContracts, newCfgVal);
                if (newCfgVal > 0)
                    _lastPositiveNumLots = newCfgVal;
                if (Math.Sign(newCfgVal)!= Math.Sign(oldCfgVal))
                    OnPropertyChanged(nameof(InWorkingSet));
            }
        }

        //public string Name => Path.GetFileNameWithoutExtension(SignalGeneratorDescription.ShortDllName);
        public string Name
        {
            get=> Cfg.StrategyName;// Path.GetFileNameWithoutExtension(SignalGeneratorDescription.ShortDllName);
            set
            {
                if (string.IsNullOrWhiteSpace(value) || !value.IsIdentifier())
                    throw new Exception(
                        "Strategy name must start with Letter character and consists from Letter and Digit characters only");

                SetField(ref Cfg.StrategyName, value);
            }
        }
        public string TimeFrame => Cfg.Timeframe;// ?? SignalGeneratorDescription.SearchTimeFrame;

        public StrategyRow(string exchange, string mktName, SignalGeneratorDescription sgd)
        {
            Exchange = exchange;
            MarketName = mktName;
            SGDescription = sgd;
            Cfg = new StrategyConfiguration
            {

                StrategyName = Path.GetFileNameWithoutExtension(sgd.ShortDllName),
                StrategyDll = sgd.ShortDllName,
                Timeframe = sgd.SearchTimeFrame,
                ModelID = sgd.ModelID,
                StrategyParameters = sgd.CloneDefautParameters()
            };
            UpdateSummaryCells();
        }

        public void SetFixedStopLossDelta(double newVal)
        {
            if (newVal <= 0) throw new Exception("Value must be > 0");
            Cfg.FixedStopLossDelta = newVal;
            UpdateSummaryCells();
        }
        public void SetInitialStopLossDelta(double newVal)
        {
            if (newVal <= 0) throw new Exception("Value must be > 0");
            Cfg.TrailedStopLossInitialDelta = newVal;
            UpdateSummaryCells();
        }
        public void SetActivationProfit(double newVal)
        {
            if (newVal <= 0) throw new Exception("Value must be > 0");
            Cfg.ActivationProfit = newVal;
            UpdateSummaryCells();
        }
        public void SetTrailingDelta(double newVal)
        {
            if (newVal <= 0) throw new Exception("Value must be > 0");
            Cfg.TrailingDelta = newVal;
            UpdateSummaryCells();
        }
        public void SetTakeProfitDeltaInPips(double newVal)
        {
            if (newVal <= 0) throw new Exception("Value must be > 0");
            Cfg.TakeProfitDelta = newVal;
            UpdateSummaryCells();
        }

        private string slg;
        private string tpg;
        private string dsg;
        private string dtg;
        private string rr;
        public string StopLossGuarg { get=>slg; private set => SetField(ref slg, value); }
        public string TakeProfitGuarg { get => tpg; private set => SetField(ref tpg, value); }
        public string DynamicStopGuarg { get => dsg; private set => SetField(ref dsg, value); }
        public string DynamicTargetGuarg { get => dtg; private set => SetField(ref dtg, value); }
        public string ReenterRestriction { get => rr; private set => SetField(ref rr, value); }

        public void UpdateSummaryCells()
        {
            StopLossGuarg = StrSL();
            TakeProfitGuarg = StrTP();
            DynamicStopGuarg = StrDSL();
            DynamicTargetGuarg = StrDTP();
            ReenterRestriction = StrRR();
        }

        private string StrSL()
        {
            switch (Cfg.StopLossPositionGuardType)
            {
                case StopLossPositionGuardTypes.Fixed:
                    return $"{Cfg.FixedStopLossDelta}";
                case StopLossPositionGuardTypes.Trailed:
                    return $"{Cfg.TrailedStopLossInitialDelta}; {Cfg.TrailingDelta}; {Cfg.ActivationProfit}";
                default: //case StopLossPositionGuardTypes.No:
                    return "";
            }
        }

        private string StrTP()
        {
            if (Cfg.UseTakeProfitGuard)
                return $"{Cfg.TakeProfitDelta}";
            return "";
        }

        private string StrDSL()
        {
            var _dynGuard = Cfg.DynamicGuardDescription;
            if (_dynGuard.StopMode == DynamicGuardMode.NotUse) return "";
            return string.Format("{0}; {1}; {2}",
                _dynGuard.StopMode.DynamicStopToUser(),
                MacroSubstServer.ToShort(_dynGuard.StopGuardLongExpression),
                MacroSubstServer.ToShort(_dynGuard.StopGuardShortExpression));
        }

        private string StrDTP()
        {
            var _dynGuard = Cfg.DynamicGuardDescription;
            if (_dynGuard.TargetMode == DynamicGuardMode.NotUse) return "";
            return string.Format("{0}; {1}; {2}",
                _dynGuard.TargetMode.DynamicTargetToUser(),
                MacroSubstServer.ToShort(_dynGuard.TargetGuardLongExpression),
                MacroSubstServer.ToShort(_dynGuard.TargetGuardShortExpression));
        }

        private string StrRR()
        {
            if (Cfg.StoplossRestriction_MaxBarsToWaitForOppositeSignal <= 0) return "";
            return string.Format("{0}|{1}", Cfg.StoplossRestriction_MaxBarsToWaitForOppositeSignal,
                Cfg.StoplossRestriction_GoToFlatMustLiftRestriction ? "OppositeOrFlat" : "Opposite"
            );
        }
    }
}