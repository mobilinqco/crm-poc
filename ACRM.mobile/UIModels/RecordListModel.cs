using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class RecordListModel : UIWidget, IFilterItemSelectionHandler
    {
        private readonly ISearchContentService _contentService;
        private readonly UserAction _userAction;
        private readonly ResetTimer timer;
        private readonly int SelectedTabId = 0;
        private bool _isInitialRequest = true;
        public bool CanApplyFilter { get; set; }
        private Dictionary<int, List<FilterUI>> searchFilters = new Dictionary<int, List<FilterUI>>();
        public ICommand SearchCommand => new Command(async () => await PerformSearch(true));
        public ICommand SwitchRequestModeCommand => new Command(async () => await SwitchRequestMode());
        public ICommand RecordSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await RecordSelected(evt));
        public ICommand ShowFilterCommand => new Command(async () => await FilterCommandHandler());
        public ICommand LoadMoreCommand => new Command<object>(LoadMoreItems, CanLoadMoreItems);

        public RecordListModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<ISearchContentService>();
            if (widgetArgs is UserAction)
            {
                _userAction = widgetArgs as UserAction;
            }
            this.timer = new ResetTimer(this.PerformAsyncSearch);
        }

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

        public override async ValueTask<bool> InitializeControl()
        {
            _contentService.SetSourceAction(_userAction);
            await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
            await PerformSearch();
            return true;
        }

        private void dependentDataChnaged(WidgetMessage obj)
        {
           
        }

        private async Task PerformSearch(bool useDelay = false)
        {
            try
            {
                double delay = 0;
                if (useDelay)
                {
                    // TODO: here we may need to test if the related data has been received in online mode
                    // then the search should be carried only online.
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
                var salist = SearchAndListContentData;
                salist.EnableNoResultsText = false;
                salist.SearchResults.Clear();
                salist.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(0);
                salist.IsFilteringEnabled = _contentService.HasUserFilters(0);
                var userFilters = await GetEnabledUserFilters();
                int filterCount = userFilters?.Count ?? 0;
                salist.IsUserFilterEnabled = filterCount > 0 ? true : false;
                salist.UserFilterCount = filterCount;
                salist.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
                salist.SearchTextBoxPlaceholderText = _contentService.SearchColumns(0);

                ResetCancelationToken();
                SearchAndListContentData = salist;
                var selectedTab = _contentService.GetTabData(SelectedTabId);
                if (selectedTab != null && CanApplyFilter)
                {
                    selectedTab.EnabledUserFilters = userFilters;
                }
                else if (selectedTab != null)
                {
                    selectedTab.EnabledUserFilters = null;
                }
                
                if (await _contentService.PerformSearch(0,
                    SearchAndListContentData.SearchText,
                    DetermineRequestMode(),
                    _cancellationTokenSource.Token))
                {
                    var salcd = SearchAndListContentData;
                    salcd.SearchResults.Clear();
                    salcd.SearchResults.AddRange(await _contentService.RecordListViewDataPageAsync(0, 0, salcd.InitialPageSize, _cancellationTokenSource.Token));
                    salcd.ResultsCount = _contentService.CountResults(0);
                    salcd.EnableNoResultsText = !_contentService.HasResults(0);
                    if (salcd.EnableNoResultsText)
                    {
                        salcd.SearchResults.Clear();
                        salcd.NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                            LocalizationKeys.KeyErrorsNoResults);
                    }
                    salcd.OnlineMode = _contentService.AreResultsRetrievedOnline(0);
                    SearchAndListContentData = salcd;

                }
                else
                {
                    var salcd = SearchAndListContentData;
                    salcd.EnableNoResultsText = true;
                    salcd.SearchResults.Clear();
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

        private async Task<List<Filter>> GetEnabledUserFilters()
        {
            List<Filter> filters = new List<Filter>();
            var uiFilter = await GetUserFilters();
            if (uiFilter != null && uiFilter.Count > 0)
            {
                var enabledfilter = uiFilter.Where(a => a.IsEnabled).ToList();
                if (enabledfilter != null && enabledfilter.Count > 0)
                {
                    foreach (var uifilter in enabledfilter)
                    {
                        filters.Add(uifilter.OutputFilter);
                    }
                }
            }
            return filters;
        }

        private async Task SwitchRequestMode()
        {
            var salcd = SearchAndListContentData;
            salcd.OnlineMode = !SearchAndListContentData.OnlineMode;
            SearchAndListContentData = salcd;
            await PerformSearch();
        }

        private async Task RecordSelected(Syncfusion.ListView.XForms.ItemTappedEventArgs evt)
        {
            if (evt.ItemData is ListDisplayRow selectedItem)
            {
                UserAction userAction = await _contentService.ActionForItemSelect(0, selectedItem, _cancellationTokenSource.Token);
                IUserActionSchuttle userActionSchuttle = AppContainer.Resolve<IUserActionSchuttle>();
                await userActionSchuttle.Carry(userAction, selectedItem.RecordId, _cancellationTokenSource.Token);
            }
        }

        private async Task FilterCommandHandler()
        {
            await _navigationController.NavigateToAsync<FilterUIPageViewModel>(parameter: this);
        }

        public async Task<List<FilterUI>> GetUserFilters()
        {
            if (searchFilters.ContainsKey(SelectedTabId) && searchFilters[SelectedTabId] != null)
            {
                return searchFilters[SelectedTabId];
            }

            if (searchFilters.ContainsKey(SelectedTabId))
            {
                searchFilters[SelectedTabId] = SearchAndListCommons.GetFilterUIList(_contentService, SelectedTabId);
            }
            else
            {
                searchFilters.Add(SelectedTabId, SearchAndListCommons.GetFilterUIList(_contentService, SelectedTabId));
            }

            return searchFilters[SelectedTabId];
        }

        public async Task ApplyUserFilters(List<FilterUI> filters)
        {
            if (searchFilters.ContainsKey(SelectedTabId))
            {
                searchFilters[SelectedTabId] = filters;
            }
            await PerformSearch();
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
            try { 
                SearchAndListContentData.SearchResults.AddRange(await _contentService.RecordListViewDataPageAsync(0,
                    SearchAndListContentData.SearchResults.Count, SearchAndListContentData.SearchResults.Count + SearchAndListContentData.PageSize, _cancellationTokenSource.Token));
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

        private RequestMode DetermineRequestMode()
        {
            if(_sessionContext.IsInOfflineMode)
            {
                return RequestMode.Offline;
            }

            if(_isInitialRequest)
            {
                _isInitialRequest = false;
                return _contentService.InitialRequestMode(0);
            }

            return SearchAndListContentData.OnlineMode ? RequestMode.Online : RequestMode.Offline;
        }
    }
}
