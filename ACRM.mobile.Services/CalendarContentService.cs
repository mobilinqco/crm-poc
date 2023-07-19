using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Logging;
using System.Threading;
using ACRM.mobile.Domain.Application.Calendar;
using System;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application.ActionTemplates;
using System.Linq;
using AsyncAwaitBestPractices;
using ACRM.mobile.Services.Utils;
using System.Collections.Concurrent;

namespace ACRM.mobile.Services
{
    public class CalendarContentService : ContentServiceBase, ICalendarContentService
    {
        private readonly IRepService _repService;
        private readonly IDeviceCalendarService _deviceCalendarService;
        private readonly ISearchContentService _searchContentService;
        private readonly ITokenProcessor _tokenProcessor;
        private CalendarViewTemplate _actionTemplate;
        private SearchAndList _searchAndList;
        private ExpandComponent _expandComponent;

        private bool _includeSystemCalendar;

        private bool _isCalendarPermissionGranted = false;

        private RequestMode _requestMode = RequestMode.Offline;

        private DateTime _startDate = DateTime.Today.AddMonths(-1);
        private DateTime _endDate = DateTime.Today.AddMonths(1).AddDays(1).AddTicks(-1);

        private List<CrmRep> _crmReps = new List<CrmRep>();
        private HashSet<string> _selectedCrmRepIds = new HashSet<string>();

        private HashSet<string> _selectedCrmCalendarIds = new HashSet<string>();
        private List<DeviceCalendar> _crmCalendars = new List<DeviceCalendar>();
        private HashSet<string> _selectedDeviceCalendarIds = new HashSet<string>();
        private List<DeviceCalendar> _deviceCalendars = new List<DeviceCalendar>();

        private ConcurrentDictionary<string, CRMCalendarItem> _crmCalendarEvents = new ConcurrentDictionary<string, CRMCalendarItem>();
        private List<DeviceCalendarEvent> _deviceCalendarEvents = new List<DeviceCalendarEvent>();

        private Dictionary<string, CalendarEventComponent> _calendarEventComponents = new Dictionary<string, CalendarEventComponent>();

        public ISearchContentService SourceSearchService
        {
            get => _searchContentService;
        }

        public CalendarContentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            IUserActionBuilder userActionBuilder,
            ITokenProcessor tokenProcessor,
            HeaderComponent headerComponent,
            ImageResolverComponent imageResolverComponent,
            FieldGroupComponent fieldGroupComponent,
            ILogService logService,
            IRepService repService,
            IDeviceCalendarService deviceCalendarService,
            ISearchContentService searchContentService,
            ExpandComponent expandComponent,
            IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _repService = repService;
            _deviceCalendarService = deviceCalendarService;
            _expandComponent = expandComponent;
            _searchContentService = searchContentService;
            _tokenProcessor = tokenProcessor;
        }

        public void SetCalendarPermissionFlag(bool isCalendarPermissionGranted)
        {
            _isCalendarPermissionGranted = isCalendarPermissionGranted;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");

            ViewReference vr = _action.ViewReference;

            if (vr == null)
            {
                vr = await _configurationService.GetViewForMenu(_action.ActionUnitName, cancellationToken).ConfigureAwait(false);
            }

            _actionTemplate = new CalendarViewTemplate(vr);
            _searchAndList = await _configurationService.GetSearchAndList(_actionTemplate.ConfigName(), cancellationToken).ConfigureAwait(false);

            string infoAreaId = InfoAreaUnitName();

            if (!string.IsNullOrEmpty(infoAreaId))
            {
                _infoArea = _configurationService.GetInfoArea(infoAreaId);

                if (_searchAndList == null)
                {
                    _searchAndList = await _configurationService.GetSearchAndList(infoAreaId, cancellationToken).ConfigureAwait(false);
                }

                string headerName = _searchAndList.HeaderGroupName + ".Search";
                Header header = await _configurationService.GetHeader(headerName, cancellationToken).ConfigureAwait(false);
                _headerComponent.InitializeContext(header, _action);
                _headerButtons = await _headerComponent.HeaderButtons(cancellationToken).ConfigureAwait(false);

                FieldControl fieldControl = await _configurationService.GetFieldControl(_searchAndList.FieldGroupName + ".List", cancellationToken).ConfigureAwait(false);

                TableInfo tableInfo = await _configurationService.GetTableInfoAsync(InfoAreaUnitName(), cancellationToken).ConfigureAwait(false);
                _fieldGroupComponent.InitializeContext(fieldControl, tableInfo);

                _expandComponent.InitializeContext(infoAreaId, infoAreaId, tableInfo);
                if (_expandComponent.IsExpandDefined(_actionTemplate.ConfigName()))
                {
                    _expandComponent.ExpandName = _actionTemplate.ConfigName();
                }

                await SetConfigurationValues(cancellationToken);
            }

            _searchContentService.SetSourceAction(_action);
            _searchContentService.PrepareContentAsync(cancellationToken).SafeFireAndForget<Exception>(onException: ex =>
            {
                _logService.LogError($"Unable to prepare content {ex.Message}");
            });

            OnDataReady();

            _logService.LogDebug("End PrepareContentAsync");
        }

