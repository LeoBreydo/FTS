using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.Customization;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace Configurator
{
    public static class GridControlEx
    {
        public static void CommonInit(this GridView gridView, bool multiSelect, bool readOnly)
        {
            gridView.OptionsBehavior.AllowAddRows = DefaultBoolean.False;
            gridView.OptionsBehavior.AllowDeleteRows = DefaultBoolean.False;
            gridView.OptionsBehavior.AutoExpandAllGroups = true;
            gridView.OptionsBehavior.Editable = !readOnly;
            gridView.OptionsBehavior.ReadOnly = readOnly;

            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsView.ShowAutoFilterRow = false;
            gridView.OptionsView.ColumnAutoWidth = false;
            gridView.OptionsView.ShowIndicator = false;
            gridView.OptionsView.ShowGroupedColumns = false;
            gridView.OptionsView.ShowFilterPanelMode = ShowFilterPanelMode.Never;//.ShowAlways;

            gridView.OptionsSelection.MultiSelect = multiSelect;
            gridView.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;

            DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = "Office 2010 Silver";

            // toggle customization form by Ctrl-1; toggle Foother visibility by Ctrl-2

            //gridView.KeyDown += gridView_KeyDown;
            //gridView.ShowCustomizationForm += gridView_ShowCustomizationForm;
            gridView.MouseDown += gridView_MouseDown_ToggleCheckBoxEditor;
        }
        static void gridView_MouseDown_ToggleCheckBoxEditor(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Control) != Keys.Control)
            {
                var view = (GridView)sender;
                GridHitInfo hi = view.CalcHitInfo(e.Location);
                if (hi.InRowCell)
                {
                    if (hi.Column.RealColumnEdit.GetType() == typeof(RepositoryItemCheckEdit))
                    {
                        view.FocusedRowHandle = hi.RowHandle;
                        view.FocusedColumn = hi.Column;
                        view.ShowEditor();
                        var edit = (view.ActiveEditor as CheckEdit);
                        if (edit == null) return;
                        edit.Toggle();
                        // ReSharper disable once PossibleNullReferenceException
                        (e as DXMouseEventArgs).Handled = true;
                    }

                }
            }
        }

        public static void SetGridFont(this GridView Gv, float fontSize)
        {
            Gv.BeginUpdate();
            Gv.Appearance.HeaderPanel.Font = new Font(Gv.Appearance.HeaderPanel.Font.Name, fontSize);
            Gv.Appearance.Row.Font = new Font(Gv.Appearance.Row.Name, fontSize);
            Gv.Appearance.GroupRow.Font = new Font(Gv.Appearance.GroupRow.Font.Name, fontSize);
            Gv.EndUpdate();
        }
        #region GetSelection/SetSelection/FocusToItem using the bound objects
        public static List<T> GetSelectedItems<T>(this GridView gridView)
        {
            int[] selRows = gridView.GetSelectedRows().Where(gridView.IsValidRowHandle).ToArray();
            if (selRows.Length == 0)
            {
                int focusedRow = gridView.GetFocusedDataSourceRowIndex();
                if (gridView.IsValidRowHandle(focusedRow))
                    selRows = new[] { focusedRow };
            }

            var result = new List<T>();
            foreach (int h in selRows)
                GetItemsForGroupRowHandle(gridView, h, result);
            return result;
        }

        public static void GetItemsForGroupRowHandle<T>(this GridView gridView, int h, List<T> listToPopulate)
        {
            if (!gridView.IsValidRowHandle(h)) return;
            if (!gridView.IsGroupRow(h))
            {
                var data = gridView.GetRow(h);
                if (!(data is T)) return;
                var castedData = (T)data;
                if (!listToPopulate.Contains(castedData))
                    listToPopulate.Add(castedData);
            }
            else
            {
                for (int i = 0; i < gridView.GetChildRowCount(h); ++i)
                {
                    GetItemsForGroupRowHandle(gridView, gridView.GetChildRowHandle(h, i), listToPopulate);
                }
            }

        }
        public static T GetSingleSelectedItem<T>(this GridView gridView)
            where T : class
        {
            int[] selRows = gridView.GetSelectedRows().Where(rowHandle => rowHandle != GridControl.InvalidRowHandle).ToArray();

            int selRowHandle;
            switch (selRows.Length)
            {
                case 1:
                    selRowHandle = selRows[0];
                    break;

                default:
                    return default(T);

                case 0:
                    selRowHandle = gridView.GetFocusedDataSourceRowIndex();
                    if (selRowHandle == GridControl.InvalidRowHandle)
                        return default(T);
                    break;
            }
            return gridView.GetRow(selRowHandle) as T;
        }

        public static void SelectItem<T>(this GridView gridView, T item)
        {
            gridView.ClearSelection();
            int rowHandle = gridView.FindRow(item);
            if (rowHandle == GridControl.InvalidRowHandle) return;
            gridView.SetRowExpanded(rowHandle);
            gridView.SelectRow(rowHandle);
            gridView.FocusedRowHandle = rowHandle;
        }

        public static void SelectItems<T>(this GridView gridView, IEnumerable<T> boundItemsToSelect)
        {
            gridView.ClearSelection();

            bool first = true;
            foreach (T item in boundItemsToSelect)
            {
                int rowHandle = gridView.FindRow(item);
                if (rowHandle == GridControl.InvalidRowHandle)
                    continue;

                gridView.SetRowExpanded(rowHandle);
                gridView.SelectRow(rowHandle);
                if (first)
                {
                    first = false;
                    gridView.FocusedRowHandle = rowHandle;
                }
            }
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
        #endregion

    }
}
