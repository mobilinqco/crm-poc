using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Localization;
using ACRM.mobile.UIModels;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class LogPageViewModel: BaseViewModel
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

        public LogPageViewModel()
        {
            InitProperties();
        }

        private void InitProperties()
        {
            CloseText = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);
        }

        public override async Task InitializeAsync(object data)
        {
            IsLoading = true;
            _logViewModel = new LogModel(false, _cancellationTokenSource);
            _logViewModel.ParentBaseModel = this;
            await _logViewModel.InitializeControl();
            LogViewModel = _logViewModel;
            await base.InitializeAsync(data);
            IsLoading = false;
        }

        private async Task Close()
        {
            await _navigationController.PopPopupAsync();
        }
    }
}
