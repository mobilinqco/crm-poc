using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.ViewModels.Base
{
    public interface INavigationController
    {
        BaseViewModel PreviousPageViewModel { get; }

        Task InitializeAsync();

        Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel;

        Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : BaseViewModel;

        Task RemoveLastFromBackStackAsync();

        Task RemoveBackStackAsync();

        Task BackAsync(bool refreshParent = false);
        Task PopToRoot();

        Task Logout();

        Task DisplayPopupAsync<TViewModel>(object parameter = null) where TViewModel : BaseViewModel;
        Task PopAllPopupAsync(object navigationData);
        Task PopAllPopupAsync();
        Task PopPopupAsync();
        Task NavigateAsyncForAction(UserAction userAction, CancellationToken cancellationToken);
        Task NavigateAsyncForActionAndRemoveParent(UserAction userAction, CancellationToken cancellationToken);
        Task SimpleNavigateWithAction(UserAction userAction);
        Task RefreshPopupDisplayPage();

        bool IsInPopupStack<TViewModel>();
        Task ForceRefreshForParent();
    }
}
