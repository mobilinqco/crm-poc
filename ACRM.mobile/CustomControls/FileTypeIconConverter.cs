using System;
using System.Globalization;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class FileTypeIconConverter : IValueConverter
    {
        public FileTypeIconConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var icon = MaterialDesignIcons.File;
            if (value != null && value is string title)
            {

                if (title.ToLower().EndsWith("pdf"))
                {
                    icon = MaterialDesignIcons.FilePdf;
                }
                else if (title.ToLower().EndsWith("doc"))
                {
                    icon = MaterialDesignIcons.FileWord;
                }
                else if (title.ToLower().EndsWith("jpeg")
                    || title.ToLower().EndsWith("jpg")
                    || title.ToLower().EndsWith("png"))
                {
                    icon = MaterialDesignIcons.FileImage;
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
