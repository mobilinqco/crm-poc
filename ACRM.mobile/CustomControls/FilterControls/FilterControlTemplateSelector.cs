using System;
using ACRM.mobile.CustomControls.FilterControls.Models;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.FilterControls
{
    public class FilterControlTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextControlTemplate { get; set; }
        public DataTemplate CatalogControlTemplate { get; set; }
        public DataTemplate BoolControlTemplate { get; set; }
        public DataTemplate DateControlTemplate { get; set; }
        public DataTemplate DateRangeControlTemplate { get; set; }
        public DataTemplate TextRangeControlTemplate { get; set; }
        public DataTemplate HierarchicalCatalogControlTemplate { get; set; }

        public FilterControlTemplateSelector()
        {
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is TextFilterControlModel)
            {
                return TextControlTemplate;
            }
            else if (item is HierarchicalCatalogFilterControlModel)
            {
                return HierarchicalCatalogControlTemplate;
            }
            else if (item is RepFilterControlModel)
            {
                return CatalogControlTemplate;
            }
            else if (item is CatalogFilterControlModel)
            {
                return CatalogControlTemplate;
            }
            else if (item is BoolFilterControlModel)
            {
                return BoolControlTemplate;
            }
            else if (item is DateFilterControlModel)
            {
                return DateControlTemplate;
            }
            else if (item is DateRangeFilterControlModel)
            {
                return DateRangeControlTemplate;
            }
            else if (item is TextRangeFilterControlModel)
            {
                return TextRangeControlTemplate;
            }
            
            return TextControlTemplate;
        }
    }
}
