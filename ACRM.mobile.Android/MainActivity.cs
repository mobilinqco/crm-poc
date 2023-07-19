using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Acr.UserDialogs;
using System.Threading.Tasks;
using ACRM.mobile.Utils;
using ACRM.mobile.Logging;
using static Android.OS.PowerManager;
using Android.Content;
using Android;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace ACRM.mobile.Droid
{
    [Activity(Label = "CRM.Client", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter (new[] { "android.intent.action.VIEW" },
        Categories = new[]
        {
            "android.intent.category.DEFAULT",
            "android.intent.category.BROWSABLE"
        },
        DataSchemes = new[] { "crmclient" },
        DataHost = "*"
    )]
     
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private WakeLock wakeLock;
        const int RequestLocationId = 0;

        readonly string[] LocationPermissions =
        {
        Manifest.Permission.AccessCoarseLocation,
        Manifest.Permission.AccessFineLocation
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.FormsGoogleMaps.Init(this, savedInstanceState);
            UserDialogs.Init(this);

            LoadApplication(new App());
            SetScreenOrientation();
            InitNLog();

            var data = Intent.Data;

            if (data != null)
            {
                Task.Run(async () =>
                {
                    await ((App)Xamarin.Forms.Application.Current).ProccessAppUrlAsync(data.Host, data.EncodedQuery).ConfigureAwait(false);
                });
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            PowerManager powerManager = (PowerManager)this.GetSystemService(Context.PowerService);
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.Full, "My Lock");
            wakeLock.Acquire();

        }

        protected override void OnPause()
        {
            wakeLock.Release();
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            //if (wakeLock != null)
            //{
            //    wakeLock.Release();
            //}

            base.OnDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    RequestPermissions(LocationPermissions, RequestLocationId);
                }
                else
                {
                    //permision granted
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void SetScreenOrientation()
        {
            if (DeviceInfo.Idiom != DeviceIdiom.Tablet)
            {
                RequestedOrientation = ScreenOrientation.Portrait;
            }
        }

        private void InitNLog()
        {
            var assembly = GetType().Assembly;
            var assemblyName = assembly.GetName().Name;

            AppContainer.Resolve<ILogService>().Initialize(assembly, assemblyName);
        }
    }
}
