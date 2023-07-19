using System;
using System.Collections.Generic;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Logging;
using ACRM.mobile.UIModels;
using ACRM.mobile.Utils;
using Xamarin.Forms;

namespace ACRM.mobile.Views.Widgets
{
    public partial class DiplayPanelView : ContentView
    {
        private readonly UIViewBuilder _viewBuilder;

        public DiplayPanelView()
        {
            InitializeComponent();
            panelGrid.BindingContextChanged += panel_BindingContextChanged;
            _viewBuilder = AppContainer.Resolve<UIViewBuilder>();

        }

        private void panel_BindingContextChanged(object sender, EventArgs e)
        {
            if (sender is Grid cellWrapper)
            {
                if (cellWrapper.BindingContext is PanelData data)
                {
                    _viewBuilder.GenerateDetailsContent(cellWrapper, data);
                }
            }
        }

    }
}
