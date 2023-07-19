using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Utils;

namespace ACRM.mobile.ViewModels.ObservableGroups.Breadcrumbs
{
    public class CatalogBreadcrumbsFilter : BreadcrumbsFilter
    {
        private readonly CatalogComponent _catalogComponent;
        private readonly IFilterProcessor _filterProcessor;
        private readonly IConfigurationService _configurationService;
        private FieldInfo _fieldInfo;
        private Filter _filter;
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

        public List<FilterCatalogItem> AllFilterItems { get; set; } = new List<FilterCatalogItem>();

        public Filter ProcessedFilter
        {
            get
            {
                if (_filter != null)
                {
                    if (FilterItems.Count > 0 && !string.IsNullOrWhiteSpace(SelectedFilterItem?.CatalogItem?.RecordId))
                    {
                        var outfilter = _filter.DeepClone();
                        if (outfilter?.RootTable?.ExpandedConditions?.FieldValues != null)
                        {
                            outfilter?.RootTable?.ExpandedConditions?.FieldValues.Clear();
                            foreach (var items in FilterItems)
                            {
                                if (items.Selected)
                                {
                                    outfilter.RootTable?.ExpandedConditions?.FieldValues.Add(items.CatalogItem.RecordId);
                                }
                            }

                        }
                        return outfilter;
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
        }

        public override string Title
        {
            get
            {
                var selectedFilterItem = SelectedFilterItem;
                if(selectedFilterItem == null || selectedFilterItem.CatalogItem.RecordId.Equals(string.Empty))
                {
                    if (_fieldInfo != null)
                    {
                        return _fieldInfo.Name;
                    }
                }
                else
                {
                    return selectedFilterItem?.CatalogItem?.DisplayValue;
                }

                return string.Empty;
            }
        }


        public override FieldInfo FieldInfo
        {
            get
            {
                return _fieldInfo;
            }
        }

        public string FilterKey
        {
            get
            {
                if (_fieldInfo == null)
                {
                    return string.Empty;
                }
                else
                {
                    return $"{_fieldInfo.TableInfoInfoAreaId.ToLower()}_{_fieldInfo.FieldId}";
                }
            }
        }

        public CatalogBreadcrumbsFilter(string filterName, IBreadcrumbsFilterParent parent) : base(parent)
        {
            FilterName = filterName;
            _catalogComponent = AppContainer.Resolve<CatalogComponent>();
            _filterProcessor = AppContainer.Resolve<IFilterProcessor>();
            _configurationService = AppContainer.Resolve<IConfigurationService>();
        }

        public async override Task<bool> LoadFilterItems(CancellationToken cancellationToken)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(FilterName))
            {
                _filter = await _filterProcessor.RetrieveFilterDetails(FilterName, cancellationToken);
                if (_filter != null)
                {

                    _fieldInfo = await getFieldInfo(_filter, cancellationToken);
                    if (_fieldInfo != null)
                    {
                        AllFilterItems.Add(new FilterCatalogItem
                        {

                            CatalogItem = new SelectableFieldValue
                            {
                                Id = -1,
                                DisplayValue = "All",
                                RecordId = string.Empty
                            },
                            Selected = true
                        });
                        AllFilterItems.Add(new FilterCatalogItem
                        {

                            CatalogItem = new SelectableFieldValue
                            {
                                Id = 0,
                                DisplayValue = "<Empty>",
                                RecordId = "0"
                            },
                            Selected = true
                        });
                        var catalogItems = await _catalogComponent.GetCatalogDisplayListAsync(_fieldInfo, cancellationToken);
                        if (catalogItems != null && catalogItems.Count > 0)
                        {
                            foreach (var item in catalogItems)
                            {
                                AllFilterItems.Add(new FilterCatalogItem { CatalogItem = item });
                            }
                        }
                        result = true;
                    }

                }
                RaisePropertyChanged(() => Title);
            }

            return result;
        }

        public void RefreshFilterItems(List<string> items)
        {
            if (items != null)
            {
                var selectedFilterItem = SelectedFilterItem;
                if (items?.Count > 0)
                {
                    var filterItems = AllFilterItems.Where(x => items.Contains(x.CatalogItem.RecordId)).ToList();
                    if (filterItems?.Count > 0)
                    {
                        if (selectedFilterItem != null)
                        {
                            foreach (var filterItem in filterItems)
                            {
                                if (selectedFilterItem.CatalogItem.RecordId == filterItem.CatalogItem.RecordId)
                                {
                                    filterItem.Selected = true;
                                }
                                else
                                {
                                    filterItem.Selected = false;
                                }
                            }
                        }
                    }
                    FilterItems = filterItems;
                }
            }
            else
            {
                FilterItems = AllFilterItems;
                // Load all the items, When HierarchicalPositionFilter is disabled
            }
            RaisePropertyChanged(() => FilterItems);
            RaisePropertyChanged(() => Title);

        }

        public void ClearFilterItems()
        {
            var filterItems = AllFilterItems.Where(x => x.CatalogItem.RecordId.Equals(string.Empty)).ToList();
            if (filterItems?.Count > 0)
            {
                foreach (var filterItem in filterItems)
                {
                    if (filterItem.CatalogItem.RecordId.Equals(string.Empty))
                    {
                        filterItem.Selected = true;
                    }
                    else
                    {
                        filterItem.Selected = false;
                    }
                }

            }
            FilterItems = filterItems;
            RaisePropertyChanged(() => FilterItems);
            RaisePropertyChanged(() => Title);

        }

        private async Task<FieldInfo> getFieldInfo(Filter filter, CancellationToken cancellationToken)
        {
            var infoAreaid = filter?.RootTable?.InfoAreaId;
            var fieldID = filter?.RootTable?.ExpandedConditions?.FieldId;
            var subTables = filter?.RootTable?.SubTables;
            if (filter.RootTable?.ExpandedConditions == null && subTables != null && subTables.Count > 0)
            {
                infoAreaid = subTables[0]?.InfoAreaId;
                fieldID = subTables[0]?.ExpandedConditions?.FieldId;

            }

            if (fieldID == null || fieldID == 0)
            {
                var conditions = filter?.RootTable?.ExpandedConditions?.Conditions;
                if (conditions != null && conditions.Count > 1)
                {
                    fieldID = conditions[0].FieldId;
 
                }
            }

            if (!string.IsNullOrWhiteSpace(infoAreaid) && fieldID != null)
            {
                var tableinfo = await _configurationService.GetTableInfoAsync(infoAreaid, cancellationToken);
                FieldInfo fieldInfo = _configurationService.GetFieldInfo(tableinfo, fieldID.Value);
                if (fieldInfo != null)
                {
                    if (fieldInfo.IsCatalog)
                    {
                        return fieldInfo;
                    }
                }
            }

            return null;
        }
    }
}
