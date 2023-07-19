using System;
using ACRM.mobile.CustomControls.EditControls.Models;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls
{
    public class EditControlTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BoolControlTemplate { get; set; }
        public DataTemplate CatalogControlTemplate { get; set; }
        public DataTemplate DateTimeControlTemplate { get; set; }
        public DataTemplate RecordSelectorControlTemplate { get; set; }
        public DataTemplate TextControlTemplate { get; set; }
        public DataTemplate TextEditorControlTemplate { get; set; }
        public DataTemplate NotSupportedControlTemplate { get; set; }
        public DataTemplate MultiSelectInputTemplate { get; set; }
        public DataTemplate ImageInputTemplate { get; set; }


        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is TextControlModel)
            {
                return TextControlTemplate;
            }
            else if (item is BoolControlModel)
            {
                return BoolControlTemplate;
            }
            else if (item is MultiSelectInputModel)
            {
                return MultiSelectInputTemplate;
            }
            else if (item is DateTimeControlModel)
            {
                return DateTimeControlTemplate;
            }
            else if (item is RecordSelectorControlModel || item is RecordSelectorCatalogControlModel)
            {
                return RecordSelectorControlTemplate;
            }
            else if (item is CatalogControlModel)
            {
                return CatalogControlTemplate;
            }
            else if (item is TextEditorControlModel)
            {
                return TextEditorControlTemplate;
            }
            else if (item is ImageControlModel)
            {
                return ImageInputTemplate;
            }
            else
            {
                return NotSupportedControlTemplate;
            }
        }
    }
}
