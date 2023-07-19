using ACRM.mobile.Localization;
using ACRM.mobile.UIModels;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class LogListPageViewModel : BaseViewModel
    {
        public ICommand CloseCommand => new Command(async () => await Close());

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

        private LogModel _logViewModel;
        public LogModel LogViewModel
        {
            get => _logViewModel;
            set
            {
                _logViewModel = value;
                RaisePropertyChanged(() => LogViewModel);
            }
        }

        public LogListPageViewModel()
        {
            InitProperties();
            RegisterMessages();
        }

        private void InitProperties()
        {
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
        }

        private void RegisterMessages()
        {
            RegisterMessage(WidgetEventType.ShowAppLogsRequested, "LogModel", OnShowAppLogsRequested);
        }

        private async Task OnShowAppLogsRequested(WidgetMessage widgetMessage)
        {
            await ShowLogPage();
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            _logViewModel = new LogModel(true, _cancellationTokenSource);
            _logViewModel.ParentBaseModel = this;
            await _logViewModel.InitializeControl();
            LogViewModel = _logViewModel;
            await base.InitializeAsync(navigationData);
            IsLoading = false;
        }

        private async Task ShowLogPage()
        {
            await _navigationController.DisplayPopupAsync<LogPageViewModel>();
        }

        private async Task Close()
        {
            await _navigationController.PopPopupAsync();
        }
    }
}
