using System;
using System.Globalization;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class FileDownloadIconConverter : IValueConverter
    {
        public FileDownloadIconConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var icon = MaterialDesignIcons.Download;
            if (value != null && value is FileDownloadStatus status)
            {
                if (status ==  FileDownloadStatus.offline)
                {
                    icon = MaterialDesignIcons.OpenInApp;
                }
                else if (status == FileDownloadStatus.DownloadProgress)
                {
                    icon = MaterialDesignIcons.ProgressDownload;
                }
            }
            return icon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
