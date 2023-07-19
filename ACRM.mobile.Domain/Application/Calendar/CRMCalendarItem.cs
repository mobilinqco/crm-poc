using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.Calendar
{
    public class CRMCalendarItem
    {
        public String RecordId { get; set; }
        public DeviceCalendarEvent CalendarEvent { get; set; }
        public ListDisplayRow ListEvent { get; set; }
    }
}
