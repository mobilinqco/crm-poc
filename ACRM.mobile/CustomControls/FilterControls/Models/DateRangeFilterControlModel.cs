using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Utils;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.ObservableGroups;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.FilterControls.Models
{
    public class DateRangeFilterControlModel : BaseFilterControlModel
    {
        public ICommand FromDateSelectorEntryCommand => new Command(async () => await FromDateSelectorEntryTapped());

        private async Task FromDateSelectorEntryTapped()
        {
            setDateTimeObject(FromDateTimeString);
            OpenFromDatePicker = true;
        }

        public ICommand ToDateSelectorEntryCommand => new Command(async () => await ToDateSelectorEntryTapped());

        private async Task ToDateSelectorEntryTapped()
        {
            setDateTimeObject(ToDateTimeString);
            OpenToDatePicker = true;
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

        private bool _openFromDatePicker;
        public bool OpenFromDatePicker
        {
            get => _openFromDatePicker;
            set
            {
                _openFromDatePicker = value;
                RaisePropertyChanged(() => OpenFromDatePicker);
            }
        }

        private DateTime _selectedFromDataTime;
        public DateTime SelectedFromDataTime
        {
            get => _selectedFromDataTime;
            set
            {
                _selectedFromDataTime = value;
                RaisePropertyChanged(() => SelectedFromDataTime);
            }
        }

        private string _fromDateTimeString;
        public string FromDateTimeString
        {
            get => _fromDateTimeString;
            set
            {
                _fromDateTimeString = value;
                RaisePropertyChanged(() => FromDateTimeString);
            }
        }
        private string _fromText;
        public string FromText
        {
            get => _fromText;
            set
            {
                _fromText = value;
                RaisePropertyChanged(() => FromText);
            }
        }
        private string _toText;
        public string ToText
        {
            get => _toText;
            set
            {
                _toText = value;
                RaisePropertyChanged(() => ToText);
            }
        }
        private bool _openToDatePicker;
        public bool OpenToDatePicker
        {
            get => _openToDatePicker;
            set
            {
                _openToDatePicker = value;
                RaisePropertyChanged(() => OpenToDatePicker);
            }
        }

        private DateTime _selectedToDataTime;
        public DateTime SelectedToDataTime
        {
            get => _selectedToDataTime;
            set
            {
                _selectedToDataTime = value;
                RaisePropertyChanged(() => SelectedToDataTime);
            }
        }

        private string _toDateTimeString;
        public string ToDateTimeString
        {
            get => _toDateTimeString;
            set
            {
                _toDateTimeString = value;
                RaisePropertyChanged(() => ToDateTimeString);
            }
        }
        public DateRangeFilterControlModel(FilterUI filter, CancellationTokenSource parentCancellationTokenSource)
            : base(filter, parentCancellationTokenSource)
        {
        }

        public override async ValueTask<bool> InitializeControl()
        {
            InitialiseProperties();
            if (Filter.FieldInfo != null)
            {
                HasDate = true;
                HasTime = false;
                DateTime selectedFromDateTime = DateTime.Now;
                DateTime selectedToDateTime = DateTime.Now;
                if (Filter.FilterData is List<DateTime> dates)
                {
                    if (dates != null && dates.Count == 2)
                    {
                        selectedFromDateTime = dates[0];
                        selectedToDateTime = dates[1];
                    }
                    // End Workaround
                    FromDateTimeString = selectedFromDateTime.ToString(CrmConstants.DateFormat);
                    ToDateTimeString = selectedToDateTime.ToString(CrmConstants.DateFormat);
                }

            }
            if (string.IsNullOrEmpty(FromDateTimeString))
            {
                FromDateTimeString = " ";
            }
            SelectedFromDataTime = setDateTimeObject(FromDateTimeString);
            if (string.IsNullOrEmpty(ToDateTimeString))
            {
                ToDateTimeString = " ";
            }
            SelectedToDataTime = setDateTimeObject(ToDateTimeString);
            return true;
        }

        public void InitialiseProperties()
        {
            FromText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicFrom);
            ToText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicTo);
        }

        private DateTime setDateTimeObject(string dateTimeString)
        {
            DateTime selectedDateTime = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(dateTimeString))
            {
                if (dateTimeString.Length == 16)
                {
                    selectedDateTime = DateTime.ParseExact(dateTimeString, CrmConstants.DateTimeFormat, null);
                }
                else if (dateTimeString.Length == 10)
                {
                    selectedDateTime = DateTime.ParseExact(dateTimeString, CrmConstants.DateFormat, null);
                }
                else if (dateTimeString.Length == 5)
                {
                    selectedDateTime = DateTime.ParseExact(dateTimeString, CrmConstants.TimeFormat, null);
                }
                else if (dateTimeString.Length == 4)
                {
                    selectedDateTime = DateTime.ParseExact(dateTimeString, CrmConstants.DbFieldTimeFormat, null);
                }
            }
            return selectedDateTime;
        }

        public override bool HasData
        {
            get => !string.IsNullOrWhiteSpace(FromDateTimeString) || !string.IsNullOrWhiteSpace(ToDateTimeString);

        }

        public override FilterUI ProcessedFilter
        {
            get
            {
                if (Filter != null)
                {
                    if (HasData)
                    {
                        SelectedFromDataTime = setDateTimeObject(FromDateTimeString);
                        SelectedToDataTime = setDateTimeObject(ToDateTimeString);
                        Filter.FilterData = new List<DateTime> { SelectedFromDataTime, SelectedToDataTime };
                        var fromResultValue = FromDateTimeString.Replace("-", "").Replace(":", "");
                        var toResultValue = ToDateTimeString.Replace("-", "").Replace(":", "");

                        Dictionary<string, string> Values = new Dictionary<string, string>() { { ">=", fromResultValue }, { ">", fromResultValue },{ "<=", toResultValue }, { "<", toResultValue }};
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
