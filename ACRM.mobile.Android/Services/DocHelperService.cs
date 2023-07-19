using System;
using System.IO;
using System.Threading.Tasks;
using ACRM.mobile.Services.Contracts;
using Android.Graphics.Pdf;
using Android.Webkit;
using AsyncAwaitBestPractices;
using Java.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(ACRM.mobile.Droid.Services.DocHelperService))]
namespace ACRM.mobile.Droid.Services
{
    public class DocHelperService : IDocHelperService
    {
        public DocHelperService()
        {
        }

        public void ConvertToPDF(string htmlContent, string docfilePath, string baseUrl, Func<string, Task> callbackFun)
        {
            var file = new Java.IO.File(docfilePath);

            var webpage = new Android.Webkit.WebView(Android.App.Application.Context);

            int width = 2102;
            int height = 2973;

            webpage.Layout(0, 0, width, height);
            webpage.LoadDataWithBaseURL("", htmlContent, "text/html", "UTF-8", null);
            webpage.SetWebViewClient(new WebViewCallBack(file.ToString(), callbackFun));
        }

    }
}
class WebViewCallBack : WebViewClient
{

    string fileNameWithPath = null;
    Func<string, Task> _callbackFun = null;

    public WebViewCallBack(string path, Func<string, Task> callbackFun)
    {
        this.fileNameWithPath = path;
        this._callbackFun = callbackFun;
    }


    public override void OnPageFinished(Android.Webkit.WebView myWebview, string url)
    {
        PdfDocument document = new PdfDocument();
        PdfDocument.Page page = document.StartPage(new PdfDocument.PageInfo.Builder(2120, 3000, 1).Create());

        myWebview.Draw(page.Canvas);
        document.FinishPage(page);
        Stream filestream = new MemoryStream();
        FileOutputStream fos = new Java.IO.FileOutputStream(fileNameWithPath, false); ;
        try
        {
            document.WriteTo(filestream);
            fos.Write(((MemoryStream)filestream).ToArray(), 0, (int)filestream.Length);
            fos.Close();
            if (_callbackFun != null)
            {
                _callbackFun(fileNameWithPath).SafeFireAndForget<Exception>();
            }
        }
        catch
        {
        }
    }
}
