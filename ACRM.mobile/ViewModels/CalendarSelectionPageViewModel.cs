using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application.Messages;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.Utils.Calendar;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class CalendarSelectionPageViewModel: BaseViewModel
    {
        public ICommand CloseCommand => new Command(async () => await Close());
        public ICommand SelectButtonCommand => new Command(() => OnSelectButton());
        public ICommand ItemTappedCommand => new Command<Syncfusion.ListView.XForms.ItemTappedEventArgs>((args) => OnItemTapped(args));
        public ICommand ConfirmCommand => new Command(async () => await OnConfirm());

        private SelectButtonState _selectButtonState;

        private HashSet<string> _selectedCrmCalendarIds = new HashSet<string>();
        private HashSet<string> _selectedDeviceCalendarIds = new HashSet<string>();

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

        private string _selectButtonText;
        public string SelectButtonText
        {
            get => _selectButtonText;
            set
            {
                _selectButtonText = value;
                RaisePropertyChanged(() => SelectButtonText);
            }
        }

        private ObservableCollection<BindableCalendar> _bindableCalendars = new ObservableCollection<BindableCalendar>();
        public ObservableCollection<BindableCalendar> BindableCalendars
        {
            get => _bindableCalendars;
            set
            {
                _bindableCalendars = value;
                RaisePropertyChanged(() => BindableCalendars);
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

        public CalendarSelectionPageViewModel()
        {
            InitProperties();
        }

        private void InitProperties()
        {
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
            TitleText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasiciPadCalendar);
            ConfirmText = _localizationController.GetString(LocalizationKeys.TextGroupProcesses, LocalizationKeys.KeyProcessesSignatureConfirmButtonTitle); // TODO using correct localization
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if(navigationData is List<BindableCalendar> bindableCalendars)
            {
                foreach(BindableCalendar bindableCalendar in bindableCalendars)
                {
                    _bindableCalendars.Add(bindableCalendar);

                    if(bindableCalendar.IsSelected)
                    {
                        if (bindableCalendar.IsCRMCalendar)
                        {
                            _selectedCrmCalendarIds.Add(bindableCalendar.Identifier);
                        }
                        else
                        {
                            _selectedDeviceCalendarIds.Add(bindableCalendar.Identifier);
                        }
                    }
                }
                BindableCalendars = _bindableCalendars;
            }

            SetSelectButtonState();
        }

        private async Task Close()
        {
            await _navigationController.PopPopupAsync();
        }

        private void OnSelectButton()
        {
            switch(_selectButtonState)
            {
                case SelectButtonState.SelectAll:
                    SelectAllCalendars();
                    break;
                case SelectButtonState.DeselectAll:
                    DeselectAllCalendars();
                    break;
            }

            SetSelectButtonState();
        }

        private void SelectAllCalendars()
        {
            foreach(BindableCalendar bindableCalendar in BindableCalendars)
            {
                bindableCalendar.IsSelected = true;

                if (bindableCalendar.IsCRMCalendar)
                {
                    _selectedCrmCalendarIds.Add(bindableCalendar.Identifier);
                }
                else
                {
                    _selectedDeviceCalendarIds.Add(bindableCalendar.Identifier);
                }
            }
        }

        private void DeselectAllCalendars()
        {
            _selectedCrmCalendarIds.Clear();
            _selectedDeviceCalendarIds.Clear();

            foreach (BindableCalendar bindableCalendar in BindableCalendars)
            {
                bindableCalendar.IsSelected = false;
            }
        }

        private void OnItemTapped(Syncfusion.ListView.XForms.ItemTappedEventArgs itemTappedEventArgs)
        {
            if (itemTappedEventArgs.ItemData is BindableCalendar calendar)
            {
                calendar.IsSelected = !calendar.IsSelected;

                if (calendar.IsCRMCalendar)
                {
                    if (calendar.IsSelected)
                    {
                        _selectedCrmCalendarIds.Add(calendar.Identifier);
                    }
                    else
                    {
                        _selectedCrmCalendarIds.Remove(calendar.Identifier);
                    }
                }
                else
                {
                    if (calendar.IsSelected)
                    {
                        _selectedDeviceCalendarIds.Add(calendar.Identifier);
                    }
                    else
                    {
                        _selectedDeviceCalendarIds.Remove(calendar.Identifier);
                    }
                }

                SetSelectButtonState();
            }
        }

        private void SetSelectButtonState()
        {
            if (_selectedCrmCalendarIds.Count == 0 && _selectedDeviceCalendarIds.Count == 0)
            {
                _selectButtonState = SelectButtonState.SelectAll;
            }
            else if (_selectButtonState != SelectButtonState.DeselectAll)
            {
                _selectButtonState = SelectButtonState.DeselectAll;
            }

            SetSelectButtonText(_selectButtonState);
        }

        private void SetSelectButtonText(SelectButtonState selectButtonState)
        {
            switch(selectButtonState)
            {
                case SelectButtonState.SelectAll:
                    SelectButtonText = "Select All"; // TODO using localization
                    break;
                case SelectButtonState.DeselectAll:
                    SelectButtonText = "Deselect All"; // TODO using localization
                    break;
            }
        }

        private async Task OnConfirm()
        {
            SelectedCalendarsMessage message = new SelectedCalendarsMessage(_selectedCrmCalendarIds, _selectedDeviceCalendarIds);
            MessagingCenter.Send<BaseViewModel, SelectedCalendarsMessage>(this, InAppMessages.SelectedCalendars, message);
            await _navigationController.PopPopupAsync();
        }
    }

    public enum SelectButtonState
    {
        SelectAll, DeselectAll
    }
}
