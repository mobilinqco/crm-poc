using System;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.ViewModels
{
    public class ClientReportPageViewModel : NavigationBarBaseViewModel
    {
        private UIWidget _content;
        public UIWidget Content
        {
            get => _content;
            set
            {
                _content = value;
                RaisePropertyChanged(() => Content);
            }
        }
        public ClientReportPageViewModel()
        {
            IsBackButtonVisible = true;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData is UserAction selectedUserAction)
            {
                PageTitle = selectedUserAction.ActionDisplayName;
                Content = await FormBuilderExtensions.BuildWidget(selectedUserAction.ViewReference.Name, selectedUserAction, this, _cancellationTokenSource);
            }
            await base.InitializeAsync(navigationData);
        }

    }
}
