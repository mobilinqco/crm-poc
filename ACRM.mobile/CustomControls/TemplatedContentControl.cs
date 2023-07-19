using System;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class TemplatedContentControl : ContentView
    {
        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(TemplatedContentControl), propertyChanged: ItemTemplateChanged);

        public object Item
        {
            get => (object)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }
        public static readonly BindableProperty ItemProperty = BindableProperty.Create(nameof(Item), typeof(object), typeof(TemplatedContentControl), null, propertyChanged: SourceChanged);

        private static void ItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as TemplatedContentControl;
            control.BuildItem();
        }

        private static void SourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as TemplatedContentControl;
            control.BuildItem();
        }

        public bool HideOnNullContent { get; set; } = false;

        protected void BuildItem()
        {
            if (Item == null)
            {
                Content = null;
                return;
            }

            //Create the content
            try
            {
                Content = CreateTemplateForItem(Item, ItemTemplate, false);
            }
            catch
            {
                Content = null;
            }
            finally
            {
                if (HideOnNullContent)
                    IsVisible = Content != null;
            }
        }

        public static View CreateTemplateForItem(object item, DataTemplate itemTemplate, bool createDefaultIfNoTemplate = true)
        {
            //Check to see if we have a template selector or just a template
            var templateToUse = itemTemplate is DataTemplateSelector templateSelector ? templateSelector.SelectTemplate(item, null) : itemTemplate;

            //If we still don't have a template, create a label
            if (templateToUse == null)
                return createDefaultIfNoTemplate ? new Label() { Text = item.ToString() } : null;

            //Create the content
            //If a view wasn't created, we can't use it, exit
            if (!(templateToUse.CreateContent() is View view)) return new Label() { Text = item.ToString() }; ;

            //Set the binding
            view.BindingContext = item;

            return view;
        }
    }
}
