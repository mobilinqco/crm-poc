using System;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Calendar;
using Syncfusion.SfSchedule.XForms;
using Xamarin.Forms;

namespace ACRM.mobile.Utils
{
    public class CrmScheduleAppointment: ScheduleAppointment
    {

        public CalendarViewTemplate CalendarViewTemplate { get; private set; }
        public UserAction UserAction { get; private set; }
        public DeviceCalendarEvent DeviceCalendarEvent { get; private set; }

        public CrmScheduleAppointment(CalendarViewTemplate calendarViewTemplate,
            UserAction userAction,
            DeviceCalendarEvent deviceCalendarEvent)
        {
            CalendarViewTemplate = calendarViewTemplate;
            UserAction = userAction;
            DeviceCalendarEvent = deviceCalendarEvent;
            SetProperties();
        }

        private void SetProperties()
        {
            Subject = DeviceCalendarEvent.Title;
            IsAllDay = DeviceCalendarEvent.IsAllDay;
            if (DeviceCalendarEvent.StartDate == DateTime.MinValue)
            {
                StartTime = DateTime.Now;
            }
            else
            {
                StartTime = DeviceCalendarEvent.StartDate;
            }

            if (DeviceCalendarEvent.EndDate == DateTime.MinValue)
            {
                EndTime = DateTime.Now.AddHours(1);
            }
            else
            {
                EndTime = DeviceCalendarEvent.EndDate;
            }
            
            Color = Color.FromHex(DeviceCalendarEvent.Color);
        }
    }
}
