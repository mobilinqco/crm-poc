using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.ObservableGroups;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class BoolFilterControlModel : BaseFilterControlModel
    {
        private bool? _isSelected = false;
        public bool? IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }
        public BoolFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(filter, parentCancellationTokenSource)
        {
            
        }

        public override async ValueTask<bool> InitializeControl()
        {
            if (Filter.FieldInfo != null)
            {
                if (Filter.FilterData is bool selection)
                {
                    IsSelected = selection;
                }

            }
            return true;
        }

        public override bool HasData
        {
            get => IsSelected!=null;

        }

        public override FilterUI ProcessedFilter
        {
            get
            {
                if (Filter != null)
                {
                    if (IsSelected != null)
                    {
                        Filter.FilterData = IsSelected.Value;
                        var resultValue = IsSelected.Value.ToString();

                        Dictionary<string, string> Values = new Dictionary<string, string>() { { "*", resultValue } };
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
    }
}
