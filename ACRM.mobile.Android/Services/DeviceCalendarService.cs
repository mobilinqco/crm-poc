using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;

using ACRM.mobile.Services.Contracts;
using Android.Provider;
using Android.Content;
using Xamarin.Essentials;
using System.Threading;
using ACRM.mobile.Domain.Application.Calendar;
using Java.Util;

// Android Calendar Integration
[assembly: Dependency(typeof(ACRM.mobile.Droid.Services.DeviceCalendarService))]
namespace ACRM.mobile.Droid.Services
{
    public class DeviceCalendarService : IDeviceCalendarService
    {
        private readonly Context _context;

        public DeviceCalendarService()
        {
            _context = Android.App.Application.Context;
        }

        public async Task<List<DeviceCalendar>> GetDeviceCalendarsAsync(CancellationToken cancellationToken)
        {
            var calendars = new List<DeviceCalendar>();
            var permissions = await HasCalendarPermissions();
            if (!permissions)
            {
                return calendars;
            }

            var calendarUri = CalendarContract.Calendars.ContentUri;

            string[] calendarsProjection =
            {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
                CalendarContract.Calendars.InterfaceConsts.AccountName,
                CalendarContract.Calendars.InterfaceConsts.CalendarColor
            };

            var cursor = _context.ContentResolver.Query(calendarUri, calendarsProjection, null, null, null);

            while(cursor.MoveToNext())
            {
                calendars.Add(new DeviceCalendar(cursor.GetString(1), cursor.GetString(0), cursor.GetString(3), false));
            }

            return calendars;
        }

        public async Task<List<DeviceCalendarEvent>> GetDeviceCalendarEventsAsync(DeviceCalendar deviceCalendar, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var events = new List<DeviceCalendarEvent>();
            var permissions = await HasCalendarPermissions();
            if (!permissions)
            {
                return events;
            }

            var eventsUri = CalendarContract.Events.ContentUri;

            string[] eventsProjection = {
                CalendarContract.Events.InterfaceConsts.Id,
                CalendarContract.Events.InterfaceConsts.Title,
                CalendarContract.Events.InterfaceConsts.EventLocation,
                CalendarContract.Events.InterfaceConsts.Dtstart,
                CalendarContract.Events.InterfaceConsts.Dtend,
                CalendarContract.Events.InterfaceConsts.AllDay,
                CalendarContract.Events.InterfaceConsts.Status
            };

            Calendar queryStartDate = Calendar.Instance;
            queryStartDate.Set(startDate.Year, startDate.Month, startDate.Day, 0, 0);

            Calendar queryEndDate = Calendar.Instance;
            queryEndDate.Set(endDate.Year, endDate.Month, endDate.Day, 0, 0);

            string selection = $"((calendar_id = {deviceCalendar.Identifier}) " +
                $"AND (dtstart >= {queryStartDate.TimeInMillis}) " +
                $"AND (dtend <= {queryEndDate.TimeInMillis}))";

            var cursor = _context.ContentResolver.Query(eventsUri, eventsProjection, selection, null, null);
            while (cursor.MoveToNext())
            {
                events.Add(new DeviceCalendarEvent
                {
                    CalendarId = deviceCalendar.Identifier,
                    Title = cursor.GetString(1),
                    StartDate = GetDateTimeFromMilliseconds(cursor.GetLong(cursor.GetColumnIndex("dtstart"))),
                    EndDate = GetDateTimeFromMilliseconds(cursor.GetLong(cursor.GetColumnIndex("dtend"))),
                    Location = cursor.GetString(2),
                    Status = EventStatus.NotSet,
                    Color = "#ff0000ff",
                    IsAllDay = cursor.GetLong(5) == 1,
                    IsCrmEvent = false
                });
            }

            return events;
        }

        private DateTime GetDateTimeFromMilliseconds(long milliseconds)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(milliseconds);
        }

        public async Task<bool> HasCalendarPermissions()
        {
            return await Permissions.CheckStatusAsync<Permissions.CalendarRead>() == PermissionStatus.Granted;
        }
    }
}