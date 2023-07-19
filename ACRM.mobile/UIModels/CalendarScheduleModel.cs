using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application.Calendar;
using ACRM.mobile.Services.Utils;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Syncfusion.SfSchedule.XForms;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class CalendarScheduleModel: UIWidget
    {

        public ICommand HeaderTappedCommand => new Command<HeaderTappedEventArgs>(async (args) => await OnHeaderTapped(args));
        public ICommand CellTappedCommand => new Command<CellTappedEventArgs>(async (args) => await OnCellTapped(args));
        public ICommand CellLongPressedCommand => new Command<CellTappedEventArgs>(async (args) => await OnCellLongPressed(args));
        public ICommand ScheduleDatesChangedCommand => new Command<VisibleDatesChangedEventArgs>(async (args) => await OnScheduleDatesChanged(args));

        private int _firstWorkingHour = 8;
        private int _numberOfWorkingHours = 10;

        private bool _isCalendarScheduleViewVisible = false;

        private bool _isDateTimePickerOpen = false;
        public bool IsDateTimePickerOpen
        {
            get => _isDateTimePickerOpen;
            set
            {
                _isDateTimePickerOpen = value;
                RaisePropertyChanged(() => IsDateTimePickerOpen);
            }
        }

        private ScheduleView _scheduleViewMode;
        public ScheduleView ScheduleViewMode
        {
            get => _scheduleViewMode;
            set
            {
                _scheduleViewMode = value;
                RaisePropertyChanged(() => ScheduleViewMode);
            }
        }

        private ScheduleAppointmentCollection _scheduleData = new ScheduleAppointmentCollection();
        public ScheduleAppointmentCollection ScheduleData
        {
            get => _scheduleData;
            set
            {
                _scheduleData = value;
                RaisePropertyChanged(() => ScheduleData);
            }
        }

        private DateTime _currentSelectedDate;
        public DateTime CurrentSelectedDate
        {
            get => _currentSelectedDate;
            set
            {
                _currentSelectedDate = value;
                RaisePropertyChanged(() => CurrentSelectedDate);
            }
        }

        private DayViewSettings _dayViewSettings;
        public DayViewSettings DayViewSettings
        {
            get => _dayViewSettings;
            set
            {
                _dayViewSettings = value;
                RaisePropertyChanged(() => DayViewSettings);
            }
        }

        private WeekViewSettings _weekViewSettings;
        public WeekViewSettings WeekViewSettings
        {
            get => _weekViewSettings;
            set
            {
                _weekViewSettings = value;
                RaisePropertyChanged(() => WeekViewSettings);
            }
        }

        public CalendarScheduleModel(CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            InitiliseProperties();
        }

        private void InitiliseProperties()
        {
            CurrentSelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, _firstWorkingHour, 0, 0);

            DayViewSettings dayViewSettings = new DayViewSettings();
            dayViewSettings.WorkStartHour =_firstWorkingHour;
            dayViewSettings.WorkEndHour = _firstWorkingHour + _numberOfWorkingHours;
            dayViewSettings.NonWorkingHoursTimeSlotColor = Color.DarkGray;
            dayViewSettings.TimeSlotColor = Color.White;
            DayViewSettings = dayViewSettings;

            WeekViewSettings weekViewSettings = new WeekViewSettings();
            weekViewSettings.WorkStartHour = _firstWorkingHour;
            weekViewSettings.WorkEndHour = _firstWorkingHour + _numberOfWorkingHours;
            weekViewSettings.NonWorkingHoursTimeSlotColor = Color.DarkGray;
            weekViewSettings.TimeSlotColor = Color.White;
            WeekViewSettings = weekViewSettings;
        }

        public override async ValueTask<bool> InitializeControl()
        {
            return true;
        }

        private async Task OnHeaderTapped(HeaderTappedEventArgs headerTappedEventArgs)
        {
            IsDateTimePickerOpen = !IsDateTimePickerOpen;
        }

        private async Task OnCellTapped(CellTappedEventArgs args)
        {
            if (ScheduleViewMode != ScheduleView.MonthView)
            {
                if (args.Appointment is CrmScheduleAppointment crmScheduleAppointment)
                {
                    await ParentBaseModel?.PublishMessage(new WidgetMessage
                    {
                        EventType = WidgetEventType.ShowCalendarEventPageRequested,
                        ControlKey = "CalendarSchedule",
                        Data = crmScheduleAppointment
                    });
                }
            }
            else
            {
                await ParentBaseModel?.PublishMessage(new WidgetMessage
                {
                    EventType = WidgetEventType.ScheduleViewChanged,
                    ControlKey = "CalendarSchedule",
                    Data = ScheduleView.DayView
                });
            }
        }

        private async Task OnCellLongPressed(CellTappedEventArgs args)
        {
            if (ScheduleViewMode == ScheduleView.MonthView)
            {
                DateTime argsDayDateTime = new DateTime(args.Datetime.Year, args.Datetime.Month, args.Datetime.Day);
                DateTime nowDayDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                //CLIENT-536
                //if (argsDayDateTime >= nowDayDateTime)
                {
                    await ParentBaseModel?.PublishMessage(new WidgetMessage
                    {
                        EventType = WidgetEventType.ShowNewOrEditRequested,
                        ControlKey = "CalendarSchedule",
                        Data = BuildAdditionalArgumentsDictionary(new DateTime(args.Datetime.Year, args.Datetime.Month,
                        args.Datetime.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second))
                    });
                }
            }
            else
            {     //CLIENT-536           
                //if (args.Datetime >= DateTime.Now)
                {
                    await ParentBaseModel?.PublishMessage(new WidgetMessage
                    {
                        EventType = WidgetEventType.ShowNewOrEditRequested,
                        ControlKey = "CalendarSchedule",
                        Data = BuildAdditionalArgumentsDictionary(args.Datetime)
                    });
                }
            }
        }

        private Dictionary<string, string> BuildAdditionalArgumentsDictionary(DateTime dateTime)
        {
            Dictionary<string, string> additionalArguments = new Dictionary<string, string>();

            additionalArguments.Add("Date", dateTime.ToString(CrmConstants.DateFormat));
            additionalArguments.Add("Time", dateTime.ToString(CrmConstants.TimeFormat));

            return additionalArguments;
        }

        private async Task OnScheduleDatesChanged(VisibleDatesChangedEventArgs obj)
        {
            if (obj.visibleDates.Count > 0)
            {
                var firstVisibleDate = obj.visibleDates[0];
                var lastVisibleDate = obj.visibleDates[obj.visibleDates.Count - 1];

                var startDate = new DateTime(firstVisibleDate.Year, firstVisibleDate.Month, firstVisibleDate.Day);
                var endDate = new DateTime(lastVisibleDate.Year, lastVisibleDate.Month, lastVisibleDate.Day);

                CalendarDateTimeInterval calendarDateTimeInterval = new CalendarDateTimeInterval(startDate, endDate);

                await ParentBaseModel?.PublishMessage(new WidgetMessage
                {
                    EventType = WidgetEventType.ScheduleVisibleDatesChanged,
                    ControlKey = "CalendarSchedule",
                    Data = calendarDateTimeInterval
                });
            }
        }
        public void UpdateData(List<CrmScheduleAppointment> crmScheduleAppointments)
        {
            ScheduleAppointmentCollection newSchedule = new ScheduleAppointmentCollection();
            foreach (CrmScheduleAppointment crmScheduleAppointment in crmScheduleAppointments)
            {
                newSchedule.Add(crmScheduleAppointment);
            }
            ScheduleData = newSchedule;

        }

        public async Task CalendarViewModeChanged(CalendarViewModes calendarViewModes)
        {
            switch (calendarViewModes)
            {
                case CalendarViewModes.Day:
                    ScheduleViewMode = ScheduleView.DayView;
                    break;
                case CalendarViewModes.Week:
                    ScheduleViewMode = ScheduleView.WeekView;
                    break;
                case CalendarViewModes.Month:
                    ScheduleViewMode = ScheduleView.MonthView;
                    break;
                case CalendarViewModes.Timeline:
                    ScheduleViewMode = ScheduleView.TimelineView;
                    break;
                default:
                    ScheduleViewMode = ScheduleView.DayView;
                    break;
            }
        }

        public void SetWorkingHours(int firstWorkingHour, int numberOfWorkingHours)
        {
            if(firstWorkingHour > 0)
            {
                _firstWorkingHour = firstWorkingHour;
            }

            if(numberOfWorkingHours > 0)
            {
                _numberOfWorkingHours = numberOfWorkingHours;
            }

            _dayViewSettings.WorkStartHour = _firstWorkingHour;
            _dayViewSettings.WorkEndHour = _firstWorkingHour + _numberOfWorkingHours;
            DayViewSettings = _dayViewSettings;

            _weekViewSettings.WorkStartHour = _firstWorkingHour;
            _weekViewSettings.WorkEndHour = _firstWorkingHour + _numberOfWorkingHours;
            WeekViewSettings = _weekViewSettings;
        }
    }
}
