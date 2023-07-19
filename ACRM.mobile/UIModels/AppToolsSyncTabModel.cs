using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.FullSync;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class AppToolsSyncTabModel: UIWidget
    {
        private readonly IConfigurationService _configurationService;
        public ICommand DataSyncCommand => new Command(() => OnSyncRequested(WidgetEventType.DataSyncRequested));
        public ICommand IncrementalSyncCommand => new Command(() => OnSyncRequested(WidgetEventType.IncrementalSyncRequested));
        public ICommand ConfigSyncCommand => new Command(() => OnSyncRequested(WidgetEventType.ConfigurationSyncRequested));
        public ICommand FullSyncCommand => new Command(() => OnSyncRequested(WidgetEventType.FullSyncRequested));
        public ICommand ChangeLanguageCommand => new Command(() => OnChangeLanguageRequested());

        private bool _isFullSyncRequiredTextVisible = false;
        public bool IsFullSyncRequiredTextVisible
        {
            get => _isFullSyncRequiredTextVisible;
            set
            {
                _isFullSyncRequiredTextVisible = value;
                RaisePropertyChanged(() => IsFullSyncRequiredTextVisible);
            }
        }

        private string _fullSyncRequiredText;
        public string FullSyncRequiredText
        {
            get => _fullSyncRequiredText;
            set
            {
                _fullSyncRequiredText = value;
                RaisePropertyChanged(() => FullSyncRequiredText);
            }
        }

        private bool _dataSyncOn = true;
        public bool DataSyncOn
        {
            get => _dataSyncOn;
            set
            {
                _dataSyncOn = value;
                RaisePropertyChanged(()=>DataSyncOn);
            }
        }

        private string _dataSyncText;
        public string DataSyncText
        {
            get => _dataSyncText;
            set
            {
                _dataSyncText = value;
                RaisePropertyChanged(() => DataSyncText);
            }
        }

        private string _incrementalSyncText;
        public string IncrementalSyncText
        {
            get => _incrementalSyncText;
            set
            {
                _incrementalSyncText = value;
                RaisePropertyChanged(() => IncrementalSyncText);
            }
        }

        private bool _configOn = true;
        public bool ConfigOn
        {
            get => _configOn;
            set
            {
                _configOn = value;
                RaisePropertyChanged(() => ConfigOn);
            }
        }

        private string _configSyncText;
        public string ConfigSyncText
        {
            get => _configSyncText;
            set
            {
                _configSyncText = value;
                RaisePropertyChanged(() => ConfigSyncText);
            }
        }

        private string _fullSyncText;
        public string FullSyncText
        {
            get => _fullSyncText;
            set
            {
                _fullSyncText = value;
                RaisePropertyChanged(() => FullSyncText);
            }
        }

        private bool _changeLanguageOn = true;
        public bool ChangeLanguageOn
        {
            get => _changeLanguageOn;
            set
            {
                _changeLanguageOn = value;
                RaisePropertyChanged(() => ChangeLanguageOn);
            }
        }

        private string _changeLanguageText;
        public string ChangeLanguageText
        {
            get => _changeLanguageText;
            set
            {
                _changeLanguageText = value;
                RaisePropertyChanged(() => ChangeLanguageText);
            }
        }

        public AppToolsSyncTabModel(FullSyncStatusType fullSyncStatusType, CancellationTokenSource parentCancellationTokenSource)
            : base (parentCancellationTokenSource)
        {
            _configurationService = AppContainer.Resolve<IConfigurationService>();
            InitProperties(fullSyncStatusType);
        }

        private void InitProperties(FullSyncStatusType fullSyncStatusType)
        {
            FullSyncRequiredText = _localizationController.GetString(LocalizationKeys.TextGroupLogin, LocalizationKeys.KeyLoginErrorMessageFullSyncRequired);
            DataSyncText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSyncOrganizerActionUpdateData);
            IncrementalSyncText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicLabelIncrementalDataSync);
            ConfigSyncText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSyncOrganizerActionUpdateConfiguration);
            FullSyncText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicLabelFullSync);
            ChangeLanguageText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSyncOrganizerActionChangeLanguage);

            switch (fullSyncStatusType)
            {
                case FullSyncStatusType.FullSyncBlockIntervalDays:
                    DataSyncOn = false;
                    ConfigOn = false;
                    ChangeLanguageOn = false;
                    IsFullSyncRequiredTextVisible = true;
                    break;
                case FullSyncStatusType.FullSyncWarnIntervalDays:
                    SetConfigValues();
                    IsFullSyncRequiredTextVisible = true;
                    break;
                case FullSyncStatusType.NoFullSyncSuggested:
                    SetConfigValues();
                    break;
            }
        }

        private void SetConfigValues()
        {
            ConfigOn = _configurationService.GetBoolConfigValue("Sync.ConfigOnOff", true);
            ChangeLanguageOn = _configurationService.GetBoolConfigValue("Sync.LanguageOnOff", true);
        }

        private void OnSyncRequested(WidgetEventType widgetEventType)
        {
            ParentBaseModel?.PublishMessage(new WidgetMessage
            {
                EventType = widgetEventType,
                ControlKey = "AppToolsSyncTabModel"
            });
        }

        private void OnChangeLanguageRequested()
        {
            ParentBaseModel?.PublishMessage(new WidgetMessage
            {
                EventType = WidgetEventType.ChangeLanguageRequested,
                ControlKey = "AppToolsSyncTabModel"
            });
        }

        public override async ValueTask<bool> InitializeControl()
        {
            return true;
        }
    }
}
