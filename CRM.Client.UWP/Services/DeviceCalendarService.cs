using ACRM.mobile.Domain.Application.Calendar;
using ACRM.mobile.Services.Contracts;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Xamarin.Forms;

[assembly: Dependency(typeof(CRM.Client.UWP.Services.DeviceCalendarService))]
namespace CRM.Client.UWP.Services
{
    class DeviceCalendarService : IDeviceCalendarService
    {
        public async Task<List<DeviceCalendarEvent>> GetDeviceCalendarEventsAsync(DeviceCalendar deviceCalendar, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var events = new List<DeviceCalendarEvent>();
            
            var permissions = await HasCalendarPermissions();
            if (!permissions)
            {
                return events;
            }

            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);
            AppointmentCalendar calendar = null;

            try
            {
                calendar = await appointmentStore.GetAppointmentCalendarAsync(deviceCalendar.Identifier);
                var options = new FindAppointmentsOptions { IncludeHidden = false };
                options.FetchProperties.Add(AppointmentProperties.Subject);
                options.FetchProperties.Add(AppointmentProperties.StartTime);
                options.FetchProperties.Add(AppointmentProperties.Duration);
                options.FetchProperties.Add(AppointmentProperties.AllDay);
                options.FetchProperties.Add(AppointmentProperties.IsCanceledMeeting);
                options.FetchProperties.Add(AppointmentProperties.UserResponse);
                options.FetchProperties.Add(AppointmentProperties.Location);

                var appointments = await calendar.FindAppointmentsAsync(startDate, endDate - startDate, options);
                foreach(var appointment in appointments)
                {
                    if (appointment.CalendarId == deviceCalendar.Identifier)
                    {
                        events.Add(new DeviceCalendarEvent
                        {
                            Title = appointment.Subject,
                            Location = appointment.Location,
                            StartDate = appointment.StartTime.DateTime,
                            EndDate = appointment.StartTime.DateTime + appointment.Duration,
                            Color = deviceCalendar.Color,
                            IsAllDay = appointment.AllDay,
                            Status = ConvertToAcrmEventStatus(appointment.UserResponse),
                            IsCrmEvent = false
                        });
                    }
                }
            }
            catch (ArgumentException ex)
            { 
            }

            return events;
        }

        public async Task<List<DeviceCalendar>> GetDeviceCalendarsAsync(CancellationToken cancellationToken)
        {
            var calendars = new List<DeviceCalendar>();
            var permissions = await HasCalendarPermissions();

            if (!permissions)
            {
                return calendars;
            }

            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);
            var allCalendars = await appointmentStore.FindAppointmentCalendarsAsync();


            allCalendars.ToList().ForEach(fc =>
            {
                calendars.Add(new DeviceCalendar(fc.DisplayName, fc.LocalId, ColorHelper.ToHex(fc.DisplayColor), false));
            });

            return calendars;
        }

        public Task<bool> HasCalendarPermissions()
        {
            return Task.FromResult(true);
        }

        public EventStatus ConvertToAcrmEventStatus(AppointmentParticipantResponse status)
        {
            switch (status)
            {
                case AppointmentParticipantResponse.None:
                    return EventStatus.NotSet;
                case AppointmentParticipantResponse.Accepted:
                    return EventStatus.Confirmed;
                case AppointmentParticipantResponse.Tentative:
                    return EventStatus.Tentative;
                case AppointmentParticipantResponse.Declined:
                    return EventStatus.Canceled;
                default:
                    return EventStatus.NotSet;
            }
        }
    }
}
