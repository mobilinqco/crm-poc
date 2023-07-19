using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Calendar;
using ACRM.mobile.Domain.Application.Messages;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Services;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Utils;
using ACRM.mobile.Utils.Calendar;
using ACRM.mobile.ViewModels;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using Syncfusion.SfSchedule.XForms;
using Syncfusion.XForms.Buttons;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class CalendarControlModel : UIWidget, IFilterItemSelectionHandler, IPopupItemSelectionHandler
    {
        readonly private ICalendarContentService _contentService;
        private readonly ResetTimer timer;
        private readonly ResetTimer DataLoadtimer;
        private List<UserAction> _LongPressUserActions;
        public UserAction UserActionObj;
        public bool CanApplyFilter { get; set; }
        private List<FilterUI> searchFilters = new List<FilterUI>();
        public ICommand SelectionChangedCommand => new Command<Syncfusion.XForms.Buttons.SelectionChangedEventArgs>(async (args) => await OnSelectionChanged(args));
        public ICommand SearchButtonCommand => new Command(async () => await OnSearchButton());
        public ICommand TextChangedCommand => new Command(() => PerformSearch(true));
        public ICommand SelectCalendarsButtonCommand => new Command(() => OnSelectCalendarsButton());
        public ICommand SelectRepsButtonCommand => new Command(() => OnSelectRepsButton());
        public ICommand RequestModeButtonCommand => new Command(async () => await OnRequestModeButton());
        public ICommand FiltersButtonCommand => new Command(async () => await FilterCommandHandler());

        private Object thisLock = new Object();
        private CalendarDateTimeInterval _jobInterval = null;

        public CalendarDateTimeInterval JobInterval
        {
            get => _jobInterval;
            set
            {
                lock (thisLock)
                {
                    _jobInterval = value;
                }
            }
        }

        private UIWidget _content;
        public UIWidget Content
        {
            get => _content;
            set
            {
                _content = value;
                RaisePropertyChanged(() => Content);
            }
        }

        private CalendarScheduleModel _calendarScheduleModel;
        public CalendarScheduleModel CalendarScheduleModel
        {
            get => _calendarScheduleModel;
            set
            {
                _calendarScheduleModel = value;
                RaisePropertyChanged(() => CalendarScheduleModel);
            }
        }

        private CalendarListModel _calendarListModel;
        public CalendarListModel CalendarListModel
        {
            get => _calendarListModel;
            set
            {
                _calendarListModel = value;
                RaisePropertyChanged(() => CalendarListModel);
            }
        }

        private ObservableCollection<SfSegmentItem> _segmentedControlItems;
        public ObservableCollection<SfSegmentItem> SegmentedControlItems
        {
            get => _segmentedControlItems;
            set
            {
                _segmentedControlItems = value;
            }
        }

        private int _segmentedControlIndex;
        public int SegmentedControlIndex
        {
            get => _segmentedControlIndex;
            set
            {
                _segmentedControlIndex = value;
                RaisePropertyChanged(() => SegmentedControlIndex);
            }
        }

        private int _viewModesCounter;
        public int ViewModesCounter
        {
            get => _viewModesCounter;
            set
            {
                _viewModesCounter = value;
                RaisePropertyChanged(() => ViewModesCounter);
            }
        }

        private bool _isSearchBoxVisible = false;
        public bool IsSearchBoxVisible
        {
            get => _isSearchBoxVisible;
            set
            {
                _isSearchBoxVisible = value;
                RaisePropertyChanged(() => IsSearchBoxVisible);
            }
        }

        private string _searchTextBoxPlaceholderText;
        public string SearchTextBoxPlaceholderText
        {
            get => _searchTextBoxPlaceholderText;
            set
            {
                _searchTextBoxPlaceholderText = value;
                RaisePropertyChanged(() => SearchTextBoxPlaceholderText);
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged(() => SearchText);
            }
        }

        private bool _isFiltersButtonCommandEnabled = false;
        public bool IsFiltersButtonCommandEnabled
        {
            get => _isFiltersButtonCommandEnabled;
            set
            {
                _isFiltersButtonCommandEnabled = value;
                RaisePropertyChanged(() => IsFiltersButtonCommandEnabled);
            }
        }

        private bool _isUserFilterHasEnabledFilters = false;
        public bool IsUserFilterHasEnabledFilters
        {
            get => _isUserFilterHasEnabledFilters;
            set
            {
                _isUserFilterHasEnabledFilters = value;
                RaisePropertyChanged(() => IsUserFilterHasEnabledFilters);
            }
        }

        private int _userFilterCount = 0;
        public int UserFilterCount
        {
            get => _userFilterCount;
            set
            {
                _userFilterCount = value;
                RaisePropertyChanged(() => UserFilterCount);
            }
        }

        private bool _isRequestModeButtonEnabled = false;
        public bool IsRequestModeButtonEnabled
        {
            get => _isRequestModeButtonEnabled;
            set
            {
                _isRequestModeButtonEnabled = value;
                RaisePropertyChanged(() => IsRequestModeButtonEnabled);
            }
        }

        private string _requestModeButtonIconText = MaterialDesignIcons.CloudOutline;
        public string RequestModeButtonIconText
        {
            get => _requestModeButtonIconText;
            set
            {
                _requestModeButtonIconText = value;
                RaisePropertyChanged(() => RequestModeButtonIconText);
            }
        }

        private string CalendarModeTitle(CalendarViewModes mode)
        {
            var textGroup = LocalizationKeys.TextGroupProcesses;
            var key = LocalizationKeys.KeyBasicTabOverview;
            switch (mode)
            {
                case CalendarViewModes.Day:
                    key = LocalizationKeys.KeyProcessesCalendarDayView;
                    break;
                case CalendarViewModes.Week:
                    key = LocalizationKeys.KeyProcessesCalendarWeekView;
                    break;
                case CalendarViewModes.Month:
                    key = LocalizationKeys.KeyProcessesCalendarMonthView;
                    break;
                case CalendarViewModes.Timeline:
                    textGroup = LocalizationKeys.TextGroupBasic;
                    key = LocalizationKeys.KeyBasicTimeline;
                    break;
                case CalendarViewModes.List:
                    key = LocalizationKeys.KeyProcessesCalendarListView;
                    break;
                default:
                    break;
            }

            return _localizationController.GetString(textGroup, key)?.Trim();
        }

        private async Task BuildWidgets()
        {
            CalendarScheduleModel = new CalendarScheduleModel(_cancellationTokenSource);
            CalendarScheduleModel.ParentBaseModel = this;
            await CalendarScheduleModel.InitializeControl();

            CalendarListModel = new CalendarListModel(_cancellationTokenSource);
            CalendarListModel.ParentBaseModel = this;
            await CalendarListModel.InitializeControl();
        }

        private void RegisterMessages()
        {
            RegisterMessage(WidgetEventType.ScheduleViewChanged, "CalendarSchedule", OnCalendarScheduleViewChanged);
            RegisterMessage(WidgetEventType.ScheduleVisibleDatesChanged, "CalendarSchedule", OnCalendarScheduleVisibleDatesChanged);
            RegisterMessage(WidgetEventType.ShowCalendarEventPageRequested, "CalendarSchedule", OnShowCalendarEventPageRequested);
            RegisterMessage(WidgetEventType.ShowNewOrEditRequested, "CalendarSchedule", OnShowNewOrEditRequested);
            RegisterMessage(WidgetEventType.RecordSelected, "CalendarList", OnListEventSelected);
        }

        private async Task OnCalendarScheduleViewChanged(WidgetMessage widgetMessage)
        {
            if (widgetMessage.Data is ScheduleView scheduleViewMode)
            {
                switch (scheduleViewMode)
                {
                    case ScheduleView.DayView:
                        SegmentedControlIndex = (int)CalendarViewModes.Day;
                        break;
                    case ScheduleView.WeekView:
                        SegmentedControlIndex = (int)CalendarViewModes.Week;
                        break;
                    case ScheduleView.MonthView:
                        SegmentedControlIndex = (int)CalendarViewModes.Month;
                        break;
                    case ScheduleView.TimelineView:
                        SegmentedControlIndex = (int)CalendarViewModes.Timeline;
                        break;
                }
            }
        }

        private async Task OnCalendarScheduleVisibleDatesChanged(WidgetMessage widgetMessage)
        {
            if (widgetMessage.Data is CalendarDateTimeInterval calendarDateTimeInterval)
            {
                JobInterval = calendarDateTimeInterval;
                PerformDataFetch(true);
            }
        }

        private async Task HandleScheduleChange(CalendarDateTimeInterval calendarDateTimeInterval, CancellationToken token)
        {
            if (calendarDateTimeInterval.StartDateTime < _contentService.GetStartDateTime())
            {
                CalendarListModel.DataIsLoading();
                var calendarData = await _contentService.SyncEventsByNewStartDateTime(
                    calendarDateTimeInterval.StartDateTime, SearchText, token);
                CalendarScheduleModel.UpdateData(await GetCrmScheduleAppointments(calendarData.CalendarEvents));
                CalendarListModel.UpdateData(calendarData.ListEvents, _contentService.GetStartDateTime(), _contentService.GetEndDateTime());

            }

            if (calendarDateTimeInterval.EndDateTime > _contentService.GetEndDateTime())
            {
                CalendarListModel.DataIsLoading();
                var calendarData = await _contentService.SyncEventsByNewEndDateTime(
                    calendarDateTimeInterval.EndDateTime, SearchText, token);

                CalendarScheduleModel.UpdateData(await GetCrmScheduleAppointments(calendarData.CalendarEvents));
                CalendarListModel.UpdateData(calendarData.ListEvents, _contentService.GetStartDateTime(), _contentService.GetEndDateTime());
            }
        }

        private async Task OnShowCalendarEventPageRequested(WidgetMessage widgetMessage)
        {
            if (widgetMessage.Data is CrmScheduleAppointment crmScheduleAppointment)
            {
                await _navigationController.DisplayPopupAsync<CalendarEventDetailsPageViewModel>(crmScheduleAppointment);
            }
        }

        private async Task OnShowNewOrEditRequested(WidgetMessage widgetMessage)
        {
            if (widgetMessage.Data is Dictionary<string, string> additionalArguments)
            {
                _LongPressUserActions = _contentService.ActionForLongPress(additionalArguments);

                if (_LongPressUserActions?.Count > 1)
                { 

                    await _navigationController.NavigateToAsync<PopupListPageViewModel>(parameter: this);
                }
                else
                {
                    UserAction userAction = _LongPressUserActions?.Count == 0 ? null : _LongPressUserActions[0];

                    if (userAction != null)
                    {
                        await _navigationController.NavigateAsyncForAction(userAction, _cancellationTokenSource.Token);
                    }
                }
            }
        }

        private async Task OnListEventSelected(WidgetMessage widgetMessage)
        {
            if (widgetMessage.Data is ListDisplayRow selectedItem)
            {
                UserAction userAction = await _contentService.ActionForSelectedCalendarListItem(0, selectedItem, _cancellationTokenSource.Token);
                IUserActionSchuttle userActionSchuttle = AppContainer.Resolve<IUserActionSchuttle>();
                await userActionSchuttle.Carry(userAction, selectedItem.RecordId, _cancellationTokenSource.Token);
            }
        }

        public void PerformSearch(bool useDelay = false)
        {
            try
            {
                double delay = 0;
                if (useDelay)
                {
                    if (_contentService.CurrentRequestMode() != RequestMode.Offline
                        && _contentService.SearchAutoSwitchToOffline())
                    {
                        _contentService.SetRequestModeToOffline();
                        SetRequestModeIcon();
                    }
                    delay = _contentService.SearchDelay();
                }
                timer.StartOrReset(delay);
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

        public void PerformDataFetch(bool useDelay = false)
        {
            try
            {
                double delay = 0;
                if (useDelay)
                {
                    delay = 0.3;
                }
                DataLoadtimer.StartOrReset(delay);
            }
            catch (Exception ex)
            {
                _logService.LogError(ex?.Message);
            }
        }

        public async Task PerformAsyncSearch()
        {
            CalendarListModel.DataIsLoading();

            CalendarData calendarData = null;
            IsFiltersButtonCommandEnabled = _contentService.HasUserFilters();
            var userFilters = searchFilters.GetEnabledUserFilters();
            int filterCount = userFilters?.Count ?? 0;
            IsUserFilterHasEnabledFilters = filterCount > 0 ? true : false;
            UserFilterCount = filterCount;

            _contentService.SetEnabledUserFilters(userFilters);
            try
            {
                calendarData = await _contentService.SyncEventsBySearch(SearchText, _cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                _logService.LogError(e?.Message);
            }
            finally
            {
                if (calendarData != null)
                {
                    CalendarScheduleModel.UpdateData(await GetCrmScheduleAppointments(calendarData.CalendarEvents));
                    CalendarListModel.UpdateData(calendarData.ListEvents, _contentService.GetStartDateTime(), _contentService.GetEndDateTime());
                }
                else
                {
                    CalendarListModel.UpdateData(null, _contentService.GetStartDateTime(), _contentService.GetEndDateTime());
                }
            }
        }

        private async Task OnSelectionChanged(Syncfusion.XForms.Buttons.SelectionChangedEventArgs selectionChangedEventArgs)
        {
            SfSegmentItem selectedMode = SegmentedControlItems[selectionChangedEventArgs.Index];
            if (Enum.TryParse(selectedMode.StyleId.ToString(), true, out CalendarViewModes mode))
            {
               await ChangeCalendarViewMode(mode);
            }
        }

        private async Task ChangeCalendarViewMode(CalendarViewModes calendarViewMode)
        {
            if(calendarViewMode != CalendarViewModes.List)
            {
                Content = CalendarScheduleModel;
                await CalendarScheduleModel.CalendarViewModeChanged(calendarViewMode);
            }
            else
            {
                Content = CalendarListModel;
            }
        }

        private Task OnSearchButton()
        {
            IsSearchBoxVisible = !IsSearchBoxVisible;
            return Task.CompletedTask;
        }

        private void OnSelectCalendarsButton()
        {
            var bindableCalendars = new List<BindableCalendar>();

            foreach (DeviceCalendar crmCalendar in _contentService.GetCrmCalendars())
            {
                bindableCalendars.Add(new BindableCalendar(crmCalendar.Identifier, crmCalendar.Name,
                    crmCalendar.IsCRMCalendar, _contentService.IsCrmCalendarSelected(crmCalendar.Identifier)));
            }

            foreach (DeviceCalendar deviceCalendar in _contentService.GetDeviceCalendars())
            {
                bindableCalendars.Add(new BindableCalendar(deviceCalendar.Identifier, deviceCalendar.Name,
                    deviceCalendar.IsCRMCalendar, _contentService.IsDeviceCalendarSelected(deviceCalendar.Identifier)));
            }

            _navigationController.DisplayPopupAsync<CalendarSelectionPageViewModel>(bindableCalendars);
        }

        private void OnSelectRepsButton()
        {
            List<BindableCrmRep> bindableCrmReps = new List<BindableCrmRep>();

            foreach (CrmRep selectedCrmRep in _contentService.GetCrmReps())
            {
                bindableCrmReps.Add(new BindableCrmRep(selectedCrmRep.Id, selectedCrmRep.Name, _contentService.IsRepSelected(selectedCrmRep.Name)));
            }

            _navigationController.DisplayPopupAsync<RepSelectionPageViewModel>(bindableCrmReps);
        }
        private async Task FilterCommandHandler()
        {
            await _navigationController.NavigateToAsync<FilterUIPageViewModel>(parameter: this);
        }
        private void SetRequestModeIcon()
        {
            if (_contentService.CurrentRequestMode() == RequestMode.Online)
            {
                RequestModeButtonIconText = MaterialDesignIcons.Cloud;
            }
            else
            {
                RequestModeButtonIconText = MaterialDesignIcons.CloudOutline;
            }
        }

        private async Task OnRequestModeButton()
        {
            IsLoading = true;

            await Task.Run(() =>
            {
                var calendarData = _contentService.ChangeRequestMode(SearchText, _cancellationTokenSource.Token).Result;
                SetRequestModeIcon();
                CalendarScheduleModel.UpdateData(GetCrmScheduleAppointments(calendarData.CalendarEvents).Result);
                CalendarListModel.UpdateData(calendarData.ListEvents, _contentService.GetStartDateTime(), _contentService.GetEndDateTime());
                IsLoading = false;

            });
            
        }

        private void OnCalendarListReceived(BaseViewModel caller, SelectedCalendarsMessage message)
        {
            CalendarListModel.DataIsLoading();
            var calendarData = _contentService.SetSelectedCalendarsSets(message.SelectedCrmCalendarIds, message.SelectedDeviceCalendarIds);
            CalendarScheduleModel.UpdateData(GetCrmScheduleAppointments(calendarData.CalendarEvents).Result);
            CalendarListModel.UpdateData(calendarData.ListEvents, _contentService.GetStartDateTime(), _contentService.GetEndDateTime());
        }

        private void OnRepIdSetReceived(BaseViewModel caller, HashSet<string> selectedRepIdSet)
        {
            CalendarListModel.DataIsLoading();
            var calendarData = _contentService.SetSelectedRepIds(selectedRepIdSet);
            CalendarScheduleModel.UpdateData(GetCrmScheduleAppointments(calendarData.CalendarEvents).Result);
            CalendarListModel.UpdateData(calendarData.ListEvents, _contentService.GetStartDateTime(), _contentService.GetEndDateTime());
        }

        private async Task<List<CrmScheduleAppointment>> GetCrmScheduleAppointments(List<DeviceCalendarEvent> filteredCalendarEvents)
        {
            List<CrmScheduleAppointment> crmScheduleAppointments = new List<CrmScheduleAppointment>();

            var events = filteredCalendarEvents.Cast<DeviceCalendarEvent>().Select(async DeviceCalendarEvent => await GetCalendarEvent(DeviceCalendarEvent, _cancellationTokenSource.Token));
            crmScheduleAppointments.AddRange(await Task.WhenAll(events));
            return crmScheduleAppointments;
        }

        private async Task<CrmScheduleAppointment> GetCalendarEvent(DeviceCalendarEvent calendarEvent, CancellationToken token)
        {
            if (calendarEvent.IsCrmEvent)
            {
                CrmScheduleAppointment crmEvent = new CrmScheduleAppointment(
                    _contentService.GetCalendarComponentViewTemplate(calendarEvent.UserActionUnitName),
                    _contentService.ActionForCrmCalendarComponentEvent(calendarEvent.UserActionUnitName, calendarEvent.RecordId),
                    calendarEvent);

                if (crmEvent.DeviceCalendarEvent.TableCaptionContent != null)
                {
                    string subjectFromTableCaption = _localizationController.GetLocalizedValue(crmEvent.DeviceCalendarEvent.TableCaptionContent.Fields[0]);
                    if (!string.IsNullOrEmpty(subjectFromTableCaption))
                    {
                        crmEvent.Subject = subjectFromTableCaption;
                    }
                }

                return crmEvent;

            }
            else
            {
               return new CrmScheduleAppointment(null, null, calendarEvent);
            }
        }

        public CalendarControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            lazyInitialize = true;
            IsLoading = true;
            _contentService = AppContainer.Resolve<ICalendarContentService>();

            if (widgetArgs != null && widgetArgs is UserAction)
            {
                UserActionObj = widgetArgs as UserAction;

            }

            SegmentedControlItems = new ObservableCollection<SfSegmentItem>();
            SegmentedControlItems.Add(new SfSegmentItem { Text = CalendarModeTitle(CalendarViewModes.Day), StyleId = CalendarViewModes.Day.ToString() });
            SegmentedControlItems.Add(new SfSegmentItem { Text = CalendarModeTitle(CalendarViewModes.Week), StyleId = CalendarViewModes.Week.ToString() });
            SegmentedControlItems.Add(new SfSegmentItem { Text = CalendarModeTitle(CalendarViewModes.Month), StyleId = CalendarViewModes.Month.ToString() });
            SegmentedControlItems.Add(new SfSegmentItem { Text = CalendarModeTitle(CalendarViewModes.Timeline), StyleId = CalendarViewModes.Timeline.ToString() });
            SegmentedControlItems.Add(new SfSegmentItem { Text = CalendarModeTitle(CalendarViewModes.List), StyleId = CalendarViewModes.List.ToString() });
            ViewModesCounter = SegmentedControlItems.Count;

            timer = new ResetTimer(PerformAsyncSearch);
            DataLoadtimer = new ResetTimer(RunScheduleDataFetch);

            MessagingCenter.Subscribe<BaseViewModel, SelectedCalendarsMessage>(this, InAppMessages.SelectedCalendars, OnCalendarListReceived);
            MessagingCenter.Subscribe<BaseViewModel, HashSet<string>>(this, InAppMessages.SelectedRepIdSet, OnRepIdSetReceived);
        }
        public async override Task LazyInitializeControl()
        {
            IsLoading = true;
            var result = await _contentService.GetDataAsync(SearchText, _cancellationTokenSource.Token);
            CalendarScheduleModel.UpdateData(await GetCrmScheduleAppointments(result.CalendarEvents));
            CalendarListModel.UpdateData(result.ListEvents, _contentService.GetStartDateTime(), _contentService.GetEndDateTime());
            IsLoading = false;

        }


        public async override ValueTask<bool> InitializeControl()
        {
            IsLoading = true;
            await BuildWidgets();
            RegisterMessages();
            var calendarReadPermission = await Permissions.RequestAsync<Permissions.CalendarRead>();

            if (UserActionObj != null)
            {
                _contentService.SetSourceAction(UserActionObj);
                _contentService.SetCalendarPermissionFlag(calendarReadPermission == PermissionStatus.Granted);
                _contentService.SetCalendarEventComponents(await PrepareCalendarEventComponents(UserActionObj));
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                IsFiltersButtonCommandEnabled = _contentService.HasUserFilters();
                SetRequestModeIcon();
                searchFilters = SearchAndListCommons.GetFilterUIList(_contentService.SourceSearchService, 0);
                SetConfigValues();

                // Disabled Request mode untill filter is fixed to support online data correctly.
                IsRequestModeButtonEnabled = !_sessionContext.IsInOfflineMode && _sessionContext.HasNetworkConnectivity;
                SearchTextBoxPlaceholderText = _contentService.SearchColumns(0);
            }

            IsLoading = false;

            return true;
        }

        private async Task RunScheduleDataFetch()
        {
            try
            {
                if (JobInterval != null)
                {
                    var intervel = JobInterval;
                    JobInterval = null;
                    await HandleScheduleChange(intervel, _cancellationTokenSource.Token);
                }
            }
            catch (Exception ex) 
            {
                _logService.LogError(ex?.Message);
            }

        }

        private async Task<Dictionary<string, CalendarEventComponent>> PrepareCalendarEventComponents(UserAction userAction)
        {     
            IConfigurationService configurationService = AppContainer.Resolve<IConfigurationService>();
            IUserActionBuilder userActionBuilder = AppContainer.Resolve<IUserActionBuilder>();

            Dictionary<string, CalendarEventComponent> calendarEventComponents = new Dictionary<string, CalendarEventComponent>();
            calendarEventComponents.Add(userAction.ActionUnitName, await PrepareCalendarEventComponent(userAction));
            foreach(KeyValuePair<string, CalendarEventComponent> entry in await PrepareAdditionalCalendarEventComponent(configurationService, userActionBuilder, userAction))
            {
                calendarEventComponents.Add(entry.Key, entry.Value);
            }

            return calendarEventComponents;
        }

        private async Task<CalendarEventComponent> PrepareCalendarEventComponent(UserAction userAction)
        {
            CalendarEventComponent calendarEventComponent = AppContainer.Resolve<CalendarEventComponent>();
            await calendarEventComponent.PrepareComponentAsync(userAction, _cancellationTokenSource.Token);
            return calendarEventComponent;
        }

        private async Task<Dictionary<string, CalendarEventComponent>> PrepareAdditionalCalendarEventComponent(IConfigurationService configurationService,
            IUserActionBuilder userActionBuilder,
            UserAction userAction)
        {
            Dictionary<string, CalendarEventComponent> additionalCalendarEventComponents = new Dictionary<string, CalendarEventComponent>();

            CalendarViewTemplate actionTemplate = new CalendarViewTemplate(userAction.ViewReference);

            foreach(string additionalCalendarConfig in actionTemplate.AdditionalCalendarConfigs())
            {
                CalendarEventComponent calendarEventComponent = AppContainer.Resolve<CalendarEventComponent>();
                Domain.Configuration.UserInterface.Menu menu = await configurationService.GetMenu(additionalCalendarConfig, _cancellationTokenSource.Token);
                UserAction additionalConfigUserAction = userActionBuilder.UserActionFromMenu(configurationService, menu);
                await calendarEventComponent.PrepareComponentAsync(additionalConfigUserAction, _cancellationTokenSource.Token);
                additionalCalendarEventComponents.Add(additionalConfigUserAction.ActionUnitName, calendarEventComponent);
            }

            return additionalCalendarEventComponents;
        }

        private void SetConfigValues()
        {
            SetCalendarWorkingHours();
            SetDefaultViewType(_contentService.GetDefaultViewType());
        }

        private void SetCalendarWorkingHours()
        {
            int calendarFirstWorkingHour = _contentService.GetCalendarFirstWorkingHour();
            int calendarNumberOfWorkingHours = _contentService.GetCalendarNumberOfWorkingHours();
            CalendarScheduleModel.SetWorkingHours(calendarFirstWorkingHour, calendarNumberOfWorkingHours);
        }

        private void SetDefaultViewType(string defaultViewType)
        {
            if(defaultViewType == "DAY")
            {
                SegmentedControlIndex = (int)CalendarViewModes.Day;
            }
            else if (defaultViewType == "WEEK" || defaultViewType == "WORKWEEK"  )
            {
                SegmentedControlIndex = (int)CalendarViewModes.Week;
            }
            else if (defaultViewType == "MONTH")
            {
                SegmentedControlIndex = (int)CalendarViewModes.Month;
            }
            else
            {
                SegmentedControlIndex = (int)CalendarViewModes.List;
            }
            
            SfSegmentItem selectedMode = SegmentedControlItems[SegmentedControlIndex];
            if (Enum.TryParse(selectedMode.StyleId.ToString(), true, out CalendarViewModes mode))
            {
                ChangeCalendarViewMode(mode);
            }
        }

        public async Task<List<FilterUI>> GetUserFilters()
        {
            return searchFilters;
        }

        public async Task ApplyUserFilters(List<FilterUI> filters)
        {
            searchFilters = filters;
            PerformSearch();
        }

        #region Improved calender data loading.

        DateTime? lowerRange = null;
        DateTime? upperRange = null;


        private async Task LoadCalenderData(DateTime startDate, DateTime endDate, bool needRefresh = false)
        {

        }
        public async Task<List<PopupListItem>> GetPoupList()
        {

            if (_LongPressUserActions.Count > 0)
            {
                var items = new List<PopupListItem>();
                foreach (var action  in _LongPressUserActions)
                {

                    items.Add(new PopupListItem
                    {
                        RecordId = action.ActionUnitName,
                        DisplayText = action.ActionDisplayName,
                        OrginalObject = action
                    });
                }
                return items;
            }

            return null;
        }

        public async Task PopupItemSelected(PopupListItem item)
        {
            if (item != null && item.OrginalObject is UserAction action )
            {

                await _navigationController.NavigateAsyncForAction(action, _cancellationTokenSource.Token);

            }
        }
        #endregion
    }

}
