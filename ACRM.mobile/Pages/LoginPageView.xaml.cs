using System;
using System.Collections.Generic;
using System.Reflection;
using ACRM.mobile.Logging;
using Xamarin.Forms;

namespace ACRM.mobile.Pages
{
    public partial class LoginPageView : ContentPage
    {

        public LoginPageView()
        {
            InitializeComponent();
            this.UserNameEntry.Completed += (object sender, EventArgs e) => { this.PasswordEntry.Focus(); };
            this.PasswordEntry.Completed += (object sender, EventArgs e) =>
            {
                this.LoginButton.Command.Execute(null);
            };
            // TODO: remove before release
            var assembly = typeof(LoginPageView).GetTypeInfo().Assembly;
            foreach (var res in assembly.GetManifestResourceNames())
            {
                System.Diagnostics.Debug.WriteLine("found resource: " + res);
            }
        }
    }
}
