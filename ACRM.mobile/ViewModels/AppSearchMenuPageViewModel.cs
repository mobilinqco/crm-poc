using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using AsyncAwaitBestPractices;
using Syncfusion.XForms.Buttons;
using Xamarin.Essentials;
using Xamarin.Forms;
using ACRM.mobile.Utils;
using ACRM.mobile.Services.Utils;
using Xamarin.CommunityToolkit.ObjectModel;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Localization;

namespace ACRM.mobile.ViewModels
{
    public class AppSearchMenuPageViewModel : BaseViewModel
    {
        DateTime LastSearchDateTime = DateTime.Now;
        private readonly ResetTimer timer;
        private enum AppSearchMenuSearchTypes
        {
            Global, History, Favourite
        }

        private readonly IAppSearchMenuService _contentService;
        private readonly IQuickSearchService _quickSearchService;

        public ICommand RecordSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await RecordSelected(evt));
        public ICommand InfoAreaSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await InfoAreaSelected(evt));
        public ICommand SearchCommand => new Command(async () => await PerformSearch(true));

        private bool _isSearchTypeVisible;
        public bool IsSearchTypeVisible
        {
            get => _isSearchTypeVisible;
            set
            {
                _isSearchTypeVisible = value;
                RaisePropertyChanged(() => IsSearchTypeVisible);
            }
        }

        private ObservableRangeCollection<ListDisplayRow> _searchResults;
        public ObservableRangeCollection<ListDisplayRow> SearchResults
        {
            get
            {
                return _searchResults;
            }
            set
            {
                _searchResults = value;
                RaisePropertyChanged(() => SearchResults);
            }
        }

        private int _segmentedControlIndex = 0;
        public int SegmentedControlIndex
        {
            get
            {
                return _segmentedControlIndex;
            }
            set
            {
                _segmentedControlIndex = value;
                RaisePropertyChanged(() => SegmentedControlIndex);
            }
        }

        private ObservableCollection<SfSegmentItem> _searchTypes;
        public ObservableCollection<SfSegmentItem> SearchTypes
        {
            get => _searchTypes;
            set
            {
                _searchTypes = value;
                RaisePropertyChanged(() => SearchTypes);
            }
        }

        private bool _infoAreaVisible;
        public bool InfoAreaVisible
        {
            get => _infoAreaVisible;
            set
            {
                _infoAreaVisible = value;
                RaisePropertyChanged(() => InfoAreaVisible);
            }
        }

        private List<UserAction> _infoAreaActions;
        public List<UserAction> InfoAreaActions
        {
            get => _infoAreaActions;
            set
            {
                _infoAreaActions = value;
                RaisePropertyChanged(() => InfoAreaActions);
            }
        }

        private string _globalSearchPlaceholderText;
        public string GlobalSearchPlaceholderText
        {
            get => _globalSearchPlaceholderText;
            set
            {
                _globalSearchPlaceholderText = value;
                RaisePropertyChanged(() => GlobalSearchPlaceholderText);
            }
        }

        private string _globalSearchText;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                _globalSearchText = value;
                RaisePropertyChanged(() => GlobalSearchText);
            }
        }

        private bool IsGlobalSearchEnabled
        {
            get => SegmentedControlIndex == 0 ? true: false;
        }

        private bool _globalSearchVisible;
        public bool GlobalSearchVisible
        {
            get => _globalSearchVisible;
            set
            {
                _globalSearchVisible = value;
                RaisePropertyChanged(() => GlobalSearchVisible);
            }
        }

        private bool _hasSearchResults = false;
        public bool HasSearchResults
        {
            get => _hasSearchResults;
            set
            {
                _hasSearchResults = value;
                RaisePropertyChanged(() => HasSearchResults);
            }
        }

        private int _widthRequest = 200;
        public int WidthRequest
        {
            get => _widthRequest;
            set
            {
                _widthRequest = value;
                RaisePropertyChanged(() => WidthRequest);
            }
        }

        private int _viewModesCounter = 3;
        public int ViewModesCounter
        {
            get => _viewModesCounter;
            set
            {
                _viewModesCounter = value;
                RaisePropertyChanged(() => ViewModesCounter);
            }
        }


        public AppSearchMenuPageViewModel(IAppSearchMenuService contentService, IQuickSearchService quickSearchService)
        {
            _contentService = contentService;
            _quickSearchService = quickSearchService;
            GlobalSearchPlaceholderText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, 
                LocalizationKeys.KeyBasicHeadlineGlobalSearch);
            GlobalSearchVisible = false;
            InfoAreaVisible = false;
            IsSearchTypeVisible = false;
            IsLoading = true;

            if (Device.RuntimePlatform == Device.UWP)
            {
                WidthRequest = Convert.ToInt32(Math.Round(Application.Current.MainPage.Width * 0.7));
            }
            else
            {
                WidthRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density * 0.7));
            }
            this.timer = new ResetTimer(this.PerformAsyncSearch);
            _contentService.DataReady += OnDataReady;
        }

        private void OnDataReady(object sender, EventArgs e)
        {
            UpdateBindings();
        }

        public override Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            if (navigationData is UserAction ua)
            {
                _contentService.SetSourceAction(ua);
                _quickSearchService.SetSourceAction(ua);
                _contentService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                {
                    _logService.LogError($"Unable to process configuration {ex.Message}");
                });

                _quickSearchService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                {
                    _logService.LogError($"Unable to process configuration {ex.Message}");
                });
            }
            IsLoading = false;
            return Task.CompletedTask;
        }

        private void UpdateBindings()
        {
            InfoAreaActions = _contentService.InfoAreaRelatedActions();
            InfoAreaVisible = InfoAreaActions.Count > 0;
            var searcheTypes = new ObservableCollection<SfSegmentItem>();

            if (_contentService.GlobalSearch() != null)
            {
                searcheTypes.Add(new SfSegmentItem
                {
                    Text = _contentService.GlobalSearch().ActionDisplayName,
                    StyleId = AppSearchMenuSearchTypes.Global.ToString()
                });
                GlobalSearchVisible = true;
            }
            if (_contentService.HistorySearch() != null)
            {
                searcheTypes.Add(new SfSegmentItem { Text = _contentService.HistorySearch().ActionDisplayName, StyleId = AppSearchMenuSearchTypes.History.ToString() });
            }
            if (_contentService.FavoriteSearch() != null)
            {
                searcheTypes.Add(new SfSegmentItem { Text = _contentService.FavoriteSearch().ActionDisplayName, StyleId = AppSearchMenuSearchTypes.Favourite.ToString() });
            }

            if (searcheTypes.Count > 1)
            {
                IsSearchTypeVisible = true;
                ViewModesCounter = searcheTypes.Count;
                //SegmentedControlIndex = 0;
            }
            SearchTypes = searcheTypes;
        }

        private async Task InfoAreaSelected(Syncfusion.ListView.XForms.ItemTappedEventArgs evt)
        {
            if (evt.ItemData is UserAction selectedUserAction)
            {
                await _navigationController.PopAllPopupAsync(selectedUserAction);
            }
        }

        private async Task PerformAsyncSearch()
        {
            DateTime dateTime = DateTime.Now;
            LastSearchDateTime = dateTime;
            SearchResults?.Clear();
            HasSearchResults = false;
            if (string.IsNullOrWhiteSpace(GlobalSearchText))
            {
                return;
            }
            else
            {
                var searchResults = await _quickSearchService.PerformQuickSearch(GlobalSearchText, _cancellationTokenSource.Token);

                if(searchResults?.Count>0 && LastSearchDateTime.Equals(dateTime))
                {
                    SearchResults = new ObservableRangeCollection<ListDisplayRow>(searchResults);
                    HasSearchResults = true;
                }
                
            }

        }

        private async Task PerformSearch(bool useDelay = false)
        {
            try
            {
                double delay = 0;
                if (useDelay)
                {
                    delay = CrmConstants.DefaultSearchDelayTime;
                }
                if (IsGlobalSearchEnabled)
                {
                    timer.StartOrReset(delay);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

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
                await _navigationController.PopAllPopupAsync(userAction);
            }
            
        }
    }
}