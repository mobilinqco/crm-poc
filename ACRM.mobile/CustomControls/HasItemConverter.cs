using System;
using System.Collections.Generic;
using System.Globalization;
using ACRM.mobile.Domain.Application;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class HasItemConverter : IValueConverter
    {
        public HasItemConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var items = value as ObservableRangeCollection<ListDisplayRow>;

                if (items != null && items.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
