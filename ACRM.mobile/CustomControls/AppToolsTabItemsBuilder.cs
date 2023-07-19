using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.FullSync;
using ACRM.mobile.Localization;
using ACRM.mobile.UIModels;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using ACRM.mobile.Views.Widgets;
using Syncfusion.XForms.TabView;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class AppToolsTabItemsBuilder
    {
        protected readonly ILocalizationController _localizationController;

        private string GlobalTabTitle = "GLOBAL";
        private string SyncTabTitle = "SYNC";
        private string SystemTabTitle = "SYSTEM";
        private string LogTabTitle = "LOG";
        private string LogoutTabTitle = "LOGOUT";

        private string MaterialDesignFontFamily = "MaterialDesign";

        public AppToolsTabItemsBuilder()
        {
            _localizationController = AppContainer.Resolve<ILocalizationController>();
            InitTabTitles();
            GetResourceValues();
        }

        private void InitTabTitles()
        {
            GlobalTabTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicGlobal);
            SyncTabTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSync);
            SystemTabTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSystem);
            LogTabTitle = "LOG"; // TODO Using localization
            LogoutTabTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicLogout);
        }

        public async Task<TabItemCollection> BuildTabItems(BaseViewModel parentBaseViewModel, FullSyncStatusType fullSyncStatusType, CancellationTokenSource parentCancellationTokenSource)
        {
            TabItemCollection tabItems = new TabItemCollection
            {
                new SfTabItem {
                    Title = GlobalTabTitle,
                    TitleFontColor = Color.White,
                    SelectionColor = Color.Black,
                    ImageSource = GetFontImageSource(MaterialDesignIcons.Cellphone)
                },
                new SfTabItem {
                    Title = SyncTabTitle,
                    TitleFontColor = Color.White,
                    SelectionColor = Color.Black,
                    ImageSource = GetFontImageSource(MaterialDesignIcons.Sync)
                },
                new SfTabItem {
                    Title = SystemTabTitle,
                    TitleFontColor = Color.White,
                    SelectionColor = Color.Black,
                    ImageSource = GetFontImageSource(MaterialDesignIcons.Information)
                },
                new SfTabItem {
                    Title = LogTabTitle,
                    TitleFontColor = Color.White,
                    SelectionColor = Color.Black,
                    ImageSource = GetFontImageSource(MaterialDesignIcons.TextBoxMultiple)
                },
                new SfTabItem {
                    Title = LogoutTabTitle,
                    TitleFontColor = Color.White,
                    SelectionColor = Color.Black,
                    ImageSource = GetFontImageSource(MaterialDesignIcons.PowerStandby)
                }
            };

            foreach(SfTabItem tabItem in tabItems)
            {
                View tabItemView = GetTabContentView(tabItem.Title);
                if(tabItemView != null)
                {
                    tabItem.Content = tabItemView;
                }

                UIWidget tabItemWidget = await GetTabWidget(tabItem.Title, parentBaseViewModel, fullSyncStatusType, parentCancellationTokenSource);
                if (tabItemWidget != null)
                {
                    tabItem.Content.BindingContext = tabItemWidget;
                }
            }

            return tabItems;
        }

        private View GetTabContentView(string tabTitle)
        {
            if(tabTitle == GlobalTabTitle)
            {
                return new AppToolsGlobalTabView();
            }
            else if (tabTitle == SyncTabTitle)
            {
                return new AppToolsSyncTabView();
            }
            else if (tabTitle == SystemTabTitle)
            {
                return new AppToolsSystemTabView();
            }
            else if (tabTitle == LogTabTitle)
            {
                return new AppToolsLogTabView();
            }
            else
            {
                return null;
            }
        }

        private async Task<UIWidget> GetTabWidget(string tabTitle, BaseViewModel parentBaseViewModel, FullSyncStatusType fullSyncStatusType, CancellationTokenSource parentCancellationTokenSource)
        {
            UIWidget widget = null;

            if (tabTitle == GlobalTabTitle)
            {
                widget = new AppToolsGlobalTabModel(parentCancellationTokenSource);
            }
            else if (tabTitle == SyncTabTitle)
            {
                widget = new AppToolsSyncTabModel(fullSyncStatusType, parentCancellationTokenSource);
            }
            else if (tabTitle == SystemTabTitle)
            {
                widget = new AppToolsSystemTabModel(parentCancellationTokenSource);
            }
            else if (tabTitle == LogTabTitle)
            {
                widget = new AppToolsLogTabModel(parentCancellationTokenSource);
            }

            if (widget != null)
            {
                widget.ParentBaseModel = parentBaseViewModel;
                await widget.InitializeControl();
            }

            return widget;
        }

        private FontImageSource GetFontImageSource(string glyph)
        {
            return new FontImageSource { FontFamily = MaterialDesignFontFamily, Glyph = glyph, Color = Color.White };
        }

        private void GetResourceValues()
        {
            ResourceDictionary StaticResources = Application.Current.Resources;

            if (StaticResources.ContainsKey("MaterialDesignIcons"))
            {
                MaterialDesignFontFamily = (OnPlatform<string>)StaticResources["MaterialDesignIcons"];
            }
        }
    }
}
