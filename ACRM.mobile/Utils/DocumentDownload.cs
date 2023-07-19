using System;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ACRM.mobile.Utils
{
	public class DocumentDownload
	{
		public DocumentDownload()
		{
		}

        public static async Task Download(DocumentObject selectedDoc, IDocumentService contentService, ISessionContext sessionContext, CancellationToken token)
        {
            if (selectedDoc != null)
            {
                if (selectedDoc.Status == FileDownloadStatus.Online)
                {
                    selectedDoc.Status = FileDownloadStatus.DownloadProgress;
                    string filePath = await contentService.DownloadDocumentAsync(selectedDoc, token);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        selectedDoc.Status = FileDownloadStatus.offline;
                        var mime = MimeTypes.GetMimeType(filePath);
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(filePath, mime)
                        });
                    }
                    else
                    {
                        selectedDoc.Status = FileDownloadStatus.Online;
                    }
                }
                else if (selectedDoc.Status == FileDownloadStatus.offline)
                {
                    string filePath = sessionContext.DocumentPath(selectedDoc.LocalFileName);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var mime = MimeTypes.GetMimeType(filePath);
                        await Launcher.OpenAsync(new OpenFileRequest
                        {
                            File = new ReadOnlyFile(filePath, mime)
                        });
                    }
                }
            }
        }
    }
}

