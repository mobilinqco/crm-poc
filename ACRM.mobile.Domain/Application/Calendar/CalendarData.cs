using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.Calendar
{
    public class CalendarData
    {
        public List<DeviceCalendarEvent> CalendarEvents { get; }
        public List<ListDisplayRow> ListEvents { get; }

        public CalendarData(List<DeviceCalendarEvent> calendarEvents, List<ListDisplayRow> listEvents)
        {
            CalendarEvents = calendarEvents;
            ListEvents = listEvents;
        }
    }
}
