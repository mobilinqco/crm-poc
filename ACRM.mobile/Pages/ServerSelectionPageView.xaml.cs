
using System;
using Xamarin.Essentials;

namespace ACRM.mobile.Pages
{
    public partial class ServerSelectionPageView : Rg.Plugins.Popup.Pages.PopupPage
    {
        public ServerSelectionPageView()
        {
            InitializeComponent();
            SetDynamicDimensions();
        }

        private void SetDynamicDimensions()
        {
            double heightFactor = 0.4;
            double widthFactor = 0.6;

            if (DeviceDisplay.MainDisplayInfo.Height < 1500)
            {
                heightFactor = 0.6;
                widthFactor = 0.8;
            }

            SearchResultsList.WidthRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density * widthFactor));
            SearchResultsList.HeightRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * heightFactor));
        }
    }
}
