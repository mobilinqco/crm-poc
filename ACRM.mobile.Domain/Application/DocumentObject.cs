using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ACRM.mobile.Domain.OfflineSync;

namespace ACRM.mobile.Domain.Application
{
    public class DocumentObject : INotifyPropertyChanged
    {
        private static readonly List<string> ImageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".tiff", ".tif", ".bmp" };
        private static readonly List<string> VideoExtensions = new List<string> { ".mov", ".avi", ".mpeg", ".mpg", ".mpeg4" };

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Stream FileStream { get; set; }
        public string RecordIdentification { get; set; }
        public string LinkedRecordId { get; set; }
        public long Size { get; set; }
        public string Class { get; set; }
        public string MimeType { get; set; }
        public string DisplayText { get; set; }
        public string LocalPath { get; set; }
        public string IconName { get; set; }
        public Uri URL { get; set; }
        public Uri d1URL { get; set; }
        public DateTime ModificationDate { get; set; }
        public int FieldId { get; set; }

        public string SubHeader
        {
            get
            {
                string dateString = string.Empty;
                if (ModificationDate != null && ModificationDate != DateTime.MinValue)
                {
                    dateString = ModificationDate.ToShortDateString();
                }

                decimal sizeConverted = -1;
                string sizeConvStr = "KB";
                if(Size > 1000000)
                {
                    sizeConverted = (decimal)Size / 1000000;
                    sizeConvStr = "MB";
                }
                else
                {
                    sizeConverted = (decimal)Size / 1000;
                }

                var sunHeader = string.Empty;

                if(!string.IsNullOrEmpty(dateString) && sizeConverted > -1)
                {
                    sunHeader = $"{dateString}, {sizeConverted.ToString("0.#")} {sizeConvStr}";
                }
                else if(!string.IsNullOrEmpty(dateString))
                {
                    sunHeader = $"{ dateString}";
                }
                else
                {
                    sunHeader = $"{sizeConverted.ToString("0.#")} {sizeConvStr}";
                }

                return sunHeader;
            }
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LocalFileName
        {
            get
            {
                // Looks like iPad is saving the file using the FileName and not the record details
                if(!string.IsNullOrWhiteSpace(FileName))
                {
                    return FileName;
                }

                var d1Rrcord = RecordIdentification.Contains(".") ? RecordIdentification.ToLowerInvariant() : $"d1.{RecordIdentification}";
                FileInfo fi = new FileInfo(FileName);
                string extn = fi.Extension;
                return $"{d1Rrcord}{extn.ToLowerInvariant()}";
            }
        }

        private FileDownloadStatus _status = FileDownloadStatus.Online;
        public FileDownloadStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public DocumentObject()
        {
        }

        public DocumentObject(DocumentUpload docUpoloadObj)
        {

            FileName = docUpoloadObj.FileName;
            MimeType = docUpoloadObj.MimeType;
            Size = docUpoloadObj.Size;
            FieldId = docUpoloadObj.FieldId;

        }

        public bool IsImage()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                FileInfo fi = new FileInfo(FileName);
                string extn = fi.Extension;
                if (ImageExtensions.Contains(extn.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        private string Extension()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                FileInfo fi = new FileInfo(FileName);
                return fi.Extension;
                
            }
            return string.Empty;
        }

        public DocumentUpload GetDocumentUploadObject()
        {
            var UploadRequest = new DocumentUpload()
            {
                FileName = FileName,
                MimeType = MimeType,
                Size = Size,
                FieldId = FieldId,
                LocalFileName = $"{Math.Abs(Guid.NewGuid().GetHashCode())}{Extension()}"
            };
            return UploadRequest;
        }
    }


    public enum FileDownloadStatus
    {
        Online = 0,
        offline = 1,
        DownloadProgress = 2
    }
}
