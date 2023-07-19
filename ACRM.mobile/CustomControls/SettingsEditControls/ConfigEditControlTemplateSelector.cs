using System;
using ACRM.mobile.CustomControls.SettingsEditControls.Models;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.SettingsEditControls
{
	public class ConfigEditControlTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ComboboxControlTemplate { get; set; }
        public DataTemplate CheckBoxTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate NotSupportedControlTemplate { get; set; }
        

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ComboboxConfigControlModel)
            {
                return ComboboxControlTemplate;
            }
            else if (item is CheckboxConfigControlModel)
            {
                return CheckBoxTemplate;
            }
            else
            {
                return TextTemplate;
            }
        }
    }

}

