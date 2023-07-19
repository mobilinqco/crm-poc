using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace ACRM.mobile.ViewModels
{
    public delegate Task<bool> MapUIEvent(CancellationToken cancellationToken);

    public class GeoSearchPageViewModel : NavigationBarBaseViewModel
    {
        private readonly IGeoSearchService _geoSearchService;
        public MapUIEvent RefreshMapUI;
        public MapUIEvent RefreshMapData;
        DateTime lastDataPrepared = DateTime.Now;
        
        public ICommand EditLocationCommand => new Command(async () => await EditLocationCommandHandler());
        public ICommand SaveLocationCommand => new Command(async () => await SaveLocationCommandHandler());
        public ICommand SetCurrentLocationCommand => new Command(async () => await SetCurrentLocationCommandHandler());
        public ICommand RequestModeButtonCommand => new Command(async () => await OnRequestModeButton());

        public ICommand SetMapToFocusCommand => new Command(async () => await RefreshMapUIAsync(_cancellationTokenSource.Token));
        public ICommand FilterItemTappedCommand => new Command<ListUIItem>(async item => await FilterItemTappedCommandHandler(item));
        public bool UIRefreshInProgress { get; set; } = false;
        public bool DataRefreshInProgress { get; set; } = false;

        private async Task FilterItemTappedCommandHandler(ListUIItem item)
        {
            item.Selected = !item.Selected;
            new Action(async () => await RefreshMapDataAsync(_cancellationTokenSource.Token))();
        }

        private async Task EditLocationCommandHandler()
        {
            IsAddressEditMode = true;
            AddressEdit = Address;
        }
        private async Task SaveLocationCommandHandler()
        {
            IsAddressEditMode = false;
            if (!string.IsNullOrEmpty(AddressEdit))
            {
                var position = await GetPoitionForAddress(AddressEdit);
                if(position!=null)
                {
                    await LoadPosionInMap(position.Value);
                }
            }
        }

        private async Task OnRequestModeButton()
        {
            if (!OnlineMode)
            {
                OnlineMode = true;
            }
            else
            {
                OnlineMode = false;
            }

            new Action(async () => await RefreshMapDataAsync(_cancellationTokenSource.Token))();
        }

        private async Task SetCurrentLocationCommandHandler()
        {
            var position = await LoadDeviceLoaction();
            if (position.HasValue)
            {
                var address = await GetAddressForPoition(position.Value);
                AddressEdit = address;
            }
        }

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

        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                RaisePropertyChanged(() => Address);
            }
        }

        private string _addressEdit;
        public string AddressEdit
        {
            get => _addressEdit;
            set
            {
                _addressEdit = value;
                RaisePropertyChanged(() => AddressEdit);
            }
        }
        private bool _isAddressEditMode;
        public bool IsAddressEditMode
        {
            get => _isAddressEditMode;
            set
            {
                _isAddressEditMode = value;
                RaisePropertyChanged(() => IsAddressEditMode);
            }
        }

        public string MapRagiusMessage
        {
            get
            {
                string distance;
                if(MapRadius > 1000)
                {
                    var distanceinKM = MapRadius / 1000;
                    distance = distanceinKM.ToString("0.##") + " km";
                }
                else
                {
                    distance = MapRadius.ToString() + " m";
                }

                return $"Search within a radius of {distance}";
            }

        }
        private double _mapRadius = 100;
        public double MapRadius
        {
            get => _mapRadius;
            set
            {
                _mapRadius = value;
                RaisePropertyChanged(() => MapRadius);
                RaisePropertyChanged(() => MapRagiusMessage);
                new Action(async () => await RefreshMapUIAsync(_cancellationTokenSource.Token))();
                new Action(async () => await RefreshMapDataAsync(_cancellationTokenSource.Token))();
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

        private string _noResultsText;
        public string NoResultsText
        {
            get => _noResultsText;
            set
            {
                _noResultsText = value;
                RaisePropertyChanged(() => NoResultsText);
            }
        }

        private bool _enableNoResultsText;
        public bool EnableNoResultsText
        {
            get => _enableNoResultsText;
            set
            {
                _enableNoResultsText = value;
                RaisePropertyChanged(() => EnableNoResultsText);
            }
        }

        private List<ListDisplayRow> _records;
        public List<ListDisplayRow> Records
        {
            get => _records;
            set
            {
                _records = value;
                RaisePropertyChanged(() => Records);
            }
        }

        public ICommand RecordSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await RecordSelected(evt));


        private async Task RecordSelected(Syncfusion.ListView.XForms.ItemTappedEventArgs evt)
        {
            if (evt.ItemData is ListDisplayRow selectedItem)
            {
                var userAction = new UserAction
                {
                    RecordId = selectedItem.RecordId,
                    InfoAreaUnitName = selectedItem.InfoAreaId,
                    ActionUnitName = "SHOWRECORD",
                    ActionType = UserActionType.ShowRecord
                };
                IUserActionSchuttle userActionSchuttle = AppContainer.Resolve<IUserActionSchuttle>();
                await userActionSchuttle.Carry(userAction, selectedItem.RecordId, _cancellationTokenSource.Token);
            }
        }


        public GeoSearchPageViewModel(IGeoSearchService geoSearchService)
        {
            _geoSearchService = geoSearchService;
            IsBackButtonVisible = true;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData is UserAction selectedUserAction)
            {
                PageTitle = selectedUserAction.ActionDisplayName;
                _geoSearchService.SetSourceAction(selectedUserAction);
                await _geoSearchService.PrepareContentAsync(_cancellationTokenSource.Token);
                await LoadFilerItems();
                await LoadStartMapLoaction();
            }
            await base.InitializeAsync(navigationData);
        }

        private async Task<bool> PrepareAndLoadMapData(CancellationToken token)
        {
            IsLoading = true;
            DateTime currentRequest = DateTime.Now;
            var mapParmeters = await getMapParameters(token);
            _geoSearchService.SetAdditionalParams(mapParmeters);
            List<MapEntry> mapEntries = await _geoSearchService.GetMapEntries(FilterItems, OnlineMode ,token);
            if (lastDataPrepared > currentRequest)
            {
                IsLoading = false;
                return false;
            }
            lastDataPrepared = currentRequest;
            Locations.Clear();
            Locations.Add(SelectedPin);
            if (mapEntries?.Count > 0)
            {
                var listRecords = new List<ListDisplayRow>();
                foreach (var mapItem in mapEntries)
                {
                    listRecords.Add(mapItem?.DisplayRow);
                    if (mapItem.Latitude != 0 || mapItem.Longitude != 0)
                    {
                        var position = new Position(mapItem.Latitude, mapItem.Longitude);
                        Pin marker = new Pin()
                        {
                            Address = mapItem.Address,
                            Label = mapItem.Label,
                            Position = position,
                            IsVisible = true,
                            Type = PinType.Place,
                        };
                        Locations.Add(marker);
                    }
                }
                Records = listRecords;
                EnableNoResultsText = false;
            }
            else
            {
                EnableNoResultsText = true;
                NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                    LocalizationKeys.KeyErrorsNoResults);
            }
            
            IsLoading = false;
            return await RefreshMapData(token);
        }

        private async Task<Dictionary<string, string>> getMapParameters(CancellationToken token)
        {
            var region = MapSpan.FromCenterAndRadius(MapPosition, Distance.FromMeters(MapRadius));
            var center = region.Center;
            var halfheightDegrees = region.LatitudeDegrees / 2;
            var halfwidthDegrees = region.LongitudeDegrees / 2;

            var left = center.Longitude - halfwidthDegrees;
            var right = center.Longitude + halfwidthDegrees;
            var top = center.Latitude + halfheightDegrees;
            var bottom = center.Latitude - halfheightDegrees;

            if (left < -180) left = 180 + (180 + left);
            if (right > 180) right = (right - 180) - 180;

            
            var maxLongitude = top;
            var minLongitude = bottom;

            var minLatitude = left;
            var maxLatitude = right;

            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add("GPSXmax", maxLatitude.ToString());
            additionalParams.Add("GPSXmin", minLatitude.ToString());
            additionalParams.Add("GPSYmax", maxLongitude.ToString());
            additionalParams.Add("GPSYmin", minLongitude.ToString());
            return additionalParams;
        }

        private async Task LoadStartMapLoaction()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location!=null)
            {
                var position = await LoadDeviceLoaction();
                await LoadPosionInMap(position, false);
            }
            
        }

        private async Task<Position?> LoadDeviceLoaction()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                return new Position(location.Latitude, location.Longitude);
            }

            return null;
        }


        private async Task LoadPosionInMap(Position? position, bool withDelay = true)
        {
            if (position.HasValue)
            {
                var address = await GetAddressForPoition(position.Value);

                Address = address;
                Pin marker = new Pin()
                {
                    Address = address,
                    Label = "Current Location",
                    Position = position.Value,
                    IsVisible = true,
                    Type = PinType.Generic,

                };
                Locations.Add(marker);
                MapPosition = position.Value;
                SelectedPin = marker;
            }
            await RefreshMapUIAsync(_cancellationTokenSource.Token, withDelay);
            await RefreshMapDataAsync(_cancellationTokenSource.Token, withDelay);
        }

        async Task<bool> RefreshMapUIAsync(CancellationToken cancellationToken, bool withDelay = true)
        {
            bool result = false;
            if (RefreshMapUI != null && !UIRefreshInProgress)
            {
                UIRefreshInProgress = true;
                var timeout = TimeSpan.FromSeconds(2);
                if(withDelay)
                {
                    result = await CancelAfterAsync<bool>(ct => RefreshMapUI(ct), timeout,
                    cancellationToken);
                }
                else
                {
                    result = await RefreshMapUI(cancellationToken);
                }
                
                UIRefreshInProgress = false;
            }
            return result;
        }
        async Task<bool> RefreshMapDataAsync(CancellationToken cancellationToken, bool withDelay = true)
        {
            bool result = false;
            if (RefreshMapData != null && !DataRefreshInProgress)
            {
                DataRefreshInProgress = true;
                var timeout = TimeSpan.FromSeconds(2);
                if (withDelay)
                {
                    result = await CancelAfterAsync<bool>(ct => PrepareAndLoadMapData(ct), timeout,
                    cancellationToken);
                }
                else
                {
                    result = await PrepareAndLoadMapData(cancellationToken);
                }
                DataRefreshInProgress = false;
            }
            return result;
        }

        async Task<TResult> CancelAfterAsync<TResult>(
    Func<CancellationToken, Task<TResult>> startTask,
    TimeSpan timeout, CancellationToken cancellationToken)
        {
            using (var timeoutCancellation = new CancellationTokenSource())
            using (var combinedCancellation = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token))
            {
                var originalTask = startTask(combinedCancellation.Token);
                var delayTask = Task.Delay(timeout, timeoutCancellation.Token);
                var completedTask = await Task.WhenAny(originalTask, delayTask);
                // Cancel timeout to stop either task:
                // - Either the original task completed, so we need to cancel the delay task.
                // - Or the timeout expired, so we need to cancel the original task.
                // Canceling will not affect a task, that is already completed.
                timeoutCancellation.Cancel();
                if (completedTask == originalTask)
                {
                    // original task completed
                    return await originalTask;
                }
                else
                {
                    return await originalTask;
                }
            }
        }

        private async Task<string> GetAddressForPoition(Position position)
        {

            try
            {
                Geocoder geocoder = new Geocoder();
                var address = (await geocoder.GetAddressesForPositionAsync(position)).FirstOrDefault();
                return address;
            }
            catch (Exception ex)
            {
                _logService.LogWarning($"Unable to load address for Latitude : {position.Latitude}, Longitude : {position.Longitude}, error :{ex.Message}");
            }

            return string.Empty;
        }

        private async Task<Position?> GetPoitionForAddress(string address)
        {
            try
            {
                Geocoder geocoder = new Geocoder();
                var position = (await geocoder.GetPositionsForAddressAsync(address)).FirstOrDefault();
                return position;
            }
            catch (Exception ex)
            {
                _logService.LogWarning($"Unable to load possition for address {address}, error :{ex.Message}");
            }

            return null;
        }

        private async Task LoadFilerItems()
        {
            var items = _geoSearchService.FilterItems;
            if (items?.Count > 0)
            {
                FilterItems = new ObservableCollection<ListUIItem>(items);
            }
        }

        private string _requestModeButtonIconText = MaterialDesignIcons.CloudOutline;
        public string RequestModeButtonIconText
        {
            get => _requestModeButtonIconText;
            set
            {
                _requestModeButtonIconText = value;
                RaisePropertyChanged(() => RequestModeButtonIconText);
            }
        }

        private bool _onlineMode = false;
        public bool OnlineMode
        {
            get => _onlineMode;
            set
            {
                _onlineMode = value;
                if (value)
                {
                    RequestModeButtonIconText = MaterialDesignIcons.Cloud;
                }
                else
                {
                    RequestModeButtonIconText = MaterialDesignIcons.CloudOutline;
                }
                RaisePropertyChanged(() => OnlineMode);

            }
        }

        private string _locationText = "Location";
        public string LocationText
        {
            get => _locationText;
            private set
            {
                _locationText = value;
                RaisePropertyChanged(() => LocationText);
            }
        }

        private string _positionText = "Starting Position";
        public string PositionText
        {
            get => _positionText;
            private set
            {
                _positionText = value;
                RaisePropertyChanged(() => PositionText);
            }
        }

        private string _searchResultsText = "Search Results";
        public string SearchResultsText
        {
            get => _searchResultsText;
            private set
            {
                _searchResultsText = value;
                RaisePropertyChanged(() => SearchResultsText);
            }
        }
        private ObservableCollection<ListUIItem> _filterItems;
        public ObservableCollection<ListUIItem> FilterItems
        {
            get => _filterItems;
            set
            {
                _filterItems = value;
                RaisePropertyChanged(() => FilterItems);
            }
        }
    }
}
