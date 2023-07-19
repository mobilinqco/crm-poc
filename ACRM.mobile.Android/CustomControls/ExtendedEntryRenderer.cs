using System;
using System.ComponentModel;
using ACRM.mobile.CustomControls;
using ACRM.mobile.Droid.CustomControls;
using Android.Content;
using Android.Graphics.Drawables;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedEntry), typeof(ExtendedEntryRenderer ))]

namespace ACRM.mobile.Droid.CustomControls
{
    public class ExtendedEntryRenderer : EntryRenderer
    {
        public ExtendedEntryRenderer (Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (e.NewElement is ExtendedEntry customEntry)
                {
                    if (customEntry.BorderWidth == 0)
                    {
                        Control.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
                        Control.Background = new ColorDrawable(Android.Graphics.Color.Transparent);
                    }

                    customEntry.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
                }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            ExtendedEntry customEntry = (ExtendedEntry)this.Element;

            if (e.PropertyName.Equals(customEntry.BorderWidth))
            {
                if (customEntry.BorderWidth == 0)
                {
                    Control.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
                    Control.Background = new ColorDrawable(Android.Graphics.Color.Transparent);
                }
            }
        }
    }
}
