using System;
namespace ACRM.mobile.Domain.Application.Calendar
{
    public class CalendarDateTimeInterval
    {
        public DateTime StartDateTime { get; }
        public DateTime EndDateTime { get; }

        public CalendarDateTimeInterval(DateTime startDateTime, DateTime endDateTime)
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }
    }
}
