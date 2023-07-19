using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class BaseFilterControlModel: UIWidget
    {
        private Keyboard _keyboardType = Keyboard.Text;
        public Keyboard KeyboardType
        {
            get => _keyboardType;
            set
            {
                _keyboardType = value;
                RaisePropertyChanged(() => KeyboardType);
            }
        }
        private string _stringValue = null;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                _stringValue = value;
                RaisePropertyChanged(() => StringValue);
            }
        }
        public virtual bool HasData
        {
            get => !string.IsNullOrWhiteSpace(StringValue);

        }
        private FilterUI _filter = null;
        public FilterUI Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                RaisePropertyChanged(() => Filter);
            }
        }
        public BaseFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            Filter = filter;
            if (Filter.FieldInfo.IsNumeric)
            {
                KeyboardType = Keyboard.Numeric;
            }
        }

        public override async ValueTask<bool> InitializeControl()
        {
            if (Filter.FieldInfo != null)
            {
               
                if (Filter.FilterData is string data)
                {
                    StringValue = data;
                }

            }
            return true;
        }

        public virtual FilterUI ProcessedFilter
        {
            get
            {
                if (Filter != null)
                {
                    Filter.FilterData = StringValue;

                    Dictionary<string,string> Values= new Dictionary<string, string>() { { "*", StringValue } };
                    SetFilterValues(Filter, Values);
                  
                    return Filter;
                }
                else
                {
                    return null;
                }
            }
        }

        protected void SetFilterValues(FilterUI filter, Dictionary<string, string> values)
        {
            var outfilter = filter.Filter.DeepClone();
            List<NodeCondition> nodeConditions = outfilter.GetParValueNodes();

            if (nodeConditions?.Count > 0 && values.Keys?.Count > 0)
            {
                foreach (NodeCondition nodeCondition in nodeConditions)
                {
                    nodeCondition.FieldValues = new List<string>();
                    foreach (var key in values.Keys)
                    {
                        if(key.StartsWith("*") || nodeCondition.CompareOperator.Equals(key))
                        {
                            nodeCondition.FieldValues.Add(values[key]);
                        }
                    }

                }

                filter.OutputFilter = outfilter;
            }
        }
    }
}
