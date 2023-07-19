using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace ACRM.mobile.Pages
{
    public partial class SignaturePageView : PopupPage
    {
        public SignaturePageView()
        {
            InitializeComponent();
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            var vm = (SignaturePageViewModel)BindingContext; // Warning, the BindingContext View <-> ViewModel is already set

            vm.SignatureFromStream = async () =>
            {
                if (MainSignaturePad.Points.Count() > 0)
                {
                    using (var stream = await MainSignaturePad.GetImageStreamAsync(SignaturePad.Forms.SignatureImageFormat.Png))
                    {
                        return await ImageConverter.ReadFully(stream);
                    }
                }

                return await Task.Run(() => (byte[])null);
            };
        }
    }
}
