using System;
using System.Threading.Tasks;
using ACRM.mobile.Utils;
using ACRM.mobile.Pages;
using Xamarin.Forms;
using ACRM.mobile.ViewModels.Base;
using System.Reflection;
using System.IO;
using Xamarin.Essentials;
using System.Diagnostics;
using ACRM.mobile.Services;
using ACRM.mobile.Services.Contracts;
using System.Globalization;
using ACRM.mobile.Domain.Application;

[assembly: ExportFont("materialdesignicons-webfont.ttf", Alias = "MaterialDesign")]
[assembly: ExportFont("GlyphIconsRegular.ttf", Alias = "GlyphIconsRegular")]
[assembly: ExportFont("GlyphIconsHalflingsRegular.ttf", Alias = "GlyphIconsHalflingsRegular")]
[assembly: ExportFont("GlyphIconsSocialRegular.ttf", Alias = "GlyphIconsSocialRegular")]
[assembly: ExportFont("GlyphIconsFiletypesRegular.ttf", Alias = "GlyphIconsFiletypesRegular")]


namespace ACRM.mobile
{
    public partial class App : Application
    {

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzcwNjE0QDMyMzAyZTMzMmUzMGpBNmJKUlVtVE00UDRXNHBKeUxzUUhwd0xRK3RuQW0wbENvKzJjREFQcm89");
            InitializeComponent();

            CopyResources();
            AppContainer.RegisterDependencies();
            AppContainer.Resolve<ConnectivityManager>();

            MainPage = new LoginPageView();
            if (Device.RuntimePlatform == Device.UWP)
            {
                InitNavigation();
            }
        }

        public async Task<bool> ProccessAppUrlAsync(string host, string parameters)
        {
            bool returnValue = true;
            Debug.WriteLine($"{"Application stared with " + host + "and parameters: " + parameters}");
            if (host != null)
            {
                if (host.Equals("configureserver"))
                {
                    CrmInstanceService crmInstanceService = (CrmInstanceService)AppContainer.Resolve(typeof(ICrmInstanceService));
                    if (crmInstanceService != null)
                    {
                        returnValue = await crmInstanceService.CreateOrUpdateCrmInstanceAsync(parameters);
                    }
                }
                else if (host.Equals("removeServer"))
                {
                    CrmInstanceService crmInstanceService = (CrmInstanceService)AppContainer.Resolve(typeof(ICrmInstanceService));
                    if (crmInstanceService != null)
                    {
                        returnValue = await crmInstanceService.RemoveCrmInstanceAsync(parameters);
                    }
                }
            }

            MessagingCenter.Send(new RefreshEvent { IsRefreshNeeded = true, Reason = "ServerRegistration" }, "RefreshEvent");
            return returnValue;
        }

        private Task InitNavigation()
        {
            var navigationController = AppContainer.Resolve<INavigationController>();
            return navigationController.InitializeAsync();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            if (Device.RuntimePlatform != Device.UWP)
            {
                await InitNavigation();
            }
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void CopyResources()
        {
#if DEBUG
            // Copy the Developer configured crm instances to user folder.
            string devCrmInstancesResourceName = "ACRM.mobile.Resources.DevCrmInstances.json";
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(devCrmInstancesResourceName))
            {
                try
                {
                    string appLocalsPath;
                    if (DeviceInfo.Platform == DevicePlatform.Unknown)
                    {
                        appLocalsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    }
                    else
                    {
                        appLocalsPath = FileSystem.AppDataDirectory;
                    }

                    var filePath = Path.Combine(appLocalsPath, "DevCrmInstances.json");
                    using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                } 
                catch (Exception ex)
                {

                }
            }
#endif

        }


    }
}
