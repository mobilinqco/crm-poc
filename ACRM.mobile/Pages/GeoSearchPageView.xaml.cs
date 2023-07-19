using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace ACRM.mobile.Pages
{
    public partial class GeoSearchPageView : ContentPage
    {
        private bool _eventRegistered = false;
        public GeoSearchPageView()
        {
            InitializeComponent();
            mapControl.BindingContextChanged += map_BindingContextChanged;
            mapControl.SelectedPinChanged += map_BindingContextChanged;
        }

        private void map_BindingContextChanged(object sender, EventArgs e)
        {
            if (BindingContext is GeoSearchPageViewModel model)
            {
                if (!_eventRegistered)
                {
                    model.RefreshMapUI += new MapUIEvent(RefreshMapUI);
                    model.RefreshMapData += new MapUIEvent(RefreshMapData);
                    _eventRegistered = true;
                }
            }
        }

        private async Task<bool> RefreshMapUI(CancellationToken cancellationToken)
        {
            SetUIData();
            return true;
        }

        private async Task<bool> RefreshMapData(CancellationToken cancellationToken)
        {
            SetData();
            return true;
        }

        private void BtnFocus_Clicked(object sender, EventArgs e)
        {
            if (BindingContext is GeoSearchPageViewModel model)
            {
                if (model.MapPosition != null)
                {
                    mapControl.MoveToRegion(MapSpan.FromCenterAndRadius(model.MapPosition, Distance.FromMeters(model.MapRadius)));
                }
            }
        }
        private void SetData()
        {

            if (BindingContext is GeoSearchPageViewModel model)
            {
                mapControl.Pins.Clear();
                foreach (var pin in model.Locations)
                {
                    mapControl.Pins.Add(pin);
                }
            }
        }

        private void SetUIData()
        {
            

            if (BindingContext is GeoSearchPageViewModel model)
            {
                if (model.MapPosition != null)
                {
                    mapControl.MoveToRegion(MapSpan.FromCenterAndRadius(model.MapPosition, Distance.FromMeters(model.MapRadius)));
                }
            }
        }
    }
}
