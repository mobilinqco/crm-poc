using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.ViewModels.ObservableGroups.Breadcrumbs
{
    public class MultiBreadcrumbsFilter : BreadcrumbsFilter
    {
        private Dictionary<string, string> _filterName = null;
        public Dictionary<string, string> FilterNames
        {
            get => _filterName;
            set
            {
                _filterName = value;
                RaisePropertyChanged(() => FilterNames);
            }
        }
        public override string Title
        {
            get
            {
                var selectedFilterItem = SelectedFilterItem;
                if (selectedFilterItem == null || string.IsNullOrEmpty(selectedFilterItem.CatalogItem.RecordId))
                {
                    return string.Empty;
                }
                else
                {
                    return selectedFilterItem?.CatalogItem?.DisplayValue;
                }

            }
        }

        public string selectedFilterName
        {
            get
            {
                return SelectedFilterItem?.CatalogItem?.RecordId;
            }
        }
        public MultiBreadcrumbsFilter(Dictionary<string, string> filterName, IBreadcrumbsFilterParent parent) : base(parent)
        {
            FilterNames = filterName;
        }

        public override FieldInfo FieldInfo
        {
            get
            {
                return null;
            }
        }

        public async override Task<bool> LoadFilterItems(CancellationToken cancellationToken)
        {
            bool firstItem = true;
            foreach (var key in FilterNames.Keys)
            {
                FilterItems.Add(new FilterCatalogItem { Selected = firstItem,
                    CatalogItem =  new SelectableFieldValue {
                        DisplayValue = FilterNames[key],
                        RecordId = key
                    }
                });
                firstItem = false;
            }
            RaisePropertyChanged(() => Title);
            return true;
        }
    }
}
