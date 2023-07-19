using System;
using System.Collections.Generic;
using System.Text;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.ViewModels.ObservableGroups
{
    public class FilterUI : ExtendedBindableObject
    {
        public Guid RowIdentification { get; set; }
        public FieldInfo FieldInfo { get; set; }
        private object _filterData = null;
        public object FilterData
        {
            get => _filterData;
            set
            {
                _filterData = value;
                RaisePropertyChanged(() => SelectedText);
            }
        }

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

        private Filter _outputFilter = null;
        public Filter OutputFilter
        {
            get
            {
                return _outputFilter == null ? Filter : _outputFilter;
            }
            set
            {
                _outputFilter = value;
            }
        }

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        public string DisplayName
        {
            get
            {
                var filterName = string.Empty;
                if(Filter == null)
                {
                    return filterName;
                }
                else if(!string.IsNullOrWhiteSpace(Filter.DisplayName))
                {
                    filterName = Filter.DisplayName;
                }
                else if (!string.IsNullOrWhiteSpace(Filter.UnitName))
                {
                    filterName = Filter.UnitName;
                }
                return filterName;
            } 

        }

        public string SelectedText
        {
            get
            {
                var selectedText = string.Empty;
                if (FilterData is string data)
                {
                    selectedText = data;
                }
                else if (FilterData is List<FilterCatalogItem> catalogItems)
                {
                    StringBuilder sBuilder = new StringBuilder();
                    bool isEmpty = true;
                    foreach(var item in catalogItems)
                    {
                        if(item.Selected)
                        {
                            if(!isEmpty)
                            {
                                sBuilder.Append(" , ");
                            }
                            sBuilder.Append(item.CatalogItem.DisplayValue);
                            isEmpty = false;
                        }

                    }
                    selectedText = sBuilder.ToString();
                }
                else if (FilterData is List<string> items && items?.Count >1)
                {
                    selectedText = $"{items[0]} - {items[1]}";
                }
                else if (FilterData is List<DateTime> dates && dates?.Count > 1)
                {
                    selectedText = $"{dates[0].ToLongDateString()} - {dates[1].ToLongDateString()}";
                }
                return selectedText;
            }

        }

        public FilterUI(Filter filter, bool isEnabled)
        {
            Filter = filter;
            RowIdentification = Guid.NewGuid();
            _isEnabled = isEnabled;
        }

        internal bool HasParameter()
        {
            bool result = false;
            var definition = this.Filter.Definition;

            if (definition != null && !string.IsNullOrWhiteSpace(definition))
            {
                if (definition.Contains("$parValue"))
                {
                    result = true;
                }

            }
            return result;
        }

    }
}
