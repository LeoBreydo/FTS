using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Threading;
using Binosoft.TraderLib.Indicators;
using Configurator.ViewModel;

namespace Configurator
{
    public partial class Form1 : Form
    {
        private Controller _controller;
        private RowType _rowType = RowType.Unknown;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //IndicatorsServer.Init(
            //    ConfigurationManager.AppSettings["IndicatorsFolder"],
            //    ConfigurationManager.AppSettings["UserIndicatorsFolder"]);

            _controller = new Controller(ConfigurationManager.AppSettings["StrategiesRootFolder"]);
            TradingConfiguration tcfg = TradingConfiguration.Restore(@"D:\Work_VS2019\FTS_Project\FTS\FTS\bin\Debug\net5.0-windows\cfg.xml");
            _controller.SetTradingConfiguration(tcfg);
            InitGridView();
            ListOfCurrencies.OnNewCurrencyAdded += ListOfCurrencies_OnNewCurrencyAdded;
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

            //SetAsFlatList(false);

            //if (gridView1.FocusedRowHandle != GridControl.InvalidRowHandle)
            //    gridView1.SelectRow(gridView1.FocusedRowHandle);



            rbStrategies.Checked = true;
            //new Thread(() =>
            //{
            //    Thread.Sleep(2000);
            //    InvokeAction(() =>
            //    {
            //        if (gridView1.FocusedRowHandle != GridControl.InvalidRowHandle)
            //            gridView1.SelectRow(gridView1.FocusedRowHandle);

            //    });
            //}).Start();

        }

        private void InitGridView()
        {
            //gridControl1.DataSource = new List<StrategyRow>(); // formal assignment to setup the grid
            gridView1.CommonInit(true, false);
            gridView1.SetGridFont(11);
            new DxForcePropagateCheckBoxValuesToDataSource(gridView1);
            //gridView1.BestFitColumns();
            gridView1.ShowingEditor += GvStrategies_ShowingEditor;
            gridView1.SelectionChanged += GvStrategies_SelectionChanged;
            gridView1.FocusedRowChanged += GvStrategies_FocusedRowChanged;
            gridView1.RowCellStyle += GvStrategies_RowCellStyle;

            gridView1.AsyncCompleted += GridView1_AsyncCompleted;
            gridView1.RowCountChanged += GridView1_RowCountChanged;
            gridView1.DataSourceChanged += GridView1_DataSourceChanged;
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
            //   BeginInvoke()
            if (gridView1.FocusedRowHandle != GridControl.InvalidRowHandle)
                gridView1.SelectRow(gridView1.FocusedRowHandle);
        }

        private void GvStrategies_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (gridView1.GetRow(e.RowHandle) is StrategyRow si)
            {
                if (!si.InWorkingSet)
                    e.Appearance.ForeColor = Color.DimGray;
            }
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



        private void Display1(string txt)
        {
            label2.Text = txt;
        }
        private void Display2(string txt)
        {
            label3.Text = txt;
        }

        private void GvStrategies_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            var parent = gridView1.GetParentRowHandle(gridView1.FocusedRowHandle);
            bool isExchange = parent == GridControl.InvalidRowHandle;
            var groupName = gridView1.GetGroupRowValue(e.FocusedRowHandle);
            Display1($"FocusedHandle={e.FocusedRowHandle}, groupName={groupName?.ToString()}, isExchange={isExchange}");
            OnFocusedEntityChanged();
        }
        private void GvStrategies_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            //var items=gvStrategies.GetSelectedItems<StrategyInfo>();
            int[] selRows = gridView1.GetSelectedRows().Where(gridView1.IsValidRowHandle).ToArray();
            Display2($"cnt={selRows.Length}; items:={string.Join(",",selRows)}");
            OnFocusedEntityChanged();
        }


        private void OnFocusedEntityChanged()
        {
            var counterVal = ++_counterShp;
            InvokeAction(() => ShowPropertiesForSelectedItems(counterVal));
        }

        private long _counterShp;
        private void ShowPropertiesForSelectedItems(long counterVal)
        {
            if (counterVal == _counterShp)
                InitPropertyGridObject();
        }

        private void InitPropertyGridObject()
        {
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
                    return;
                case 1:
                    propertyGrid1.SelectedObject = selItems[0].GetPropertyGridEditor(propertyGrid1);
                    gbPropGridTitle.Text = selItems[0].GetTitle();
                    return;
            }
            // multiple items selected
            var types = selItems.Select(x => x.GetRowType()).Distinct().ToList();

            if (types.Count == 1)
            {
                propertyGrid1.SelectedObjects = selItems.Select(x => x.GetPropertyGridEditor(propertyGrid1))
                    .Where(x => x != null).ToArray();
                gbPropGridTitle.Text = string.Format("{0} selected {1}", selItems.Count, types[0]);
                return;

            }
            propertyGrid1.SelectedObject = null;
            gbPropGridTitle.Text = "";
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

            //            var parentRh = gridView1.GetParentRowHandle(handle);
            //return _controller.GetGroupRow(gridView1.GetGroupRowValue(handle) as string,
            //    parentRh == GridControl.InvalidRowHandle
            //        ? null
            //        : gridView1.GetGroupRowValue(parentRh) as string
            //);
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
            if (rbStrategies.Checked)
                OnShowData(true);
        }

        private void rbMarkets_CheckedChanged(object sender, EventArgs e)
        {
            if (rbMarkets.Checked)
                OnShowData(true);
        }

        private void rbExchanges_CheckedChanged(object sender, EventArgs e)
        {
            if (rbExchanges.Checked)
                OnShowData(true);
            cbFlatList.Visible = !rbExchanges.Checked;
        }

        private void cbFlatList_CheckedChanged(object sender, EventArgs e)
        {
            OnShowData();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            OnShowData();
        }


        private void OnShowData(bool reinitColumns=false)
        {
            bool asFlatList = cbFlatList.Checked;
            bool onlyWorkingSet = cbOnlyWorkingSet.Checked; //todo!!!

            if (reinitColumns)
            {
                gridControl1.DataSource = null;
                gridView1.Columns.Clear();
            }

            if (rbExchanges.Checked)
            {
                _rowType = RowType.Exchanges;
                gridControl1.DataSource = new BindingList<ExchangeRow>(_controller.GetExchanges(onlyWorkingSet));
                gridView1.Columns[nameof(ExchangeRow.Configuration)].GroupIndex = 0;
            }
            else if (rbMarkets.Checked)
            {
                _rowType = RowType.Markets;
                gridControl1.DataSource = new BindingList<MarketRow>(_controller.GetMarkets(onlyWorkingSet));
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
            }
            else
            {
                _rowType = RowType.Strategies;
                gridControl1.DataSource = new BindingList<StrategyRow>(_controller.GetStrategies(onlyWorkingSet));
                if (asFlatList)
                {
                    gridView1.Columns[nameof(StrategyRow.MarketName)].GroupIndex = -1;
                    gridView1.Columns[nameof(StrategyRow.Exchange)].GroupIndex = -1;
                    gridView1.Columns[nameof(ExchangeRow.Configuration)].GroupIndex = -1;
                }
                else
                {
                    gridView1.Columns[nameof(ExchangeRow.Configuration)].GroupIndex = 0;
                    gridView1.Columns[nameof(StrategyRow.Exchange)].GroupIndex = 1;
                    gridView1.Columns[nameof(StrategyRow.MarketName)].GroupIndex = 2;
                }
            }

            gridView1.Columns[nameof(StrategyRow.Configuration)].Visible = false;
            gridView1.BestFitColumns();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _controller.Save();
        }
    }
}

