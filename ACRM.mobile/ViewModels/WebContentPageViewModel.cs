using System;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.ViewModels
{
    public class WebContentPageViewModel : NavigationBarBaseViewModel
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
        public WebContentPageViewModel()
        {
            IsBackButtonVisible = true;
            var loadingText = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicHeadlineLoading);
            var h1Style = "\"text-align:center;\"";
            var htmlContent =
                   @$"<!DOCTYPE html>
   <html>
  <head>
  </head>
  <body>
  <div>
    <h1 style={h1Style} >{loadingText}</h1>
    </div>
  </body >
  </html>";
            var WebContent = new WebContentParams
            {
                BaseUrl = _sessionContext.ResourcesFolder(),
                HTMLContent = htmlContent
            };
            Content =  FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource).Result;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData is UserAction)
            {
               Content = await FormBuilderExtensions.BuildWidget("WebContentView", navigationData, this, _cancellationTokenSource);
               PageTitle = ((UserAction)navigationData).ActionDisplayName;
            }
            await base.InitializeAsync(navigationData);
        }
    }
}
