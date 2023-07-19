using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FullSync;
using ACRM.mobile.Services;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Utils;
using ACRM.mobile.UIModels;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.ViewModels.ObservableGroups;
using ACRM.mobile.Views.Widgets;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class DashboardPageViewModel : NavigationBarBaseViewModel
    {
        private readonly IDashboardContentService _contentService;
        private readonly IConfigurationService _configurationService;
        private readonly ISyncStatusService _syncStatusService;
        private readonly BackgroundSyncManager _backgroundSyncManager;
        private UserAction _userAction;
        private Form _formData;
        private Dictionary<string, Dictionary<string, string>> FormParams;

        public ICommand OnHeaderAction => new Command<HeaderActionButton>(async (selectedItem) => await HeaderAction(selectedItem));

        private HeaderGroupData _headerData;
        public HeaderGroupData HeaderData
        {
            get => _headerData;
            set
            {
                _headerData = value;
                RaisePropertyChanged(() => HeaderData);
            }
        }

        private List<UITabContent> _tabs;
        public List<UITabContent> Tabs
        {
            get => _tabs;
            set
            {
                _tabs = value;
                RaisePropertyChanged(() => Tabs);
            }
        }

        private bool _isTabsViewVisible = false;
        public bool IsTabStripVisible
        {
            get => _isTabsViewVisible;
            set
            {
                _isTabsViewVisible = value;
                RaisePropertyChanged(() => IsTabStripVisible);
            }
        }

        public DashboardPageViewModel(IDashboardContentService contentService,
            IConfigurationService configurationService,
            ISyncStatusService syncStatusService,
            BackgroundSyncManager backgroundSyncWorkerProvider)
        {
            _contentService = contentService;
            _configurationService = configurationService;
            _syncStatusService = syncStatusService;
            FormParams = new Dictionary<string, Dictionary<string, string>>();
            _backgroundSyncManager = backgroundSyncWorkerProvider;
            PageTitle = "Dashboard";
            _localizationController.AttachConfiguration();
            HeaderGroupData headerData = new HeaderGroupData();
            headerData.IsOrganizerHeaderVisible = false;
            HeaderData = headerData;
            Tabs = new List<UITabContent>() { new UITabContent() };
        }

        public override async Task InitializeAsync(object navigationData)
        {
            _logService.LogDebug("Start  InitializeAsync");
            await _navigationController.PopAllPopupAsync();
            RegisterMessageIfNotExist(WidgetEventType.FormItemChanged, "*", OnFormItemChanged);
            _backgroundSyncManager.InitNewBackgroundSyncWorker();
            await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
            _userAction = _contentService.StartUserAction;
            await UpdateBindingsAsync();
            await base.InitializeAsync(navigationData);
            _logService.LogDebug("End  InitializeAsync");
        }

        private async Task OnFormItemChanged(WidgetMessage widgetMessage)
        {
            if (!string.IsNullOrWhiteSpace(widgetMessage.ControlKey) && widgetMessage.Data is Dictionary<string, string> Params)
            {
                if(Params==null)
                {
                    Params = new Dictionary<string, string>();
                }

                if (FormParams.ContainsKey(widgetMessage.ControlKey))
                {
                    FormParams[widgetMessage.ControlKey] = Params;
                }
                else
                {
                    FormParams.Add(widgetMessage.ControlKey, Params);
                }

                await PublishMessage(new WidgetMessage()
                {
                    EventType = WidgetEventType.FormParamsChanged,
                    ControlKey = widgetMessage.ControlKey,
                    Data = FormParams,

                },MessageDirections.ToChildren);
            }
        }

        private async Task UpdateBindingsAsync()
        {
            if (!string.IsNullOrWhiteSpace(_contentService.HeaderLabel))
            {
                PageTitle = _contentService.HeaderLabel;
            }

            var headerData = HeaderData;
            headerData.AreActionsViewVisible = false;
            headerData.SetHeaderActionButtons(_contentService.HeaderButtons());

            if (headerData.HeaderActions.Count > 0)
            {
                headerData.AreActionsViewVisible = true;
            }

            HeaderData = headerData;
            _formData = await _contentService.GetDashboardForm(_cancellationTokenSource.Token);
            await BuildDashboardPageAsync(_formData);
            await CheckFullSyncStatus();

        }

        private async Task BuildDashboardPageAsync(Form formData)
        {
            if (formData != null && formData.Tabs.Count > 0)
            {
                List<UITabContent> pageTabs = new List<UITabContent>();
                var tabsConfig = formData.Tabs.OrderBy(a => a.OrderId).ToList();
                Widgets = new ObservableCollection<UIWidget>();
                foreach (var tab in tabsConfig)
                {
                    var widgets = await tab.BuildFormWidgets(this, FormParams, _userAction, _cancellationTokenSource);
                    var items = widgets.OfType<InsightBoardModel>();
                    if (items.Count() == 0)
                    {
                        try
                        {
                            InsightBoardModel insightBoardModel = new InsightBoardModel(_contentService.InsightBoardActions(), _cancellationTokenSource);
                            await insightBoardModel.InitializeControl();
                            widgets.Insert(0, insightBoardModel);
                        }
                        catch
                        {
                            _logService.LogError("One of the tab is wrongly configured and we can not even get some insightboard actions");
                        }
                    }

                    pageTabs.Add(new UITabContent { Title = tab.Label, Widgets = widgets });
                    foreach (var widget in widgets)
                    {
                        Widgets.Add(widget);
                    }
                }

                if (pageTabs.Count == 1)
                {
                    pageTabs[0].IsOnlyTab = true;
                }
                else if (pageTabs.Count > 1)
                {
                    IsTabStripVisible = true;
                }

                Tabs = pageTabs;
            }

            
        }

        private bool HasTabs(List<UITabContent> tabs)
        {
            var nonEmptyTabs = 0;
            foreach(var tab in tabs)
            {
                if(tab.Widgets.Count > 0)
                {
                    nonEmptyTabs++;
                }
            }

            return nonEmptyTabs > 1;
        }

        private async Task HeaderAction(HeaderActionButton headerActionButton)
        {
            if (headerActionButton != null)
            {
                _logService.LogInfo($"SearchAndListPage header user action: {headerActionButton.UserAction.ActionDisplayName}");
                await _navigationController.NavigateAsyncForAction(headerActionButton.UserAction, _cancellationTokenSource.Token);
            }
        }

        public override async Task RefreshAsync(object data)
        {
            await BuildDashboardPageAsync(_formData);
        }

        private async Task CheckFullSyncStatus()
        {
            try
            {
                SyncStatus syncStatus = await _syncStatusService.GetSyncStatusAsync();

                if (long.TryParse(syncStatus.UserInterfaceConfigurationSyncInfo.FullSyncTimestamp, out long fullSyncTimestamp))
                {
                    // Default timezone
                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("UTC");

                    if (!string.IsNullOrEmpty(_configurationService.GetServerTimezone()))
                    {
                        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(_configurationService.GetServerTimezone());
                    }

                    DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(fullSyncTimestamp);
                    DateTime lastFullSyncDateTime = TimeZoneInfo.ConvertTime(offset, timeZoneInfo).DateTime;

                    WebConfigValue fullSyncDate = _configurationService.GetConfigValue("Sync.FullSyncDate");

                    if (fullSyncDate != null && !string.IsNullOrEmpty(fullSyncDate.Value))
                    {
                        DateTime configDateTime = DateTime.ParseExact(fullSyncDate.Value, CrmConstants.DateTimeFormat, null);

                        DateTime fullSyncDateTime = TimeZoneInfo.ConvertTime(configDateTime, timeZoneInfo);

                        if (lastFullSyncDateTime < fullSyncDateTime)
                        {
                            await _navigationController.DisplayPopupAsync<SyncPageViewModel>(new UserAction { ActionType = UserActionType.SyncFull, UseForce = true });
                            return;
                        }
                    }

                    int fullSyncIntervalDays = _configurationService.GetNumericConfigValue<int>("Sync.FullSyncIntervalDays", -1);

                    if (fullSyncIntervalDays != -1 && (TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo) - lastFullSyncDateTime).TotalDays > fullSyncIntervalDays)
                    {
                        await _navigationController.DisplayPopupAsync<SyncPageViewModel>(new UserAction { ActionType = UserActionType.SyncFull, UseForce = true });
                        return;
                    }

                    int fullSyncBlockIntervalDays = _configurationService.GetNumericConfigValue<int>("Sync.FullSyncBlockIntervalDays", -1);

                    if (fullSyncBlockIntervalDays != -1 && (TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo) - lastFullSyncDateTime).TotalDays > fullSyncBlockIntervalDays)
                    {
                        await _navigationController.DisplayPopupAsync<AppToolsPageViewModel>(FullSyncStatusType.FullSyncBlockIntervalDays);
                        return;
                    }

                    int fullSyncWarnIntervalDays = _configurationService.GetNumericConfigValue<int>("Sync.FullSyncWarnIntervalDays", -1);

                    if (fullSyncWarnIntervalDays != -1 && (TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo) - lastFullSyncDateTime).TotalDays > fullSyncWarnIntervalDays)
                    {
                        await _navigationController.DisplayPopupAsync<AppToolsPageViewModel>(FullSyncStatusType.FullSyncWarnIntervalDays);
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                _logService.LogError($"Error during configuration ordered FullSync {ex.Message}");
            }
        }
    }
}
