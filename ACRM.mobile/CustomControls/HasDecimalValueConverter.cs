using System;
using System.Globalization;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class HasDecimalValueConverter : IValueConverter
    {
        public HasDecimalValueConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;
            if (value != null && value is decimal decvalue)
            {
                if (decvalue > 0)
                {
                    result = true;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
