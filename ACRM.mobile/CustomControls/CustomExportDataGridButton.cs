using System;
using Syncfusion.SfDataGrid.XForms;
using Syncfusion.XForms.Buttons;

namespace ACRM.mobile.CustomControls
{
    public class CustomExportDataGridButton : SfButton
    {
        public event EventHandler<ExportDataGridEventArgs> Export = (sender, args) => { };

        public void ExportDataGrid(SfDataGrid dataGrid)
        {
            Export(this, new ExportDataGridEventArgs(dataGrid));
        }
    }

    public class ExportDataGridEventArgs : EventArgs
    {
        public SfDataGrid DataGrid { get; private set; }

        public ExportDataGridEventArgs(SfDataGrid dataGrid)
        {
            DataGrid = dataGrid;
        }
    }
}
