using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Services.Utils;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class DateFilterControlModel : BaseFilterControlModel
    {
        public ICommand DateSelectorEntryCommand => new Command(async () => await DateSelectorEntryTapped());

        private async Task DateSelectorEntryTapped()
        {
            setDateTimeObject(DateTimeString);
            OpenDatePicker = true;
        }

        private bool _hasDate = false;
        public bool HasDate
        {
            get => _hasDate;
            set
            {
                _hasDate = value;
                RaisePropertyChanged(() => HasDate);
            }
        }

        private bool _hasTime = false;
        public bool HasTime
        {
            get => _hasTime;
            set
            {
                _hasTime = value;
                RaisePropertyChanged(() => HasTime);
            }
        }

        private bool _openDatePicker;
        public bool OpenDatePicker
        {
            get => _openDatePicker;
            set
            {
                _openDatePicker = value;
                RaisePropertyChanged(() => OpenDatePicker);
            }
        }

        private DateTime _selectedDataTime;
        public DateTime SelectedDataTime
        {
            get => _selectedDataTime;
            set
            {
                _selectedDataTime = value;
                RaisePropertyChanged(() => SelectedDataTime);
            }
        }

        private string _dateTimeString;
        public string DateTimeString
        {
            get => _dateTimeString;
            set
            {
                _dateTimeString = value;
                RaisePropertyChanged(() => DateTimeString);
            }
        }
        public DateFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(filter, parentCancellationTokenSource)
        {
        }

        public override async ValueTask<bool> InitializeControl()
        {
            if (Filter.FieldInfo != null)
            {
                HasDate = true;
                HasTime = false;
                DateTime selectedDateTime = DateTime.Now;
                if (Filter.FilterData is List<DateTime> dates)
                {
                    if (dates != null && dates.Count > 0)
                    {
                        selectedDateTime = dates[0];
                    }
                    // End Workaround
                    DateTimeString = selectedDateTime.ToString(CrmConstants.DateFormat);
                }

            }
            if (string.IsNullOrEmpty(DateTimeString))
            {
                DateTimeString = " ";
            }
            setDateTimeObject(DateTimeString);
            return true;
        }

        private void setDateTimeObject(string dateTimeString)
        {
            DateTime selectedDateTime = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(DateTimeString))
            {
                if (DateTimeString.Length == 16)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DateTimeFormat, null);
                }
                else if (DateTimeString.Length == 10)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DateFormat, null);
                }
                else if (DateTimeString.Length == 5)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.TimeFormat, null);
                }
                else if (DateTimeString.Length == 4)
                {
                    selectedDateTime = DateTime.ParseExact(DateTimeString, CrmConstants.DbFieldTimeFormat, null);
                }
            }
            SelectedDataTime = selectedDateTime;
        }

        public override bool HasData
        {
            get => !string.IsNullOrWhiteSpace(DateTimeString);

        }

        public override FilterUI ProcessedFilter
        {
            get
            {
                if (Filter != null)
                {
                    if (HasData)
                    {
                        setDateTimeObject(DateTimeString);
                        Filter.FilterData = new List<DateTime> { SelectedDataTime};
                        var resultValue = DateTimeString.Replace("-", "").Replace(":", "");

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
