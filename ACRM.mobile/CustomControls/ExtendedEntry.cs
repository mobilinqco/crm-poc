using System;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public partial class ExtendedEntry: Entry
    {
        public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(ExtendedEntry), default(double));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(ExtendedEntry), Color.FromHex("#e1e2e5"));

        public double BorderWidth
        {
            get => (double)this.GetValue(BorderWidthProperty);
            set => this.SetValue(BorderWidthProperty, value);
        }

        public Color BorderColor
        {
            get => (Color)this.GetValue(BorderColorProperty);
            set => this.SetValue(BorderColorProperty, value);
        }
    }
}
