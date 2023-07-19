using ACRM.mobile.Logging;
using ACRM.mobile.Utils;
using Syncfusion.ListView.XForms.UWP;
using Syncfusion.SfAutoComplete.XForms.UWP;
using Syncfusion.SfPdfViewer.XForms.UWP;
using Syncfusion.SfPicker.XForms.UWP;
using Syncfusion.SfRangeSlider.XForms.UWP;
using Syncfusion.XForms.Pickers.UWP;
using Syncfusion.XForms.UWP.Accordion;
using Syncfusion.XForms.UWP.Expander;
using Syncfusion.XForms.UWP.TabView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Essentials;
using Popup = Rg.Plugins.Popup.Popup;

namespace CRM.Client.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            StartupActions(e);
        }

       

        private static List<System.Reflection.Assembly> InitAsssemblies()
        {
            List<System.Reflection.Assembly> assembliesToInclude = new List<System.Reflection.Assembly>();
            // Add all the renderer assemblies your app uses 
            assembliesToInclude.Add(typeof(Syncfusion.XForms.UWP.ComboBox.SfComboBoxRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(Syncfusion.SfSchedule.XForms.UWP.SfScheduleRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfPickerRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(Syncfusion.XForms.UWP.Buttons.SfSegmentedControlRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(Syncfusion.SfCalendar.XForms.UWP.SfCalendarRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfDatePickerRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfTimePickerRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfAutoCompleteRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfListViewRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfTabViewRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfExpanderRenderer).GetTypeInfo().Assembly);

            assembliesToInclude.Add(typeof(SfPdfDocumentViewRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfRangeSliderRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(SfAccordionRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(Xamarin.Forms.GoogleMaps.UWP.MapRenderer).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(Popup).GetTypeInfo().Assembly);
            assembliesToInclude.Add(typeof(ACRM.mobile.App).GetTypeInfo().Assembly);

            return assembliesToInclude;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected async override void OnActivated(IActivatedEventArgs e)
        {
            if (e.Kind == ActivationKind.Protocol)
            {
               
                StartupActions(e);
                var args = e as ProtocolActivatedEventArgs;
                if (args.Uri.PathAndQuery != string.Empty)
                {
                    await (Xamarin.Forms.Application.Current as ACRM.mobile.App).ProccessAppUrlAsync(args.Uri.Host, args.Uri.Query).ConfigureAwait(false);
                }
            }
        }

        private void StartupActions(IActivatedEventArgs args)
        {
            UnhandledException += OnUnhandledException;

            var frame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (frame == null)
            {
                var assemblies = InitAsssemblies();
                Popup.Init();

                Xamarin.Forms.Forms.Init(args, assemblies);
                Xamarin.FormsGoogleMaps.Init("AtTd8zlUjTyZBFXsaaeVCItRUDv6dB6WnlGsIkl9TmiwDVjPnK5-nAyHMFVCJOTq");

                // Create a Frame to act as the navigation context and navigate to the first page
                frame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                frame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = frame;
            }

            if (frame.Content == null)
            {
                frame.Navigate(typeof(MainPage), args);
            }

            SetScreenOrientation();

            Window.Current.Activate();
        }

        private static void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {

            //Messenger.Default.Send(new ToastrMessage
            //{
            //    MessageText = e.Exception.ToString(),
            //    DetailedMessage = e.Message
            //});

            e.Handled = true;
        }

        private void SetScreenOrientation()
        {
            if(DeviceInfo.Idiom != DeviceIdiom.Tablet)
            {
                Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = 
                    Windows.Graphics.Display.DisplayOrientations.Landscape;
            }
        }
    }
}
