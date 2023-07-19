using System;
using System.Collections.Generic;
using ACRM.mobile.Views.Widgets;
using Xamarin.Forms;

namespace ACRM.mobile.Pages
{
    public partial class MapPageView : Rg.Plugins.Popup.Pages.PopupPage
    {
        public MapPageView()
        {
            InitializeComponent();
            mapView.LayoutChanged += MapView_LayoutChanged;
        }

        private void MapView_LayoutChanged(object sender, EventArgs e)
        {
            MapView content = ((MapView)mapView.Content);
            content.IsMaximizable = false;
        }
    }
}
