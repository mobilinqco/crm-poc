using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.ObservableGroups;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class TextRangeFilterControlModel: BaseFilterControlModel
    {
        private string _fromDataString;
        public string FromDataString
        {
            get => _fromDataString;
            set
            {
                _fromDataString = value;
                RaisePropertyChanged(() => FromDataString);
            }
        }
        private string _toDataString;
        public string ToDataString
        {
            get => _toDataString;
            set
            {
                _toDataString = value;
                RaisePropertyChanged(() => ToDataString);
            }
        }
        private string _fromText = "From";
        public string FromText
        {
            get => _fromText;
            set
            {
                _fromText = value;
                RaisePropertyChanged(() => FromText);
            }
        }
        private string _toText = "To";
        public string ToText
        {
            get => _toText;
            set
            {
                _toText = value;
                RaisePropertyChanged(() => ToText);
            }
        }

        public TextRangeFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(filter, parentCancellationTokenSource)
        {
        }

        public override async ValueTask<bool> InitializeControl()
        {
            if (Filter.FieldInfo != null)
            {
                DateTime selectedFromDateTime = DateTime.Now;
                DateTime selectedToDateTime = DateTime.Now;
                if (Filter.FilterData is List<string> items)
                {
                    if (items != null && items.Count == 2)
                    {
                        FromDataString = items[0];
                        ToDataString = items[1];
                    }

                }

            }
            return true;
        }


        public override bool HasData
        {
            get => !string.IsNullOrWhiteSpace(FromDataString) || !string.IsNullOrWhiteSpace(ToDataString);

        }

        public override FilterUI ProcessedFilter
        {
            get
            {
                if (Filter != null)
                {
                    if (HasData)
                    {

                        Filter.FilterData = new List<string> { FromDataString, ToDataString };
                        var fromResultValue = FromDataString;
                        var toResultValue = ToDataString;

                        Dictionary<string, string> Values = new Dictionary<string, string>() { { ">=", fromResultValue }, { ">", fromResultValue }, { "<=", toResultValue }, { "<", toResultValue } };
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