        public void SetCalendarEventComponents(Dictionary<string, CalendarEventComponent> calendarEventComponents)
        {
            foreach (KeyValuePair<string, CalendarEventComponent> entry in calendarEventComponents)
            {
                _calendarEventComponents.Add(entry.Key, entry.Value);
            }
        }

        public bool HasUserFilters()
        {
            return _searchContentService.HasUserFilters(0);
        }

        public List<Filter> GetUserFilters()
        {
            return _searchContentService.GetUserFilters(0);
        }

        public void SetEnabledUserFilters(List<Filter> filters)
        {
            foreach (var calendarEventComponentKey in _calendarEventComponents.Keys)
            {
                _calendarEventComponents[calendarEventComponentKey].SetEnabledUserFilters(filters);
            }
        }

        private string InfoAreaUnitName()
        {
            string infoAreaUnitName = string.Empty;

            if (_infoArea != null && !string.IsNullOrEmpty(_infoArea.UnitName))
            {
                infoAreaUnitName = _infoArea.UnitName;
            }

            if (string.IsNullOrEmpty(infoAreaUnitName))
            {
                infoAreaUnitName = _actionTemplate.InfoArea();
            }

            if (string.IsNullOrEmpty(infoAreaUnitName))
            {
                infoAreaUnitName = _searchAndList.InfoAreaId;
            }

            return infoAreaUnitName;
        }


        private async Task SetConfigurationValues(CancellationToken cancellationToken)
        {
            IncludeSystemCalendar();
            StartDateLimit();
        }

        private void IncludeSystemCalendar()
        {
            if(_actionTemplate.ShowSystemCalendarFilter())
            {
                _includeSystemCalendar = _actionTemplate.IncludeSystemCalendar();
            } 
            else
            {
                WebConfigValue configurationValue = _configurationService.GetConfigValue("CalendarIncludeSystemCalendar");

                if (configurationValue == null)
                {
                    configurationValue = _configurationService.GetConfigValue("Calendar.IncludeSystemCalendar");
                }

                bool includeSystemCalendar = false;

                if (configurationValue != null && !string.IsNullOrWhiteSpace(configurationValue.Value))
                {
                    bool.TryParse(configurationValue.Value, out includeSystemCalendar);
                }

                _includeSystemCalendar = includeSystemCalendar;
            }
        }

        private void StartDateLimit()
        {
            WebConfigValue configurationValue = _configurationService.GetConfigValue("CalendarStartDateLimit");

            if(configurationValue == null)
            {
                configurationValue = _configurationService.GetConfigValue("Calendar.StartDateLimit");
            }

            if(configurationValue != null && !string.IsNullOrWhiteSpace(configurationValue.Value))
            {
                // _startDate = _tokenProcessor.TokenDateValue(configurationValue.Value); TODO in TokenProcessor
            }
        }


        public string SearchColumns(int tabId)
        {
            return _searchContentService.SearchColumns(tabId);
        }

        public bool HasResults(int tabId)
        {
            return _searchContentService.HasResults(tabId);
        }

        public bool AreResultsRetrievedOnline(int tabId)
        {
            return _searchContentService.AreResultsRetrievedOnline(tabId);
        }

