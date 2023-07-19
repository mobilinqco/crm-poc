using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class MvvmWebView : WebView
    {
        public static readonly BindableProperty NavigatingCommandProperty =
            BindableProperty.Create(nameof(NavigatingCommand), typeof(ICommand), typeof(MvvmWebView), null);

        public static readonly BindableProperty NavigatedCommandProperty =
            BindableProperty.Create(nameof(NavigatedCommand), typeof(ICommand), typeof(MvvmWebView), null);

        public MvvmWebView()
        {
            Navigating += (s, e) =>
            {
                if (NavigatingCommand?.CanExecute(e) ?? false)
                    NavigatingCommand.Execute(e);
            };

            Navigated += (s, e) =>
            {
                if (NavigatedCommand?.CanExecute(e) ?? false)
                    NavigatedCommand.Execute(e);
            };
        }

        public ICommand NavigatingCommand
        {
            get { return (ICommand)GetValue(NavigatingCommandProperty); }
            set { SetValue(NavigatingCommandProperty, value); }
        }

        public ICommand NavigatedCommand
        {
            get { return (ICommand)GetValue(NavigatedCommandProperty); }
            set { SetValue(NavigatedCommandProperty, value); }
        }
    }
}
