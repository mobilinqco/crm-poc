using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels.Base
{
    public class NavigationBarBaseViewModel: BaseViewModel
    {
        protected BackgroundSyncManager _backgroundSyncManager;

        public ICommand ShowSearchPageCommand => new Command(async () => await OnShowSearchPageCommandAsync());
        public ICommand BackButtonCommand => new Command(async () => await OnBackButtonCommandAsync());
        public ICommand GoToHomePageCommand => new Command(async () => await OnGoToHomePageCommand());
        public ICommand ShowSettingsCommand => new Command(async () => await OnShowSettingsCommand());
        public ICommand ShowConflictsCommand => new Command(async () => await OnShowConflictsCommand());

        private bool _isBackButtonVisible = false;
        public bool IsBackButtonVisible {
            get
            {
                return _isBackButtonVisible;
            }
            set
            {
                _isBackButtonVisible = value;
                RaisePropertyChanged(() => IsBackButtonVisible);
            }
        }

        private string _pageTitle = "";
        public string PageTitle
        {
            get
            {
                return _pageTitle;
            }
            set
            {
                _pageTitle = value.TrimEnd('\r', '\n');
                RaisePropertyChanged(() => PageTitle);
            }
        }

        private bool _hasOfflineRequestsSyncConflicts = false;
        public bool HasOfflineRequestsSyncConflicts
        {
            get => _hasOfflineRequestsSyncConflicts;
            set
            {
                _hasOfflineRequestsSyncConflicts = value;
                RaisePropertyChanged(() => HasOfflineRequestsSyncConflicts);
            }
        }

        protected bool IsConflictsButtonEnabled = true;

        public NavigationBarBaseViewModel()
        {
            _backgroundSyncManager = AppContainer.Resolve<BackgroundSyncManager>();

            InitialiseProperties();
            InitialiseSubscriptions();
        }

        private void InitialiseProperties()
        {
            HasOfflineRequestsSyncConflicts = _backgroundSyncManager.GetOfflineRequestsSyncErrorStatus();
        }

        private void InitialiseSubscriptions()
        {
            MessagingCenter.Subscribe<BackgroundSyncManager, bool>(this, InAppMessages.SendOfflineRequestsSyncErrorsFlag, SetHasOfflineRequestsSyncConflicts);
        }

        private void SetHasOfflineRequestsSyncConflicts(BackgroundSyncManager backgroundSyncManager, bool flag)
        {
            HasOfflineRequestsSyncConflicts = flag;
        }

        private async Task OnShowSearchPageCommandAsync()
        {
            if (!_navigationController.IsInPopupStack<AppSearchMenuPageViewModel>())
            {
                UserAction parameters = new UserAction { ActionUnitName = "$AppSearchMenu" };
                await _navigationController.DisplayPopupAsync<AppSearchMenuPageViewModel>(parameters);
            }
        }

        protected virtual void CancelWidgetsLoading()
        {

        }

        private async Task OnBackButtonCommandAsync()
        {
            CancelWidgetsLoading();
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            await _navigationController.BackAsync();
        }

        private async Task OnGoToHomePageCommand()
        {
            CancelWidgetsLoading();
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            await _navigationController.PopToRoot();
        }

        private async Task OnShowSettingsCommand()
        {
            if (!_navigationController.IsInPopupStack<AppToolsPageViewModel>())
            {
                await _navigationController.DisplayPopupAsync<AppToolsPageViewModel>();
            }
        }

        private async Task OnShowConflictsCommand()
        {
            if(IsConflictsButtonEnabled)
            {
                await _navigationController.NavigateToAsync<ConflictListPageViewModel>();
            } 
        }
    }
}
