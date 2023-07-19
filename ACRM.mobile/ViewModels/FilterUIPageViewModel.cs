using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.CustomControls.FilterControls.Models;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class FilterUIPageViewModel : BaseViewModel
    {
        private readonly IConfigurationService _configurationService;
        private IFilterItemSelectionHandler PatentHandler;

        public ICommand OnCloseButtonTapped => new Command(async () =>
        {
            PatentHandler.CanApplyFilter = ApplyFilter;
            await _navigationController.PopPopupAsync();
            await PatentHandler?.ApplyUserFilters(UserFilters.ToList());
        });

        public ICommand OnBackButtonTapped => new Command(async () =>
        {
            if (Content is BaseFilterControlModel editWidget)
            {
                UserFilters[_selectedItemIndex] = editWidget.ProcessedFilter;
                UserFilters[_selectedItemIndex].IsEnabled = editWidget.HasData;
            }
            SelectedItemIndex = -1;
        });

        public ICommand ClearAllButtonTapped => new Command(async () =>
        {
            if (UserFilters != null && UserFilters.Count > 0)
            {
                foreach (var item in UserFilters)
                {
                    item.FilterData = null;
                    item.IsEnabled = false;
                }
            }
        });

        public ICommand ItemTappedCommand => new Command<FilterUI>(async (item) =>
        {
            if (item != null)
            {
                if (item.IsEnabled)
                {
                    item.FilterData = null;
                    item.IsEnabled = false;
                }
                else
                {
                    item.IsEnabled = true;
                    SelectedItemIndex = UserFilters.FindIndex(i => i.RowIdentification.Equals(item.RowIdentification));
                }
            }
        });

        private ObservableCollection<FilterUI> _userFilters;
        public ObservableCollection<FilterUI> UserFilters
        {
            get => _userFilters;
            set
            {
                _userFilters = value;
                RaisePropertyChanged(() => UserFilters);
            }
        }

        public FilterUI SelectedItem
        {
            get
            {
                if (UserFilters != null && _selectedItemIndex > -1)
                {
                    return UserFilters[_selectedItemIndex];
                }
                else
                {
                    return null;
                }

            }
        }

        private UIWidget _content;
        public UIWidget Content
        {
            get => _content;
            set
            {
                _content = value;
                RaisePropertyChanged(() => Content);
            }
        }

        private int _selectedItemIndex = -1;
        public int SelectedItemIndex
        {
            get => _selectedItemIndex;
            set
            {
                var currentItem = SelectedItem;
                if (UserFilters == null || value >= UserFilters.Count)
                {
                    _selectedItemIndex = -1;
                }
                else
                {
                    _selectedItemIndex = value;
                }

                RaisePropertyChanged(() => SelectedItemIndex);
                RaisePropertyChanged(() => SelectedItem);

                if (_selectedItemIndex > -1 && SelectedItem != null && SelectedItem.HasParameter())
                {
                    IsDetailView = true;
                    new Action(async () => await ProcessItems(currentItem, SelectedItem))();
                }
                else
                {
                    IsDetailView = false;
                }
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

        private bool _applyFilter = true;
        public bool ApplyFilter
        {
            get => _applyFilter;
            set
            {
                _applyFilter = value;
                RaisePropertyChanged(() => ApplyFilter);
            }
        }

        private bool _showApplyButton = false;
        public bool ShowApplyButton
        {
            get => _showApplyButton;
            set
            {
                _showApplyButton = value;
                RaisePropertyChanged(() => ShowApplyButton);
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

        private string _filterTitle = "Filter";
        public string FilterTitle
        {
            get => _filterTitle;
            set
            {
                _filterTitle = value;
                RaisePropertyChanged(() => FilterTitle);
            }
        }

        private string _clearAllText = "Clear all";
        public string ClearAllText
        {
            get => _clearAllText;
            set
            {
                _clearAllText = value;
                RaisePropertyChanged(() => ClearAllText);
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

        private string _backButtonTitle = "Back";
        public string BackButtonTitle
        {
            get => _backButtonTitle;
            set
            {
                _backButtonTitle = value;
                RaisePropertyChanged(() => BackButtonTitle);
            }
        }
        private string _applyText = "Apply";
        public string ApplyText
        {
            get => _applyText;
            set
            {
                _applyText = value;
                RaisePropertyChanged(() => ApplyText);
            }
        }
        
        public FilterUIPageViewModel(IConfigurationService configurationService)
        {
            if (Device.Idiom == TargetIdiom.Phone)
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
            BackButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                LocalizationKeys.KeyBasicBackButton);
            ClearAllText = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                LocalizationKeys.KeyBasicClearAll);
            FilterTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                LocalizationKeys.KeyBasicFilter);
            ApplyText = _localizationController.GetString(LocalizationKeys.TextButtonCore,
                LocalizationKeys.TextButtonCoreApply, ApplyText);
            _configurationService = configurationService;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            if (navigationData is IFilterItemSelectionHandler parentObj)
            {
                PatentHandler = parentObj;
                var items = await PatentHandler.GetUserFilters();
                ApplyFilter = PatentHandler.CanApplyFilter;
                if (items != null && items.Count > 0)
                {
                    UserFilters = new ObservableCollection<FilterUI>(items);
                }
                else
                {
                    UserFilters = new ObservableCollection<FilterUI>();
                }

            }
            await base.InitializeAsync(navigationData);
            bool showApplyButton = false;
            if (_configurationService.GetConfigValue("UserFilter.ShowApplyFilterButton") != null)
            {
                showApplyButton = _configurationService.GetBoolConfigValue("UserFilter.ShowApplyFilterButton", false);
            }
            ShowApplyButton = showApplyButton;
            if (!showApplyButton)
            {
                ApplyFilter = true;
            }
            IsLoading = false;
        }

        private async Task ProcessItems(FilterUI currentItem, FilterUI selectedItem)
        {
            if (selectedItem != null)
            {
                string infoAreaid = null;
                int? fieldID = null;
                bool hasMultiCondition = false;

                List<NodeCondition> nodeConditions = selectedItem.Filter?.GetParValueNodes();

                if(nodeConditions?.Count > 0)
                {
                    infoAreaid = nodeConditions[0].InfoAreaId;
                    fieldID = nodeConditions[0].FieldId;
                }

                if (nodeConditions?.Count > 1)
                {

                    if(nodeConditions.Count(n => n.FieldId == fieldID) >1)
                    {
                        hasMultiCondition= true;
                    }
                }


                if (!string.IsNullOrWhiteSpace(infoAreaid) && fieldID != null)
                {
                    var tableinfo = await _configurationService.GetTableInfoAsync(infoAreaid, _cancellationTokenSource.Token);
                    FieldInfo fieldInfo = _configurationService.GetFieldInfo(tableinfo, fieldID.Value);
                    selectedItem.FieldInfo = fieldInfo;

                    if (fieldInfo != null)
                    {
                        if (fieldInfo.IsCatalog)
                        {
                            if (fieldInfo.Ucat > 0)
                            {
                                Content = new HierarchicalCatalogFilterControlModel(selectedItem, _cancellationTokenSource);
                            }
                            else
                            {
                                Content = new CatalogFilterControlModel(selectedItem, _cancellationTokenSource);
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(fieldInfo.RepMode))
                        {
                            Content = new RepFilterControlModel(selectedItem, _cancellationTokenSource);
                        }
                        else if (fieldInfo.IsBoolean)
                        {
                            Content = new BoolFilterControlModel(selectedItem, _cancellationTokenSource);
                        }
                        else if (fieldInfo.IsDate)
                        {
                            if (hasMultiCondition)
                            {
                                Content = new DateRangeFilterControlModel(selectedItem, _cancellationTokenSource);
                            }
                            else
                            {
                                Content = new DateFilterControlModel(selectedItem, _cancellationTokenSource);
                            }
                        }
                        else
                        {
                            if (hasMultiCondition)
                            {
                                Content = new TextRangeFilterControlModel(selectedItem, _cancellationTokenSource);
                            }
                            else
                            {
                                Content = new TextFilterControlModel(selectedItem, _cancellationTokenSource);
                            }
                        }

                        Content.ParentBaseModel = this;
                        await Content.InitializeControl();
                    }

                }
            }
        }

    }
}
