using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class StringToColorConverter: IValueConverter
    {
        static Dictionary<string, Color> ColorMap = new Dictionary<string, Color> {
                {"red", Color.Red},
                {"blue", Color.Blue},
                {"green", Color.Green},
                {"black", Color.Black},
                {"white", Color.White},
                {"brown", Color.Brown},
                {"gray", Color.Gray},
                {"lightgray", Color.LightGray},
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.ToString().StartsWith("#"))
                {
                    return Color.FromHex(value.ToString());
                }

                if (value.ToString().Contains(";"))
                {
                    string[] components = value.ToString().Split(';');
                    if (components.Length == 4)
                    {
                        try
                        {
                            return Color.FromHex(string.Format("#{0}{1}{2}{3}",
                                ((int)(float.Parse(components[0]) * 255)).ToString("X2"),
                                ((int)(float.Parse(components[1]) * 255)).ToString("X2"),
                                ((int)(float.Parse(components[2]) * 255)).ToString("X2"),
                                ((int)(float.Parse(components[3]) * 255)).ToString("X2")));
                        }
                        catch
                        {
                            return Color.Black;
                        }
                    }
                }

                if (ColorMap.ContainsKey(value.ToString().ToLower()))
                {
                    return ColorMap[value.ToString().ToLower()];
                }
            }
            return Color.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
