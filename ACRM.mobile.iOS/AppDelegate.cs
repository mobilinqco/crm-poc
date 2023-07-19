using System.Threading.Tasks;
using ACRM.mobile.Logging;
using ACRM.mobile.Utils;
using CoreLocation;
using Foundation;
using Syncfusion.SfAutoComplete.XForms.iOS;
using Syncfusion.XForms.iOS.ComboBox;
using Syncfusion.XForms.iOS.TextInputLayout;
using Syncfusion.XForms.Pickers.iOS;
using Syncfusion.SfSchedule.XForms.iOS;
using Syncfusion.XForms.iOS.Buttons;
using UIKit;
using Syncfusion.ListView.XForms.iOS;
using Syncfusion.SfPicker.XForms.iOS;
using Syncfusion.SfCalendar.XForms.iOS;
using Syncfusion.XForms.iOS.TabView;
using Syncfusion.SfDataGrid.XForms.iOS;
using Syncfusion.XForms.iOS.Accordion;
using Syncfusion.XForms.iOS.Expander;
using Syncfusion.SfRangeSlider.XForms.iOS;
using Syncfusion.SfPdfViewer.XForms.iOS;
using ObjCRuntime;
using Xamarin.Essentials;
using System.Globalization;
using System.Linq;

namespace ACRM.mobile.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
#if ENABLE_TEST_CLOUD
            Xamarin.Calabash.Start();
#endif
            Rg.Plugins.Popup.Popup.Init();

            global::Xamarin.Forms.Forms.Init();
            Xamarin.FormsGoogleMaps.Init("AIzaSyBemPJKDHTYM46cmdFrbxdw16E8OvQhQZc");
            LoadApplication(new App());
            
            UIApplication.SharedApplication.IdleTimerDisabled = true;
            SyncfusionInit();

            InitNLog();

            // Here we have a problem for cases when the devices language is set to a languge different that
            // the region (example: english language, region: romania, this will allways return the en-en and will create problems with number and date formatting).
            // For this reason i have added a kind of hack to set a fake culture.

            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(NSLocale.CurrentLocale.LocaleIdentifier.Replace('_', '-'));
            }
            catch
            {
                try
                {
                    var cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures)
                                  .Where(c => c.Name.EndsWith($"-{NSLocale.CurrentLocale.CountryCode}"));
                    if (cultureInfos != null && cultureInfos.Count() > 0)
                    {
                        CultureInfo.CurrentUICulture = cultureInfos.First();
                    }
                }
                catch
                {
                    // we can't change the settings :(
                }
            }

            CLLocationManager locationManager = new CLLocationManager();
            locationManager.RequestWhenInUseAuthorization();
            return base.FinishedLaunching(app, options);
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, [Transient] UIWindow forWindow)
        {
            if(DeviceInfo.Idiom != DeviceIdiom.Tablet)
            {
                return UIInterfaceOrientationMask.Portrait;
            }
            return UIInterfaceOrientationMask.All;
        }

        private void InitNLog()
        {
            var assembly = GetType().Assembly;
            var assemblyName = assembly.GetName().Name;

            AppContainer.Resolve<ILogService>().Initialize(assembly, assemblyName);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            Task.Run(async () =>
            {
                await ((App)Xamarin.Forms.Application.Current).ProccessAppUrlAsync(url.Host, url.Query).ConfigureAwait(false);
            });
            return true;
        }

        private void SyncfusionInit()
        {
            SfScheduleRenderer.Init();
            SfSegmentedControlRenderer.Init();
            SfPickerRenderer.Init();
            SfCalendarRenderer.Init();
            SfDatePickerRenderer.Init();
            SfTimePickerRenderer.Init();
            SfTextInputLayoutRenderer.Init();
            SfComboBoxRenderer.Init();
            SfAutoCompleteRenderer.Init();
            SfListViewRenderer.Init();
            SfTabViewRenderer.Init();
            SfDataGridRenderer.Init();
            SfAccordionRenderer.Init();
            SfExpanderRenderer.Init();
            SfRangeSliderRenderer.Init();
            SfPdfDocumentViewRenderer.Init();
            SfRangeSliderRenderer.Init();
        }
    }
}
