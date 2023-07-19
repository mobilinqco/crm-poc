using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Essentials;

using EventKit;

using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Domain.Application.Calendar;
using Foundation;
using System.Threading;
using CoreGraphics;


// iOS Calendar Integration
[assembly: Dependency(typeof(ACRM.mobile.iOS.Services.DeviceCalendarService))]
namespace ACRM.mobile.iOS.Services
{
    public class DeviceCalendarService : IDeviceCalendarService
    {
        public DeviceCalendarService()
        {
        }

        public async Task<List<DeviceCalendar>> GetDeviceCalendarsAsync(CancellationToken cancellationToken)
        {
            var calendars = new List<DeviceCalendar>();
            var permissions = await HasCalendarPermissions();
            if (!permissions)
            {
                return calendars;
            }

            var store = new EKEventStore();
            var fetchedCalendars = store.GetCalendars(EKEntityType.Event);
            fetchedCalendars.ToList().ForEach(fc =>
            {
                calendars.Add(new DeviceCalendar(fc.Title, fc.CalendarIdentifier, ToHex(fc.CGColor), false));
            });

            return calendars;
        }

        private string ToHex(CGColor color)
        {
            if (color.Components.Count() >= 3)
            {
                var r = (int)(color.Components[0] * 255);
                var g = (int)(color.Components[1] * 255);
                var b = (int)(color.Components[2] * 255);
                var a = (int)255;
                if (color.Components.Count() > 3)
                {
                    a = (int)(color.Components[3] * 255);
                }

                return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
            }
            return "#ffffffff";
        }

        public async Task<List<DeviceCalendarEvent>> GetDeviceCalendarEventsAsync(DeviceCalendar deviceCalendar, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var events = new List<DeviceCalendarEvent>();
            var permissions = await HasCalendarPermissions();
            if (!permissions)
            {
                return events;
            }

            var store = new EKEventStore();

            var start = (NSDate)DateTime.SpecifyKind(startDate.ToUniversalTime(), DateTimeKind.Utc);
            var end = (NSDate)DateTime.SpecifyKind(endDate.ToUniversalTime(), DateTimeKind.Utc);

            // TODO: improve predicate to fetch only for the devicecalendar from the arguments
            var eventPredicate = store.PredicateForEvents(start, end, store.Calendars);
            var fetchedEvents = store.EventsMatching(eventPredicate);

            fetchedEvents.ToList().ForEach(fe =>
            {
                if(fe.Calendar.CalendarIdentifier == deviceCalendar.Identifier)
                {
                    events.Add(new DeviceCalendarEvent
                    {
                        CalendarId = deviceCalendar.Identifier,
                        Title = fe.Title,
                        Location = fe.Location,
                        StartDate = ((DateTime)fe.StartDate).ToLocalTime(),
                        EndDate = ((DateTime)fe.EndDate).ToLocalTime(),
                        Color = deviceCalendar.Color,
                        IsAllDay = fe.AllDay,
                        Status = ConvertToAcrmEventStatus(fe.Status),
                        IsCrmEvent = false
                    }); ;
                }
            });

            return events;
        }

        public async Task<bool> HasCalendarPermissions()
        {
            return await Permissions.CheckStatusAsync<Permissions.CalendarRead>() == PermissionStatus.Granted;
        }

        public EventStatus ConvertToAcrmEventStatus(EKEventStatus status)
        {
            switch (status)
            {
                case EKEventStatus.None:
                    return EventStatus.NotSet;
                case EKEventStatus.Confirmed:
                    return EventStatus.Confirmed;
                case EKEventStatus.Tentative:
                    return EventStatus.Tentative;
                case EKEventStatus.Cancelled:
                    return EventStatus.Canceled;
                default:
                    return EventStatus.NotSet;
            }
        }
    }
}
