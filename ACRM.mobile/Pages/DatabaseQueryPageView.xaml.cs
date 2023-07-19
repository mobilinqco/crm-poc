using System;
using ACRM.mobile.CustomControls;

namespace ACRM.mobile.Pages
{
    public partial class DatabaseQueryPageView : Rg.Plugins.Popup.Pages.PopupPage
    {
        public DatabaseQueryPageView()
        {
            InitializeComponent();

            SendButton.Clicked += (sender, args) => OnSendButtonClicked(sender, args);
        }

        private void OnSendButtonClicked(object sender, EventArgs eventArgs)
        {
            SendButton.ExportDataGrid(DataGrid);
        }
    }
}
