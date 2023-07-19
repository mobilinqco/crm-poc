using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Pages;
using ACRM.mobile.Views;
using ACRM.mobile.ViewModels;
using Xamarin.Forms;
using ACRM.mobile.ViewModels.Base;
using Rg.Plugins.Popup.Services;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Logging;
using ACRM.mobile.Utils;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Localization;
using ACRM.mobile.ViewModels.ObservableGroups;
using System.Threading;

namespace ACRM.mobile.CustomControls
{
    public class NavigationController : INavigationController
    {
        private ISessionContext _sessionContext;
        private IUserActionResolver _userActionResolver;
        private ILogService _logService;
        private INetworkRepository _networkRepository;
        private ICacheService _cacheService;
        private ILocalizationController _localizationController;

        public NavigationController(ISessionContext sessionContext,
            IUserActionResolver userActionResolver,
            INetworkRepository networkRepository,
            ICacheService cacheService,
            ILocalizationController localizationController)
        {
            _sessionContext = sessionContext;
            _networkRepository = networkRepository;
            _userActionResolver = userActionResolver;
            _logService = AppContainer.Resolve<ILogService>();
            _cacheService = cacheService;
            _localizationController = localizationController;
        }

        public BaseViewModel PreviousPageViewModel
        {
            get
            {
                var mainPage = Application.Current.MainPage as NavigationView;
                var viewModel = mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 2].BindingContext;
                return viewModel as BaseViewModel;
            }
        }

        public Task Logout()
        {
            _sessionContext.LogoutCleanup();
            _networkRepository.LogoutCleanup();
            _cacheService.ResetCache();
            _localizationController.ResetConfiguration();
            return InitializeAsync();
        }

        public Task InitializeAsync()
        {
            if (!_sessionContext.IsAuthenticated())
            {
                return NavigateToAsync<LoginPageViewModel>();
            }
            else
            {
                return NavigateToAsync<DashboardPageViewModel>();
            }
        }


        public Task NavigateAsyncForAction(UserAction userAction, CancellationToken cancellationToken)
        {
            if(userAction != null)
            {
                if (userAction.ActionType == UserActionType.NavigateBack)
                {
                    return BackAsync(true);
                }
                else if (userAction.ActionType == UserActionType.NavigateHome)
                {
                    return PopToRoot();
                }
                else
                {
                    return UserActionNavigation(userAction, cancellationToken);
                }
            }

            return Task.CompletedTask;
        }

        public async Task NavigateAsyncForActionAndRemoveParent(UserAction userAction, CancellationToken cancellationToken)
        {
            if (userAction != null)
            {
                if (userAction.ActionType == UserActionType.NavigateBack)
                {
                    await BackAsync(true);
                }
                else if (userAction.ActionType == UserActionType.NavigateHome)
                {
                    await PopToRoot();
                }
                else
                {
                    await UserActionNavigation(userAction, cancellationToken);
                    await RemoveLastFromBackStackAsync();
                }
            }
        }

        private async Task UserActionNavigation(UserAction userAction, CancellationToken cancellationToken)
        {
            if (userAction.ActionType == UserActionType.OpenURL || (userAction.ViewReference != null && userAction.ViewReference.IsOpenUrlAction()))
            {
                IUserActionSchuttle userActionSchuttle = AppContainer.Resolve<IUserActionSchuttle>();
                await userActionSchuttle.Carry(userAction, userAction.RecordId, cancellationToken);
            }
            else
            {
                await SimpleNavigateWithAction(userAction);
            }
        }

        public Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel
        {
            return InternalNavigateToAsync(typeof(TViewModel), null);
        }

        public Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : BaseViewModel
        {
            return InternalNavigateToAsync(typeof(TViewModel), parameter);
        }

        public Task RemoveLastFromBackStackAsync()
        {
            var mainPage = Application.Current.MainPage as NavigationView;

            if (mainPage != null)
            {
                mainPage.Navigation.RemovePage(
                    mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 2]);
            }

