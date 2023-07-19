using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class PdfViewerControlModel : UIWidget
    {
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

        private Stream _pdfDocumentStream;
        public Stream PdfDocumentStream
        {
            get => _pdfDocumentStream;

            set
            {
                _pdfDocumentStream = value;
                RaisePropertyChanged(() => PdfDocumentStream);
            }
        }

        public PdfViewerControlModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            if (widgetArgs is WebContentParams webContentParams)
            {
                WebContent = webContentParams;
            }
        }

        public async override ValueTask<bool> InitializeControl()
        {
            if(WebContent!=null && WebContent.IsURLSource)
            {
                var fileName = WebContent.BaseUrl;
                if (File.Exists(fileName))
                {
                    PdfDocumentStream = new FileStream(fileName, FileMode.Open,FileAccess.Read);

                }
            }

            return true;
        }
    }
}
