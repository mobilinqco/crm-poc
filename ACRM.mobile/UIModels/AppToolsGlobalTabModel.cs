using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Localization;
using ACRM.mobile.Services;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class AppToolsGlobalTabModel: UIWidget
    {
        protected readonly IConfigurationService _configurationService;
        protected readonly IUserActionBuilder _userActionBuilder;
        public ICommand SettingsCommand => new Command(async () => await OnSettingsRequested());
        public ICommand InboxCommand => new Command(() => OnInboxRequested());

        private string _settingsIconText;
        public string SettingsIconText
        {
            get => _settingsIconText;
            set
            {
                _settingsIconText = value;
                RaisePropertyChanged(() => SettingsIconText);
            }
        }

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

        private string _inboxIconText;
        public string InboxIconText
        {
            get => _inboxIconText;
            set
            {
                _inboxIconText = value;
                RaisePropertyChanged(() => InboxIconText);
            }
        }

        private string _inboxText;
        public string InboxText
        {
            get => _inboxText;
            set
            {
                _inboxText = value;
                RaisePropertyChanged(() => InboxText);
            }
        }

        private string _inboxAdditionalTitleText;
        public string InboxAdditionalTitleText
        {
            get => _inboxAdditionalTitleText;
            set
            {
                _inboxAdditionalTitleText = value;
                RaisePropertyChanged(() => InboxAdditionalTitleText);
            }
        }

        public AppToolsGlobalTabModel(CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _configurationService = AppContainer.Resolve<IConfigurationService>();
            _userActionBuilder = AppContainer.Resolve<IUserActionBuilder>();
            InitProperties();
        }

        private void InitProperties()
        {
            SettingsIconText = MaterialDesignIcons.Cog;
            SettingsText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSettings);
            InboxIconText = MaterialDesignIcons.InboxArrowDownOutline;
            InboxText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicInboxTitle);
            InboxAdditionalTitleText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicInboxAdditionalTitle);
        }

        private async Task OnSettingsRequested()
        {
            var infoMenuItem = await _configurationService.GetMenu("INFO_SETTINGS", _cancellationTokenSource.Token);
            if (infoMenuItem?.ViewReference != null)
            {

                var _userAction = _userActionBuilder.UserActionFromMenu(_configurationService, infoMenuItem);
                await _navigationController.PopPopupAsync();
                await _navigationController.NavigateAsyncForAction(_userAction, _cancellationTokenSource.Token);
            }
        }

        private void OnInboxRequested()
        {
            //WidgetEventType.InboxTapped TODO
        }

        public override async ValueTask<bool> InitializeControl()
        {
            return true;
        }

    }
}
