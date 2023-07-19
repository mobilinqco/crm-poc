using ACRM.mobile.Logging;
using ACRM.mobile.Utils;
using Syncfusion.ListView.XForms.UWP;
using Syncfusion.SfPicker.XForms.UWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace CRM.Client.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {

            InitializeComponent();

            SfPickerRenderer.Init();
            SfListViewRenderer.Init();
            Syncfusion.XForms.UWP.Buttons.SfSegmentedControlRenderer.Init();
            LoadApplication(new ACRM.mobile.App());
            InitNLog();
        }

        private void InitNLog()
        {
            var assembly = GetType().Assembly;
            var assemblyName = assembly.GetName().Name;

            AppContainer.Resolve<ILogService>().Initialize(assembly, assemblyName);
        }
    }
}
