using System;
namespace ACRM.mobile.Domain.Application.Calendar
{
    public class DeviceCalendarEvent
    {
        // Generic Event part
        public string CalendarId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EventStatus Status { get; set; }
        public string Color { get; set; }
        public bool IsCrmEvent { get; set; }

        // Device calendar part
        public string Location { get; set; }
        public bool IsAllDay { get; set; }

        // CRM Event part
        public string UserActionUnitName { get; set; }
        public string RecordId { get; set; }
        public string PersonLabel { get; set; }
        public string CompanyLabel { get; set; }
        public string RepLabel { get; set; }
        public string RepId { get; set; }
        public string Type { get; set; }

        // Table Caption for Subject
        public ListDisplayRow TableCaptionContent { get; set; }

        public DeviceCalendarEvent()
        {
        }

        public void SetEventStatus(string crmStatus)
        {
            switch(crmStatus)
            {
                default:
                    Status = EventStatus.NotSet;
                    break;
            }
        }
    }
}