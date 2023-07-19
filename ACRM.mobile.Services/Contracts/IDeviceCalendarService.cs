using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using ACRM.mobile.Domain.Application.Calendar;

namespace ACRM.mobile.Services.Contracts
{
    public interface IDeviceCalendarService
    {
        Task<bool> HasCalendarPermissions();

        Task<List<DeviceCalendar>> GetDeviceCalendarsAsync(CancellationToken cancellationToken);
        Task<List<DeviceCalendarEvent>> GetDeviceCalendarEventsAsync(DeviceCalendar deviceCalendar, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    }
}
