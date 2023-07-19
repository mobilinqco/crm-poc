using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class CalendarEventDetailsPageViewModel: BaseViewModel
    {
        private ICalendarEventDetailsContentService _contentService;

        public ICommand CloseCommand => new Command(async () => await Close());
        public ICommand ShowCommand => new Command(async () => await Show());

        private CrmScheduleAppointment _crmScheduleAppointment;

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

        private string _showText;
        public string ShowText
        {
            get => _showText;
            set
            {
                _showText = value;
                RaisePropertyChanged(() => ShowText);
            }
        }

        private bool _isCrmCalendarEvent = false;
        public bool IsCrmCalendarEvent
        {
            get => _isCrmCalendarEvent;
            set
            {
                _isCrmCalendarEvent = value;
                RaisePropertyChanged(() => IsCrmCalendarEvent);
            }
        }

        private bool _isDeviceCalendarEvent = false;
        public bool IsDeviceCalendarEvent
        {
            get => _isDeviceCalendarEvent;
            set
            {
                _isDeviceCalendarEvent = value;
                RaisePropertyChanged(() => IsDeviceCalendarEvent);
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

        private string _eventTitleText;
        public string EventTitleText
        {
            get => _eventTitleText;
            set
            {
                _eventTitleText = value;
                RaisePropertyChanged(() => EventTitleText);
            }
        }

        private string _eventTimeIntervalText;
        public string EventTimeIntervalText
        {
            get => _eventTimeIntervalText;
            set
            {
                _eventTimeIntervalText = value;
                RaisePropertyChanged(() => EventTimeIntervalText);
            }
        }

        private string _locationText;
        public string LocationText
        {
            get => _locationText;
            set
            {
                _locationText = value;
                RaisePropertyChanged(() => LocationText);
            }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                RaisePropertyChanged(() => StatusText);
            }
        }

        private ObservableCollection<PanelData> _panels = new ObservableCollection<PanelData>();
        public ObservableCollection<PanelData> Panels
        {
            get => _panels;
            set
            {
                _panels = value;
                RaisePropertyChanged(() => Panels);
            }
        }

        public CalendarEventDetailsPageViewModel(ICalendarEventDetailsContentService calendarContentService)
        {
            _contentService = calendarContentService;
            _contentService.DataReady += OnDataReady;
            InitProperties();
        }

        private void InitProperties()
        {
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
            ShowText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicShowRecord);
        }

        public override async Task InitializeAsync(object data)
        {
            _logService.LogDebug($"Started initialization of CalendarEventDetails with {data}");
            if(data is CrmScheduleAppointment crmScheduleAppointment)
            {
                IsLoading = true;
                _crmScheduleAppointment = crmScheduleAppointment;

                if (_crmScheduleAppointment.UserAction?.RecordId != null &&
                     _crmScheduleAppointment.UserAction.RecordId != "-1")
                {
                    _contentService.SetCalendarViewTemplate(_crmScheduleAppointment.CalendarViewTemplate);
                    _contentService.SetSourceAction(_crmScheduleAppointment.UserAction);
                    _contentService.PrepareContentAsync(_cancellationTokenSource.Token).SafeFireAndForget<Exception>(onException: ex =>
                    {
                        _logService.LogError($"Unable to prepare content {ex.Message}");
                    });
                }
                else
                {
                    IsLoading = false;
                    UpdateDeviceEventBindingsAsync();
                }
            }
            _logService.LogDebug($"End of initialization of CalendarEventDetails");
        }

        private void OnDataReady(object sender, EventArgs e)
        {
            _logService.LogDebug($"Data Ready for CalendarEventDetails");
            IsLoading = false;
            UpdateCrmEventBindingsAsync();
        }

        private void UpdateCrmEventBindingsAsync()
        {
            _logService.LogDebug($"Updating bindings for CalendarEventDetails");
            IsCrmCalendarEvent = true;
            TitleText = _contentService.PageTitle();
            EventTitleText = _crmScheduleAppointment.Subject;
            EventTimeIntervalText = GetTimeIntervalString(_crmScheduleAppointment.StartTime, _crmScheduleAppointment.EndTime);

            foreach (PanelData panelData in _contentService.Panels())
            {
                _panels.Add(panelData);
            }

            Panels = _panels;
            _logService.LogDebug($"Finished updating bindings for CalendarEventDetails");
        }

        private void UpdateDeviceEventBindingsAsync()
        {
            IsDeviceCalendarEvent = true;
            TitleText = "Activity"; // TODO using localization
            EventTitleText = _crmScheduleAppointment.Subject;
            EventTimeIntervalText = GetTimeIntervalString(_crmScheduleAppointment.StartTime, _crmScheduleAppointment.EndTime);
            LocationText = _crmScheduleAppointment.DeviceCalendarEvent.Location;
            StatusText = _crmScheduleAppointment.DeviceCalendarEvent.Status.ToString();
        }

        private string GetTimeIntervalString(DateTime startDateTime, DateTime endDateTime)
        {
            return startDateTime.ToString("HH:mm") + " - " + endDateTime.ToString("HH:mm");
        }

        private async Task Close()
        {
            await _navigationController.PopPopupAsync();
        }

        private async Task Show()
        {
            await _navigationController.PopPopupAsync();
            await _navigationController.NavigateAsyncForAction(_crmScheduleAppointment.UserAction, _cancellationTokenSource.Token);
        }
    }
}
