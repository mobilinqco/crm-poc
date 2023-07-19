using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class HierarchicalCatalogFilterControlModel: CatalogFilterControlModel
    {
        private readonly IConfigurationService _configurationService;
        private List<SelectableFieldValue> _parentCatalogItems = null;

        public List<SelectableFieldValue> ParentCatalogItems
        {
            get => _parentCatalogItems;
            set
            {
                _parentCatalogItems = value;
                RaisePropertyChanged(() => ParentCatalogItems);
            }
        }

        private bool _isParentView = true;
        public bool IsParentView
        {
            get => _isParentView;
            set
            {
                _isParentView = value;
                RaisePropertyChanged(() => IsParentView);
            }
        }

        private string _catalogParentTitle = null;
        public string CatalogParentTitle
        {
            get => _catalogParentTitle;
            set
            {
                _catalogParentTitle = value;
                RaisePropertyChanged(() => CatalogParentTitle);
            }
        }

        public ICommand ParentCatalogSelectedCommand => new Command<SelectableFieldValue>(async (parent) => await SelectChildCatalog(parent));

        public HierarchicalCatalogFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(filter, parentCancellationTokenSource)
        {
            _configurationService = AppContainer.Resolve<IConfigurationService>();
        }

        public override async ValueTask<bool> InitializeControl()
        {
            IsLoading = true;
            if (Filter.FieldInfo != null && Filter.FieldInfo.Ucat>0)
            {
                var infoAreaid = Filter.FieldInfo.TableInfoInfoAreaId;
                var tableinfo = await _configurationService.GetTableInfoAsync(infoAreaid, _cancellationTokenSource.Token);
                FieldInfo parentFieldInfo = _configurationService.GetFieldInfo(tableinfo, Filter.FieldInfo.Ucat);
                CatalogParentTitle = $"<< { parentFieldInfo.Name}";
                ParentCatalogItems = await CatalogObject.GetCatalogDisplayListAsync(parentFieldInfo, _cancellationTokenSource.Token);
                CatalogItems = new List<FilterCatalogItem>();
                if (Filter.FilterData is List<FilterCatalogItem> catalogItems)
                {
                    foreach (var item in catalogItems)
                    {
                        CatalogItems.Add(item);
                    }
                    ClearFilterItems();
                    await PerformAsyncSearch();
                }

            }
            IsLoading = false;
            return true;
        }

        public ICommand OnBackParentTapped => new Command(async () =>
        {
            IsParentView = true;
        });

        private async Task SelectChildCatalog(SelectableFieldValue parent)
        {
            var ParentCode = int.Parse(parent.RecordId);
            IsParentView = false;
            CatalogItems = new List<FilterCatalogItem>();
            if (AllowEmptyItem)
            {
                CatalogItems.Add(new FilterCatalogItem
                {
                    CatalogItem = new SelectableFieldValue
                    {
                        Id = -1,
                        DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicEmptyCatalog),
                        RecordId = "",
                        ParentCode = ParentCode
                    }
                });
            }

            var catalogItems = await CatalogObject.GetCatalogDisplayListAsync(Filter.FieldInfo, _cancellationTokenSource.Token);
            if (catalogItems != null && catalogItems.Count > 0)
            {
                var filteredCatalogs = catalogItems.Where(a => a.ParentCode == ParentCode).ToList();
                if (filteredCatalogs != null && filteredCatalogs.Count > 0)
                {
                    foreach (var item in filteredCatalogs)
                    {
                        CatalogItems.Add(new FilterCatalogItem { CatalogItem = item });
                    }
                }
            }

            ClearFilterItems();
            await PerformAsyncSearch();
        }
    }
}
