using System;
using System.IO;
using System.Threading.Tasks;
using ACRM.mobile.Services.Contracts;
using AsyncAwaitBestPractices;
using CoreGraphics;
using Foundation;
using UIKit;
using WebKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ACRM.mobile.iOS.Services.DocHelperService))]
namespace ACRM.mobile.iOS.Services
{
    public class DocHelperService: IDocHelperService
    {
        public DocHelperService()
        {
        }

        public void ConvertToPDF(string htmlContent, string docfilePath, string baseUrl, Func<string, Task> callbackFun)
        {
            WKWebViewConfiguration configuration = new WKWebViewConfiguration();
            WKWebView webView = new WKWebView(new CGRect(0, 0, 6.5 * 72, 9 * 72), configuration);
            webView.NavigationDelegate = new CustomDelegate(docfilePath, callbackFun);
            webView.UserInteractionEnabled = false;
            webView.BackgroundColor = UIColor.White;
            var uri = NSUrl.FromString(baseUrl);
            webView.LoadHtmlString(htmlContent, null);
        }

    }
    class CustomDelegate : WKNavigationDelegate
    {
        string _filename = null;
        Func<string, Task> _callbackFun = null;
        public CustomDelegate(string path, Func<string, Task> callbackFun)
        {
            this._filename = path;
            this._callbackFun = callbackFun;
        }
        override public void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            double height, width;
            int header, sidespace;
            width = 595.2;
            height = 841.8;
            header = 10;
            sidespace = 10;
            UIEdgeInsets pageMargins = new UIEdgeInsets(header, sidespace, header, sidespace);
            webView.ViewPrintFormatter.ContentInsets = pageMargins;
            UIPrintPageRenderer renderer = new UIPrintPageRenderer();
            renderer.AddPrintFormatter(webView.ViewPrintFormatter, 0);
            CGSize pageSize = new CGSize(width, height);
            CGRect printableRect = new CGRect(sidespace,
                              header,
                              pageSize.Width - (sidespace * 2),
                              pageSize.Height - (header * 2));
            CGRect paperRect = new CGRect(0, 0, width, height);
            renderer.SetValueForKey(NSValue.FromObject(paperRect), (NSString)"paperRect");
            renderer.SetValueForKey(NSValue.FromObject(printableRect), (NSString)"printableRect");
            NSData file = PrintToPDFWithRenderer(renderer, paperRect);
            File.WriteAllBytes(_filename, file.ToArray());
            if (_callbackFun != null)
            {
                _callbackFun(_filename).SafeFireAndForget<Exception>();
            }
        }

        private NSData PrintToPDFWithRenderer(UIPrintPageRenderer renderer, CGRect paperRect)
        {
            NSMutableData pdfData = new NSMutableData();
            UIGraphics.BeginPDFContext(pdfData, paperRect, null);
            renderer.PrepareForDrawingPages(new NSRange(0, renderer.NumberOfPages));
            CGRect bounds = UIGraphics.PDFContextBounds;
            for (int i = 0; i < renderer.NumberOfPages; i++)
            {
                UIGraphics.BeginPDFPage();
                renderer.DrawPage(i, paperRect);
            }
            UIGraphics.EndPDFContent();
            return pdfData;
        }
    }
}
