using System;
using Xamarin.Essentials;

namespace ACRM.mobile.Pages
{
    public partial class SettingsSelectionPageView : Rg.Plugins.Popup.Pages.PopupPage
    {
        public SettingsSelectionPageView()
        {
            InitializeComponent();
            SetDynamicDimensions();
        }

        private void SetDynamicDimensions()
        {
            double heightFactor = 0.3;
            double widthFactor = 0.6;

            if (DeviceDisplay.MainDisplayInfo.Height < 1500)
            {
                heightFactor = 0.5;
                widthFactor = 0.8;
            }

            FrameContainer.WidthRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density * widthFactor));
            FrameContainer.HeightRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * heightFactor));
        }
    }
}