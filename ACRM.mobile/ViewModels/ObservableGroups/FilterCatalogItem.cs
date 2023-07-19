using System;
using System.Collections.ObjectModel;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.ViewModels.ObservableGroups
{
    public class FilterCatalogItem : ExtendedBindableObject
    {
        private SelectableFieldValue _catalogItem = null;
        public  SelectableFieldValue CatalogItem
        {
            get => _catalogItem;
            set
            {
                _catalogItem = value;
                RaisePropertyChanged(() => CatalogItem);
            }
        }

        private bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                RaisePropertyChanged(() => Selected);
            }
        }

        public FilterCatalogItem()
        {
        }
    }
}
