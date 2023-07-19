using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Services.Utils;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class CatalogFilterControlModel : BaseFilterControlModel
    {
        private readonly ResetTimer timer;
        public List<FilterCatalogItem> CatalogItems { get; set; }

        private ObservableRangeCollection<FilterCatalogItem> _flteredCatalogItems = null;
        public ObservableRangeCollection<FilterCatalogItem> FilteredCatalogItems
        {
            get => _flteredCatalogItems;
            set
            {
                var start = DateTime.Now;
                _flteredCatalogItems = value;
                RaisePropertyChanged(() => FilteredCatalogItems);
                var loadingTime = (DateTime.Now - start).TotalMilliseconds;
                _logService.LogDebug($"Notifying the UI was done in {loadingTime} miliseconds");
            }
        }

        private readonly CatalogComponent _catalogComponent;
        private readonly IConfigurationService _configurationService;
        protected CatalogComponent CatalogObject => _catalogComponent;
        public Dictionary<string, int> ItemIndex = new Dictionary<string, int>();
        public ICommand SearchCommand => new Command(async () => PerformSearch(true));
        public ICommand SelectedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>(async evt => await SelectItem(evt));

        public CatalogFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(filter, parentCancellationTokenSource)
        {
            FilteredCatalogItems = new ObservableRangeCollection<FilterCatalogItem>();
            _catalogComponent = AppContainer.Resolve<CatalogComponent>();
            _configurationService = AppContainer.Resolve<IConfigurationService>();
            this.timer = new ResetTimer(this.PerformAsyncSearch);
        }

        private void PerformSearch(bool useDelay = false)
        {
            try
            {
                double delay = 0;
                if (useDelay)
                {
                    delay = CrmConstants.DefaultSearchDelayTime;
                }
                timer.StartOrReset(delay);
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

        protected async Task PerformAsyncSearch()
        {
            try
            {
                IsLoading = true;
                var start = DateTime.Now;

                SetFiletredItems();
                ClearFilterItems();

                if (string.IsNullOrWhiteSpace(StringValue))
                {
                    FilteredCatalogItems.AddRange(CatalogItems);
                }
                else
                {
                    for (int i = 0; i < CatalogItems.Count; i++)
                    {
                        var item = CatalogItems[i];
                        if (item.CatalogItem.DisplayValue.IndexOf(StringValue, 0, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            FilteredCatalogItems.Add(item);
                            if (!ItemIndex.Keys.Contains(item.CatalogItem.RecordId))
                            {
                                ItemIndex.Add(item.CatalogItem.RecordId, i);
                            }
                        }
                    }

                }
                var loadingTime = (DateTime.Now - start).TotalMilliseconds;
                _logService.LogDebug($"Loading of filter data was done in {loadingTime} miliseconds");
                IsLoading = false;
            }
            catch (Exception e)
            {
                _logService.LogError(e?.Message);
                IsLoading = false;
            }
        }

        private void SetFiletredItems()
        {
            if (FilteredCatalogItems != null && FilteredCatalogItems.Count > 0)
            {
                foreach (var filterItem in FilteredCatalogItems)
                {
                    if (ItemIndex.Keys.Contains(filterItem.CatalogItem.RecordId))
                    {
                        var index = ItemIndex[filterItem.CatalogItem.RecordId];
                        CatalogItems[index].Selected = filterItem.Selected;
                    }
                }
            }
        }

        protected void ClearFilterItems()
        {
            FilteredCatalogItems.Clear();
            ItemIndex.Clear();
        }

        public bool AllowEmptyItem
        {
            get
            {
                bool allowEmpty = true;
                if (_configurationService.GetConfigValue("UserFilter.AllowEmptyItem") != null)
                {
                    allowEmpty = _configurationService.GetBoolConfigValue("UserFilter.AllowEmptyItem", true);
                }
                return allowEmpty;
            }

        }

        public override async ValueTask<bool> InitializeControl()
        {
            var start = DateTime.Now;
            if (Filter.FieldInfo != null)
            {
                IsLoading = true;
                CatalogItems = new List<FilterCatalogItem>();
                if (Filter.FilterData == null)
                {
                    var catalogItems = await _catalogComponent.GetCatalogDisplayListAsync(Filter.FieldInfo, _cancellationTokenSource.Token);
                    if (catalogItems != null && catalogItems.Count > 0)
                    {
                        if (AllowEmptyItem)
                        {
                            CatalogItems.Add(new FilterCatalogItem
                            {
                                CatalogItem = new SelectableFieldValue
                                {
                                    Id = -1,
                                    DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicEmptyCatalog),
                                    RecordId = ""
                                }
                            });
                        }
                        foreach (var item in catalogItems)
                        {
                            CatalogItems.Add(new FilterCatalogItem { CatalogItem = item });
                        }
                    }
                }
                else if (Filter.FilterData is List<FilterCatalogItem> catalogItems)
                {
                    foreach (var item in catalogItems)
                    {
                        CatalogItems.Add(item);
                    }
                }

                await PerformAsyncSearch();
            }

            var loadingTime = (DateTime.Now - start).TotalMilliseconds;
            _logService.LogDebug($"Initialization of filter UI was done in {loadingTime} miliseconds");
            return true;
        }

        public override bool HasData
        {
            get => CatalogItems.Any(a => a.Selected);
        }

        public override FilterUI ProcessedFilter
        {
            get
            {
                if (Filter != null)
                {
                    if (CatalogItems != null)
                    {
                        SetFiletredItems();
                        Filter.FilterData = CatalogItems.ToList();
                        Dictionary<string, string> Values = new Dictionary<string, string>();

                        foreach (var items in CatalogItems)
                        {
                            if (items.Selected)
                            {
                                Values.Add($"*{items.CatalogItem.RecordId}", items.CatalogItem.RecordId);
                            }
                        }

                        SetFilterValues(Filter, Values);
                    }
                    return Filter;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task SelectItem(Syncfusion.ListView.XForms.ItemTappedEventArgs eventArgs)
        {
            if(eventArgs.ItemData is FilterCatalogItem catalogItem)
            {
                catalogItem.Selected = !catalogItem.Selected;
            }
        }
    }
}