        public bool IsOfflineSearch()
        {
            if(_requestMode == RequestMode.Offline || _requestMode == RequestMode.Fastest)
            {
                return true;
            }
            return false;
        }

        public async Task<UserAction> ActionForSelectedCalendarListItem(int tabId, ListDisplayRow selectedRecord, CancellationToken cancellationToken)
        {
            return await _searchContentService.ActionForItemSelect(tabId, selectedRecord, cancellationToken);
        }

        public async Task<CalendarData> ChangeRequestMode(string searchText, CancellationToken cancellationToken)
        {
            if (_requestMode == RequestMode.Offline)
            {
                _requestMode = RequestMode.Online;
            }
            else
            {
                _requestMode = RequestMode.Offline;
            }

            _crmCalendarEvents.Clear();

            await SyncCrmCalendarEvents(searchText, _startDate, _endDate, cancellationToken);

            return FilterSelectedEvents();
        }

        public async Task<CalendarData> GetDataAsync(string searchText, CancellationToken cancellationToken)
        {
            await SyncCrmReps(cancellationToken);
            await SyncCalendars(cancellationToken);
            return await SyncEvents(_startDate, _endDate, searchText, cancellationToken);
        }

        private async Task SyncCrmReps(CancellationToken cancellationToken)
        {
            var reps = await GetRepsDataAsync(cancellationToken);
            reps.ForEach(rep =>
            {
                _crmReps.Add(rep);
            });

            if(_actionTemplate.RepFilterCurrentRepActive())
            {
                _selectedCrmRepIds.Add(_sessionContext.User.SessionInformation.Attributes.RepName);
            }
        }

        private async Task<List<CrmRep>> GetRepsDataAsync(CancellationToken cancellationToken)
        {
            List<CrmRep> employees = await _repService.GetAllCrmReps(cancellationToken);

            // Additional Processing

            return employees;
        }

        private async Task SyncCalendars(CancellationToken cancellationToken)
        {
            SyncCrmCalendars();
            if(_includeSystemCalendar)
            {
                await SyncDeviceCalendars(cancellationToken);
            }
        }

        private void SyncCrmCalendars()
        {
            var crmCalendar = new DeviceCalendar("CRM Calendar", "CrmCalendar", null, true);
            _crmCalendars.Add(crmCalendar);
            _selectedCrmCalendarIds.Add(crmCalendar.Identifier);
        }

        private async Task SyncDeviceCalendars(CancellationToken cancellationToken)
        {
            if(_isCalendarPermissionGranted)
            {
                foreach (DeviceCalendar deviceCalendar in await GetDeviceCalendarsAsync(cancellationToken))
                {                  
                    _deviceCalendars.Add(deviceCalendar);
                    _selectedDeviceCalendarIds.Add(deviceCalendar.Identifier);
                }
            }
        }

        private async Task<List<DeviceCalendar>> GetDeviceCalendarsAsync(CancellationToken cancellationToken)
        {
            var calendars = await _deviceCalendarService.GetDeviceCalendarsAsync(cancellationToken);

            // Additional Processing

            return calendars;
        }

        public async Task<CalendarData> SyncEventsByNewStartDateTime(DateTime newStartDateTime, string searchText, CancellationToken cancellationToken)
        {
            var calendarData = await SyncEvents(newStartDateTime, _startDate.AddTicks(-1), searchText, cancellationToken);
            _startDate = newStartDateTime;
            return calendarData;
        }

        public async Task<CalendarData> SyncEventsByNewEndDateTime(DateTime newEndDateTime, string searchText, CancellationToken cancellationToken)
        {
            var newEndDateTimeLimit = newEndDateTime.AddDays(1).AddTicks(-1);
            var calendarData = await SyncEvents(_endDate.AddTicks(1), newEndDateTimeLimit, searchText, cancellationToken);
            _endDate = newEndDateTimeLimit;
            return calendarData;
        }

        public async Task<CalendarData> SyncEventsBySearch(string searchText, CancellationToken cancellationToken)
        {
            _crmCalendarEvents.Clear();
            await SyncCrmCalendarEvents(searchText, _startDate, _endDate, cancellationToken);
            return FilterSelectedEvents();
        }

