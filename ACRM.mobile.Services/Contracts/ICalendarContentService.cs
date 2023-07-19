using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Calendar;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICalendarContentService: IContentServiceBase
    {
        ISearchContentService SourceSearchService
        {
            get;
        }

        void SetCalendarPermissionFlag(bool isCalendarPermissionGranted);

        Task<CalendarData> ChangeRequestMode(string searchText, CancellationToken cancellationToken);
        CalendarData SetSelectedCalendarsSets(HashSet<string> selectedCrmCalendarIds, HashSet<string> selectedDeviceCalendarIds);
        CalendarData SetSelectedRepIds(HashSet<string> selectedRepIdSet);
        Task<CalendarData> GetDataAsync(string searchText, CancellationToken cancellationToken);
        Task<CalendarData> SyncEventsByNewStartDateTime(DateTime newStartDateTime, string searchText, CancellationToken cancellationToken);
        Task<CalendarData> SyncEventsByNewEndDateTime(DateTime newEndDateTime, string searchText, CancellationToken cancellationToken);
        Task<CalendarData> SyncEventsBySearch(string searchText, CancellationToken cancellationToken);
        Task<CalendarData> SyncEvents(DateTime startDate, DateTime endDate, string searchText, CancellationToken cancellationToken);

        //-- Calendar List Events --//

        string SearchColumns(int tabId);
        bool HasResults(int tabId);
        bool AreResultsRetrievedOnline(int tabId);
        Task<UserAction> ActionForSelectedCalendarListItem(int tabId, ListDisplayRow selectedRecord, CancellationToken cancellationToken);

        //-- CRM Reps --//

        List<CrmRep> GetCrmReps();

        //-- Calendar Schedule Events --//

        List<DeviceCalendar> GetCrmCalendars();
        List<DeviceCalendar> GetDeviceCalendars();
        UserAction ActionForCrmCalendarComponentEvent(string userActionUnitName, string recordId);
        List<UserAction> ActionForLongPress(Dictionary<string, string> additionalArguments);
        DateTime GetStartDateTime();
        DateTime GetEndDateTime();
        bool IsRepSelected(string repId);
        bool IsCrmCalendarSelected(string calendarId);
        bool IsDeviceCalendarSelected(string calendarId);
        bool IsOfflineSearch();
        double SearchDelay();
        bool SearchAutoSwitchToOffline();

        void SetRequestModeToOffline();
        RequestMode CurrentRequestMode();

        //-- Configuration --//

        CalendarViewTemplate GetCalendarComponentViewTemplate(string userActionUnitName);
        public void SetCalendarEventComponents(Dictionary<string, CalendarEventComponent> calendarEventComponents);
        string GetDefaultViewType();
        int GetCalendarFirstWorkingHour();
        int GetCalendarNumberOfWorkingHours();
        List<Filter> GetUserFilters();
        bool HasUserFilters();
        void SetEnabledUserFilters(List<Filter> filters);
    }
}
