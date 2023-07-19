using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class SettingsSelectionPageViewModel : BaseViewModel
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public ICommand OnCloseButtonTapped => new Command(async () =>
        {
            await _navigationController.PopAllPopupAsync(null);
        });

        public ICommand ChangePasswordCommand => new Command(async () =>
        {
            _sessionContext.IsChangePasswordEnabled = !_sessionContext.IsChangePasswordEnabled;
            MessagingCenter.Send(new RefreshEvent { IsRefreshNeeded = true, Reason = "ChangePasswordTriggred" }, "RefreshEvent");
            await _navigationController.PopAllPopupAsync(null);
        });
        
        public ICommand ShowAppLogs => new Command(async () => await ShowAppLogsAsync());

        private string _settingsText;
        public string SettingsText
        {
            get => _settingsText;
            set
            {
                _settingsText = value;
                RaisePropertyChanged(() => SettingsText);
            }
        }

        private string _offlineLoginText;
        public string OfflineLoginText
        {
            get => _offlineLoginText;
            set
            {
                _offlineLoginText = value;
                RaisePropertyChanged(() => OfflineLoginText);
            }
        }

        public bool OfflineLogin
        {
            get => _sessionContext.IsOfflineModeToggled;
            set
            {
                _sessionContext.IsOfflineModeToggled = value;
            }
        }

        private string _viewLogsText;
        public string ViewLogsText
        {
            get => _viewLogsText;
            set
            {
                _viewLogsText = value;
                RaisePropertyChanged(() => ViewLogsText);
            }
        }

        private string _changePasswordTest;
        public string ChangePasswordText
        {
            get => _changePasswordTest;
            set
            {
                _changePasswordTest = value;
                RaisePropertyChanged(() => ChangePasswordText);
            }
        }

        private string _closeButtonText;
        public string CloseButtonText
        {
            get => _closeButtonText;
            set
            {
                _closeButtonText = value;
                RaisePropertyChanged(() => CloseButtonText);
            }
        }

        public SettingsSelectionPageViewModel()
        {
            InitBindings();
        }

        private void InitBindings() 
        {
            SettingsText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSettings);
            OfflineLoginText = _localizationController.GetString(LocalizationKeys.TextGroupLogin, LocalizationKeys.KeyLoginOptionOfflineLogin);
            ViewLogsText = _localizationController.GetString(LocalizationKeys.TextGroupLogin, LocalizationKeys.KeyLoginOptionViewLogs);
            ChangePasswordText = _localizationController.GetString(LocalizationKeys.TextGroupLogin, LocalizationKeys.KeyLoginOptionChangePassword);
            CloseButtonText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
        }

        private async Task ShowAppLogsAsync()
        {
            try
            {
                await _navigationController.PopAllPopupAsync(null);
                string folder = string.Empty;
                IsBusy = true;

                if (Device.RuntimePlatform == Device.Android)
                    folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                else
                    folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");

                string logFolder = Path.Combine(folder, "logs");
                string logText = string.Empty;

                if (Directory.Exists(logFolder))
                {
                    var fileName = $"{logFolder}/nlog.log";
                    if (File.Exists(fileName))
                    {
                        using var streamReader = new StreamReader(fileName);
                        logText = await streamReader.ReadToEndAsync();
                    }
                }

                await _navigationController.DisplayPopupAsync<LogListPageViewModel>(logText);
                IsBusy = false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unable to show logs, error is: {ex.Message}");
            }
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await base.InitializeAsync(navigationData);
        }
    }
}