        public async Task<CalendarData> SyncEvents(DateTime startDate, DateTime endDate, string searchText, CancellationToken cancellationToken)
        {
            await SyncCrmCalendarEvents(searchText, startDate, endDate, cancellationToken);
            await SyncDeviceCalendarsEvents(startDate, endDate, cancellationToken);
            return FilterSelectedEvents();
        }

        private async Task SyncCrmCalendarEvents(string searchText, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var crmEvents = await GetCrmCalendarEventsAsync(searchText, _requestMode, startDate, endDate, cancellationToken);
            Parallel.ForEach(crmEvents, crmEvent =>
            {
                if (!_crmCalendarEvents.Keys.Contains(crmEvent.RecordId))
                {
                    _crmCalendarEvents.TryAdd(crmEvent.RecordId, crmEvent);
                }
            });
        }

        private async Task<List<CRMCalendarItem>> GetCrmCalendarEventsAsync(string searchText, RequestMode requestMode, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            List<CRMCalendarItem> crmCalendarItems = new List<CRMCalendarItem>();

            var tasks = _calendarEventComponents?.Keys.Select(f => _calendarEventComponents[f].GetCrmCalendarEventsAsync(searchText, requestMode, startDate, endDate, cancellationToken));
            var lstItems = await Task.WhenAll(tasks);

            foreach (var items in lstItems)
            {
                if (items?.Count > 0 )
                {
                    crmCalendarItems.AddRange(items);
                }
            }

            return crmCalendarItems;
        }

        private async Task SyncDeviceCalendarsEvents(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            foreach (var calendar in _deviceCalendars)
            {
                var deviceEvents = await GetDeviceCalendarEventsAsync(calendar, startDate, endDate, cancellationToken);
                deviceEvents.ForEach(deviceEvent =>
                {
                    _deviceCalendarEvents.Add(deviceEvent);
                });
            }
        }

        private async Task<List<DeviceCalendarEvent>> GetDeviceCalendarEventsAsync(DeviceCalendar deviceCalendar, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            var events = await _deviceCalendarService.GetDeviceCalendarEventsAsync(deviceCalendar, startDate, endDate, cancellationToken);

            return events;
        }

        public CalendarData SetSelectedCalendarsSets(HashSet<string> selectedCrmCalendarIds, HashSet<string> selectedDeviceCalendarIds)
        {
            _selectedCrmCalendarIds = selectedCrmCalendarIds;
            _selectedDeviceCalendarIds = selectedDeviceCalendarIds;
            return FilterSelectedEvents();
        }

        public CalendarData SetSelectedRepIds(HashSet<string> selectedRepIdSet)
        {
            _selectedCrmRepIds = selectedRepIdSet;
            return FilterSelectedEvents();
        }

        private CalendarData FilterSelectedEvents()
        {
            ConcurrentBag<DeviceCalendarEvent> filteredCalendarEvents = new ConcurrentBag<DeviceCalendarEvent>();
            ConcurrentBag<ListDisplayRow> filteredListEvents = new ConcurrentBag<ListDisplayRow>();

            Parallel.ForEach(_crmCalendarEvents, pair =>
            {
                if (_selectedCrmCalendarIds.Contains(pair.Value?.CalendarEvent.CalendarId)
                    && (_selectedCrmRepIds.Contains(pair.Value?.CalendarEvent.RepId) || string.IsNullOrEmpty(pair.Value?.CalendarEvent.RepId)))
                {
                    filteredCalendarEvents.Add(pair.Value.CalendarEvent);
                    filteredListEvents.Add(pair.Value.ListEvent);
                }
            });

            Parallel.ForEach(_deviceCalendarEvents, deviceCalendarEvent =>
            {
                if (_selectedDeviceCalendarIds.Contains(deviceCalendarEvent.CalendarId))
                {
                    filteredCalendarEvents.Add(deviceCalendarEvent);
                }
            });

            return new CalendarData(filteredCalendarEvents.ToList(), filteredListEvents.ToList());
        }

        public List<CrmRep> GetCrmReps()
        {
            return _crmReps;
        }

        public List<DeviceCalendar> GetCrmCalendars()
        {
            return _crmCalendars;
        }

