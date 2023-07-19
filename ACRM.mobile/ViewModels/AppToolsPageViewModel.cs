using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.FullSync;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Syncfusion.XForms.TabView;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class AppToolsPageViewModel: BaseViewModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly AppToolsTabItemsBuilder _tabItemsBuilder;
        private readonly BackgroundSyncManager _backgroundSyncManager;

        public ICommand SelectionChangingCommand => new Command<SelectionChangingEventArgs>(async (args) => await SelectionChanging(args));
        public ICommand ToggleOfflineModeCommand => new Command(() => OnToggleOfflineMode());
        public ICommand DismissToolsViewCommand => new Command(async () => await DismissToolsView());

        private TabItemCollection tabItems = new TabItemCollection();
        public TabItemCollection TabItems
        {
            get { return tabItems; }
            set
            {
                tabItems = value;
                RaisePropertyChanged(() => TabItems);
            }
        }

        private int _currentTabViewIndex = 0;
        public int CurrentTabViewIndex
        {
            get => _currentTabViewIndex;
            set
            {
                _currentTabViewIndex = value;
                RaisePropertyChanged(() => CurrentTabViewIndex);
            }
        }

        private string _statusIcon;
        public string StatusIcon
        {
            get => _statusIcon;
            set
            {
                _statusIcon = value;
                RaisePropertyChanged(() => StatusIcon);
            }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                RaisePropertyChanged(() => StatusText);
            }
        }

        private Color _statusColor;
        public Color StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                RaisePropertyChanged(() => StatusColor);
            }
        }

        private string _closeText;
        public string CloseText
        {
            get => _closeText;
            set
            {
                _closeText = value;
                RaisePropertyChanged(() => CloseText);
            }
        }

        private bool _isClosingEnabled = true;
        public bool IsClosingEnabled
        {
            get => _isClosingEnabled;
            set
            {
                _isClosingEnabled = value;
                RaisePropertyChanged(() => IsClosingEnabled);
            }
        }

        public AppToolsPageViewModel(IAuthenticationService authenticationService,
            AppToolsTabItemsBuilder tabItemsBuilder,
            BackgroundSyncManager backgroundSyncWorkerProvider)
        {
            _authenticationService = authenticationService;
            _tabItemsBuilder = tabItemsBuilder;
            _backgroundSyncManager = backgroundSyncWorkerProvider;
            InitProperties();
            RegisterMessages();

            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        private void InitProperties()
        {
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
            UpdateStatusInfo();
        }

        private void RegisterMessages()
        {
            RegisterMessage(WidgetEventType.ShowAppLogsRequested, "LogModel", OnShowAppLogsRequested);
            RegisterMessage(WidgetEventType.ShowCrmDBRequested, "AppToolsLogTabModel", OnShowDBRequested);
            RegisterMessage(WidgetEventType.ShowConfigDBRequested, "AppToolsLogTabModel", OnShowDBRequested);
            RegisterMessage(WidgetEventType.ShowOfflineDBRequested, "AppToolsLogTabModel", OnShowDBRequested);
            RegisterMessage(WidgetEventType.DataSyncRequested, "AppToolsSyncTabModel", OnSyncRequested);
            RegisterMessage(WidgetEventType.IncrementalSyncRequested, "AppToolsSyncTabModel", OnSyncRequested);
            RegisterMessage(WidgetEventType.ConfigurationSyncRequested, "AppToolsSyncTabModel", OnSyncRequested);
            RegisterMessage(WidgetEventType.FullSyncRequested, "AppToolsSyncTabModel", OnSyncRequested);
            RegisterMessage(WidgetEventType.ChangeLanguageRequested, "AppToolsSyncTabModel", OnChangeLanguageRequested);
        }

        private async Task OnShowAppLogsRequested(WidgetMessage widgetMessage)
        {
            await ShowLogPage();
        }

        private async Task OnShowDBRequested(WidgetMessage widgetMessage)
        {
            await ShowDatabaseQueryPage(widgetMessage.EventType);
        }

        private async Task OnSyncRequested(WidgetMessage widgetMessage)
        {
            switch (widgetMessage.EventType)
            {
                case WidgetEventType.FullSyncRequested:
                    await FullSync();
                    break;
                case WidgetEventType.ConfigurationSyncRequested:
                    await ConfigSync();
                    break;
                case WidgetEventType.DataSyncRequested:
                    await DataSync();
                    break;
                case WidgetEventType.IncrementalSyncRequested:
                    await IncrementalSync();
                    break;
            }
        }

        private async Task OnChangeLanguageRequested(WidgetMessage widgetMessage)
        {
            var authenticationResponse = await _authenticationService.Authenticate(_sessionContext.CrmInstance, 
               _sessionContext.User.Username, _sessionContext.User.Password);
            await _navigationController.DisplayPopupAsync<LanguageSelectionPageViewModel>(authenticationResponse.ServerInformation.ServerLanguages);
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            UpdateStatusInfo();
        }

        private void UpdateStatusInfo()
        {
            if(_sessionContext.IsOfflineModeToggled)
            {
                StatusText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicOffline);
                StatusColor = Color.DarkBlue;
                StatusIcon = MaterialDesignIcons.CloudOffOutline;
            }
            else if(_sessionContext.IsInOfflineMode)
            {
                StatusText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicOffline); ;
                StatusColor = Color.IndianRed;
                StatusIcon = MaterialDesignIcons.CloudOffOutline;
            }
            else
            {
                StatusText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicOnline); ;
                StatusColor = Color.Green;
                StatusIcon = MaterialDesignIcons.CloudCheckOutline;
            }
        }

        public override async Task InitializeAsync(object navigationData)
        {
            FullSyncStatusType fullSyncStatusType = FullSyncStatusType.NoFullSyncSuggested;
            if(navigationData is FullSyncStatusType)
            {
                fullSyncStatusType = (FullSyncStatusType) navigationData;
            }
            await BuildTabItems(fullSyncStatusType);
            await base.InitializeAsync(navigationData);
        }

        private async Task BuildTabItems(FullSyncStatusType fullSyncStatusType)
        {
            TabItems = await _tabItemsBuilder.BuildTabItems(this, fullSyncStatusType, _cancellationTokenSource);

            switch (fullSyncStatusType)
            {
                case FullSyncStatusType.FullSyncBlockIntervalDays:
                    CurrentTabViewIndex = 1;
                    IsClosingEnabled = false;
                    break;
                case FullSyncStatusType.FullSyncWarnIntervalDays:
                    CurrentTabViewIndex = 1;
                    break;
            }
        }

        private async Task SelectionChanging(SelectionChangingEventArgs args)
        {
            if (!IsClosingEnabled && args.Index != 4)
            {
                args.Cancel = true;
            }
            if (args.Index == 4)
            {
                args.Cancel = true;
                await Logout();
            }
        }

        private async Task Logout()
        {
            _backgroundSyncManager.StopBackgroundSyncWorker();
            await _navigationController.PopAllPopupAsync();
            await _navigationController.Logout();
        }

        private void OnToggleOfflineMode()
        {
            _sessionContext.IsOfflineModeToggled = !_sessionContext.IsOfflineModeToggled;
            UpdateStatusInfo();
        }

        private async Task DismissToolsView()
        {
            if(IsClosingEnabled)
            {
                await _navigationController.PopAllPopupAsync();
            }
        }

        private async Task FullSync()
        {
            await _navigationController.DisplayPopupAsync<SyncPageViewModel>(new UserAction { ActionType = UserActionType.SyncFull, UseForce = true });
        }

        private async Task ConfigSync()
        {
            await _navigationController.DisplayPopupAsync<SyncPageViewModel>(new UserAction { ActionType = UserActionType.SyncConfig, UseForce = true });
        }

        private async Task DataSync()
        {
            await _navigationController.DisplayPopupAsync<SyncPageViewModel>(new UserAction { ActionType = UserActionType.SyncData, UseForce = true });
        }

        private async Task IncrementalSync()
        {
            await _navigationController.DisplayPopupAsync<SyncPageViewModel>(new UserAction { ActionType = UserActionType.SyncIncremental });
        }

        private async Task ShowLogPage()
        {
            await _navigationController.DisplayPopupAsync<LogPageViewModel>();
        }

        private async Task ShowDatabaseQueryPage(WidgetEventType widgetEventType)
        {
            await _navigationController.DisplayPopupAsync<DatabaseQueryPageViewModel>(widgetEventType);
        }
    }
}
