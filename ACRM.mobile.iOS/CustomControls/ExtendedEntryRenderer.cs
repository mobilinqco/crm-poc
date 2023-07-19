using System;
using System.ComponentModel;
using ACRM.mobile.CustomControls;
using ACRM.mobile.iOS.CustomControls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtendedEntry), typeof(ExtendedEntryRenderer))]

namespace ACRM.mobile.iOS.CustomControls
{
    public class ExtendedEntryRenderer: EntryRenderer
    {
        public ExtendedEntryRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (e.NewElement is ExtendedEntry customEntry)
                {
                    if (customEntry.BorderWidth == 0)
                    {
                        Control.BorderStyle = UITextBorderStyle.None;
                    }
                }

                var entry = Control as UITextField;
                entry.ClearButtonMode = UITextFieldViewMode.WhileEditing;

            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            ExtendedEntry customEntry = (ExtendedEntry)Element;

            if (e.PropertyName.Equals(customEntry.BorderWidth))
            {
                if (customEntry.BorderWidth == 0)
                {
                    Control.BorderStyle = UITextBorderStyle.None;
                }
            }
        }
    }
}