        public List<DeviceCalendar> GetDeviceCalendars()
        {
            return _deviceCalendars;
        }

        public UserAction ActionForCrmCalendarComponentEvent(string userActionUnitName, string recordId)
        {
            return _calendarEventComponents[userActionUnitName].ActionForCrmCalendarEvent(recordId);
        }

        public List<UserAction> ActionForLongPress(Dictionary<string, string> additionalArguments)
        {
            List<UserAction> UserActions = new List<UserAction>();

            foreach (var key in _calendarEventComponents.Keys)
            {
                var calender = _calendarEventComponents[key];
                if (calender != null && calender.LongPressUserActions?.Count > 0)
                {
                    for (int i = 0; i < calender.LongPressUserActions?.Count; i++)
                    {
                        UserAction userAction = calender.LongPressUserActions[i];
                        userAction.AdditionalArguments = additionalArguments;
                        if (!UserActions.Any(a => a.ActionUnitName.Equals(userAction.ActionUnitName)))
                        {
                            UserActions.Add(userAction);
                        }
                    }
                }
            }
            return UserActions;
        }

        private string InfoAreaUnitName(InfoArea infoArea, ActionTemplateBase actionTemplate, SearchAndList searchAndList)
        {
            string infoAreaUnitName = string.Empty;

            if (infoArea != null && !string.IsNullOrEmpty(infoArea.UnitName))
            {
                infoAreaUnitName = infoArea.UnitName;
            }

            if (string.IsNullOrEmpty(infoAreaUnitName) && actionTemplate != null)
            {
                infoAreaUnitName = actionTemplate.InfoArea();
            }

            if (string.IsNullOrEmpty(infoAreaUnitName) && searchAndList != null)
            {
                infoAreaUnitName = searchAndList.InfoAreaId;
            }

            return infoAreaUnitName;
        }

        public DateTime GetStartDateTime()
        {
            return _startDate;
        }

        public DateTime GetEndDateTime()
        {
            return _endDate;
        }

        public bool IsRepSelected(string repId)
        {
            return _selectedCrmRepIds.Contains(repId);
        }

        public bool IsCrmCalendarSelected(string calendarId)
        {
            return _selectedCrmCalendarIds.Contains(calendarId);
        }

        public bool IsDeviceCalendarSelected(string calendarId)
        {
            return _selectedDeviceCalendarIds.Contains(calendarId);
        }

        public double SearchDelay()
        {
            string configParameter = "Search.OnlineDelayTime";

            if (IsOfflineSearch())
            {
                configParameter = "Search.OfflineDelayTime";
            }

            return _configurationService.GetNumericConfigValue<double>(configParameter, CrmConstants.DefaultSearchDelayTime);
        }

        public bool SearchAutoSwitchToOffline()
        {
            return _configurationService.GetBoolConfigValue("Search.AutoSwitchToOffline");
        }

        public void SetRequestModeToOffline()
        {
            _requestMode = RequestMode.Offline;
        }

        public RequestMode CurrentRequestMode()
        {
            return _requestMode;
        }

        public CalendarViewTemplate GetCalendarComponentViewTemplate(string userActionUnitName)
        {
            return _calendarEventComponents[userActionUnitName].GetCalendarViewTemplate();
        }

        public string GetDefaultViewType()
        {
            return _actionTemplate.DefaultViewType();
        }

        public int GetCalendarFirstWorkingHour()
        {
            int calendarFirstWorkingHour = _configurationService.GetNumericConfigValue("CalendarFirstWorkingHour", 0);

            if(calendarFirstWorkingHour != 0)
            {
                return calendarFirstWorkingHour;
            }
            else
            {
                return _configurationService.GetNumericConfigValue("Calendar.FirstWorkingHour", 0);
            }
        }

        public int GetCalendarNumberOfWorkingHours()
        {
            int calendarNumberOfWorkingHours = _configurationService.GetNumericConfigValue("CalendarNumberOfWorkingHours", 0);

            if(calendarNumberOfWorkingHours != 0)
            {
                return calendarNumberOfWorkingHours;
            }
            else
            {
                return _configurationService.GetNumericConfigValue("Calendar.NumberOfWorkingHours", 0);
            }
        }
    }
}
