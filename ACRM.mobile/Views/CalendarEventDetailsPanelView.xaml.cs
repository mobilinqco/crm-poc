using System;
using System.Collections.Generic;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using Xamarin.Forms;

namespace ACRM.mobile.Views
{
    public partial class CalendarEventDetailsPanelView : ContentView
    {
        private readonly CalendarEventDetailsPanelViewBuilder _viewBuilder;

        public CalendarEventDetailsPanelView()
        {
            InitializeComponent();
            _viewBuilder = AppContainer.Resolve<CalendarEventDetailsPanelViewBuilder>();
            PanelGrid.BindingContextChanged += panel_BindingContextChanged;
        }

        private void panel_BindingContextChanged(object sender, EventArgs e)
        {
            if (sender is Grid cellWrapper)
            {
                if (cellWrapper.BindingContext is PanelData panelData)
                {
                    _viewBuilder.GenerateDetailsContent(cellWrapper, panelData);
                }
            }
        }
    }
}
