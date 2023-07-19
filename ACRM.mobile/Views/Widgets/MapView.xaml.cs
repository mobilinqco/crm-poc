using System;
using System.Collections.Generic;
using System.ComponentModel;
using ACRM.mobile.UIModels;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace ACRM.mobile.Views.Widgets
{
    public partial class MapView : ContentView
    {
        private bool _isMaximizable = true;
        public bool IsMaximizable
        {
            get => _isMaximizable;
            set
            {
                _isMaximizable = value;
                OnPropertyChanged("IsMaximizable");
            }
        }

        private bool _canOpenInNativMaps = false;
        public bool CanOpenInNativeMaps
        {
            get => _canOpenInNativMaps;
            set
            {
                _canOpenInNativMaps = value;
                OnPropertyChanged("CanOpenInNativeMaps");
            }
        }

        public MapView()
        {
            InitializeComponent();
            mapControl.BindingContextChanged += map_BindingContextChanged;
            btnFocus.Clicked += BtnFocus_Clicked;
            btnMaximize.Clicked += BtnMaximize_Clicked;
            btnMaximize.BindingContext = this;
            btnOpenNativeMaps.Clicked += BtnOpenNativeMaps_Clicked;
            btnOpenNativeMaps.BindingContext = this;
            mapControl.SelectedPinChanged += map_SelectedPinChanged;
        }

        private async void BtnOpenNativeMaps_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("lol");
            if (BindingContext is MapControlModel)
            {
                var model = this.BindingContext as MapControlModel;
                if (model.MapPosition != null)
                {
                    var positionStr = model.MapPosition.Latitude + "," + model.MapPosition.Longitude;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await Launcher.OpenAsync("http://maps.apple.com/?q=" + positionStr);
                    }
                    else if (Device.RuntimePlatform == Device.Android)
                    {
                        await Launcher.OpenAsync("geo:0,0?q=" + positionStr);
                    }
                }
            }
        }

        void map_SelectedPinChanged(object sender, SelectedPinChangedEventArgs e)
        {
            SetData();
        }

        private void BtnFocus_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MapControlModel)
            {
                var model = this.BindingContext as MapControlModel;
                if (model.MapPosition != null)
                {
                    mapControl.MoveToRegion(MapSpan.FromCenterAndRadius(model.MapPosition, Distance.FromMeters(5000)));
                }
            }
        }

        private async void BtnMaximize_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is MapControlModel)
            {
                var model = this.BindingContext as MapControlModel;
                var navigationController = AppContainer.Resolve<INavigationController>();
                await navigationController.DisplayPopupAsync<MapPageViewModel>(model.Data);
            }
        }


        private void map_BindingContextChanged(object sender, EventArgs e)
        {
            SetData();
        }

        private void SetData()
        {
            // This complete thing is a hack because the binding context is updated
            // before the location data is resolved so we need to trigger update
            // of the position and pins after the resolve is done. We achieve this
            // by setting the selected pin which will trigger an update of the data.

            if(mapControl.Pins.Count > 0)
            {
                return;
            }

            if (BindingContext is MapControlModel)
            {
                var model = this.BindingContext as MapControlModel;
                CanOpenInNativeMaps = model.MapPosition != null;
                foreach (var pin in model.Locations)
                {
                    mapControl.Pins.Add(pin);
                }

                if (model.MapPosition != null)
                {
                    mapControl.MoveToRegion(MapSpan.FromCenterAndRadius(model.MapPosition, Distance.FromMeters(5000)));
                }
            }
        }
    }
}
