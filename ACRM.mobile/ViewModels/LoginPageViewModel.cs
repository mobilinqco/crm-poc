using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.DataAccess;
using ACRM.mobile.DataAccess.Network;
using ACRM.mobile.Domain;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Network.Responses;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using NLog;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        private string _selectedServerString = "";
        public string SelectedServerString
        {
            get => _selectedServerString;
            set
            {
                _selectedServerString = value;
                RaisePropertyChanged(() => SelectedServerString);
            }
        }

        private string _applicationVersion = "1.0.0";
        public string ApplicationVersion
        {
            get => _applicationVersion;
            set
            {
                _applicationVersion = value;
                RaisePropertyChanged(() => ApplicationVersion);
            }
        }

        private string _textPlaceholderUsername = "Username";
        public string TextPlaceholderUsername
        {
            get => _textPlaceholderUsername;
            set
            {
                _textPlaceholderUsername = value;
                RaisePropertyChanged(() => TextPlaceholderUsername);
            }
        }
        public string _textPlaceholderPassword = "Password";

        public string TextPlaceholderPassword
        {
            get => _textPlaceholderPassword;
            set
            {
                _textPlaceholderPassword = value;
                RaisePropertyChanged(() => TextPlaceholderPassword);
            }
        }

        public string _textPlaceholderNewPassword = "New password";
        public string TextPlaceholderNewPassword
        {
            get => _textPlaceholderNewPassword;
            set
            {
                _textPlaceholderNewPassword = value;
                RaisePropertyChanged(() => TextPlaceholderNewPassword);
            }
        }

        public string _textPlaceholderConfirmNewPassword = "Confirm new password";
        public string TextPlaceholderConfirmNewPassword
        {
            get => _textPlaceholderConfirmNewPassword;
            set
            {
                _textPlaceholderConfirmNewPassword = value;
                RaisePropertyChanged(() => TextPlaceholderConfirmNewPassword);
            }
        }

        public string ApplicationName => "CRM.Client";
        public string LoginError = "";

        public string _textSignIn = "Login";
        public string TextSignIn
        {
            get => _textSignIn;
            set
            {
                _textSignIn = value;
                RaisePropertyChanged(() => TextSignIn);
            }
        }

        private readonly IAuthenticationService _authenticationService;
        private readonly ICrmInstanceService _crmInstanceService;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ISyncStatusService _syncStatusService;
        private readonly IOfflineAuthenticationService _offlineAuthenticationService;

        private readonly ICacheService _cacheService;
        public ICommand LoginCommand => new Command(async () => await OnLogin());
        public ICommand ServerListCommand => new Command(async () => await ShowConfiguredServer());
        public ICommand ShowSettingsCommand => new Command(async () => await ShowSettings());
        public ICommand PageAppearingCommand => new Command(async () => await OnPageAppearing());

        public LoginPageViewModel(IAuthenticationService authenticationService,
            ICrmInstanceService crmInstanceService,
            ICacheService cacheService,
            ISyncStatusService syncStatusService,
            IOfflineAuthenticationService offlineAuthenticationService)
        {
            _authenticationService = authenticationService;
            _crmInstanceService = crmInstanceService;
            _cacheService = cacheService;
            _syncStatusService = syncStatusService;
            _offlineAuthenticationService = offlineAuthenticationService;

            // TODO: This is a dirty fix and maybe the best would be to enable this part in the login settings
            // and allow customers who are not having proper certificates to enable this.
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            if (DeviceInfo.Platform == DevicePlatform.UWP)
            {
                ApplicationVersion = VersionTracking.CurrentVersion;
            }
            else
            {
                ApplicationVersion = VersionTracking.CurrentVersion + " build " + VersionTracking.CurrentBuild;
            }

            MessagingCenter.Subscribe<RefreshEvent>(this, "RefreshEvent", (refreshEvent) =>
            {
                if (refreshEvent.IsRefreshNeeded)
                {
                    if (refreshEvent.Reason.Equals("ChangePasswordTriggred"))
                    {
                        IsChangePasswordEnabled = _sessionContext.IsChangePasswordEnabled;
                    }
                    else
                    {
                        _ = SetSelectedInstance();
                    }
                }
            });
        }

        private bool _isChangePasswordEnabled = false;
        public bool IsChangePasswordEnabled
        {
            get
            {
                return _isChangePasswordEnabled;
            }
            set
            {
                _isChangePasswordEnabled = value;
                _sessionContext.IsChangePasswordEnabled = _isChangePasswordEnabled;
                RaisePropertyChanged(() => IsChangePasswordEnabled);
            }
        }

        private string _username;
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                RaisePropertyChanged(() => Username);
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        private string _newPassword = string.Empty;
        public string NewPassword
        {
            get
            {
                return _newPassword;
            }
            set
            {
                _newPassword = value;
                RaisePropertyChanged(() => NewPassword);
            }
        }

        private string _confirmNewPassword = string.Empty;
        public string ConfirmNewPassword
        {
            get
            {
                return _confirmNewPassword;
            }
            set
            {
                _confirmNewPassword = value;
                RaisePropertyChanged(() => ConfirmNewPassword);
            }
        }

        private async Task OnPageAppearing()
        {
            await SetSelectedInstance();
        }

        private async Task ShowConfiguredServer()
        {
            IsBusy = true;
            await _navigationController.DisplayPopupAsync<ServerSelectionPageViewModel>();
            IsBusy = false;
        }

        private async Task ShowSettings()
        {
            IsBusy = true;
            await _navigationController.DisplayPopupAsync<SettingsSelectionPageViewModel>();
            IsBusy = false;
        }

        private async Task OnLogin()
        {
            IsBusy = true;
            _localizationController.AttachConfiguration();
            if (_sessionContext.CrmInstance == null)
            {
                List<CrmInstance> crmInstances = await _crmInstanceService.GetCrmInstancesAsync();
                if (crmInstances.Count < 1)
                {
                    var defaultServer = await _crmInstanceService.GetDefaultServer();
                    defaultServer.Username = Username;
                    _sessionContext.CrmInstance = defaultServer;
                    await _crmInstanceService.AddNewCrmInstance(defaultServer);
                    if (_sessionContext.CrmInstance != null)
                    {
                        if (SelectedServerString != _sessionContext.CrmInstance.Name)
                        {
                            SelectedServerString = _sessionContext.CrmInstance.Name;
                        }
       
                    }

                }
                else
                {
                    await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                        LocalizationKeys.KeyLoginErrorTitleLoginFailed,
                        LocalizationKeys.KeyLoginSelectDefaultServerMessage);
                    return;
                }
                    
            }

            _dialogContorller.ShowProgress("Loading");
            try
            {
                _sessionContext.LogoutCleanup();
                _cacheService.ResetCache();
                //_localizationController.ResetConfiguration();
                _sessionContext.CrmInstance.Username = Username;

                SyncStatus syncConfig = await _syncStatusService.GetSyncStatusAsync();
                bool initialLogin = syncConfig == null;

                PasswordChangeStatus passwordChangeStatus = null;

                if(IsChangePasswordEnabled)
                {
                    if(!NewPassword.Equals(ConfirmNewPassword))
                    {
                        await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                            LocalizationKeys.KeyLoginErrorTitleLoginNotPossible,
                            LocalizationKeys.KeyLoginErrorNewPasswordAndConfirmPasswordDontMatch);
                        return;
                    }

                    if(string.IsNullOrWhiteSpace(NewPassword))
                    {
                        passwordChangeStatus = new PasswordChangeStatus { NewPassword = "", EmptyPassword = true };
                    }
                    else
                    {
                        passwordChangeStatus = new PasswordChangeStatus { NewPassword = NewPassword, EmptyPassword = false };
                    }
                }

                AuthenticationResponse authenticationResponse;
                if (initialLogin)
                {
                    if(_sessionContext.IsOfflineModeToggled)
                    {
                        await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                            LocalizationKeys.KeyLoginErrorTitleLoginNotPossible,
                            LocalizationKeys.KeyLoginErrorMessageOfflineModeRejected);
                        return;
                    }
                    authenticationResponse = await _authenticationService.Authenticate(_sessionContext.CrmInstance, Username, Password, passwordChangeStatus : passwordChangeStatus);
                }
                else
                {
                    if (_sessionContext.IsOfflineModeToggled)
                    {
                        if (!_offlineAuthenticationService.IsOfflinePossible())
                        {
                            await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                                LocalizationKeys.KeyLoginErrorTitleLoginNotPossible,
                                LocalizationKeys.KeyLoginErrorMessageOfflineModeRejected);
                            return;
                        }
                        else
                        {
                            HandleIsCaseInsensitive();
                            authenticationResponse = await _offlineAuthenticationService.Authenticate(_sessionContext.CrmInstance, Username, Password);
                            _sessionContext.IsInOfflineMode = true;
                        }
                    }
                    else
                    {
                        HandleIsCaseInsensitive();
                        authenticationResponse = await _authenticationService.Authenticate(_sessionContext.CrmInstance, Username, Password, syncConfig.LanguageCode, passwordChangeStatus: passwordChangeStatus);
                    }

                    _sessionContext.LanguageCode = syncConfig.LanguageCode;
                }
                IsChangePasswordEnabled = false;
                if (authenticationResponse.IsPasswordChanged)
                {
                    Password = NewPassword;
                }
                _sessionContext.SessionCookies = authenticationResponse.Cookies;
                _sessionContext.User = new User(Username, Password, authenticationResponse.SessionInformation);
                _sessionContext.CrmInstance.RevolutionRuntimeUrl = authenticationResponse.RedirectionUrl;

                _dialogContorller.HideProgress();

                if (_sessionContext.IsAuthenticated())
                {

                    if (authenticationResponse.IsPasswordChanged)
                    {
                        await _navigationController.NavigateToAsync<DashboardPageViewModel>();
                        await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupBasic,
                            LocalizationKeys.KeyBasicChangePassword,
                            LocalizationKeys.KeyBasicPasswordChanged);
                        await _navigationController.RemoveBackStackAsync();

                        if(!initialLogin)
                        {
                            return;
                        }
                    }

                    if (initialLogin)
                    {
                        await _navigationController.DisplayPopupAsync<LanguageSelectionPageViewModel>(authenticationResponse.ServerInformation.ServerLanguages);
                    }
                    else
                    {
                        if (_syncStatusService.IsPartialSync())
                        {
                            bool shouldResume = await _dialogContorller.ShowConfirm(
                                _localizationController.GetString(LocalizationKeys.TextGroupLogin, LocalizationKeys.KeyLoginIncompleteSyncTitle),
                                _localizationController.GetString(LocalizationKeys.TextGroupLogin, LocalizationKeys.KeyLoginIncompleteSyncMessage),
                                _localizationController.GetString(LocalizationKeys.TextGroupLogin, LocalizationKeys.KeyLoginResume),
                                _localizationController.GetString(LocalizationKeys.TextGroupLogin, LocalizationKeys.KeyLoginRestart));

                            if(shouldResume)
                            {
                                await _navigationController.DisplayPopupAsync<SyncPageViewModel>(new UserAction { ActionType = UserActionType.SyncFull });
                            }
                            else
                            {
                                await _navigationController.DisplayPopupAsync<LanguageSelectionPageViewModel>(authenticationResponse.ServerInformation.ServerLanguages);
                            }
                        }
                        else
                        {
                            var userAction = new UserAction { ActionType = UserActionType.SyncFull };
                            if (_sessionContext.CrmInstance?.GetSettingValue(CrmLocalSettings.FullSyncAfterLoginKey) == "1")
                            {
                                userAction.UseForce = true;
                            }
                            await _navigationController.DisplayPopupAsync<SyncPageViewModel>(userAction);
                        }
                    }
                }
                else
                {
                    await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                        LocalizationKeys.KeyLoginErrorTitleLoginFailed,
                        LocalizationKeys.KeyLoginErrorMessageCheckLoginData);
                }
            }
            catch (AuthenticationException)
            {
                _dialogContorller.HideProgress();
                await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                    LocalizationKeys.KeyLoginErrorTitleLoginFailed,
                    LocalizationKeys.KeyLoginErrorMessageCheckLoginData);
            }
            catch (HttpRequestExceptionEx)
            {
                _dialogContorller.HideProgress();
                await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                    LocalizationKeys.KeyLoginErrorTitleLoginFailed,
                    LocalizationKeys.KeyLoginErrorMessageCheckLoginData);
            }
            catch (HttpRequestException)
            {
                _dialogContorller.HideProgress();
                await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                    LocalizationKeys.KeyLoginErrorTitleLoginFailed,
                    LocalizationKeys.KeyLoginErrorMessageGeneral);
            }
            catch (Exception ex)
            {
                _dialogContorller.HideProgress();
                await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                    LocalizationKeys.KeyLoginErrorTitleLoginFailed,
                    LocalizationKeys.KeyLoginErrorMessageGeneral);
            }
            IsBusy = false;
        }

        private void HandleIsCaseInsensitive()
        {
            IConfigurationService configurationService = AppContainer.Resolve<IConfigurationService>();

            if (configurationService.GetBoolConfigValue("Login.CaseInsensitive"))
            {
                Password = Password.ToLower();
            }
        }

        

        private async Task SetSelectedInstance()
        { 
            IsBusy = true;
            List<CrmInstance> crmInstances = await _crmInstanceService.GetCrmInstancesAsync();

            if (crmInstances.Count > 0)
            {
                _sessionContext.CrmInstance = await _crmInstanceService.GetLastUsedInstance();
                _localizationController.AttachConfiguration();

                if (_sessionContext.CrmInstance != null)
                {
                    if(SelectedServerString != _sessionContext.CrmInstance.Name) {
                        SelectedServerString = _sessionContext.CrmInstance.Name;
                        Username = _sessionContext.CrmInstance.Username;
                        Password = String.Empty;
                    }
                }
                else
                {
                    SelectedServerString = "";
                    Username = "";
                    Password = String.Empty;
                }
            }
            else
            {
                _sessionContext.CrmInstance = null;
                SelectedServerString = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                           LocalizationKeys.KeyLoginRegisterServerMessage);
                Username = "";
            }

            SetLocalizedStrings();

            IsBusy = false;
        }

        private void SetLocalizedStrings()
        {
            TextPlaceholderUsername = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
               LocalizationKeys.KeyLoginUsernamePlaceholder);
            TextPlaceholderPassword = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                LocalizationKeys.KeyLoginPasswordPlaceholder);
            TextPlaceholderNewPassword = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                LocalizationKeys.KeyLoginNewPassword);
            TextPlaceholderConfirmNewPassword = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                LocalizationKeys.KeyLoginConfirmNewPassword);
            TextSignIn = _localizationController.GetString(LocalizationKeys.TextGroupLogin,
                LocalizationKeys.KeyLoginTapLogin);
        }

        public override async Task RefreshAsync(object data)
        {
            if (data is SyncResult)
            {
                if ((data as SyncResult).IsSuccessful)
                {
                    await _navigationController.NavigateToAsync<DashboardPageViewModel>();
                    await _navigationController.RemoveBackStackAsync();
                }
                else if (!(data as SyncResult).IsCancelled)
                {
                    await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupLogin,
                        LocalizationKeys.KeyLoginErrorTitleSyncFailed,
                        LocalizationKeys.KeyLoginErrorMessageErrorOccuredDuringSync);
                }
            }
            else
            {
                await SetSelectedInstance();
            }
        }
    }
}
