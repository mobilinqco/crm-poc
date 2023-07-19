using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class CoreReportViewModel : UIWidget
    {
        private readonly UserAction _userAction;
        private readonly IReportingService _contentService;
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

        public CoreReportViewModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _contentService = AppContainer.Resolve<IReportingService>();
            if (widgetArgs is UserAction)
            {
                _userAction = widgetArgs as UserAction;
                _contentService.SetSourceAction(_userAction);
            }
        }

        public async override ValueTask<bool> InitializeControl()
        {
            await setLoadingHTML();
            await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
            string reportUrl = await _contentService.GetReportURL(_cancellationTokenSource.Token);
            if(!string.IsNullOrEmpty(reportUrl))
            {
                var WebContent = new WebContentParams
                {
                    BaseUrl = reportUrl,
                    IsURLSource = true,
                    CookieContainer = _sessionContext.GetCookieContainer()

                };
                Content = await FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource);
                return true;
            }
            else
            {
                var h1Style = "\"text-align:center;\"";
                var htmlContent =
                       @$"<!DOCTYPE html>
   <html>
  <head>
  </head>
  <body>
  <div>
    <h1 style={h1Style} >Faild loading Core Report.</h1>
    </div>
  </body >
  </html>";
                var WebContent = new WebContentParams
                {
                    BaseUrl = _sessionContext.ResourcesFolder(),
                    HTMLContent = htmlContent
                };
                Content = await FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource);
                return false;
            }
            
        }

        private async Task setLoadingHTML()
        {
            var loadingText = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                           LocalizationKeys.KeyBasicMessageLoadingReport);
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
            Content = await FormBuilderExtensions.BuildWidget("WebContentView", WebContent, this, _cancellationTokenSource);
        }
    }
}
