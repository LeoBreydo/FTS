using System;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;

namespace Configurator
{
    public class DxForcePropagateCheckBoxValuesToDataSource
    {
        private readonly GridView _gridView;
        public DxForcePropagateCheckBoxValuesToDataSource(GridView gridView)
        {
            _gridView = gridView;
            gridView.ShownEditor += gridView_ShownEditor;
            gridView.HiddenEditor += gridView_HiddenEditor;
        }

        BaseEdit gridViewActiveEditor;
        void gridView_ShownEditor(object sender, EventArgs e)
        {
            if (_gridView.ActiveEditor is CheckEdit)
            {
                gridViewActiveEditor = _gridView.ActiveEditor;
                gridViewActiveEditor.EditValueChanged += ActiveEditor_EditValueChanged;
            }
        }
        void gridView_HiddenEditor(object sender, EventArgs e)
        {
            if (gridViewActiveEditor is CheckEdit)
                gridViewActiveEditor.EditValueChanged -= ActiveEditor_EditValueChanged;
        }
        void ActiveEditor_EditValueChanged(object sender, EventArgs e)
        {
            _gridView.PostEditor();
        }
    }
}
