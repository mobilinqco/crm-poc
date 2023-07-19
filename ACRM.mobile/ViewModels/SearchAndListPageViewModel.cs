using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using AsyncAwaitBestPractices;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class SearchAndListPageViewModel : NavigationBarBaseViewModel, IFilterItemSelectionHandler
    {
        private readonly ISearchContentService _contentService;
        private readonly IUserActionSchuttle _userActionSchuttle;
        private readonly ResetTimer timer;
        private Dictionary<int, List<FilterUI>> searchFilters = new Dictionary<int, List<FilterUI>>();

        public ICommand RecordSelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await RecordSelected(evt));
        public ICommand LoadMoreCommand => new Command<object>(LoadMoreItems, CanLoadMoreItems);
        public ICommand OnRelatedInfoAreaSelected => new Command<UserAction>(async (selectedItem) => await RelatedInfoAreaSelected(selectedItem));
        public ICommand OnHeaderAction => new Command<HeaderActionButton>(async (selectedItem) => await HeaderAction(selectedItem));
        public ICommand SearchCommand => new Command(async () => PerformSearch(true));
        public ICommand SwitchRequestModeCommand => new Command(async () => await SwitchRequestMode());
        public ICommand ShowFilterCommand => new Command(async () => await FilterCommandHandler());

        private UserAction _action;

        private HeaderGroupData _headerData;
        public HeaderGroupData HeaderData
        {
            get => _headerData;
            set
            {
                _headerData = value;
                RaisePropertyChanged(() => HeaderData);
            }
        }

        public bool CanApplyFilter { get; set; }

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

        private int SelectedTabId = 0;

        public SearchAndListPageViewModel(ISearchContentService contentService, IUserActionSchuttle userActionSchuttle)
        {
            _contentService = contentService;
            _userActionSchuttle = userActionSchuttle;
            IsLoading = true;
            IsBackButtonVisible = true;
            HeaderGroupData headerData = new HeaderGroupData();
            headerData.IsOrganizerHeaderVisible = false;
            HeaderData = headerData;
            this.timer = new ResetTimer(this.PerformAsyncSearch);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            _logService.LogDebug("Start  InitializeAsync");
            if (navigationData is UserAction)
            {
                _action = navigationData as UserAction;
                _contentService.SetSourceAction(navigationData as UserAction);
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
            PageTitle = _contentService.PageTitle();

            // Prepare the page header data
            var headerData = HeaderData;
            headerData.InfoAreaColor = Color.FromHex(_contentService.PageAccentColor());

            List<UserAction> relatedInfoAreas = _contentService.HeaderRelatedInfoAreas();
            if (relatedInfoAreas?.Count() > 0)
            {
                if (relatedInfoAreas[0].ActionDisplayName.Equals("All"))
                {
                    relatedInfoAreas[0].ActionDisplayName = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                        LocalizationKeys.KeyBasicTabAll);
                }
                headerData.RelatedInfoAreas = relatedInfoAreas;
                if (headerData.RelatedInfoAreas.Count > 0)
                {
                    headerData.SelectedRelatedInfoArea = headerData.RelatedInfoAreas[0];
                }
                headerData.AreActionsViewVisible = false;
                headerData.SetHeaderActionButtons(_contentService.HeaderButtons());
                if (headerData.HeaderActions.Count > 0)
                {
                    headerData.AreActionsViewVisible = true;
                }
            }
            HeaderData = headerData;
            await BindListData();
            _logService.LogDebug("End UpdateBindingsAsync");
        }

        private async Task BindListData()
        {
            // Prepare the search page data
            SearchAndListContentData searchData = new SearchAndListContentData();
            searchData.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(SelectedTabId);
            searchData.IsFilteringEnabled = _contentService.HasUserFilters(SelectedTabId);
            searchData.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
            searchData.AreSectionsEnabled = _action?.AreSectionsEnabled() ?? false;
            searchData.IsCounterVisible = _contentService.IsRowCountDisplayActive(SelectedTabId);

            if (_contentService.InitialRequestMode(SelectedTabId) == RequestMode.Online
                || _contentService.InitialRequestMode(SelectedTabId) == RequestMode.Best)
            {
                searchData.OnlineMode = true;
            }

            searchData.SearchTextBoxPlaceholderText = _contentService.SearchColumns(SelectedTabId);
            SearchAndListContentData = searchData;

            // Perform the record fetch.
            PerformSearch();
        }

        private async Task RecordSelected(Syncfusion.ListView.XForms.ItemTappedEventArgs evt)
        {
            if (evt.ItemData is ListDisplayRow selectedItem)
            {
                UserAction userAction = await _contentService.ActionForItemSelect(SelectedTabId, selectedItem, _cancellationTokenSource.Token);
                await _userActionSchuttle.Carry(userAction, selectedItem.RecordId, _cancellationTokenSource.Token);

            }
        }

        private async Task RelatedInfoAreaSelected(UserAction selectedUserAction)
        {
            if (selectedUserAction != null && selectedUserAction != HeaderData.SelectedRelatedInfoArea)
            {
                _logService.LogInfo($"SearchAndListPage user action: {selectedUserAction.ActionDisplayName}");
                List<UserAction> userActions = new List<UserAction>();
                HeaderGroupData hgd = HeaderData;

                int i = 0;
                foreach (UserAction ua in hgd.RelatedInfoAreas)
                {
                    ua.IsSelected = false;
                    if (ua == selectedUserAction)
                    {
                        ua.IsSelected = true;
                        hgd.SelectedRelatedInfoArea = ua;
                        SelectedTabId = i;
                        PerformSearch();
                    }

                    userActions.Add(ua);
                    i++;
                }
                hgd.RelatedInfoAreas = userActions;
                HeaderData = hgd;


            }
        }

        private async Task HeaderAction(HeaderActionButton headerActionButton)
        {
            if (headerActionButton != null)
            {
                _logService.LogInfo($"SearchAndListPage header user action: {headerActionButton.UserAction.ActionDisplayName}");
                await _navigationController.NavigateAsyncForAction(headerActionButton.UserAction, _cancellationTokenSource.Token);
            }
        }

        private void PerformSearch(bool useDelay = false)
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
                var salist = SearchAndListContentData;
                salist.AreSectionsEnabled = _action?.AreSectionsEnabled() ?? false;
                salist.IsCounterVisible = _contentService.IsRowCountDisplayActive(SelectedTabId);
                salist.IsOnlinePossible = false;
                salist.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(SelectedTabId);
                salist.IsFilteringEnabled = _contentService.HasUserFilters(SelectedTabId);
                var userFilters = await GetEnabledUserFilters();
                int filterCount = userFilters?.Count ?? 0;

                salist.IsUserFilterEnabled = filterCount > 0 ? true : false;
                salist.UserFilterCount = filterCount;
                salist.EnableNoResultsText = false;
                SearchAndListContentData = salist;
                RequestMode requestMode = SearchAndListContentData.OnlineMode ? RequestMode.Online : RequestMode.Offline;
                ResetCancelationToken();

                if (CanApplyFilter)
                {
                    _contentService.GetTabData(SelectedTabId).EnabledUserFilters = userFilters;
                }

                if (await _contentService.PerformSearch(SelectedTabId, SearchAndListContentData.SearchText, requestMode, _cancellationTokenSource.Token))
                {
                    var salcd = SearchAndListContentData;
                    salcd.SearchResults.Clear();
                    salcd.SearchResults.AddRange(await _contentService.RecordListViewDataPageAsync(SelectedTabId, 0, salcd.InitialPageSize, _cancellationTokenSource.Token));
                    salcd.ResultsCount = _contentService.CountResults(SelectedTabId);
                    salcd.EnableNoResultsText = !_contentService.HasResults(SelectedTabId);
                    if (salcd.EnableNoResultsText)
                    {
                        salcd.SearchResults.Clear();
                        salcd.NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                            LocalizationKeys.KeyErrorsNoResults);
                    }
                    salcd.OnlineMode = _contentService.AreResultsRetrievedOnline(SelectedTabId);
                    salcd.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
                    SearchAndListContentData = salcd;
                }
                else
                {
                    SetNoResultUI();
                }

                IsLoading = false;
            }
            catch (Exception e)
            {
                _logService.LogError(e?.Message);
                SetNoResultUI();
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

        private void SetNoResultUI()
        {
            var salcd = SearchAndListContentData;
            salcd.IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(SelectedTabId);
            salcd.IsFilteringEnabled = _contentService.HasUserFilters(SelectedTabId);
            salcd.IsCounterVisible = _contentService.IsRowCountDisplayActive(SelectedTabId);
            salcd.AreSectionsEnabled = _action?.AreSectionsEnabled() ?? false;
            salcd.IsOnlinePossible = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
            salcd.EnableNoResultsText = true;
            salcd.SearchResults.Clear();
            salcd.NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                LocalizationKeys.KeyErrorsNoResults);
            SearchAndListContentData = salcd;
        }

        private async Task SwitchRequestMode()
        {
            var salcd = SearchAndListContentData;
            salcd.OnlineMode = !SearchAndListContentData.OnlineMode;
            SearchAndListContentData = salcd;
            PerformSearch();
        }

        public override async Task RefreshAsync(object data)
        {
            if (data is SyncResult)
            {
                PerformSearch();
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
            PerformSearch();
        }

        private bool CanLoadMoreItems(object obj)
        {
            if (SearchAndListContentData == null ||
                SearchAndListContentData.SearchResults.Count >= _contentService.CountResults(SelectedTabId))
            {
                return false;
            }

            return true;
        }

        private async void LoadMoreItems(object obj)
        {
            try
            {
                var listView = obj as Syncfusion.ListView.XForms.SfListView;
                listView.IsBusy = true;
                await Task.Delay(2500);
                int pageSize = SearchAndListContentData.SearchResults.Count == 0 ? SearchAndListContentData.InitialPageSize : SearchAndListContentData.PageSize;
                SearchAndListContentData.SearchResults.AddRange(await _contentService.RecordListViewDataPageAsync(SelectedTabId,
                    SearchAndListContentData.SearchResults.Count, SearchAndListContentData.SearchResults.Count + pageSize, _cancellationTokenSource.Token));
                listView.IsBusy = false;
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }

        }
    }
}
