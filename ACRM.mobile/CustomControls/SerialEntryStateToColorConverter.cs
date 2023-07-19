using System;
using System.Globalization;
using ACRM.mobile.Domain.Application.SerialEntry;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class SerialEntryStateToColorConverter : IValueConverter
    {
        public SerialEntryStateToColorConverter()
        {
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is SerialEntryItemState state)
                {
                    switch (state)
                    {
                        case SerialEntryItemState.WithDestinationEntry:
                            return Color.Blue;
                        case SerialEntryItemState.InErrorState:
                            return Color.Red;
                        case SerialEntryItemState.SaveInprogress:
                            return Color.Yellow;

                    }
                }
            }
            return Color.Gray;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
