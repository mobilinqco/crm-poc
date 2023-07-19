using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class UserActionSchuttle: IUserActionSchuttle
    {
        protected readonly IDialogContorller _dialogContorller;
        protected readonly INavigationController _navigationController;

        public UserActionSchuttle(IDialogContorller dialogContorller,
            INavigationController navigationController)
        {
            _dialogContorller = dialogContorller;
            _navigationController = navigationController;
        }


        public async Task Carry(UserAction action, string recordId, CancellationToken cancellationToken)
        {
            if (action.ActionType == UserActionType.OpenURL || (action.ViewReference != null && action.ViewReference.IsOpenUrlAction()))
            {
                IOpenUrlService openUrlService = (IOpenUrlService)AppContainer.Resolve(typeof(IOpenUrlService));
                await openUrlService.PrepareContentAsync(action, recordId, cancellationToken);
                string urlString = openUrlService.UrlString();

                if (string.IsNullOrWhiteSpace(urlString))
                {
                    await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupBasic,
                        LocalizationKeys.KeyBasicErrorTitleCouldNotOpenUrl,
                        LocalizationKeys.KeyBasicErrorMessageCouldNotOpenUrl, new[] { urlString });
                }
                else
                {
                    if(openUrlService.IsCustomUrl())
                    {
                        if(await Launcher.CanOpenAsync(new Uri(urlString)))
                        {
                            await Launcher.OpenAsync(new Uri(urlString));
                        }
                        else
                        {
                            await _dialogContorller.ShowErrorMessageAsync(LocalizationKeys.TextGroupBasic,
                                LocalizationKeys.KeyBasicErrorTitleCouldNotOpenUrl,
                                LocalizationKeys.KeyBasicErrorMessageCouldNotOpenUrl, new[] { urlString });
                        }
                    }
                    else
                    {
                        await Browser.OpenAsync(new Uri(urlString), BrowserLaunchMode.SystemPreferred);
                    }
                    
                    if (openUrlService.PopToPrevious())
                    {
                        await _navigationController.BackAsync();
                    }
                }
            }
            else
            {
                await _navigationController.NavigateAsyncForAction(action, cancellationToken);
            }
        }

    }
}
