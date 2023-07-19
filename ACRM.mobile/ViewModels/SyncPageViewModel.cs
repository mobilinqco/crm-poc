using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.DataAccess.Network;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class SyncPageViewModel : BaseViewModel
    {
        private readonly IDataSyncService _dataSyncService;
        private readonly ISyncStatusService _syncStatusService;
        private readonly IOfflineAuthenticationService _offlineAuthenticationService;
        private readonly IConfigurationService _configurationService;

        private readonly BackgroundSyncManager _backgroundSyncManager;

        public ICommand OnCloseButtonTapped => new Command(() =>
        {
            _cancellationTokenSource.Cancel();
            //await _navigationController.PopPopupAsync();
        });

        private Color _syncStatusTextColor = Color.White;
        public Color SyncStatusTextColor
        {
            get => _syncStatusTextColor;
            set
            {
                _syncStatusTextColor = value;
                RaisePropertyChanged(() => SyncStatusTextColor);
            }
        }

        private string _syncStatus = "";
        public string SyncStatus
        {
            get => _syncStatus;
            set
            {
                _syncStatus = value;
                RaisePropertyChanged(() => SyncStatus);
            }
        }

        public SyncPageViewModel(IDataSyncService dataSyncService,
            ISyncStatusService syncStatusService,
            IOfflineAuthenticationService offlineAuthenticationService,
            IConfigurationService configurationService,
            BackgroundSyncManager backgroundSyncManager)
        {
            _offlineAuthenticationService = offlineAuthenticationService;
            _dataSyncService = dataSyncService;
            _syncStatusService = syncStatusService;
            _configurationService = configurationService;

            _backgroundSyncManager = backgroundSyncManager;

            _syncStatusService.ActiveSyncChanged += (obj, eArgs) =>
            {
                StatusUpdate();
            };
        }

        private void StatusUpdate()
        {
            if (_syncStatusService.ActiveSyncStatus.Type == SyncType.DataSetSync)
            {
                SyncStatus = String.Format(_localizationController.GetString(LocalizationKeys.TextGroupBasic,
                    LocalizationKeys.KeyBasicSyncPlaceHolder), "", _syncStatusService.ActiveSyncStatus.Message);
                return;
            }

            if (_syncStatusService.ActiveSyncStatus.Type == SyncType.IncrementalDataSetSync)
            {
                SyncStatus = String.Format(_localizationController.GetString(LocalizationKeys.TextGroupBasic,
                    LocalizationKeys.KeyBasicIncrementalSyncPlaceHolder), "", _syncStatusService.ActiveSyncStatus.Message);
                return;
            }

            if (_syncStatusService.ActiveSyncStatus.Type == SyncType.CatalogSync)
            {
                SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                       LocalizationKeys.KeyBasicSyncCatalogs);
                return;
            }

            if (_syncStatusService.ActiveSyncStatus.Type == SyncType.ResourceSync)
            {
                SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                       LocalizationKeys.KeyBasicSyncResources);
                return;
            }

            SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                       LocalizationKeys.KeyBasicSyncCore);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            UserAction ua;
            SyncResult result = new SyncResult { IsSuccessful = true, IsCancelled = false };

            if (navigationData is UserAction)
            {
                ua = (UserAction)navigationData;
            }
            else
            {
                await _navigationController.PopAllPopupAsync(new SyncResult { IsSuccessful = false, Error = "Wrong UserAction" });
                return;
            }
            bool isInitialSync = ua.ActionType == UserActionType.SyncFull;

            _backgroundSyncManager.ForegroundSyncStarted();

            try
            {
                SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicHeadlineLoading);

                var syncConfiguration = CreateSyncConfigurationAndBackup(ua);

                if (syncConfiguration.Config)
                {
                    var didSync = await _dataSyncService.FullConfigSync(_cancellationTokenSource.Token,
                        syncConfiguration.UseForce);
                }

                if (syncConfiguration.Data)
                {
                    var didSync = await _dataSyncService.FullDataSync(_cancellationTokenSource.Token,
                        syncConfiguration.UseForce);
                }

                if (syncConfiguration.Incremental)
                {
                    var didSync = await _dataSyncService.IncrementalDataSync(_cancellationTokenSource.Token);
                }

                if (syncConfiguration.Resources)
                {
                    var didSync = await _dataSyncService.SyncResources(_cancellationTokenSource.Token, syncConfiguration.UseForce);
                }

                if (syncConfiguration.ShouldBuildConfigurationCache)
                {
                    SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicLoadingData);
                    await _dataSyncService.BuildConfigurationCache(_cancellationTokenSource.Token);
                }

                if (!_sessionContext.IsInOfflineMode)
                {
                    bool caseInsensitive = _configurationService.GetBoolConfigValue("Login.CaseInsensitive");
                    bool passwordSaveAllowed = _configurationService.GetBoolConfigValue("Login.PasswordSaveAllowed", true);
                    bool allowOnlyOnlineLogin = _configurationService.GetBoolConfigValue("Login.AllowOnlineLoginOnly", false);

                    if (passwordSaveAllowed && !allowOnlyOnlineLogin)
                    {
                        // TODO: this may need a bit more logic for the case the user has performed a full sync
                        // and after that the value of this parameter has changed.
                        _offlineAuthenticationService.StoreContextForOfflineAuthentication(_sessionContext, caseInsensitive);
                    }
                }

                SyncStatus = string.Format(_localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicSyncPlaceHolder), "",
                           _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicOK));
            }
            catch (AuthenticationException)
            {
                SyncStatusTextColor = Color.Red;
                SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           LocalizationKeys.KeyLoginErrorMessageCheckLoginData);
                result.IsSuccessful = false;
                result.Error = "Authentication Error";
                RevertSyncChanges(ua);
            }
            catch (HttpRequestExceptionEx)
            {
                SyncStatusTextColor = Color.Red;
                SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           LocalizationKeys.KeyLoginErrorMessageCheckLoginData);
                result.IsSuccessful = false;
                result.Error = "Http Error";
                RevertSyncChanges(ua);
            }
            catch (HttpRequestException)
            {
                SyncStatusTextColor = Color.Red;
                SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           LocalizationKeys.KeyLoginErrorMessageGeneral);
                result.IsSuccessful = false;
                result.Error = "Http Error";
                RevertSyncChanges(ua);
            }
            catch (CrmException)
            {
                SyncStatusTextColor = Color.Red;
                SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           LocalizationKeys.KeyLoginErrorMessageUserCancelledSync);
                result.IsSuccessful = false;
                result.IsCancelled = true;
                result.Error = "User Canceled";
                RevertSyncChanges(ua);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Unexpeted sync error: {ex.Message}");
                SyncStatusTextColor = Color.Red;
                SyncStatus = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           LocalizationKeys.KeyLoginErrorMessageGeneral);
                result.IsSuccessful = false;
                result.Error = "General Error";
                RevertSyncChanges(ua);
            }

            if (isInitialSync)
            {
                if(result.IsSuccessful)
                {
                    try
                    {
                        IntalizeSessionData();
                        await _navigationController.NavigateToAsync<DashboardPageViewModel>(result);
                        return;
                    }
                    catch
                    {
                        await _navigationController.PopAllPopupAsync(new SyncResult { IsSuccessful = false, Error = "Something is wrong with your data model" });
                        return;
                    }
                }
                await _navigationController.PopAllPopupAsync(result);
            }
            else
            {
                if (result.IsSuccessful)
                {
                    IntalizeSessionData();
                    await _navigationController.NavigateToAsync<DashboardPageViewModel>();
                    return;
                }
                await _navigationController.PopPopupAsync();
            }

            _backgroundSyncManager.ForegroundSyncEnded();
        }

        private void IntalizeSessionData()
        {
            var serverTimeZone = _configurationService.GetServerTimezone();
            if(!string.IsNullOrEmpty(serverTimeZone))
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(serverTimeZone);
                _sessionContext.ServerTimeZone = timeZone;
                _sessionContext.ClientTimeZone = TimeZoneInfo.Local;
            }
        }


        private (bool Config, bool Data, bool Incremental, bool Resources, bool UseForce, bool ShouldBuildConfigurationCache) CreateSyncConfigurationAndBackup(UserAction userAction)
        {
            var syncConfiguration = (Config: true,
                Data: true,
                Incremental: true,
                Resources: true,
                UseForce: userAction.UseForce,
                ShouldBuildConfigurationCache: true);

            switch(userAction.ActionType)
            {
                case UserActionType.SyncConfig:
                    syncConfiguration.Data = false;
                    syncConfiguration.Incremental = false;
                    _dataSyncService.CreateConfigDatabaseBackup();
                    break;
                case UserActionType.SyncData:
                    syncConfiguration.Config = false;
                    syncConfiguration.Incremental = false;
                    syncConfiguration.Resources = false;
                    _dataSyncService.CreateCrmDataDatabaseBackup();
                    break;
                case UserActionType.SyncIncremental:
                    syncConfiguration.Data = false;
                    syncConfiguration.Config = false;
                    syncConfiguration.Resources = false;
                    _dataSyncService.CreateCrmDataDatabaseBackup();
                    break;
                case UserActionType.SyncFull:
                    syncConfiguration.Config = true;
                    syncConfiguration.Data = true;
                    syncConfiguration.Incremental = false;
                    syncConfiguration.Resources = true;
                    _dataSyncService.CreateLocalStoreBackup();
                    break;
                default: 
                    break;
            }

            return syncConfiguration;
        }

        private void RevertSyncChanges(UserAction userAction)
        {
            switch (userAction.ActionType)
            {
                case UserActionType.SyncConfig:
                    _dataSyncService.RestoreConfigDatabase();
                    break;
                case UserActionType.SyncData:
                case UserActionType.SyncIncremental:
                    _dataSyncService.RestoreCrmDataDatabase();
                    break;
                case UserActionType.SyncFull:
                    _dataSyncService.RestoreLocalStore();
                    break;
                default: 
                    break;
            }
        }
    }
}
