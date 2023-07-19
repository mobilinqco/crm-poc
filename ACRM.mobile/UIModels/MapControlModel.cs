using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace ACRM.mobile.UIModels
{
    public class MapControlModel : UIPanelWidget
    {
        private ObservableCollection<Pin> _Locations = new ObservableCollection<Pin>();
        public ObservableCollection<Pin> Locations
        {
            get => _Locations;
            set
            {
                _Locations = value;
                RaisePropertyChanged(() => Locations);
            }
        }

        private string _label;
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                RaisePropertyChanged(() => Label);
            }
        }
        private Position _mapPosition;
        public Position MapPosition
        {
            get => _mapPosition;
            set
            {
                _mapPosition = value;
                RaisePropertyChanged(() => MapPosition);
            }
        }
        private Pin _selectedPin;
        public Pin SelectedPin
        {
            get => _selectedPin;
            set
            {
                _selectedPin = value;
                RaisePropertyChanged(() => SelectedPin);
            }
        }

        public MapControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if(widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
            }

        }

        public override async ValueTask<bool> InitializeControl()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            string address = "";
            string title = "";
            Position? position = null;

            bool isAddressResolved = false;
            
            Label = Data.Label;

            if (Data.Fields.Count > 0)
            {
                // The convention for the data is that if more than 2 fields are
                // present in the Fields array then the first 2 should be numeric
                // and should contain latitude and longitude coordinantes.
                // Case 4 fields: longitude, latitude, title, address (address is optional)
                // Case 2 fields: title, address
                // other cases just have a look for the fields with a colspandata set.

                if (Data.Fields.Count >= 3)
                {
                    if (double.TryParse(Data.Fields[0].Data.StringData, NumberStyles.Number, CultureInfo.InvariantCulture, out double longitude) &&
                        double.TryParse(Data.Fields[1].Data.StringData, NumberStyles.Number, CultureInfo.InvariantCulture, out double latitude))
                    {
                        if (longitude != 0 || latitude != 0)
                        {
                            position = new Position(latitude, longitude);
                            isAddressResolved = true;
                        }
                    }
                    title = _localizationController.GetLocalizedValue(Data.Fields[2]);
                    if (Data.Fields.Count > 3)
                    {
                        address = _localizationController.GetLocalizedValue(Data.Fields[3]);
                    }
                }
                else
                {
                    ListDisplayField ldf = null;
                    foreach (ListDisplayField field in Data.Fields)
                    {
                        if (field.Data.ColspanData != null && field.Data.ColspanData.Count > 0)
                        {
                            ldf = field;
                        }
                    }

                    if (ldf is ListDisplayField ldField)
                    {
                        address = _localizationController.GetLocalizedValue(ldf);
                        title = _localizationController.GetLocalizedValue(Data.Fields[0]);
                    }
                }

                if (position == null && !string.IsNullOrEmpty(address))
                {
                    Geocoder geocoder = new Geocoder();
                    try
                    {
                        var location = (await geocoder.GetPositionsForAddressAsync(address)).FirstOrDefault();

                        if (location != null)
                        {
                            position = new Position(location.Latitude, location.Longitude);
                            isAddressResolved = true;
                        }
                    }
                    catch
                    {

                    }
                }
            }
            
            if (!isAddressResolved)
            {
                try
                {
                    Location location = await Geolocation.GetLocationAsync(request, _cancellationTokenSource.Token);
                    if (location != null)
                    {
                        position = new Position(location.Latitude, location.Longitude);
                    }
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                if (!string.IsNullOrWhiteSpace(address))
                {
                    title = address;
                }
                else
                {
                    title = "Not Defined";
                    address = " ";
                }
            }

            if (position is Position pos)
            {
                Pin marker = new Pin()
                {
                    Address = address,
                    Label = title,
                    Position = pos,
                    IsVisible = true,
                    Type = PinType.Place

                };
                Locations.Add(marker);
                MapPosition = pos;

                SelectedPin = marker;
            }

            return true;
        }
    }
}
