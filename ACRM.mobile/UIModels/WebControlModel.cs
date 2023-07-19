using System;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;
using System.Net;
using System.Threading;

namespace ACRM.mobile.UIModels
{
    public class WebControlModel : UIWidget
    {
        private readonly UserAction _userAction;
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
        private WebContentParams _webContent;
        public WebContentParams WebContent
        {
            get => _webContent;

            set
            {
                _webContent = value;
                RaisePropertyChanged(() => WebContent);
            }
        }
        private CookieContainer _webCookieContainer;
        public CookieContainer WebCookieContainer
        {
            get => _webCookieContainer;

            set
            {
                _webCookieContainer = value;
                RaisePropertyChanged(() => WebCookieContainer);
            }
        }
        
        public WebControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if (widgetArgs is UserAction)
            {
                _userAction = widgetArgs as UserAction;
            }
            else if (widgetArgs is WebContentParams webContentParams)
            {
                WebContent = webContentParams;
            }
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if (_userAction != null)
            {
                var url = _userAction?.ViewReference?.GetArgumentValue("Url");
                if (!string.IsNullOrEmpty(url))
                {
                    var tokenProcessor = AppContainer.Resolve<ITokenProcessor>();
                    WebViewSource = tokenProcessor.ProcessURL(url);
                }
            }
            else if (WebContent!=null)
            {
                if (WebContent.IsURLSource)
                {
                    var source = new UrlWebViewSource();
                    source.Url = WebContent.BaseUrl;
                    WebViewSource = source;
                    WebCookieContainer = WebContent.CookieContainer;
                }
                else
                {
                    var source = new HtmlWebViewSource();
                    source.Html = WebContent.HTMLContent;
                    if (!string.IsNullOrWhiteSpace(WebContent.BaseUrl))
                    {
                        source.BaseUrl = WebContent.BaseUrl;
                    }
                    WebViewSource = source;
                }
            }
            return true;
        }
    }
}
