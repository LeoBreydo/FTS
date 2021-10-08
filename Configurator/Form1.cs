using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using Binosoft.TraderLib.Indicators;
using Configurator.Tools;
using Configurator.ViewModel;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace Configurator
{
    public partial class Form1 : Form
    {
        private Controller _controller;
        private RowType _rowType = RowType.Unknown;
        private Font rbFontNormal, rbFontBold;
        public Form1()
        {
            InitializeComponent();

            rbFontNormal = rbExchanges.Font = rbMarkets.Font = rbStrategies.Font;
            rbFontBold = new Font(rbFontNormal, FontStyle.Bold);
            cbFlatList.Font = rbFontNormal;
            cbOnlyWorkingSet.Font= rbFontNormal;


        }
        private void Form1_Load(object sender, EventArgs e)
        {
            _controller = new Controller(ConfigurationManager.AppSettings["StrategiesRootFolder"]);
            IndicatorsServer.Init(
                ConfigurationManager.AppSettings["IndicatorsFolder"],
                ConfigurationManager.AppSettings["UserIndicatorsFolder"]
            );
            
            _controller.OnSchedulerTimeStepChanged += _controller_OnSchedulerTimeStepChanged;
            InitGridView();
            ListOfCurrencies.OnNewCurrencyAdded += ListOfCurrencies_OnNewCurrencyAdded;

            gridControl3.DataSource = _controller.ScheduleItemsBList;


        }

        private void _controller_OnSchedulerTimeStepChanged()
        {
            gridView3.RefreshData();
        }

        private void ListOfCurrencies_OnNewCurrencyAdded()
        {
            InvokeAction(InitPropertyGridObject);
        }

        private void InvokeAction(Action action)
        {
            BeginInvoke(action);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (IndicatorsServer.HasLastLoadErros)
                MessageBox.Show(IndicatorsServer.GetLastLoadErrorsReport(), "Warning");

            rbStrategies.Checked = true;
        }
        private void gbPropGridTitle_TextChanged(object sender, EventArgs e)
        {
            groupBox1.Text = gbPropGridTitle.Text;
        }

        private void InitGridView()
        {
            //gridControl1.DataSource = new List<StrategyRow>(); // formal assignment to setup the grid
            gridView1.CommonInit(true, false);
            gridView1.SetGridFont(11);
            new DxForcePropagateCheckBoxValuesToDataSource(gridView1);
            gridView1.ShowingEditor += GvStrategies_ShowingEditor;
            gridView1.SelectionChanged += GvStrategies_SelectionChanged;
            gridView1.FocusedRowChanged += GvStrategies_FocusedRowChanged;
            gridView1.CustomDrawCell += GridView1_CustomDrawCell;
            gridView1.DoubleClick += GridView1_DoubleClick;

            gridView1.AsyncCompleted += GridView1_AsyncCompleted;
            gridView1.RowCountChanged += GridView1_RowCountChanged;
            gridView1.DataSourceChanged += GridView1_DataSourceChanged;


            gridView2.CommonInit(true, true);
            gridView2.OptionsCustomization.AllowSort = false;
            gridView2.OptionsCustomization.AllowColumnMoving = false;
            gridView2.OptionsCustomization.AllowGroup = false;
            gridView2.OptionsCustomization.AllowFilter = false;
            gridView2.OptionsCustomization.AllowQuickHideColumns = false;
            gridView2.OptionsCustomization.AllowRowSizing = false;

            gridView2.SetGridFont(11);
            gridView2.OptionsView.ColumnAutoWidth = true;
            gridView2.OptionsBehavior.Editable = true; // allow to get focus to copy text but read only!


            gridView3.CommonInit(true, true);
            gridView3.OptionsCustomization.AllowSort = true;
            gridView3.OptionsCustomization.AllowColumnMoving = false;
            gridView3.OptionsCustomization.AllowGroup = false;
            gridView3.OptionsCustomization.AllowFilter = false;
            gridView3.OptionsCustomization.AllowQuickHideColumns = false;
            gridView3.OptionsCustomization.AllowRowSizing = false;

            gridView3.SetGridFont(11);
            gridControl3.DataSource = new List<ScheduledIntervalDescription>();
            gridView3.Columns[nameof(ScheduledIntervalDescription.Id)].Visible = false;

            gridView3.Columns[nameof(ScheduledIntervalDescription.StartUtc)].Visible = false;
            gridView3.Columns[nameof(ScheduledIntervalDescription.StartUtc)].SortIndex = 0;

            gridView3.Columns[nameof(ScheduledIntervalDescription.Info)].Width = 100;
            const int TZ_WIDTH = 175;
            const int TIME_WIDTH = 125;
            gridView3.Columns[nameof(ScheduledIntervalDescription.EnterTimeZoneName)].Width = TZ_WIDTH;
            gridView3.Columns[nameof(ScheduledIntervalDescription.ExitTimeZoneName)].Width = TZ_WIDTH;

            gridView3.Columns[nameof(ScheduledIntervalDescription.SoftStopTime)].Width = TIME_WIDTH;
            gridView3.Columns[nameof(ScheduledIntervalDescription.HardStopTime)].Width = TIME_WIDTH;
            gridView3.Columns[nameof(ScheduledIntervalDescription.NoRestrictionTime)].Width = TIME_WIDTH;

            gridView3.Columns[nameof(ScheduledIntervalDescription.SoftStopUtc)].Width = TIME_WIDTH;
            gridView3.Columns[nameof(ScheduledIntervalDescription.HardStopUtc)].Width = TIME_WIDTH;
            gridView3.Columns[nameof(ScheduledIntervalDescription.NoRestrictionUtc)].Width = TIME_WIDTH;
            gridView3.Columns[nameof(ScheduledIntervalDescription.StartUtc)].Width = TIME_WIDTH;

            gridView3.CustomDrawCell += GridView3_CustomDrawCell;
            gridView3.DoubleClick += GridView3_DoubleClick;
            gridView3.SelectionChanged += GvSchedule_SelectionChanged;
            gridView3.FocusedRowChanged += GvSchedule_FocusedRowChanged;
        }

        private void GridView1_DoubleClick(object sender, EventArgs e)
        {
            var pt = gridControl1.PointToClient(MousePosition);
            GridHitInfo hitInfo = gridView1.CalcHitInfo(pt);
            if (!hitInfo.InRowCell) return;
            if (gridView1.GetSingleSelectedItem<IRow>() is StrategyRow &&
                gridView1.FocusedColumn.FieldName ==nameof(StrategyRow.StrategyName))
                OnRenameStrategy();
        }

        private void GvSchedule_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            EnableScheduleCmds();
        }

        private void GvSchedule_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            EnableScheduleCmds();
        }

        private void GridView3_DoubleClick(object sender, EventArgs e)
        {
            var pt = gridControl3.PointToClient(MousePosition);
            GridHitInfo hitInfo = gridView3.CalcHitInfo(pt);
            if (hitInfo.InRow) 
                OnEditScheduleRec();
        }

        private void GridView3_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (!(gridView3.GetRow(e.RowHandle) is ScheduledIntervalDescription sid)) return;
            if (e.CellValue == null) return;

            switch (sid.GetState(DateTime.UtcNow))
            {
                case ScheduledIntervalState.Finished:
                    e.Appearance.ForeColor = Color.DimGray;
                    break;
                case ScheduledIntervalState.SoftStopping:
                case ScheduledIntervalState.Stopped:
                    e.Appearance.BackColor = Color.AntiqueWhite;
                    break;

            }
            switch (e.Column.FieldName)
            {
                case nameof(ScheduledIntervalDescription.SoftStopTime):
                case nameof(ScheduledIntervalDescription.SoftStopUtc):
                    var val = (DateTime?) e.CellValue;
                    e.DisplayText = val == null ? "" : val.Value.ToString(UserSettings.DateTimeFormat);
                    break;

                case nameof(ScheduledIntervalDescription.HardStopTime):
                case nameof(ScheduledIntervalDescription.NoRestrictionTime):
                case nameof(ScheduledIntervalDescription.HardStopUtc):
                case nameof(ScheduledIntervalDescription.NoRestrictionUtc):
                case nameof(ScheduledIntervalDescription.StartUtc):
                    e.DisplayText = ((DateTime) e.CellValue).ToString(UserSettings.DateTimeFormat);
                    break;
            }
            
        }

        private void GridView1_DataSourceChanged(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle != GridControl.InvalidRowHandle)
                gridView1.SelectRow(gridView1.FocusedRowHandle);
        }
        private void GridView1_AsyncCompleted(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle != GridControl.InvalidRowHandle)
                gridView1.SelectRow(gridView1.FocusedRowHandle);

        }
        private void GridView1_RowCountChanged(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle != GridControl.InvalidRowHandle)
                gridView1.SelectRow(gridView1.FocusedRowHandle);
        }

        private void GridView1_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (rbStrategies.Checked && gridView1.GetRow(e.RowHandle) is StrategyRow str)
            {
                if (!str.InWorkingSet)
                    e.Appearance.ForeColor = Color.DimGray;
            }
            else if (rbMarkets.Checked && gridView1.GetRow(e.RowHandle) is MarketRow mkt)
            {
                if (!mkt.IsInWorkingSet())
                    e.Appearance.ForeColor = Color.DimGray;
                else
                {
                    switch (e.Column.FieldName)
                    {
                        case nameof(MarketRow.BigPointValue):
                            if (mkt.BigPointValue <= 0)
                                HighlightErrorCell(e.Appearance);
                            break;
                        case nameof(MarketRow.MinMove):
                            if (mkt.MinMove <= 0)
                                HighlightErrorCell(e.Appearance);
                            break;
                        case nameof(MarketRow.SessionCriticalLoss):
                            if (mkt.SessionCriticalLoss != null && mkt.SessionCriticalLoss > 0)
                                HighlightErrorCell(e.Appearance);
                            break;
                        case nameof(MarketRow.MaxErrorsPerDay):
                            if (mkt.MaxErrorsPerDay != null && mkt.MaxErrorsPerDay < 0)
                                HighlightErrorCell(e.Appearance);
                            break;
                        case nameof(MarketRow.MaxNbrContracts):
                            if (mkt.MaxNbrContracts != null && mkt.MaxNbrContracts < 0)
                                HighlightErrorCell(e.Appearance);
                            break;
                        case nameof(MarketRow.SumOfStrategyContracts):
                            if (mkt.MaxNbrContracts != null && mkt.MaxNbrContracts < mkt.SumOfStrategyContracts)
                                HighlightErrorCell(e.Appearance);
                            break;
                    }
                }
            }
            else if (rbExchanges.Checked && gridView1.GetRow(e.RowHandle) is ExchangeRow exch)
            {
                if (!exch.IsInWorkingSet())
                    e.Appearance.ForeColor = Color.DimGray;
            }
        }

        private static readonly Color InvalidValueForeColor = Color.DarkRed;
        private static readonly Color InvalidValueBackColor = Color.FromArgb(255, 230, 235);
        private void HighlightErrorCell(AppearanceObject ap)
        {
            ap.ForeColor = InvalidValueForeColor;
            ap.BackColor = InvalidValueBackColor;
        }

        private void GvStrategies_ShowingEditor(object sender, CancelEventArgs e)
        {
            if (!IsColumnEditable(_rowType, gridView1.FocusedColumn.FieldName))
                e.Cancel = true;
        }

        private static bool IsColumnEditable(RowType rowType,string fieldName)
        {
            switch (rowType)
            {
                case RowType.Strategies:
                    switch (fieldName)
                    {
                        case nameof(StrategyRow.InWorkingSet):
                        case nameof(StrategyRow.NbrOfContracts):
                            return true;
                        default:
                            return false;
                    }
                case RowType.Markets:
                    switch (fieldName)
                    {
                        case nameof(MarketRow.BigPointValue):
                        case nameof(MarketRow.MinMove):
                        case nameof(MarketRow.SessionCriticalLoss):
                        case nameof(MarketRow.MaxErrorsPerDay):
                        case nameof(MarketRow.MaxNbrContracts):
                            return true;
                        default:
                            return false;
                    }
                case RowType.Exchanges:
                    switch (fieldName)
                    {
                        case nameof(ExchangeRow.MaxErrorsPerDay):
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }


        private void GvStrategies_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            OnSelectedEntityChanged();
        }
        private void GvStrategies_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            OnSelectedEntityChanged();
        }


        private long __counter_EntityChanged;
        private void OnSelectedEntityChanged()
        {
            var counterVal = ++__counter_EntityChanged;
            InvokeAction(() =>
            {
                if (counterVal == __counter_EntityChanged)
                {
                    InitPropertyGridObject();
                    InitAttributesGrid();
                    gridView3.ActiveFilterString =
                        string.Format("[{0}]={1}", nameof(ScheduledIntervalDescription.Id), _selRowId);
                }
            });
        }


        private List<IRow> _selectedRows;
        private int _selRowId = -100;
        private void InitPropertyGridObject()
        {
            _selRowId = -100;
            var selItems = gridView1
                .GetSelectedRows()
                .Select(GetRowInfo)
                .Where(item => item != null)
                .ToList();

            switch (selItems.Count)
            {
                case 0:
                    propertyGrid1.SelectedObject = null;
                    gbPropGridTitle.Text = "";
                    _selectedRows = null;

                    return;
                case 1:
                    propertyGrid1.SelectedObject = selItems[0].GetPropertyGridEditor(propertyGrid1);
                    gbPropGridTitle.Text = selItems[0].GetTitle();
                    _selectedRows = selItems;
                    _selRowId = selItems[0].GetId();
                    return;
            }
            // multiple items selected
            var types = selItems.Select(x => x.GetRowType()).Distinct().ToList();

            if (types.Count == 1)
            {
                _selectedRows = selItems;
                propertyGrid1.SelectedObjects = selItems.Select(x => x.GetPropertyGridEditor(propertyGrid1))
                    .Where(x => x != null).ToArray();
                gbPropGridTitle.Text = string.Format("{0} selected {1}", selItems.Count, types[0]);
                return;
            }

            if (types.Contains(_rowType))
            {
                _selectedRows = selItems
                    .Where(item => item.GetRowType() == _rowType).ToList();
                propertyGrid1.SelectedObjects = _selectedRows
                    .Select(x => x.GetPropertyGridEditor(propertyGrid1))
                    .Where(x => x != null).ToArray();
                gbPropGridTitle.Text = string.Format("{0} selected {1}", propertyGrid1.SelectedObjects.Length, _rowType);
                return;
            }

            propertyGrid1.SelectedObject = null;
            gbPropGridTitle.Text = "";
            _selectedRows = null;
        }

        private void InitAttributesGrid()
        {
            if (_selectedRows?.Count != 1)
                gridControl2.DataSource = new List<PropertyValue>();
            else
                gridControl2.DataSource = _selectedRows[0].GetAttributes()??new List<PropertyValue>();
        }
        private IRow GetRowInfo(int handle)
        {
            if (!gridView1.IsValidRowHandle(handle)) return null;

            if (handle >= 0) // strategy
            {
                switch (_rowType)
                {
                    case RowType.Strategies:
                        return gridView1.GetRow(handle) as StrategyRow;
                    case RowType.Markets:
                        return gridView1.GetRow(handle) as MarketRow;
                    case RowType.Exchanges:
                        return gridView1.GetRow(handle) as ExchangeRow;
                }
                return null;
            }

            return _controller.GetGroupRow(GetGroupPath(handle));
        }

        private List<string> GetGroupPath(int handle)
        {
            var ret = new List<string>();
            while (handle < 0 && gridView1.IsValidRowHandle(handle))
            {
                ret.Add(gridView1.GetGroupRowValue(handle).ToString());
                handle = gridView1.GetParentRowHandle(handle);
            }

            return ret;
        }


        private void rbStrategies_CheckedChanged(object sender, EventArgs e)
        {
            rbStrategies.Font = rbStrategies.Checked ? rbFontBold : rbFontNormal;

            if (rbStrategies.Checked)
                OnShowData(RowType.Strategies);
        }

        private void rbMarkets_CheckedChanged(object sender, EventArgs e)
        {
            rbMarkets.Font = rbMarkets.Checked ? rbFontBold : rbFontNormal;

            if (rbMarkets.Checked)
                OnShowData(RowType.Markets);
        }

        private void rbExchanges_CheckedChanged(object sender, EventArgs e)
        {
            rbExchanges.Font = rbExchanges.Checked ? rbFontBold : rbFontNormal;

            if (rbExchanges.Checked)
                OnShowData(RowType.Exchanges);
        }

        private void cbFlatList_CheckedChanged(object sender, EventArgs e)
        {
            cbFlatList.Font = cbFlatList.Checked ? rbFontBold : rbFontNormal;

            OnShowData();
        }

        private void cbOnlyWorkingSet_CheckedChanged(object sender, EventArgs e)
        {
            cbOnlyWorkingSet.Font = cbOnlyWorkingSet.Checked ? rbFontBold : rbFontNormal;
            OnShowData();
        }

        private void OnShowData(RowType rowType= RowType.Unknown)
        {
            bool asFlatList = cbFlatList.Checked;
            bool onlyWorkingSet = cbOnlyWorkingSet.Checked;

            if (rowType == RowType.Unknown)
            {
                if (_rowType== RowType.Unknown) return;
                rowType = _rowType;
            }

            
            RememberGridState();
            RememberSelection();

            if (rowType != _rowType)
            {
                _rowType = rowType;
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }

            bool restored;
            switch (_rowType)
            {
                case RowType.Strategies:
                    gridControl1.DataSource = new BindingList<StrategyRow>(_controller.GetStrategies(onlyWorkingSet));
                    restored=RestoreGridState();
                    if (asFlatList)
                    {
                        gridView1.Columns[nameof(StrategyRow.Market)].GroupIndex = -1;
                        gridView1.Columns[nameof(StrategyRow.Exchange)].GroupIndex = -1;
                        gridView1.Columns[nameof(ExchangeRow.Configuration)].GroupIndex = -1;

                        gridView1.Columns[nameof(StrategyRow.Exchange)].VisibleIndex = 0;
                        gridView1.Columns[nameof(StrategyRow.Market)].VisibleIndex = 1;
                    }
                    else
                    {
                        gridView1.Columns[nameof(ExchangeRow.Configuration)].GroupIndex = 0;
                        gridView1.Columns[nameof(StrategyRow.Exchange)].GroupIndex = 1;
                        gridView1.Columns[nameof(StrategyRow.Market)].GroupIndex = 2;
                    }

                    break;
                case RowType.Markets:
                    gridControl1.DataSource = new BindingList<MarketRow>(_controller.GetMarkets(onlyWorkingSet));
                    restored = RestoreGridState();
                    if (asFlatList)
                    {
                        gridView1.Columns[nameof(MarketRow.Exchange)].GroupIndex = -1;
                        gridView1.Columns[nameof(ExchangeRow.Configuration)].GroupIndex = -1;


                    }
                    else
                    {
                        gridView1.Columns[nameof(ExchangeRow.Configuration)].GroupIndex = 0;
                        gridView1.Columns[nameof(MarketRow.Exchange)].GroupIndex = 1;
                    }

                    break;
                case RowType.Exchanges:
                //case RowType.TCfg:
                    gridControl1.DataSource = new BindingList<ExchangeRow>(_controller.GetExchanges(onlyWorkingSet));
                    restored = RestoreGridState();
                    gridView1.Columns[nameof(ExchangeRow.Configuration)].GroupIndex = asFlatList ? -1 : 0;
                    break;
                default:
                    return;

            }

            gridView1.Columns[nameof(StrategyRow.Configuration)].Visible = false;
            if (!restored)
                gridView1.BestFitColumns();

            RestoreSelection();
            EnableScheduleCmds();
        }

        private readonly Dictionary<RowType, byte[]> _gridStates = new Dictionary<RowType, byte[]>();
        private void RememberGridState()
        {
            if (_rowType != RowType.Unknown)
                _gridStates[_rowType] = gridView1.GetGridState();
        }
        private bool RestoreGridState()
        {
            if (!_gridStates.TryGetValue(_rowType, out var data))
                return false;
                
            gridView1.RestoreGridState(data);
            return true;
        }

        private string[] keyPath;
        private void RememberSelection()
        {
            //keyPath
            if (_selectedRows == null || _selectedRows.Count == 0)
            {
                keyPath = null;
                return;
            }

            keyPath = _selectedRows[0].KeyPath();
        }

        private void RestoreSelection()
        {
            if (keyPath == null || keyPath.Length == 0) return;
            var visitor = new Visitor(keyPath, h => GetRowInfo(h)?.KeyPath());
            gridView1.Find(visitor.Match);
            if (visitor.bestRow != null)
                SelectItem(visitor.bestRow.Value);
        }

        private bool TrySelectItemById(int id)
        {
            if (id < 0) return false;
            if (id == _selRowId) return true;

            int? handle=gridView1.Find(h => GetRowInfo(h)?.GetId() == id);
            if (handle == null) return false;

            gridView1.ClearSelection();
            gridView1.SelectRow(handle.Value);
            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            OnSave();
        }

        private bool OnSave()
        {
            string error = _controller.SaveConfiguration();
            if (error == null) return true;
                
            MessageBox.Show(this, "Save operation failed:\r\n\r\n" + error, "Rejection");
            return false;
        }

        private void btnRefreshStrategies_Click(object sender, EventArgs e)
        {
            string error=_controller.RefreshStrategyDlls();
            gridView1.RefreshData();
            if (error != null)
                MessageBox.Show(this, "Refresh failed:\r\n\r\n" + error, "Rejection");
        }

        private void SelectItem(int rowHadle)
        {
            if (!gridView1.IsValidRowHandle(rowHadle)) return;

            gridControl1.Focus();
            gridView1.ClearSelection();
            gridView1.FocusedRowHandle = rowHadle;
            gridView1.SelectRow(rowHadle);
        }

        class Visitor
        {

            private readonly string[] _targetPath;
            private readonly Func<int, string[]> _decoder;

            public int? bestRow;


            public Visitor(string[] targetPath,Func<int, string[]> decoder)
            {
                _targetPath = targetPath ?? new string[0];
                _decoder = decoder;
            }

            public bool Match(int rowHandle)
            {
                var path = _decoder(rowHandle);
                if (path == null || path.Length == 0) return false;
                if (SequenceStartsWith(_targetPath, path)) // full match or parent
                {
                    bestRow = rowHandle;
                    if (path.Length == _targetPath.Length)
                        return true;
                }

                if (SequenceStartsWith(path, _targetPath)) // first child
                {
                    if (bestRow == null)
                        bestRow = rowHandle;
                    return true;
                }
                return false;
            }

            private static bool SequenceStartsWith(string[] seq, string[] subSeq)
            {
                if (seq.Length < subSeq.Length) return false;
                for(int i=0;i< subSeq.Length;++i)
                    if (seq[i] != subSeq[i])
                        return false;
                return true;
            }

        }

        private void tbtnAddScheduleRec_Click(object sender, EventArgs e)
        {
            OnAddScheduleRec();
        }
        private void tbtnCloneScheduleRec_Click(object sender, EventArgs e)
        {
            OnCloneScheduleRec();
        }
        private void tbtnEditScheduleRec_Click(object sender, EventArgs e)
        {
            OnEditScheduleRec();
        }
        private void tbtnDelScheduleRec_Click(object sender, EventArgs e)
        {
            OnDeleteScheduleRecords();
        }

        private void tbtnCleanObsoleteScheduleRecs_Click(object sender, EventArgs e)
        {
            OnCleanObsoleteScheduleRecs();
        }

        private void OnAddScheduleRec(ScheduledIntervalDescription sample=null)
        {
            if (sample == null)
            {
                int contextId = _selRowId >= 0 ? _selRowId : -1;
                sample = new ScheduledIntervalDescription {Id = contextId};
            }
            List<Tuple<int, string>> cfgItems = _controller.GetCfgIds();
            cfgItems.Insert(0, new Tuple<int, string>(-1, "Selected items"));

            ScheduleRecordEditor.AddRecords(this, AddScheduleRecord, cfgItems, sample); 
        }

        private bool CanCloneScheduleRec()
        {
            return gridView3.GetSingleSelectedItem<ScheduledIntervalDescription>() != null;
        }

        private void OnCloneScheduleRec()
        {
            OnAddScheduleRec(gridView3.GetSingleSelectedItem<ScheduledIntervalDescription>());
        }
        private string AddScheduleRecord(ScheduledIntervalDescription descr)
        {
            if (descr.Id < 0) // add to all selected items
            {
                return "not implemented yet"; // todo
            }
            else
            {
                var ret = _controller.AddScheduleRecord(descr);
                if (ret.Item2 != null)
                {
                    if (descr.Id >= 0)
                        if (TrySelectItemById(descr.Id))
                            gridView3.SelectItem(ret.Item2);
                }
                return ret.Item1;
            }

        }

        private bool CanEditScheduleRec()
        {
            return gridView3.GetSingleSelectedItem<ScheduledIntervalDescription>() != null;
        }
        private void OnEditScheduleRec()
        {
            var descr=gridView3.GetSingleSelectedItem<ScheduledIntervalDescription>();
            if (descr != null)
                ScheduleRecordEditor.EditRecord(this, editedDescr=> UpdateEditedScheduleRecord(descr, editedDescr), _controller.GetCfgIds(), descr);
        }

        private string UpdateEditedScheduleRecord(ScheduledIntervalDescription srcDescr, ScheduledIntervalDescription editedDescr)
        {
            var err = _controller.UpdateEditedScheduleRecord(srcDescr, editedDescr);
            if (err != null) return err;
            gridView3.InvalidateRows();
            return null;
        }

        private bool CanDeleteScheduleRecords()
        {
            return gridView3.GetSelectedItems<ScheduledIntervalDescription>().Count > 0;
        }
        private void OnDeleteScheduleRecords()
        {
            List<ScheduledIntervalDescription> selRecs = gridView3.GetSelectedItems<ScheduledIntervalDescription>();
            if (selRecs.Count == 0) return;
            string txt = selRecs.Count == 1
                ? "Will you delete selected Schedule Record?"
                : $"Will you delete {selRecs.Count} selected Schedule Records?";
            if (DialogResult.Yes != MessageBox.Show(this, txt, "Confirmation", MessageBoxButtons.YesNo))
                return;

            gridView3.BeginUpdate();
            _controller.DeleteScheduleRecords(selRecs);
            gridView3.EndUpdate();
        }

        private void EnableScheduleCmds()
        {
            tbtnCloneScheduleRec.Enabled = CanCloneScheduleRec();
            tbtnEditScheduleRec.Enabled = CanEditScheduleRec();
            tbtnDelScheduleRec.Enabled = CanDeleteScheduleRecords();
            tbtnCleanObsoleteScheduleRecs.Enabled = CanCleanObsoleteScheduleRecs();
        }

        private bool CanCleanObsoleteScheduleRecs()
        {
            var now = DateTime.UtcNow;
            return _controller.ScheduleItemsBList.Any(item => item.NoRestrictionTime <= now);
        }
        private void OnCleanObsoleteScheduleRecs()
        {
            var now = DateTime.UtcNow;
            var toDelete=_controller.ScheduleItemsBList.Where(item => item.NoRestrictionTime <= now).ToList();
            if (toDelete.Count == 0) return;

            gridView3.BeginUpdate();
            _controller.DeleteScheduleRecords(toDelete);
            gridView3.EndUpdate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_controller.IsConfigrationChanged()) return;
            var btn = MessageBox.Show(this, @"You are going to close Configurator. Will you save changes before exit?",
                "Question", MessageBoxButtons.YesNoCancel);
            switch (btn)
            {
                case DialogResult.Yes:
                    if (!OnSave())
                        e.Cancel = true;
                    break;
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        private void btnVerify_Click(object sender, EventArgs e)
        {
            OnVerify();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OnUpload();
        }
        private void GotoItem(IRow row)
        {
            if (row.GetRowType() == RowType.Markets)
                rbMarkets.Checked = true;

            if (TrySelectItemById(row.GetId())) return;

            cbFlatList.Checked = false;
            rbStrategies.Checked = true;
            TrySelectItemById(row.GetId());
        }

        private void OnRenameStrategy()
        {
            if (!(gridView1.GetSingleSelectedItem<IRow>() is StrategyRow str)) return;
            string newName=RenameDlg.RenameStrategy(this, str.StrategyName,
                _controller.GetStrategies(cbOnlyWorkingSet.Checked).Select(item => item.StrategyName).ToList());
            if (newName == null) return;

            str.StrategyName = newName;
            propertyGrid1.Refresh();
        }
        private void OnVerify()
        {
            string error = _controller.VerifyConfiguration(out IRow errorLocation, out _);
            if (error != null)
            {
                GotoItem(errorLocation);
                MessageBox.Show(this, error, "Verification error");
                return;
            }

            MessageBox.Show(this, "Verification succeeded", "Verification result");
        }

        private void OnUpload()
        {
            string error = _controller.VerifyConfiguration(out IRow errorLocation, out List<string> usedIndicatorDlls);
            if (error != null)
            {
                GotoItem(errorLocation);
                MessageBox.Show(this, error, "Verification error");
                return;
            }
            if (!OnSave()) return;
            //controller.PrepareZip() todo to extract CfgCopy containing the only wokring set, plus indicator and strategy dlls

        }

    }
}

