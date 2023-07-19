using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.UIModels
{
    public class AppToolsLogTabModel : UIWidget
    {
        public ICommand ShowCrmDBCommand => new Command(() => OnShowDBRequested(WidgetEventType.ShowCrmDBRequested));
        public ICommand ShowConfigDBCommand => new Command(() => OnShowDBRequested(WidgetEventType.ShowConfigDBRequested));
        public ICommand ShowOfflineDBCommand => new Command(() => OnShowDBRequested(WidgetEventType.ShowOfflineDBRequested));

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

        private string _offlineStorageText;
        public string OfflineStorageText
        {
            get => _offlineStorageText;
            set
            {
                _offlineStorageText = value;
                RaisePropertyChanged(() => OfflineStorageText);
            }
        }

        public AppToolsLogTabModel(CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            InitProperties();
        }

        private void InitProperties()
        {
            OfflineStorageText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicDebugOfflineDbPage);
        }

        private void OnShowDBRequested(WidgetEventType widgetEventType)
        {
            ParentBaseModel?.PublishMessage(new WidgetMessage
            {
                EventType = widgetEventType,
                ControlKey = "AppToolsLogTabModel"
            });
        }

        public override async ValueTask<bool> InitializeControl()
        {
            _logViewModel = new LogModel(true, _cancellationTokenSource);
            _logViewModel.ParentBaseModel = this;
            await _logViewModel.InitializeControl();
            LogViewModel = _logViewModel;
            return true;
        }
    }
}
