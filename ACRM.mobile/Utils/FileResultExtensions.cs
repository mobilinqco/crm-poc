using System;
using ACRM.mobile.Domain.Application;
using Xamarin.Essentials;

namespace ACRM.mobile.Utils
{
    public static class FileResultExtensions
    {
        public static DocumentObject GetDocumentObject(this FileResult fileresult)
        {
            if(fileresult== null)
            {
                return null;
            }

            var mime = MimeTypes.GetMimeType(fileresult.FullPath);

            var doc = new DocumentObject
            {
                LocalPath = fileresult.FullPath,
                MimeType = mime,
                FileName = fileresult.FileName
            };
            return doc;
        }
    }
}
