using System;
using System.Globalization;
using System.Threading.Tasks;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Logging;
using ACRM.mobile.Utils;
using ACRM.mobile.Utils.Formatters;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class BaseViewCell : ViewCell
    {
        protected readonly ILogService _logService;
        protected ILocalizationController _localizationController;

        public BaseViewCell()
        {
            _logService = AppContainer.Resolve<ILogService>();
            _localizationController = AppContainer.Resolve<ILocalizationController>();
        }

        protected void PrepareLabel(ListDisplayField field, Label lbl)
        {
            // Bold and/or Italic
            if (field.Config.PresentationFieldAttributes.Bold && field.Config.PresentationFieldAttributes.Italic)
            {
                lbl.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;
            }
            else if (field.Config.PresentationFieldAttributes.Italic)
            {
                lbl.FontAttributes = FontAttributes.Italic;
            }
            else if (field.Config.PresentationFieldAttributes.Bold)
            {
                lbl.FontAttributes = FontAttributes.Bold;
            }

            // Label color
            if (!String.IsNullOrEmpty(field.Config.PresentationFieldAttributes.Color))
            {
                var convertor = new StringToColorConverter();
                lbl.TextColor = (Color)convertor.Convert(field.Config.PresentationFieldAttributes.Color, null, null, CultureInfo.CurrentCulture);
            }

            string fieldData = _localizationController.GetLocalizedValue(field);

            if (string.IsNullOrEmpty(fieldData))
            {
                fieldData = " ";
            }
            else
            {
                // Funny implementation: The NoLabel it has a reverse meaning in the List.
                if (field.Config.PresentationFieldAttributes.NoLabel && field.Config.PresentationFieldAttributes.Label().Length > 0)
                {
                    fieldData = field.Config.PresentationFieldAttributes.Label() + " " + fieldData;
                }
            }

            lbl.Text = fieldData;
        }

    }
}
