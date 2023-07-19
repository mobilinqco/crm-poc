using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class WebControlPanelModel : UIPanelWidget
    {
        private object _webSource;
        public object WebViewSource
        {
            get => _webSource;

            set
            {
                _webSource = value;
                RaisePropertyChanged(() => WebViewSource);
            }
        }
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }
        public WebControlPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if (widgetArgs is PanelData)
            {
                Data = widgetArgs as PanelData;
            }
        }

        public async override ValueTask<bool> InitializeControl()
        {
            string url = string.Empty;
            if (Data != null)
            {
                string[] typeParts = Data.PanelTypeKey.Split('_');
                var contentService = AppContainer.Resolve<IConfigurationService>();
                if (typeParts.Length > 1)
                {
                    var menuName = typeParts[1];
                    var ViewRef = await contentService.GetViewForMenu(menuName, _cancellationTokenSource.Token);
                    url = ViewRef?.GetArgumentValue("Url");
                }

                Title = Data.Label;
            }

            if (!string.IsNullOrEmpty(url))
            {
                var tokenProcessor = AppContainer.Resolve<ITokenProcessor>();
                WebViewSource = tokenProcessor.ProcessURL(url);
            }

            return true;
        }
    }
}
