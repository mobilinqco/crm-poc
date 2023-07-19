using ACRM.mobile.Domain.Application.ContactTimes;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups.ContactTimes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class ContactTimesIntervalSelectionPageViewModel : BaseViewModel
    {

        public ICommand CloseCommand => new Command(async () => await Close());
        public ICommand ConfirmCommand => new Command(async () => await OnConfirm());
        public ICommand ItemTappedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>((args) => OnItemTapped(args));
        public ICommand IsFirstIntervalSelectedCommand => new Command((args) => OnIsFirstIntervalTapped(args));
        public ICommand IsFirstIntervalStartSelectedCommand => new Command(() => OnIsFirstIntervalStartTapped());
        public ICommand IsFirstIntervalEndSelectedCommand => new Command(() => OnIsFirstIntervalEndTapped());
        public ICommand IsSecondIntervalSelectedCommand => new Command((args) => OnIsSecondIntervalTapped(args));
        public ICommand IsSecondIntervalStartSelectedCommand => new Command(() => OnIsSecondIntervalStartTapped());
        public ICommand IsSecondIntervalEndSelectedCommand => new Command(() => OnIsSecondIntervalEndTapped());

        private string _typeCode = null;

        private string _closeText;
        public string CloseText
        {
            get => _closeText;
            set
            {
                _closeText = value;
                RaisePropertyChanged(() => CloseText);
            }
        }

        private string _titleText;
        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value;
                RaisePropertyChanged(() => TitleText);
            }
        }

        private string _confirmText;
        public string ConfirmText
        {
            get => _confirmText;
            set
            {
                _confirmText = value;
                RaisePropertyChanged(() => ConfirmText);
            }
        }

        private string _intervalStartText;
        public string IntervalStartText
        {
            get => _intervalStartText;
            set
            {
                _intervalStartText = value;
                RaisePropertyChanged(() => IntervalStartText);
            }
        }

        private string _intervalEndText;
        public string IntervalEndText
        {
            get => _intervalEndText;
            set
            {
                _intervalEndText = value;
                RaisePropertyChanged(() => IntervalEndText);
            }
        }

        private bool _isFirstInervalStartDatePickerOpen;
        public bool IsFirstInervalStartDatePickerOpen
        {
            get => _isFirstInervalStartDatePickerOpen;
            set
            {
                _isFirstInervalStartDatePickerOpen = value;
                RaisePropertyChanged(() => IsFirstInervalStartDatePickerOpen);
            }
        }

        private bool _isFirstInervalEndDatePickerOpen;
        public bool IsFirstInervalEndDatePickerOpen
        {
            get => _isFirstInervalEndDatePickerOpen;
            set
            {
                _isFirstInervalEndDatePickerOpen = value;
                RaisePropertyChanged(() => IsFirstInervalEndDatePickerOpen);
            }
        }

        private bool _isFirstIntervalSelected;
        public bool IsFirstIntervalSelected
        {
            get => _isFirstIntervalSelected;
            set
            {
                _isFirstIntervalSelected = value;
                RaisePropertyChanged(() => IsFirstIntervalSelected);
            }
        }

        private string _firstIntervalText;
        public string FirstIntervalText
        {
            get => _firstIntervalText;
            set
            {
                _firstIntervalText = value;
                RaisePropertyChanged(() => FirstIntervalText);
            }
        }

        private string _firstIntervalStartDateTimePrickerString;
        public string FirstIntervalStartDateTimePickerString
        {
            get => _firstIntervalStartDateTimePrickerString;
            set
            {
                _firstIntervalStartDateTimePrickerString = value;
                RaisePropertyChanged(() => FirstIntervalStartDateTimePickerString);
            }
        }

        private DateTime _firstIntervalStartDateTime = new DateTime(
            DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
        public DateTime FirstIntervalStartDateTime
        {
            get => _firstIntervalStartDateTime;
            set
            {
                _firstIntervalStartDateTime = value;
                RaisePropertyChanged(() => FirstIntervalStartDateTime);
            }
        }

        private string _firstIntervalStartDateTimeString;
        public string FirstIntervalStartDateTimeString
        {
            get => _firstIntervalStartDateTimeString;
            set
            {
                _firstIntervalStartDateTimeString = value;
                RaisePropertyChanged(() => FirstIntervalStartDateTimeString);
            }
        }

        private string _firstIntervalEndDateTimePrickerString;
        public string FirstIntervalEndDateTimePickerString
        {
            get => _firstIntervalEndDateTimePrickerString;
            set
            {
                _firstIntervalEndDateTimePrickerString = value;
                RaisePropertyChanged(() => FirstIntervalEndDateTimePickerString);
            }
        }

        private DateTime _firstIntervalEndDateTime = new DateTime(
            DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
        public DateTime FirstIntervalEndDateTime
        { 
            get => _firstIntervalEndDateTime;
            set
            {
                _firstIntervalEndDateTime = value;
                RaisePropertyChanged(() => FirstIntervalEndDateTime);
            }
        }

        private string _firstIntervalEndDateTimeString;
        public string FirstIntervalEndDateTimeString
        {
            get => _firstIntervalEndDateTimeString;
            set
            {
                _firstIntervalEndDateTimeString = value;
                RaisePropertyChanged(() => FirstIntervalEndDateTimeString);
            }
        }

        private bool _isSecondInervalStartDatePickerOpen;
        public bool IsSecondInervalStartDatePickerOpen
        {
            get => _isSecondInervalStartDatePickerOpen;
            set
            {
                _isSecondInervalStartDatePickerOpen = value;
                RaisePropertyChanged(() => IsSecondInervalStartDatePickerOpen);
            }
        }

        private bool _isSecondInervalEndDatePickerOpen;
        public bool IsSecondInervalEndDatePickerOpen
        {
            get => _isSecondInervalEndDatePickerOpen;
            set
            {
                _isSecondInervalEndDatePickerOpen = value;
                RaisePropertyChanged(() => IsSecondInervalEndDatePickerOpen);
            }
        }

        private bool _isSecondIntervalSelected;
        public bool IsSecondIntervalSelected
        {
            get => _isSecondIntervalSelected;
            set
            {
                _isSecondIntervalSelected = value;
                RaisePropertyChanged(() => IsSecondIntervalSelected);
            }
        }

        private string _secondIntervalText;
        public string SecondIntervalText
        {
            get => _secondIntervalText;
            set
            {
                _secondIntervalText = value;
                RaisePropertyChanged(() => SecondIntervalText);
            }
        }

        private string _secondIntervalStartDateTimePrickerString;
        public string SecondIntervalStartDateTimePickerString
        {
            get => _secondIntervalStartDateTimePrickerString;
            set
            {
                _secondIntervalStartDateTimePrickerString = value;
                RaisePropertyChanged(() => SecondIntervalStartDateTimePickerString);
            }
        }

        private DateTime _secondIntervalStartDateTime = new DateTime(
            DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 0, 0);
        public DateTime SecondIntervalStartDateTime
        {
            get => _secondIntervalStartDateTime;
            set
            {
                _secondIntervalStartDateTime = value;
                RaisePropertyChanged(() => SecondIntervalStartDateTime);
            }
        }

        private string _secondIntervalStartDateTimeString;
        public string SecondIntervalStartDateTimeString
        {
            get => _secondIntervalStartDateTimeString;
            set
            {
                _secondIntervalStartDateTimeString = value;
                RaisePropertyChanged(() => SecondIntervalStartDateTimeString);
            }
        }

        private string _secondIntervalEndDateTimePrickerString;
        public string SecondIntervalEndDateTimePickerString
        {
            get => _secondIntervalEndDateTimePrickerString;
            set
            {
                _secondIntervalEndDateTimePrickerString = value;
                RaisePropertyChanged(() => SecondIntervalEndDateTimePickerString);
            }
        }

        private DateTime _secondIntervalEndDateTime = new DateTime(
            DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 0, 0);
        public DateTime SecondIntervalEndDateTime
        {
            get => _secondIntervalEndDateTime;
            set
            {
                _secondIntervalEndDateTime = value;
                RaisePropertyChanged(() => SecondIntervalEndDateTime);
            }
        }

        private string _secondIntervalEndDateTimeString;
        public string SecondIntervalEndDateTimeString
        { 
            get => _secondIntervalEndDateTimeString;
            set
            {
                _secondIntervalEndDateTimeString = value;
                RaisePropertyChanged(() => SecondIntervalEndDateTimeString);
            }
        }

        private ObservableCollection<BindableContactTimesSelectableDay> _bindableContactTimesSelectableDays = new ObservableCollection<BindableContactTimesSelectableDay>();
        public ObservableCollection<BindableContactTimesSelectableDay> BindableContactTimesSelectableDays
        {
            get => _bindableContactTimesSelectableDays;
            set
            {
                _bindableContactTimesSelectableDays = value;
                RaisePropertyChanged(() => BindableContactTimesSelectableDays);
            }
        }

        public ContactTimesIntervalSelectionPageViewModel()
        {
            InitialiseProperties();
        }

        private void InitialiseProperties()
        {
            IsLoading = true;
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
            ConfirmText = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesSignatureConfirmButtonTitle); // TODO using correct localization
            IntervalStartText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicStart);
            IntervalEndText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicEnd);
            FirstIntervalText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicFirstInterval);
            SecondIntervalText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSecondInterval);
            FirstIntervalStartDateTimePickerString = string.Format("{0} {1}", FirstIntervalText, IntervalStartText);
            FirstIntervalEndDateTimePickerString = string.Format("{0} {1}", FirstIntervalText, IntervalEndText);
            SecondIntervalStartDateTimePickerString = string.Format("{0} {1}", SecondIntervalText, IntervalStartText);
            SecondIntervalEndDateTimePickerString = string.Format("{0} {1}", SecondIntervalText, IntervalEndText);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;

            if (navigationData is ContactTimesIntervalSelectionData contactTimesIntervalSelectionData)
            {
                _typeCode = contactTimesIntervalSelectionData.TypeCode;
                TitleText = contactTimesIntervalSelectionData.TypeName;
                IsFirstIntervalSelected = IsFirstIntervalValid(contactTimesIntervalSelectionData.MorningFromDateTime, contactTimesIntervalSelectionData.MorningToDateTime);
                IsSecondIntervalSelected = IsSecondIntervalValid(contactTimesIntervalSelectionData.AfternoonFromDateTime, contactTimesIntervalSelectionData.AfternoonToDateTime);
                BuildBindableContactTimesSelectableDays(contactTimesIntervalSelectionData.ContactTimesDayAbbreviations);
                UpdateTimeIntervals(contactTimesIntervalSelectionData.MorningFromDateTime, contactTimesIntervalSelectionData.MorningToDateTime,
                    contactTimesIntervalSelectionData.AfternoonFromDateTime, contactTimesIntervalSelectionData.AfternoonToDateTime);
                UpdateTimeIntervalsStrings();
            }

            IsLoading = false;
        }

        private void BuildBindableContactTimesSelectableDays(List<ContactTimesDayAbbreviation> contactTimesDayAbbreviations)
        {
            foreach(ContactTimesDayAbbreviation contactTimesDayAbbreviation in contactTimesDayAbbreviations)
            {
                _bindableContactTimesSelectableDays.Add(new BindableContactTimesSelectableDay(contactTimesDayAbbreviation.WeekDayId,
                    contactTimesDayAbbreviation.WeekDayNameAbbreviation, contactTimesDayAbbreviation.IsSelected));
            }
            BindableContactTimesSelectableDays = _bindableContactTimesSelectableDays;
        }

        private void UpdateTimeIntervals(DateTime firstIntervalStartDateTime, DateTime firstIntervalEndDateTime,
            DateTime secondIntervalStartDateTime, DateTime secondIntervalEndDateTime)
        {
            if (IsFirstIntervalValid(firstIntervalStartDateTime, firstIntervalEndDateTime))
            {
                FirstIntervalStartDateTime = firstIntervalStartDateTime;
                FirstIntervalEndDateTime = firstIntervalEndDateTime;
            }

            if (IsSecondIntervalValid(secondIntervalStartDateTime, secondIntervalEndDateTime))
            {
                SecondIntervalStartDateTime = secondIntervalStartDateTime;
                SecondIntervalEndDateTime = secondIntervalEndDateTime;
            }
        }

        private bool IsFirstIntervalValid(DateTime firstIntervalStartDateTime, DateTime firstIntervalEndDateTime)
        {
            return firstIntervalStartDateTime.Hour != 0 || firstIntervalStartDateTime.Minute != 0 || 
                firstIntervalEndDateTime.Hour != 0 || firstIntervalEndDateTime.Minute != 0;
        }

        private bool IsSecondIntervalValid(DateTime secondIntervalStartDateTime, DateTime secondIntervalEndDateTime)
        {
            return secondIntervalStartDateTime.Hour != 0 || secondIntervalStartDateTime.Minute != 0 || 
                secondIntervalEndDateTime.Hour != 0 || secondIntervalEndDateTime.Minute != 0;
        }

        private void UpdateTimeIntervalsStrings()
        {
            FirstIntervalStartDateTimeString = FirstIntervalStartDateTime.ToString("HH:mm");
            FirstIntervalEndDateTimeString = FirstIntervalEndDateTime.ToString("HH:mm");
            SecondIntervalStartDateTimeString = SecondIntervalStartDateTime.ToString("HH:mm");
            SecondIntervalEndDateTimeString = SecondIntervalEndDateTime.ToString("HH:mm");
        }

        private async Task Close()
        {
            await _navigationController.PopPopupAsync();
        }

        private async Task OnConfirm()
        {
            if(_typeCode != null)
            {
                List<ContactTimesDayAbbreviation> contactTimesDayAbbreviations = new List<ContactTimesDayAbbreviation>();

                foreach (BindableContactTimesSelectableDay bindableContactTimesSelectableDay in _bindableContactTimesSelectableDays)
                {
                    contactTimesDayAbbreviations.Add(new ContactTimesDayAbbreviation(bindableContactTimesSelectableDay.WeekDayId,
                        bindableContactTimesSelectableDay.WeekDayAbbreviation, bindableContactTimesSelectableDay.IsSelected));
                }

                DateTime firstIntervalStartDateTime;
                if(_isFirstIntervalSelected)
                {
                    TimeSpan timeSpan = TimeSpan.Parse(FirstIntervalStartDateTimeString);
                    firstIntervalStartDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, timeSpan.Hours, timeSpan.Minutes, 0);
                }
                else
                {
                    firstIntervalStartDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                }

                DateTime firstIntervalEndDateTime;
                if (_isFirstIntervalSelected)
                {
                    TimeSpan timeSpan = TimeSpan.Parse(FirstIntervalEndDateTimeString);
                    firstIntervalEndDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, timeSpan.Hours, timeSpan.Minutes, 0);
                }
                else
                {
                    firstIntervalEndDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                }

                DateTime secondIntervalStartDateTime;
                if (_isSecondIntervalSelected)
                {
                    TimeSpan timeSpan = TimeSpan.Parse(SecondIntervalStartDateTimeString);
                    secondIntervalStartDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, timeSpan.Hours, timeSpan.Minutes, 0);
                }
                else
                {
                    secondIntervalStartDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                }

                DateTime secondIntervalEndDateTime;
                if(_isSecondIntervalSelected)
                {
                    TimeSpan timeSpan = TimeSpan.Parse(SecondIntervalEndDateTimeString);
                    secondIntervalEndDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, timeSpan.Hours, timeSpan.Minutes, 0);
                }
                else
                {
                    secondIntervalEndDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                }

                if(await AreIntervalsValid(firstIntervalStartDateTime, firstIntervalEndDateTime, secondIntervalStartDateTime, secondIntervalEndDateTime))
                {
                    ContactTimesIntervalSelectionData contactTimesIntervalSelectionData = new ContactTimesIntervalSelectionData(_typeCode, TitleText, contactTimesDayAbbreviations,
                        firstIntervalStartDateTime, firstIntervalEndDateTime, secondIntervalStartDateTime, secondIntervalEndDateTime);

                    MessagingCenter.Send<BaseViewModel, ContactTimesIntervalSelectionData>(this, InAppMessages.ContactTimesDayAbbreviations, contactTimesIntervalSelectionData);

                    await _navigationController.PopPopupAsync();
                }
            }
            else
            {
                await _navigationController.PopPopupAsync();
            }
        }

        private async Task<bool> AreIntervalsValid(DateTime firstIntervalStartDateTime, DateTime firstIntervalEndDateTime,
            DateTime secondIntervalStartDateTime, DateTime secondIntervalEndDateTime)
        {
            // First, we need to check if there is no interval selected.
            // If there isn't any interval selected, the DateTimes do overlap, but 00:00 represents deletion.
            // Therefore we do not continue to check the overlapping.
            if(!_isFirstIntervalSelected && !_isSecondIntervalSelected)
            {
                return true;
            }

            if (_isFirstIntervalSelected && firstIntervalStartDateTime >= firstIntervalEndDateTime || IsDateTimeOverlappingInterval(firstIntervalStartDateTime,
                secondIntervalStartDateTime, secondIntervalEndDateTime))
            {
                DisplayErrorMessage();
                return false;
            }

            if (_isFirstIntervalSelected && firstIntervalEndDateTime <= firstIntervalStartDateTime || IsDateTimeOverlappingInterval(firstIntervalEndDateTime,
                secondIntervalStartDateTime, secondIntervalEndDateTime))
            {
                DisplayErrorMessage();
                return false;
            }

            if (_isSecondIntervalSelected && secondIntervalStartDateTime >= secondIntervalEndDateTime || IsDateTimeOverlappingInterval(secondIntervalStartDateTime,
                firstIntervalStartDateTime, firstIntervalEndDateTime))
            {
                DisplayErrorMessage();
                return false;
            }

            if (_isSecondIntervalSelected && secondIntervalEndDateTime <= secondIntervalStartDateTime || IsDateTimeOverlappingInterval(secondIntervalEndDateTime,
                firstIntervalStartDateTime, firstIntervalEndDateTime))
            {
                DisplayErrorMessage();
                return false;
            }

            return true;
        }

        private async void DisplayErrorMessage()
        {
            await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupErrors,
                LocalizationKeys.KeyErrorsInvalidTimeIntervalTitle,
                LocalizationKeys.KeyErrorsInvalidTimeIntervalDescription);
        }

        private bool IsDateTimeOverlappingInterval(DateTime dateTime, DateTime intervalStartDateTime, DateTime intervalEndDateTime)
        {
            if ((dateTime >= intervalStartDateTime &&
                dateTime <= intervalEndDateTime))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnItemTapped(Syncfusion.ListView.XForms.ItemTappedEventArgs itemTappedEventArgs)
        {
            if (itemTappedEventArgs.ItemData is BindableContactTimesSelectableDay bindableContactTimesSelectableDay)
            {
                bindableContactTimesSelectableDay.IsSelected = !bindableContactTimesSelectableDay.IsSelected;
            }
        }

        private void OnIsFirstIntervalTapped(object args)
        {
            IsFirstIntervalSelected = !IsFirstIntervalSelected;
        }

        private void OnIsFirstIntervalStartTapped()
        {
            IsFirstInervalStartDatePickerOpen = true;
        }

        private void OnIsFirstIntervalEndTapped()
        {
            IsFirstInervalEndDatePickerOpen = true;
        }

        private void OnIsSecondIntervalTapped(object args)
        {
            IsSecondIntervalSelected = !IsSecondIntervalSelected;
        }

        private void OnIsSecondIntervalStartTapped()
        {
            IsSecondInervalStartDatePickerOpen = true;
        }

        private void OnIsSecondIntervalEndTapped()
        {
            IsSecondInervalEndDatePickerOpen = true;
        }
    }
}
