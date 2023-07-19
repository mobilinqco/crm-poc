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
using ACRM.mobile.Domain.Application.SerialEntry;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using ACRM.mobile.ViewModels.ObservableGroups.Breadcrumbs;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class SerialEntryControlModel : UIWidget, ISerialEntryListing, IFilterItemSelectionHandler, IBreadcrumbsFilterParent
    {
        private readonly ISerialEntryService _contentService;
        private readonly IRightsProcessor _rightsProcessor;
        private readonly IConfigurationService _configurationService;
        private readonly UserAction _userAction;
        private readonly ResetTimer timer;
        public List<PanelData> RootPanels { get; set; }
        private bool _firstLoad = true;
        private bool _hierarchicalPositionFilter = true;
        private bool _ListingOn = false;
        public bool CanApplyFilter { get; set; }
        private UserAction FinishUserAction;
        private Dictionary<string, List<string>> _positionFilterkeyValuePairs = new Dictionary<string, List<string>>();
        private List<FilterUI> searchFilters = new List<FilterUI>();
        public ICommand SearchCommand => new Command(async () => await PerformSearch(true));
        public ICommand OnCancleButtonTappedCommand => new Command(async () => await OnCancleButtonTapped());
        public ICommand OnDetailsBackCommand => new Command(async () => await OnBackButtonTapped());
        public ICommand SearchButtonCommand => new Command(async () => await OnSearchButton());
        public ICommand RequestModeButtonCommand => new Command(async () => await OnRequestModeButton());
        public ICommand ViewModeButtonCommand => new Command(async () => await OnViewModeButton());
        public ICommand OverviewCommand => new Command(async () => await OverviewCommandTapped());
        public ICommand AllItemsCommand => new Command(async () => await AllItemsCommandTapped());
        public ICommand SelectionChangedCommand => new Command<Syncfusion.XForms.Buttons.SelectionChangedEventArgs>(async (args) => await OnSelectionChanged(args));
        public ICommand FiltersButtonCommand => new Command(async () => await FilterCommandHandler());
        public ICommand CompleteActionCommand => new Command(async () => await CompleteActionCommandHandler());

        private async Task CompleteActionCommandHandler()
        {
            if (FinishUserAction != null)
            {
                await _navigationController.NavigateAsyncForAction(FinishUserAction, _cancellationTokenSource.Token);
                await _navigationController.RemoveLastFromBackStackAsync();

            }
            else
            {
                await _navigationController.BackAsync();
            }
        }

        private async Task OnSelectionChanged(Syncfusion.XForms.Buttons.SelectionChangedEventArgs args)
        {
            await PerformAsyncSearch();
        }

        List<SerialEntryItem> AllItems;
        private ObservableCollection<SfSegmentItem> _filterSegControlItems;
        private ObservableCollection<UIWidget> RootWidgets;
        public ObservableCollection<SfSegmentItem> FilterSegControlItems
        {
            get => _filterSegControlItems;
            set
            {
                _filterSegControlItems = value;
                RaisePropertyChanged(() => FilterSegControlItems);
                if(_filterSegControlItems != null && _filterSegControlItems.Count > 0)
                {
                    FilterSegControlVisible = true;
                }
            }
        }

        private ObservableCollection<BreadcrumbsFilter> _positionFilterItems;
        public ObservableCollection<BreadcrumbsFilter> PositionFilterItems
        {
            get => _positionFilterItems;
            set
            {
                _positionFilterItems = value;
                RaisePropertyChanged(() => PositionFilterItems);
                if (_positionFilterItems != null && _positionFilterItems.Count > 0)
                {
                    PositionFilterControlVisible = true;
                }
            }
        }

        private bool _positionFilterControlVisible = false;
        public bool PositionFilterControlVisible
        {
            get => _positionFilterControlVisible;
            set
            {
                _positionFilterControlVisible = value;
                RaisePropertyChanged(() => PositionFilterControlVisible);
            }
        }


        private bool _filterSegControlVisible = false;
        public bool FilterSegControlVisible
        {
            get => _filterSegControlVisible;
            set
            {
                _filterSegControlVisible = value;
                RaisePropertyChanged(() => FilterSegControlVisible);
            }
        }

        private ObservableCollection<SerialEntryUiItem> _uiItems;
        public ObservableCollection<SerialEntryUiItem> UIItems
        {
            get => _uiItems;
            set
            {
                _uiItems = value;
                RaisePropertyChanged(() => UIItems);
            }
        }

        private int _filterSegControlIndex;
        public int FilterSegControlIndex
        {
            get => _filterSegControlIndex;
            set
            {
                _filterSegControlIndex = value;
                RaisePropertyChanged(() => FilterSegControlIndex);
            }
        }

        public SerialEntryUiItem SelectedItem
        {
            get
            {
                if(UIItems !=null && _selectedItemIndex > -1)
                {
                    return UIItems[_selectedItemIndex];
                }
                else
                {
                    return null;
                }
                
            }
        }

        private bool _disableThirdListRow = false;
        public bool DisableThirdListRow
        {
            get => _disableThirdListRow;
            set
            {
                _disableThirdListRow = value;
                RaisePropertyChanged(() => DisableThirdListRow);
            }
        }
        
        private int _selectedItemIndex = -1;
        public int SelectedItemIndex
        {
            get => _selectedItemIndex;
            set
            {
                if(_selectedItemIndex == value)
                {
                    return;
                }

                var currentItem = SelectedItem;
                if (UIItems == null || value >= UIItems.Count)
                {
                    _selectedItemIndex = -1;
                }
                else
                {
                    _selectedItemIndex = value;
                }

                RaisePropertyChanged(() => SelectedItemIndex);
                RaisePropertyChanged(() => SelectedItem);
                
                if (_selectedItemIndex > -1)
                {
                    IsDetailView = true;
                }
                else
                {
                    IsDetailView = false;
                }
                new Action(async () => await ProcessItems(currentItem, SelectedItem))();
            }
        }

        private async Task ProcessItems(SerialEntryUiItem oldItem, SerialEntryUiItem newItem)
        {
            if (newItem != null)
            {
                newItem.IsSelected = true;
                await LoadChildData(newItem);
            }
            if (oldItem != null)
            {
                oldItem.IsSelected = false;
                await SaveItem(oldItem);
            }
        }

        private bool _isDetailView = false;
        public bool IsDetailView
        {
            get => _isDetailView;
            set
            {
                _isDetailView = value;
                RaisePropertyChanged(() => IsDetailView);
            }
        }

        private bool _isSearchBoxVisible = false;
        public bool IsSearchBoxVisible
        {
            get => _isSearchBoxVisible;
            set
            {
                _isSearchBoxVisible = value;
                RaisePropertyChanged(() => IsSearchBoxVisible);
            }
        }

        private string _searchTextBoxPlaceholderText;
        public string SearchTextBoxPlaceholderText
        {
            get => _searchTextBoxPlaceholderText;
            set
            {
                _searchTextBoxPlaceholderText = value;
                RaisePropertyChanged(() => SearchTextBoxPlaceholderText);
            }
        }
        private string _currencyText;
        public string CurrencyText
        {
            get => _currencyText;
            set
            {
                _currencyText = value;
                RaisePropertyChanged(() => CurrencyText);
            }
        }
        
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged(() => SearchText);
            }
        }

        private bool _isFiltersButtonCommandEnabled = false;
        public bool IsFiltersButtonCommandEnabled
        {
            get => _isFiltersButtonCommandEnabled;
            set
            {
                _isFiltersButtonCommandEnabled = value;
                RaisePropertyChanged(() => IsFiltersButtonCommandEnabled);
            }
        }

        private bool _isAllItemsMode = true;
        public bool IsAllItemsMode
        {
            get => _isAllItemsMode;
            set
            {
                _isAllItemsMode = value;
                RaisePropertyChanged(() => IsAllItemsMode);
            }
        }

        private bool _isRequestModeButtonEnabled = false;
        public bool IsRequestModeButtonEnabled
        {
            get => _isRequestModeButtonEnabled;
            set
            {
                _isRequestModeButtonEnabled = value;
                RaisePropertyChanged(() => IsRequestModeButtonEnabled);
            }
        }
        private bool _isUserFilterHasEnabledFilters = false;
        public bool IsUserFilterHasEnabledFilters
        {
            get => _isUserFilterHasEnabledFilters;
            set
            {
                _isUserFilterHasEnabledFilters = value;
                RaisePropertyChanged(() => IsUserFilterHasEnabledFilters);
            }
        }

        private int _userFilterCount = 0;
        public int UserFilterCount
        {
            get => _userFilterCount;
            set
            {
                _userFilterCount = value;
                RaisePropertyChanged(() => UserFilterCount);
            }
        }

        private string _requestModeButtonIconText = MaterialDesignIcons.CloudOutline;
        public string RequestModeButtonIconText
        {
            get => _requestModeButtonIconText;
            private set
            {
                _requestModeButtonIconText = value;
                RaisePropertyChanged(() => RequestModeButtonIconText);
            }
        }

        private string _viewModeButtonIconText = MaterialDesignIcons.Apps;
        public string ViewModeButtonIconText
        {
            get => _viewModeButtonIconText;
            set
            {
                _viewModeButtonIconText = value;
                RaisePropertyChanged(() => ViewModeButtonIconText);
            }
        }

        private bool _isListMode = true;
        public bool IsListMode
        {
            get => _isListMode;
            set
            {
                _isListMode = value;
                RaisePropertyChanged(() => IsListMode);
            }
        }

        private bool _enableNoResultsText = false;
        public bool EnableNoResultsText
        {
            get => _enableNoResultsText;
            set
            {
                _enableNoResultsText = value;
                RaisePropertyChanged(() => EnableNoResultsText);
            }
        }

        private string _completeIconText = MaterialDesignIcons.CartOutline;
        public string CompleteIconText
        {
            get => _completeIconText;
            set
            {
                _completeIconText = value;
                RaisePropertyChanged(() => CompleteIconText);
            }
        }

        private string _completeActionText = string.Empty;
        public string CompleteActionText
        {
            get => _completeActionText;
            set
            {
                _completeActionText = value;
                RaisePropertyChanged(() => CompleteActionText);
            }
        }

        private bool _sumLineShowEndPrice = true;
        public bool SumLineShowEndPrice
        {
            get => _sumLineShowEndPrice;
            set
            {
                _sumLineShowEndPrice = value;
                RaisePropertyChanged(() => SumLineShowEndPrice);
            }
        }

        private bool _showFreeGoodsPrice = true;
        public bool ShowFreeGoodsPrice
        {
            get
            {
                return _showFreeGoodsPrice && TotalFreeGoods > 0 ? true : false;
            }
            set
            {
                _showFreeGoodsPrice = value;
                RaisePropertyChanged(() => ShowFreeGoodsPrice);
            }
        }

        private bool _showCompleteButton = false;
        public bool ShowCompleteButton
        {
            get => _showCompleteButton;
            set
            {
                _showCompleteButton = value;
                RaisePropertyChanged(() => ShowCompleteButton);
            }
        }

        private bool _sumLineWithAllRows = false;
        public bool SumLineWithAllRows
        {
            get => _sumLineWithAllRows;
            set
            {
                _sumLineWithAllRows = value;
                RaisePropertyChanged(() => SumLineWithAllRows);
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
        private decimal _totalQuantity;
        public decimal TotalQuantity
        {
            get => _totalQuantity;
            set
            {
                _totalQuantity = value;
                RaisePropertyChanged(() => TotalQuantity);
            }
        }

        private decimal _totalNetPrice;
        public decimal TotalNetPrice
        {
            get => _totalNetPrice;
            set
            {
                _totalNetPrice = value;
                RaisePropertyChanged(() => TotalNetPrice);
            }
        }

        private decimal _totalEndPrice;
        public decimal TotalEndPrice
        {
            get => _totalEndPrice;
            set
            {
                _totalEndPrice = value;
                RaisePropertyChanged(() => TotalEndPrice);
            }
        }

        private decimal _totalFreeGoods;
        public decimal TotalFreeGoods
        {
            get => _totalFreeGoods;
            set
            {
                _totalFreeGoods = value;
                RaisePropertyChanged(() => TotalFreeGoods);
                RaisePropertyChanged(() => ShowFreeGoodsPrice);
                
            }
        }

        private decimal _totalDiscount;
        public decimal TotalDiscount
        {
            get => _totalDiscount;
            set
            {
                _totalDiscount = value;
                RaisePropertyChanged(() => TotalDiscount);
            }
        }

        private bool _hasDestiRecords = true;
        public bool HasDestiRecords
        {
            get => _hasDestiRecords;
            set
            {
                _hasDestiRecords = value;
                RaisePropertyChanged(() => HasDestiRecords);
            }
        }

        private bool _onlineMode = true;
        public bool OnlineMode
        {
            get => _onlineMode;
            set
            {
                _onlineMode = value;
                RaisePropertyChanged(() => OnlineMode);
                if (value)
                {
                    RequestModeButtonIconText = MaterialDesignIcons.Cloud;
                }
                else
                {
                    RequestModeButtonIconText = MaterialDesignIcons.CloudOutline;
                }

            }
        }

        public SerialEntryControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<ISerialEntryService> ();
            _configurationService = AppContainer.Resolve<IConfigurationService>();
            _rightsProcessor = AppContainer.Resolve<IRightsProcessor>();
            if (widgetArgs != null && widgetArgs is UserAction)
            {
                _userAction = widgetArgs as UserAction;
                _contentService.SetSourceAction(_userAction);
            }
            this.timer = new ResetTimer(this.PerformAsyncSearch);
            IsLoading = true;
           
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (_contentService != null)
            {
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                IsFiltersButtonCommandEnabled = _contentService.SourceSearchService.HasUserFilters(0);
                CurrencyText = _contentService.Currency.Item2;
                searchFilters = SearchAndListCommons.GetFilterUIList(_contentService.SourceSearchService, 0);
                SearchTextBoxPlaceholderText = _contentService.SourceSearchService.SearchColumns(0);
                Dictionary<string,string> filters = await _contentService.GetFilters(0, _cancellationTokenSource.Token);
                List<string> positionFilters = await _contentService.GetPositionFilters(_cancellationTokenSource.Token);
                _hierarchicalPositionFilter = _contentService.HierarchicalPositionFilter;
                if (positionFilters?.Count > 0)
                {
                    int order = 1;
                    var filterItems = new ObservableCollection<BreadcrumbsFilter>();
                    if (filters != null && filters.Keys.Count > 0)
                    {
                        var filter = new MultiBreadcrumbsFilter(filters, this);
                        if(await filter.LoadFilterItems(_cancellationTokenSource.Token))
                        {
                            filter.Order = order;
                            filterItems.Add(filter);
                        }
                    }
                    var seatchTab = _contentService.SourceSearchService.GetTabData(0);
                    seatchTab.ExternalFields = new List<FieldControlField>();
                    foreach (var item in positionFilters)
                    {
                        if (item.IsListing())
                        {
                            var filter = new ListingBreadcrumbsFilter(item, this, _contentService.ListingService.HasListingItems);
                            if (await filter.LoadFilterItems(_cancellationTokenSource.Token))
                            {
                                order++;
                                filter.Order = order;
                                filterItems.Add(filter);
                            }
                        }
                        else
                        {
                            var filter = new CatalogBreadcrumbsFilter(item, this);
                            if (await filter.LoadFilterItems(_cancellationTokenSource.Token))
                            {
                                if (!_hierarchicalPositionFilter)
                                {
                                    filter.RefreshFilterItems(null);
                                }
                                var fieldInfo = filter.FieldInfo;
                                if (fieldInfo != null)
                                {
                                    order++;
                                    filter.Order = order;
                                    filterItems.Add(filter);
                                    seatchTab.ExternalFields.Add(FieldControlField.GetFieldControl(fieldInfo));
                                }
                            }
                        }
                    }
                    PositionFilterItems = filterItems;
                }
                else
                {

                    if (filters != null && filters.Keys.Count > 0)
                    {
                        var controlItems = new ObservableCollection<SfSegmentItem>();
                        foreach (var key in filters.Keys)
                        {
                            controlItems.Add(new SfSegmentItem { Text = filters[key], StyleId = key });
                        }
                        FilterSegControlItems = controlItems;
                    }
                }

                await PerformSearch();

                if (_contentService.DestinationRootEditService != null)
                {
                    var rootPanel = await _contentService.DestinationRootEditService.GetPanelAsync(null, _cancellationTokenSource.Token);
                    if (rootPanel != null)
                    {
                        RootPanels = new List<PanelData> { rootPanel };
                        RootWidgets = await RootPanels.BuildWidgetsAsyc(null, _cancellationTokenSource);
                    }
                }

                FinishUserAction = _contentService.FinishUserAction;
                if (FinishUserAction != null)
                {
                    ShowCompleteButton = true;
                    CompleteActionText = _contentService.FinishActionText;
                    // There is mismatch in the icon hence hard coding for now
                    // CompleteIconText = _contentService.FinishActionIcon; 
                }
                await SetActionButton();
                if (_configurationService.GetConfigValue("SerialEntry.SumLineShowEndPrice") != null)
                {
                    SumLineShowEndPrice =  _configurationService.GetBoolConfigValue("SerialEntry.SumLineShowEndPrice", false);
                }

                if (_configurationService.GetConfigValue("SerialEntry.ShowFreeGoodsPrice") != null)
                {
                    ShowFreeGoodsPrice = _configurationService.GetBoolConfigValue("SerialEntry.ShowFreeGoodsPrice", false);
                }

                if (_configurationService.GetConfigValue("SerialEntry.SumLineWithAllRows") != null)
                {
                    SumLineWithAllRows = _configurationService.GetBoolConfigValue("SerialEntry.SumLineWithAllRows", false);
                }

                if (_configurationService.GetConfigValue("SerialEntry.DisableThirdListRow") != null)
                {
                    DisableThirdListRow = _configurationService.GetBoolConfigValue("SerialEntry.DisableThirdListRow", false);
                }
            }
            return true;
        }

        private async Task SetActionButton()
        {

            if (FinishUserAction != null)
            {

                var (status, result, message) = await _rightsProcessor.EvaluateRightsFilter(FinishUserAction, _cancellationTokenSource.Token, false, "ButtonShowFilter");
                if (status)
                {
                    if (result)
                    {
                        ShowCompleteButton = false;
                        var title = $"Button Action will not be displayed, message : {message}";
                        _logService.LogInfo(title);

                    }
                    else
                    {
                        ShowCompleteButton = true;
                    }
                }
            }
            else
            {
                ShowCompleteButton = false;
            }
        }

        private async Task PerformAsyncSearch()
        {
            try
            {
                IsLoading = true;
                EnableNoResultsText = false;
                UIItems = null;
                List<Filter> applyFilter = new List<Filter>();
                if (FilterSegControlVisible)
                {
                    if (FilterSegControlIndex >= 0 && FilterSegControlItems != null && FilterSegControlItems.Count > 0)
                    {
                        string filterName = FilterSegControlItems[FilterSegControlIndex].StyleId;
                        if (filterName.IsListing())
                        {
                            _ListingOn = true;
                            await _contentService.SourceSearchService.SetAdditionalFilterName(null, _cancellationTokenSource.Token);
                        }
                        else
                        {
                            _ListingOn = false;
                            await _contentService.SourceSearchService.SetAdditionalFilterName(filterName, _cancellationTokenSource.Token);
                        }
                    }
                }
                else if (PositionFilterControlVisible)
                {
                    foreach (var filterItem in PositionFilterItems)
                    {
                        if (filterItem is MultiBreadcrumbsFilter multiBreadcrumbsFilter
                            && !string.IsNullOrWhiteSpace(multiBreadcrumbsFilter.selectedFilterName))
                        {
                            if (multiBreadcrumbsFilter.selectedFilterName.IsListing())
                            {
                                _ListingOn = true;
                                await _contentService.SourceSearchService.SetAdditionalFilterName(null, _cancellationTokenSource.Token);
                            }
                            else
                            {
                                _ListingOn = false;
                                await _contentService.SourceSearchService.SetAdditionalFilterName(multiBreadcrumbsFilter.selectedFilterName, _cancellationTokenSource.Token);
                            }
                        }
                        //else if (filterItem is CatalogBreadcrumbsFilter catalogBreadcrumbsFilter)
                        //{
                        //    var filter = catalogBreadcrumbsFilter.ProcessedFilter;
                        //    if (filter != null)
                        //    {
                        //        applyFilter.Add(filter);
                        //    }

                        //}

                    }
                    ClearPositionFilterItems(0);

                }
                IsFiltersButtonCommandEnabled = _contentService.SourceSearchService.HasUserFilters(0);
                var userFilters = searchFilters.GetEnabledUserFilters();
                int filterCount = userFilters?.Count ?? 0;
                IsUserFilterHasEnabledFilters = filterCount > 0 ? true : false;
                UserFilterCount = filterCount;
                if(userFilters?.Count > 0)
                {
                    applyFilter.AddRange(userFilters);
                }
                _contentService.SourceSearchService.SetEnabledUserFilters(applyFilter, 0);
                //IsOnlineOfflineVisible = _contentService.IsOnlineOfflineVisible(0);
                ResetCancelationToken();
                RequestMode requestMode = OnlineMode ? RequestMode.Online : RequestMode.Offline;
                if (await _contentService.SourceSearchService.PerformSearch(0, SearchText, requestMode, _cancellationTokenSource.Token))
                {
                    Device.BeginInvokeOnMainThread(async
                    () =>
                    {
                       

                        AllItems = await _contentService.ResultDataAsync(0, _cancellationTokenSource.Token);
                        SetDestinationRecordState();

                        if (AllItems != null && _firstLoad)
                        {
                            if(HasDestiRecords)
                            {
                                IsAllItemsMode = false;
                            }
                            _firstLoad = false;
                        }

                        await LoadUIItems();

                        EnableNoResultsText = !_contentService.SourceSearchService.HasResults(0);
                        if (EnableNoResultsText)
                        {
                            NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                                LocalizationKeys.KeyErrorsNoResults);
                        }
                        OnlineMode = _contentService.SourceSearchService.AreResultsRetrievedOnline(0);
                        LoadTotalItems();
                        SetPositionFilterItems(0);
                        IsLoading = false;
                    });
                }
                else
                {
                    IsLoading = false;
                    EnableNoResultsText = true;
                    NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                        LocalizationKeys.KeyErrorsNoResults);

                }
               
            }
            catch (Exception e)
            {
                _logService.LogError(e?.Message);
                IsLoading = false;
            }
        }

        private void SetPositionFilterItems(int orderIndex)
        {
            if (PositionFilterControlVisible && _hierarchicalPositionFilter && PositionFilterItems?.Count > 0)
            {
                foreach (var filterItem in PositionFilterItems)
                {
                    if (filterItem is CatalogBreadcrumbsFilter catalogBreadcrumbsFilter && catalogBreadcrumbsFilter.Order > orderIndex)
                    {
                        var allowedItems = new List<string>() { string.Empty };
                        if (_positionFilterkeyValuePairs.ContainsKey(catalogBreadcrumbsFilter.FilterKey)
                            && _positionFilterkeyValuePairs[catalogBreadcrumbsFilter.FilterKey]?.Count > 0)
                        {
                            allowedItems.AddRange(_positionFilterkeyValuePairs[catalogBreadcrumbsFilter.FilterKey]);
                        }
                        catalogBreadcrumbsFilter.RefreshFilterItems(allowedItems);
                    }
                }
            }
        }

        private void ClearPositionFilterItems(int orderIndex)
        {
            if (PositionFilterControlVisible && _hierarchicalPositionFilter && PositionFilterItems?.Count > 0)
            {
                foreach (var filterItem in PositionFilterItems)
                {
                    if (filterItem is CatalogBreadcrumbsFilter catalogBreadcrumbsFilter && catalogBreadcrumbsFilter.Order > orderIndex)
                    {
                        catalogBreadcrumbsFilter.ClearFilterItems();
                    }
                }
            }
        }

        private void SetDestinationRecordState()
        {
            if (AllItems != null && AllItems.Any(a => a.State == SerialEntryItemState.WithDestinationEntry))
            {
                HasDestiRecords = true;
            }
            else
            {
                HasDestiRecords = false;
            }
        }

        private async Task AllItemsCommandTapped()
        {
            IsAllItemsMode = true;
            await LoadUIItems();
            SetPositionFilterItems(0);
        }
        private async Task OverviewCommandTapped()
        {
            IsAllItemsMode = false;
            await LoadUIItems();
        }
        private async Task LoadUIItems()
        {
            IsLoading = true;
            
            var uiItems = new ObservableCollection<SerialEntryUiItem>();
            var positionFilterKeys = GetPositionFilterPairs();
            if (AllItems != null)
            {
                foreach (var item in AllItems)
                {

                    if (IsAllItemsMode)
                    {
                        if (item.Match(positionFilterKeys) && _contentService.ListingService.Match(_ListingOn, item))
                        {
                            var uiItem = new SerialEntryUiItem(this, item, _cancellationTokenSource);
                            await uiItem.LoadWidgetsData();
                            uiItems.Add(uiItem);
                            UpdatePositionFilterPairs(item);
                        }
                    }
                    else
                    {
                        if(item.State == SerialEntryItemState.WithDestinationEntry)
                        {
                            var uiItem = new SerialEntryUiItem(this, item, _cancellationTokenSource);
                            await uiItem.LoadWidgetsData();
                            uiItems.Add(uiItem);
                        }
                    }
                }
            }
            
            UIItems = uiItems;
            IsLoading = false;
        }

        private void UpdatePositionFilterPairs(SerialEntryItem item)
        {
            if (item != null && PositionFilterControlVisible && _hierarchicalPositionFilter && item.SearchKeyPairs.Keys.Count > 0)
            {
                foreach (var key in item.SearchKeyPairs.Keys)
                {
                    if (_positionFilterkeyValuePairs.ContainsKey(key))
                    {
                        if(!_positionFilterkeyValuePairs[key].Contains(item.SearchKeyPairs[key]))
                        {
                            _positionFilterkeyValuePairs[key].Add(item.SearchKeyPairs[key]);
                        }

                    }
                    else
                    {
                        _positionFilterkeyValuePairs.Add(key, new List<string>() { item.SearchKeyPairs[key] });
                    }
                    
                }
            }
        }

        private Dictionary<string, string> GetPositionFilterPairs()
        {
            if (PositionFilterControlVisible && PositionFilterItems.Count > 0)
            {
                _positionFilterkeyValuePairs?.Clear();
                var filterKeys = new Dictionary<string, string>();
                foreach (var filterItem in PositionFilterItems)
                {

                    if (filterItem is CatalogBreadcrumbsFilter catalogBreadcrumbsFilter)
                    {
                        var filterValue = catalogBreadcrumbsFilter?.SelectedFilterItem?.CatalogItem.RecordId;
                        if (filterValue != null)
                        {
                            var key = catalogBreadcrumbsFilter.FilterKey;
                            if(!string.IsNullOrWhiteSpace(key))
                            {
                                filterKeys.Add(key, filterValue);
                            }
                            
                        }

                    }

                }
                return filterKeys;
            }
            return null;
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
                    if (!OnlineMode
                        && _contentService.SourceSearchService.SearchAutoSwitchToOffline())
                    {
                        OnlineMode = false;
                    }
                    delay = _contentService.SourceSearchService.SearchDelay(!OnlineMode);
                }
                timer.StartOrReset(delay);
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

        private void LoadTotalItems()
        {
            TotalQuantity = AllItems != null ? AllItems.Select(i => i.Quantity).Sum() : 0 ;
            TotalNetPrice = AllItems != null ? AllItems.Select(i => i.NetPrice).Sum() : 0 ;
            TotalEndPrice = AllItems != null ? AllItems.Select(i => i.EndPrice).Sum() : 0 ;
            TotalFreeGoods = AllItems != null ? AllItems.Select(i => i.FreeGoods).Sum() : 0;
            TotalDiscount = AllItems != null ? AllItems.Select(i => i.Discount).Sum() : 0;
        }

        private Task OnSearchButton()
        {
            IsSearchBoxVisible = !IsSearchBoxVisible;
            return Task.CompletedTask;
        }

        private async Task OnViewModeButton()
        {
            if (IsListMode)
            {
                IsListMode = false;
                ViewModeButtonIconText = MaterialDesignIcons.ViewList;
            }
            else
            {
                IsListMode = true;
                ViewModeButtonIconText = MaterialDesignIcons.Apps;

            }
            
        }
        private async Task OnCancleButtonTapped()
        {
            await _navigationController.BackAsync();
        }
        private async Task OnBackButtonTapped()
        {
            SelectedItemIndex = -1;
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

            await PerformSearch();
        }

        public async Task SaveItem(SerialEntryUiItem item)
        {
            await item.SyncData();
        }

        public async Task ItemModified(SerialEntryItem item)
        {
            item.State = SerialEntryItemState.SaveInprogress;
            var index = AllItems.FindIndex(i => i.RowIdentification.Equals(item.RowIdentification));
            if(index > -1)
            {
                AllItems[index] = item;
            }
            bool isNew = string.IsNullOrWhiteSpace(item.DestRecordId);

            await UpdateRootPanels();
            
            bool result = await _contentService.SaveItem(item, RootPanels, _cancellationTokenSource.Token);


            if (result)
            {
                if (index < 0)
                {
                    AllItems.Add(item);
                    await LoadUIItems();
                }
                else
                {
                    AllItems[index] = item;
                }
                LoadTotalItems();
                SetDestinationRecordState();
            }

        }

        private async Task UpdateRootPanels()
        {
            LoadTotalItems();
            await RootWidgets.UpdateField("Quantity", TotalQuantity);
            await RootWidgets.UpdateField("EndPrice", TotalEndPrice);
            await RootWidgets.UpdateField("Discount", TotalDiscount);
            await RootWidgets.UpdateField("NetPrice", TotalNetPrice);
            await RootWidgets.UpdateField("FreeGoods", TotalFreeGoods);
            RootPanels = RootWidgets?.GetPanelDatas();
        }

        public async Task ItemDeleted(SerialEntryItem item)
        {
            item.State = SerialEntryItemState.SaveInprogress;
            var index = AllItems.FindIndex(i => i.RowIdentification.Equals(item.RowIdentification));

            if (await _contentService.DeleteItem(item, _cancellationTokenSource.Token))
            {

                if (index > -1)
                {
                    var Item = AllItems.FirstOrDefault(i => i.RowIdentification.Equals(item.RowIdentification));
                    if (Item != null)
                    {
                        AllItems.Remove(Item);
                    }

                    await LoadUIItems();

                }
                LoadTotalItems();
                SetDestinationRecordState();
            }
            else
            {
                item.State = SerialEntryItemState.InErrorState;
            }

        }

        public async Task ItemSelected(SerialEntryItem item)
        {
            SelectedItemIndex = UIItems.FindIndex(i => i.RowIdentification.Equals(item.RowIdentification));
        }

        private async Task LoadChildData(SerialEntryUiItem selectedItem)
        {
            var buzyState = selectedItem.IsBusy;
            try
            {
                selectedItem.IsBusy = true;
                await LoadChildPanels(selectedItem);
                await selectedItem.LoadChildWidgets();
                selectedItem.IsBusy = buzyState;

            }
            catch (Exception ex)
            {
                selectedItem.IsBusy = buzyState;
                _logService.LogError(ex?.Message);
            }

        }

        public async Task LoadChildPanels(SerialEntryItem selectedItem)
        {
            await _contentService.BuildDestinationChildPanels(selectedItem, _cancellationTokenSource.Token);
        }

        private async Task FilterCommandHandler()
        {
            await _navigationController.NavigateToAsync<FilterUIPageViewModel>(parameter: this);
        }

        public async Task<List<FilterUI>> GetUserFilters()
        {
            return searchFilters;
        }

        public async Task ApplyUserFilters(List<FilterUI> filters)
        {
            searchFilters = filters;
            await PerformAsyncSearch();
        }

        public async Task ApplyPositionFilters(BreadcrumbsFilter filter)
        {
            if (filter is MultiBreadcrumbsFilter)
            {
                await PerformAsyncSearch();
            }
            else if (filter is CatalogBreadcrumbsFilter catalogBreadcrumbsFilter)
            {
                ClearPositionFilterItems(catalogBreadcrumbsFilter.Order);
                await LoadUIItems();
                SetPositionFilterItems(catalogBreadcrumbsFilter.Order);
            }
            else if (filter is ListingBreadcrumbsFilter listingBreadcrumbsFilter)
            {
                _ListingOn = listingBreadcrumbsFilter.IsListingOn;
                ClearPositionFilterItems(listingBreadcrumbsFilter.Order);
                await LoadUIItems();
                SetPositionFilterItems(listingBreadcrumbsFilter.Order);
            }
        }

        public async Task EvaluatePricing(SerialEntryItem serialEntryItem)
        {
            await _contentService?.EvaluatePricing(serialEntryItem, _cancellationTokenSource.Token);
        }
    }
}
