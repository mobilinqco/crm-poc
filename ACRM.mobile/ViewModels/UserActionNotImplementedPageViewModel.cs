using System;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.ViewModels
{
    public class UserActionNotImplementedPageViewModel: NavigationBarBaseViewModel
    {
        public UserActionNotImplementedPageViewModel()
        {
            IsBackButtonVisible = true;
            PageTitle = "UserAction error";
            ErrorMessageText = "";
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsBackButtonVisible = true;
            IsLoading = true;
            if (navigationData is UserAction userAction)
            {
                IsBusy = true;
                ErrorMessageText = UserActionNotSupportedErrorMessage(userAction);
                IsBusy = false;
            }
            IsLoading = false;
            await base.InitializeAsync(navigationData);
        }
    }
}
