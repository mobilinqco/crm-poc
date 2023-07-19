using System;
using System.Globalization;
using ACRM.mobile.Domain.Application;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class UserActionToImageSourceConverter: IValueConverter
    {
        public UserActionToImageSourceConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                UserAction ua = (UserAction)value;

                if (!string.IsNullOrEmpty(ua.DisplayImageName))
                {
                    
                    return ImageSource.FromFile(ua.DisplayImageName);
                }
                else
                {
                    ResourceDictionary StaticResources = Application.Current.Resources;

                    return new FontImageSource
                    {
                        FontFamily = (OnPlatform<string>)StaticResources["HeaderButtonImagesFont"],
                        Glyph = ua.DisplayGlyphImageText,
                        Color = (Color)StaticResources["HeaderButtonTextColor"]
                    };
                }

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
