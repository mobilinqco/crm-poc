using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Messages;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using AsyncAwaitBestPractices;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class RecordSelectorPageViewModel: BaseViewModel
    {
        private readonly ISearchContentService _contentService;
        private readonly ResetTimer timer;

        public ICommand RecordSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await RecordSelected(evt));
        public ICommand SearchCommand => new Command(async () => await PerformSearch(true));
        public ICommand SwitchRequestModeCommand => new Command(async () => await SwitchRequestMode());
        public ICommand LoadMoreCommand => new Command<object>(LoadMoreItems, CanLoadMoreItems);

        public ICommand CloseCommand => new Command(async () => await OnClose());
        public ICommand ApplyCommand => new Command(async () => await OnApply());

        private SearchAndListContentData _searchAndListContentData = new SearchAndListContentData();
        public SearchAndListContentData SearchAndListContentData
        {
            get => _searchAndListContentData;

            set
            {
                _searchAndListContentData = value;
                RaisePropertyChanged(() => SearchAndListContentData);
            }
        }

        private string _recordSelectorTitle = string.Empty;
        public string RecordSelectorTitle
        {
            get => _recordSelectorTitle;
            set
            {
                _recordSelectorTitle = value;
                RaisePropertyChanged(() => RecordSelectorTitle);
            }
        }

        private int _pageWidthRequest = 200;
        public int PageWidthRequest
        {
            get => _pageWidthRequest;
            set
            {
                _pageWidthRequest = value;
                RaisePropertyChanged(() => PageWidthRequest);
            }
        }

        private int _pageHeightRequest = 200;
        public int PageHeightRequest
        {
            get => _pageHeightRequest;
            set
            {
                _pageHeightRequest = value;
                RaisePropertyChanged(() => PageHeightRequest);
            }
        }

        private string _closeButtonTitle = "Close";
        public string CloseButtonTitle
        {
            get => _closeButtonTitle;
            set
            {
                _closeButtonTitle = value;
                RaisePropertyChanged(() => CloseButtonTitle);
            }
        }

        private string _applyButtonTitle = "Apply";
        public string ApplyButtonTitle
        {
            get => _applyButtonTitle;
            set
            {
                _applyButtonTitle = value;
                RaisePropertyChanged(() => ApplyButtonTitle);
            }
        }

        private bool _isApplyVisible = false;
        public bool IsApplyVisible
        {
            get => _isApplyVisible;
            set
            {
                _isApplyVisible = value;
                RaisePropertyChanged(() => IsApplyVisible);
            }

        }

        private Color _recordSelectorSeparatorLineColor = Color.DarkGray;

        public Color RecordSelectorSeparatorLineColor {
            get => _recordSelectorSeparatorLineColor;
            set
            {
                _recordSelectorSeparatorLineColor = value;
                RaisePropertyChanged(() => RecordSelectorSeparatorLineColor);
            }
        }

        private UserAction RecordSelectorAction;
        private string OwnerKey;

        public RecordSelectorPageViewModel(ISearchContentService contentService)
        {
            if(Device.Idiom == TargetIdiom.Phone)
            {
                PageWidthRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density));
                PageHeightRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density));
            }
            else
            {
                if (Device.RuntimePlatform == Device.UWP)
                {
                    PageWidthRequest = Convert.ToInt32(Math.Round(Application.Current.MainPage.Width * 0.7));
                    PageHeightRequest = Convert.ToInt32(Application.Current.MainPage.Width);
                }
                else
                {
                    PageWidthRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density * 0.7));
                    PageHeightRequest = Convert.ToInt32(Math.Round(DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density));
                }
            }

            CloseButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                LocalizationKeys.KeyBasicClose);
            ApplyButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                LocalizationKeys.KeyBasicOK);
            IsApplyVisible = false;

            _contentService = contentService;
            IsLoading = true;
            timer = new ResetTimer(PerformAsyncSearch);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            _logService.LogDebug("Start  InitializeAsync");
            if (navigationData is RecordSelectorInput ua)
            {
                RecordSelectorAction = ua.UserAction;
                OwnerKey = ua.OwnerKey;
                _contentService.SetSourceAction(ua.UserAction);
                _contentService.SetAdditionalFilterParams(ua.UIParams);
                _contentService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                {
                    _logService.LogError($"Unable to prepare content {ex.Message}");
                });
            }

            await UpdateBindingsAsync();
            await base.InitializeAsync(navigationData);
            _logService.LogDebug("End  InitializeAsync");
        }

        private async Task UpdateBindingsAsync()
        {
            _logService.LogDebug("Start UpdateBindingsAsync");
            RecordSelectorSeparatorLineColor = Color.FromHex(_contentService.PageAccentColor());

            // Prepare the search page data
            SearchAndListContentData searchData = new SearchAndListContentData();
            searchData.IsFilterVisible = false;
            searchData.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(0);
            searchData.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
            searchData.SearchTextBoxPlaceholderText = _contentService.SearchColumns(0);
            searchData.IsCounterVisible = _contentService.IsRowCountDisplayActive(0);
            SearchAndListContentData = searchData;

            // Perform the record fetch.
            await PerformSearch();
            _logService.LogDebug("End UpdateBindingsAsync");
        }

        private async Task PerformSearch(bool useDelay = false)
        {
            try
            {
                double delay = 0;
                if (useDelay)
                {
                    if (!SearchAndListContentData.OnlineMode
                        && _contentService.SearchAutoSwitchToOffline())
                    {
                        SearchAndListContentData.OnlineMode = false;
                    }
                    delay = _contentService.SearchDelay(!SearchAndListContentData.OnlineMode);
                }
                timer.StartOrReset(delay);
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

        private async Task PerformAsyncSearch()
        {
            try
            {
                IsLoading = true;
                ResetCancelationToken();
                var salist = SearchAndListContentData;
                salist.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(0);
                salist.IsOnlinePossible = false;
                salist.EnableNoResultsText = false;
                salist.IsCounterVisible = _contentService.IsRowCountDisplayActive(0);
                salist.SearchResults.Clear();
                SearchAndListContentData = salist;
                RequestMode requestMode = SearchAndListContentData.OnlineMode ? RequestMode.Online : RequestMode.Offline;
                if (await _contentService.PerformSearch(0, SearchAndListContentData.SearchText, requestMode, _cancellationTokenSource.Token))
                {
                    var salcd = SearchAndListContentData;
                    salcd.SearchResults.Clear();
                    salcd.ResultsCount = _contentService.CountResults(0);
                    salcd.SearchResults.AddRange(await _contentService.RecordListViewDataPageAsync(0, 0, salcd.InitialPageSize, _cancellationTokenSource.Token));

                    salcd.EnableNoResultsText = !_contentService.HasResults(0);
                    if(salcd.EnableNoResultsText)
                    {
                        salcd.SearchResults.Clear();
                        salcd.NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                            LocalizationKeys.KeyErrorsNoResults);
                    }
                    salcd.OnlineMode = _contentService.AreResultsRetrievedOnline(0);
                    salcd.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
                    salcd.IsCounterVisible = _contentService.IsRowCountDisplayActive(0);
                    SearchAndListContentData = salcd;

                }
                else
                {
                    var salcd = SearchAndListContentData;
                    salcd.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(0);
                    salcd.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
                    salcd.IsCounterVisible = _contentService.IsRowCountDisplayActive(0);
                    salist.SearchResults.Clear();
                    salcd.EnableNoResultsText = true;
                    salcd.NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                        LocalizationKeys.KeyErrorsNoResults);
                    SearchAndListContentData = salcd;
                }

                IsLoading = false;
            }
            catch (Exception e)
            {
                _logService.LogError(e?.Message);
                IsLoading = false;
            }
        }

        private async Task SwitchRequestMode()
        {
            var salcd = SearchAndListContentData;
            salcd.OnlineMode = !SearchAndListContentData.OnlineMode;
            SearchAndListContentData = salcd;
            await PerformSearch();
        }

        public override async Task RefreshAsync(object data)
        {
            if (data is SyncResult)
            {
                await PerformSearch();
            }
        }

        private async Task OnClose()
        {
            await _navigationController.PopPopupAsync();
        }

        private async Task OnApply()
        {
            await _navigationController.PopPopupAsync();
        }

        private string TargetInfoAreaId(ListDisplayRow selectedItem)
        {
            string targetInfoAreaId = RecordSelectorAction?.RecordSelector.TargetLinkInfoAreaId();
            if(string.IsNullOrWhiteSpace(targetInfoAreaId) ||
                targetInfoAreaId.ToLower().Equals("nolink"))
            {
                targetInfoAreaId = RecordSelectorAction.TargetLinkInfoAreaId;
            }

            return targetInfoAreaId;
        }

        private int TargetLinkId(ListDisplayRow selectedItem)
        {
            int targetLinkId = RecordSelectorAction?.RecordSelector.TargetLinkId() ?? -1;
            if (targetLinkId < 0)
            {
                targetLinkId = _contentService.GetTargetInfoAreaLinkId(0, TargetInfoAreaId(selectedItem));
            }

            return targetLinkId;
        }

        private async Task RecordSelected(Syncfusion.ListView.XForms.ItemTappedEventArgs evt)
        {
            if (evt.ItemData is ListDisplayRow selectedItem)
            {
                ParentLink link = null;
                if(!string.IsNullOrWhiteSpace(selectedItem.RecordId))
                {
                    link = new ParentLink
                    {
                        ParentInfoAreaId = selectedItem.InfoAreaId,
                        LinkId = TargetLinkId(selectedItem),
                        RecordId = selectedItem.RecordId
                    };
                }

                RecordSelectedMessage message = new RecordSelectedMessage
                {
                    RecordSelectorAction = RecordSelectorAction,
                    SelectedRow = selectedItem,
                    LinkDetails = link,
                    OwnerKey = OwnerKey
                };
                MessagingCenter.Send<BaseViewModel, RecordSelectedMessage>(this, InAppMessages.RecordSelected, message);
                await _navigationController.PopPopupAsync();
            }
        }

        private bool CanLoadMoreItems(object obj)
        {
            if (SearchAndListContentData == null ||
                SearchAndListContentData.SearchResults == null ||
                _contentService.CountResults(0) == 0 ||
                SearchAndListContentData.SearchResults.Count >= _contentService.GetTabData(0).RawData.Result.Rows.Count)
            {
                return false;
            }

            return true;
        }

        private async void LoadMoreItems(object obj)
        {
            try
            {
                SearchAndListContentData.SearchResults.AddRange(await _contentService.RecordListViewDataPageAsync(0,
                    SearchAndListContentData.SearchResults.Count, SearchAndListContentData.SearchResults.Count + SearchAndListContentData.PageSize, _cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }
    }
}
