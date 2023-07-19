using System;
using ACRM.mobile.CustomControls.EditControls.Models;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls
{
    public class EditControlDeviceViewSelector: DataTemplateSelector
    {
        public DataTemplate PhoneTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate HiddenTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if(item is BaseEditControlModel ctrl && ctrl.Hidden)
            {
                return HiddenTemplate;
            }
            else if (Device.Idiom == TargetIdiom.Phone)
            {
                return PhoneTemplate;
            }
            else
            {
                return DefaultTemplate;
            }

        }
    }
}
