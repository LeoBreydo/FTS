using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace Configurator.Tools
{
#if notUsed
    public enum CustomColumnType
    {
        Decimal,
        Integer,
        Boolean,
        DateTime,
        String
    }

    public class CustomColumnInfo
    {
        public string FieldName;
        public string ToolTip;
        public string UnboundExpression;
        public string FormatString;
        public int VisibleIndex;
        public CustomColumnType CustomColumnType;

        #region save and restore
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(List<CustomColumnInfo>));
        public static List<CustomColumnInfo> Restore(string fileName)
        {
            try
            {
                if (!File.Exists(fileName)) return null;
                using (var myFileStream = new FileStream(fileName, FileMode.Open))
                {
                    return (List<CustomColumnInfo>)_serializer.Deserialize(myFileStream);
                }
            }
            catch
            {
                return null;
            }
        }
        public static string Save(List<CustomColumnInfo> infos, string fileName)
        {
            try
            {
                using (TextWriter writer = new StreamWriter(fileName))
                    _serializer.Serialize(writer, infos);
                return null;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        #endregion

    }
    public class GridViewState
    {
        public byte[] Layout;
        public List<CustomColumnInfo> CustomColumns;
        public int TopRowIndex = GridControl.InvalidRowHandle;
        public bool IsTopRowExpanded;
        public int SelectedRowHandle = GridControl.InvalidRowHandle;
        public int LeftCoord;
        public int SelectedColAbsIndex = -1;
    }
#endif
    public static class GridViewStateEx
    {
#if notUsed
        public static GridViewState GetGridViewState(this GridView gridView)
        {
            int row = GridControl.InvalidRowHandle;
            int[] selRows = gridView.GetSelectedRows();
            if (selRows.Length > 0)
                row = selRows[0];
            int col = gridView.FocusedColumn == null ? -1 : gridView.FocusedColumn.AbsoluteIndex;


            var topRowIndex = gridView.TopRowIndex;
            bool isTopRowExpanded = gridView.GetRowExpanded(topRowIndex);

            byte[] serializedLayout = GetGridState(gridView);
            List<CustomColumnInfo> customColumns = gridView.GetCustomColumns();

            return new GridViewState
            {
                Layout = serializedLayout,
                CustomColumns = customColumns,
                TopRowIndex = topRowIndex,
                IsTopRowExpanded = isTopRowExpanded,
                SelectedRowHandle = row,
                SelectedColAbsIndex = col,
                LeftCoord = gridView.LeftCoord
            };
        }
        public static void RestoreGridViewState(this GridView gridView, GridViewState state)
        {
            if (state == null) return;
            gridView.RestoreCustomColumns(state.CustomColumns);
            gridView.RestoreGridState(state.Layout);

            if (state.TopRowIndex != GridControl.InvalidRowHandle)
            {
                if (state.IsTopRowExpanded)
                    SetRowExpanded(gridView, state.TopRowIndex);
                gridView.TopRowIndex = state.TopRowIndex;
            }


            if (state.SelectedRowHandle != GridControl.InvalidRowHandle)
            {
                gridView.ClearSelection();
                try
                {
                    SetRowExpanded(gridView, state.SelectedRowHandle);

                    if (state.SelectedRowHandle != GridControl.InvalidRowHandle)
                    {
                        GridColumn colToSelect = state.SelectedColAbsIndex < 0 || state.SelectedColAbsIndex >= gridView.Columns.Count ? null : gridView.Columns[state.SelectedColAbsIndex];
                        if (colToSelect != null && gridView.OptionsSelection.MultiSelectMode == GridMultiSelectMode.CellSelect &&
                            state.SelectedColAbsIndex >= 0)
                        {
                            gridView.SelectCell(state.SelectedRowHandle, colToSelect);
                        }
                        else
                        {
                            gridView.SelectRow(state.SelectedRowHandle);
                        }

                        gridView.FocusedRowHandle = state.SelectedRowHandle;
                        if (colToSelect != null)
                            gridView.FocusedColumn = colToSelect;
                    }

                    gridView.MakeRowVisible(state.SelectedRowHandle, true);
                }
                catch
                {
                    // the restored row may be not exists via filter conditions or updates in the data
                }
            }
            gridView.LeftCoord = state.LeftCoord;
        }

        public static void SetRowExpanded(this GridView gridView, int rowHandle)
        {
            while (true)
            {
                rowHandle = gridView.GetParentRowHandle(rowHandle);
                if (rowHandle == GridControl.InvalidRowHandle) return;
                gridView.SetRowExpanded(rowHandle, true, false);
            }
        }

        public static List<CustomColumnInfo> GetCustomColumns(this GridView gridView)
        {
            return gridView.Columns.Where(IsCustomColumn).Select(col => new CustomColumnInfo
            {
                FieldName = col.Caption,
                CustomColumnType = ConvertType(col.UnboundType),
                ToolTip = col.ToolTip,
                UnboundExpression = col.UnboundExpression,
                FormatString = col.DisplayFormat.FormatString,
                VisibleIndex = col.VisibleIndex
            }).ToList();
        }

        public static void RestoreCustomColumns(this GridView gridView, List<CustomColumnInfo> columns)
        {
            if (columns == null || columns.Count == 0) return;
            foreach (var col in columns)
                AddCustomColumn(gridView, col.FieldName, col.UnboundExpression, col.CustomColumnType,
                    col.VisibleIndex, col.ToolTip, col.FormatString);
        }
        private static bool IsCustomColumn(GridColumn col)
        {
            return col.ShowUnboundExpressionMenu;
        }
        public static GridColumn AddCustomColumn(this GridView gridView, string fieldName, string expression, CustomColumnType type, int visibleIndex = -1,
            string toolTip = null, string formatString = null)
        {
            fieldName = SetUniqueFieldName(gridView, fieldName);
            var gridColumn = new GridColumn
            {
                FieldName = fieldName,
                Caption = fieldName,
                ToolTip = toolTip,
                //UnboundType = ConvertType(type),
                UnboundExpression = expression,
                ShowUnboundExpressionMenu = true,
                VisibleIndex = visibleIndex,
                Width = 85,
                Visible = true,
                ColumnEdit = new RepositoryItemTextEdit(),
            };
            gridColumn.OptionsColumn.AllowEdit = false;

            gridView.Columns.Add(gridColumn);
            gridColumn.UnboundType = ConvertType(type);
            SetColumnFormat(gridColumn, type, formatString);

            if (visibleIndex >= 0)
                gridColumn.VisibleIndex = visibleIndex;

            return gridColumn;
        }

        public static CustomColumnType ConvertType(UnboundColumnType type)
        {
            switch (type)
            {
                case UnboundColumnType.Decimal:
                    return CustomColumnType.Decimal;
                case UnboundColumnType.Integer:
                    return CustomColumnType.Integer;
                case UnboundColumnType.Boolean:
                    return CustomColumnType.Boolean;
                case UnboundColumnType.DateTime:
                    return CustomColumnType.DateTime;
                default:
                    //case UnboundColumnType.String:
                    return CustomColumnType.String;
            }
        }
#endif
        public static byte[] GetGridState(this GridView gridView)
        {
            using (var ms = new MemoryStream())
            {
                var opt = new OptionsLayoutGrid();
                opt.StoreAllOptions = true;
                opt.StoreFormatRules = false;
                gridView.SaveLayoutToStream(ms, opt);
                return ms.ToArray();
            }
        }

        public static void RestoreGridState(this GridView gridView, byte[] serialInfo)
        {
            var opt = new OptionsLayoutGrid();
            opt.Columns.AddNewColumns = true;
            opt.Columns.RemoveOldColumns = true;
            opt.StoreAllOptions = true;
            opt.StoreFormatRules = false;
            if (serialInfo != null)
                using (var ms = new MemoryStream(serialInfo))
                    gridView.RestoreLayoutFromStream(ms, opt);
        }

    }
}