            return Task.FromResult(true);
        }

        public async Task BackAsync(bool refreshParent = false)
        {
            var mainPage = Application.Current.MainPage as NavigationView;

            if (mainPage != null)
            {
                var page = mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 1];
                var baseModel = (page.BindingContext as BaseViewModel);
                if (await baseModel.CanClose())
                {
                    await mainPage.PopAsync();

                    if (refreshParent || baseModel.NeedRefreshOnBack)
                    {
                        page = mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 1];
                        baseModel = (page.BindingContext as BaseViewModel);
                        baseModel.NeedRefreshOnBack = true;
                        await baseModel.RefreshAsync(new SyncResult());
                    }
                }
            }
        }

        public Task PopToRoot()
        {
            var mainPage = Application.Current.MainPage as NavigationView;

            if (mainPage != null)
            {
                for (int i = 0; i < mainPage.Navigation.NavigationStack.Count - 1; i++)
                {
                    var page = mainPage.Navigation.NavigationStack[i];
                    (page.BindingContext as BaseViewModel).Cancel();
                }
                mainPage.PopToRootAsync();
            }

            return Task.FromResult(true);
        }

        public Task RemoveBackStackAsync()
        {
            var mainPage = Application.Current.MainPage as NavigationView;

            if (mainPage != null)
            {
                for (int i = 0; i < mainPage.Navigation.NavigationStack.Count - 1; i++)
                {
                    var page = mainPage.Navigation.NavigationStack[i];
                    mainPage.Navigation.RemovePage(page);
                }
            }

            return Task.FromResult(true);
        }

        private async Task InternalNavigateToAsync(Type viewModelType, object parameter)
        {
            
            Page page = CreatePage(viewModelType, parameter);
            
            if (page is LoginPageView)
            {
                Application.Current.MainPage = new NavigationView(page);
            }
            else if (page is DashboardPageView)
            {
                Application.Current.MainPage = new NavigationView(page);
            }
            else if (viewModelType == typeof(DocumentUploadPageViewModel))
            {
                await DisplayPopupAsync<DocumentUploadPageViewModel>(parameter);
            }
            else if (viewModelType == typeof(SignaturePageViewModel))
            {
                await DisplayPopupAsync<SignaturePageViewModel>(parameter);
            }
            else if (viewModelType == typeof(PopupListPageViewModel))
            {
                await DisplayPopupAsync<PopupListPageViewModel>(parameter);
            }
            else if (viewModelType == typeof(FilterUIPageViewModel))
            {
                await DisplayPopupAsync<FilterUIPageViewModel>(parameter);
            }
            else
            {
                var navigationPage = Application.Current.MainPage as NavigationView;
                if (navigationPage != null)
                {
                    await navigationPage.PushAsync(page);
                }
                else
                {
                    Application.Current.MainPage = new NavigationView(page);
                }
            }

            await (page.BindingContext as BaseViewModel).InitializeAsync(parameter);
        }


        private Type GetPageTypeForViewModel(Type viewModelType)
        {
            var viewName = viewModelType.FullName.Replace(".ViewModels.", ".Pages.").Replace("Model", string.Empty);
            var viewAssemblyName = viewModelType.GetTypeInfo().Assembly.FullName.Replace(".ViewModels", string.Empty); ;
            var viewTypeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", viewName, viewAssemblyName);
            var viewType = Type.GetType(viewTypeName);
            return viewType;
        }

        private Page CreatePage(Type viewModelType, object parameter)
        {
            Type pageType = GetPageTypeForViewModel(viewModelType);
            if (pageType == null)
            {
                throw new Exception($"Cannot locate page type for {viewModelType}");
            }

            Page page = Activator.CreateInstance(pageType) as Page;
            return page;
        }

        public bool IsInPopupStack<TViewModel>()
        {
            Type pageType = GetPageTypeForViewModel(typeof(TViewModel));
            foreach (var page in PopupNavigation.Instance.PopupStack)
            {
                if (page.GetType() == pageType)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task DisplayPopupAsync<TViewModel>(object parameter) where TViewModel : BaseViewModel
        {
            Page page = CreatePage(typeof(TViewModel), parameter);
            await PopupNavigation.Instance.PushAsync((Rg.Plugins.Popup.Pages.PopupPage)page);
            await (page.BindingContext as BaseViewModel).InitializeAsync(parameter);
        }

        public async Task PopAllPopupAsync(object navigationData)
        { 
            await PopAllPopupAsync();

            if (navigationData == null || !(navigationData is UserAction))
            {
                await RefreshPopupDisplayPage(navigationData);
                return;
            }

            await SimpleNavigateWithAction(navigationData as UserAction);
        }

        public async Task PopAllPopupAsync()
        {
            try
            {
                await PopupNavigation.Instance.PopAllAsync(true);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Trying to popup the popupts but no popup :D {ex}");
            }
        }

        public async Task SimpleNavigateWithAction(UserAction userAction)
        {
            userAction = await ResolveRecordSwitch(userAction);
            Type vmType =  _userActionResolver.Resolve(userAction);
            await InternalNavigateToAsync(vmType, userAction);
        }

        private async Task<UserAction> ResolveRecordSwitch(UserAction userAction)
        {
            if (userAction.ActionType == UserActionType.RecordSwitch)
            {
                var linkResolverService = AppContainer.Resolve<ILinkResolverService>();
                return await linkResolverService.ResolveRecordSwitch(userAction, new CancellationToken());
            }
            else
            {
                return userAction;
            }

        }

        public async Task PopPopupAsync()
        {
            await PopupNavigation.Instance.PopAsync(true);
        }

        public async Task RefreshPopupDisplayPage()
        {
             await RefreshPopupDisplayPage(new SyncResult());
        }

        private async Task RefreshPopupDisplayPage(object parameter)
        {
            var mainPage = Application.Current.MainPage as NavigationView;

            if (mainPage != null && mainPage.Navigation.NavigationStack.Count > 0)
            {
                var page = mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 1];
                await (page.BindingContext as BaseViewModel).RefreshAsync(parameter);
            }
        }

        public async Task ForceRefreshForParent()
        {
            var mainPage = Application.Current.MainPage as NavigationView;
            if (mainPage.Navigation.NavigationStack.Count > 1)
            {
                var page = mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 1];
                var baseModel = (page.BindingContext as BaseViewModel);
                baseModel.NeedRefreshOnBack = true;
            }

        }
    }
}
