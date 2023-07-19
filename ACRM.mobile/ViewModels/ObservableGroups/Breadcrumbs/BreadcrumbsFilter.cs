using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels.ObservableGroups.Breadcrumbs
{
    public abstract class BreadcrumbsFilter : ExtendedBindableObject, IPopupItemSelectionHandler
    {
        protected readonly INavigationController _navigationController;
        protected readonly ILocalizationController _localizationController;
        protected readonly IBreadcrumbsFilterParent _parent;
        public Guid RowIdentification { get; set; }
        public int Order { get; set; }

        private Filter _filter = null;
        public Filter Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                RaisePropertyChanged(() => Filter);
            }
        }


        public FilterCatalogItem SelectedFilterItem
        {
            get => FilterItems.Where(a=>a.Selected).FirstOrDefault();

        }

        public abstract FieldInfo FieldInfo { get; }

        public abstract string Title { get; }


        public List<FilterCatalogItem> FilterItems { get; set; } = new List<FilterCatalogItem>();


        public BreadcrumbsFilter(IBreadcrumbsFilterParent parent)
        {
            _parent = parent;
            RowIdentification = Guid.NewGuid();
            _navigationController = AppContainer.Resolve<INavigationController>();
            _localizationController = AppContainer.Resolve<ILocalizationController>();
        }

        public abstract Task<bool> LoadFilterItems(CancellationToken cancellationToken);
     

        public ICommand FilterTappedCommand => new Command(async () => await OnFilterTapped());

        private async Task OnFilterTapped()
        {
            if (FilterItems?.Count > 0)
            {
                await _navigationController.NavigateToAsync<PopupListPageViewModel>(parameter: this);
            }
        }

        public async Task<List<PopupListItem>> GetPoupList()
        {

            if (FilterItems.Count > 0)
            {
                var items = new List<PopupListItem>();
                foreach (var item in FilterItems)
                {
                    items.Add(new PopupListItem
                    {
                        RecordId = item.CatalogItem.RecordId,
                        DisplayText = item.CatalogItem.DisplayValue,
                        OrginalObject = item,
                        Selected = item.Selected

                    }); ;
                }
                return items;
            }
            return null;
        }

        public async Task PopupItemSelected(PopupListItem item)
        {
            if (item != null)
            {
                if(SelectedFilterItem?.CatalogItem?.RecordId == item.RecordId)
                {
                    return;
                }

                foreach (var filterItem in FilterItems)
                {
                    if(item.RecordId == filterItem.CatalogItem.RecordId)
                    {
                        filterItem.Selected = true;
                    }
                    else
                    {
                        filterItem.Selected = false;
                    }
                }

                RaisePropertyChanged(() => Title);
                await _parent.ApplyPositionFilters(this);
            }
        }



    }
}
