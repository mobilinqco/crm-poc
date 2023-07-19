using System;
using System.Globalization;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.Utils
{
    public static class UIElementExtensions
    {
        public static void ClearCellWrapper(this Grid cellWrapper)
        {
            cellWrapper.Children.Clear();
            cellWrapper.ColumnDefinitions.Clear();
            cellWrapper.RowDefinitions.Clear();
        }

       

    }
}
