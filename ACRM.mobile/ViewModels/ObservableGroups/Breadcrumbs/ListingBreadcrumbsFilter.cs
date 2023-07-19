using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;

namespace ACRM.mobile.ViewModels.ObservableGroups.Breadcrumbs
{
    public class ListingBreadcrumbsFilter : BreadcrumbsFilter
    {
        private readonly IFilterProcessor _filterProcessor;
        private readonly bool _hasListingItems;
        private readonly IConfigurationService _configurationService;
        private string _filterName = null;
        public string FilterName
        {
            get => _filterName;
            set
            {
                _filterName = value;
                RaisePropertyChanged(() => FilterName);
            }
        }
        public bool IsListingOn
        {
            get => SelectedFilterItem?.CatalogItem?.RecordId == "1";
        }
        public List<FilterCatalogItem> AllFilterItems { get; set; } = new List<FilterCatalogItem>();
        public ListingBreadcrumbsFilter(string filterName, IBreadcrumbsFilterParent parent, bool hasListingItems) : base(parent)
        {
            FilterName = filterName;
            _hasListingItems = hasListingItems;
            _filterProcessor = AppContainer.Resolve<IFilterProcessor>();
            _configurationService = AppContainer.Resolve<IConfigurationService>();
        }

        public override FieldInfo FieldInfo => null;

        public override string Title
        {
            get
            {
                return _localizationController.GetString(LocalizationKeys.TextGroupProcesses,
                LocalizationKeys.KeyProcessesFilterListings, "Listing");
            }
        }

        public async override Task<bool> LoadFilterItems(CancellationToken cancellationToken)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(FilterName))
            {
                if (FilterName.IsListing())
                {
                    AllFilterItems.Add(new FilterCatalogItem
                    {

                        CatalogItem = new SelectableFieldValue
                        {
                            Id = -1,
                            DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupSerialEntry,
            LocalizationKeys.KeySerialEntryNoListing, "No Listing"),
                            RecordId = "0"
                        },
                        Selected = true,
                    });

                    if (_hasListingItems)
                    {
                        AllFilterItems.Add(new FilterCatalogItem
                        {

                            CatalogItem = new SelectableFieldValue
                            {
                                Id = -1,
                                DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupProcesses,
                LocalizationKeys.KeyProcessesFilterListings, "Listing"),
                                RecordId = "1"
                            },
                            Selected = false,
                        });
                    }
                    result = true;
                }
                FilterItems = AllFilterItems;
                RaisePropertyChanged(() => Title);
                RaisePropertyChanged(() => FilterItems);
            }

            return result;
        }
    }
}
