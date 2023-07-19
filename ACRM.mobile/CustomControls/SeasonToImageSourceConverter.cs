using System;
using System.Globalization;
using System.Reflection;
using ACRM.mobile.Domain.Application;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class SeasonToImageSourceConverter : IValueConverter
    {
        public SeasonToImageSourceConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is SeasonType)
            {
                SeasonType season = (SeasonType)value;
                string CalenderBackground;

                if (season == SeasonType.UPDayPickerSeasonStyleAutumn)
                    CalenderBackground = "calendardashboard-panel-overlay-autumn.png";
                else if (season == SeasonType.UPDayPickerSeasonStyleSpring)
                    CalenderBackground = "calendardashboard-panel-overlay-spring.png";
                else if (season == SeasonType.UPDayPickerSeasonStyleSummer)
                    CalenderBackground = "calendardashboard-panel-overlay-summer.png";
                else
                    CalenderBackground = "calendardashboard-panel-overlay-winter.png";

                return ImageSource.FromResource(string.Format($"ACRM.mobile.Resources.SharedImages.{CalenderBackground}"), typeof(SeasonToImageSourceConverter).GetTypeInfo().Assembly);

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
