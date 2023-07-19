using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using AsyncAwaitBestPractices;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class SettingsDetailsPageViewModel : NavigationBarBaseViewModel
    {
        readonly private ISettingsContentService _contentService;
        public ICommand OnHeaderAction => new Command<ObservableGroups.HeaderActionButton>(async (selectedItem) => await HeaderAction(selectedItem));

        private ObservableGroups.HeaderGroupData _headerData;
        public ObservableGroups.HeaderGroupData HeaderData
        {
            get => _headerData;
            set
            {
                _headerData = value;
                RaisePropertyChanged(() => HeaderData);
            }
        }

        private WebConfigLayout _configLayout;
        public WebConfigLayout ConfigLayout
        {
            get => _configLayout;
            set
            {
                _configLayout = value;
                RaisePropertyChanged(() => ConfigLayout);
            }
        }

        public SettingsDetailsPageViewModel(ISettingsContentService contentService)
        {
            _contentService = contentService;
            IsBackButtonVisible = true;
            PageTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSettings);
            ObservableGroups.HeaderGroupData headerData = new ObservableGroups.HeaderGroupData();
            headerData.IsOrganizerHeaderVisible = false;
            HeaderData = headerData;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsLoading = true;
            _logService.LogDebug("Start  InitializeAsync");
            if (navigationData is Domain.Application.UserAction)
            {
                _contentService.SetSourceAction(navigationData as Domain.Application.UserAction);
                await _contentService.PrepareContentAsync(_cancellationTokenSource.Token);
                await UpdateBindingsAsync();
            }
            
            await base.InitializeAsync(navigationData);
            _logService.LogDebug("End  InitializeAsync");
        }

        private async Task UpdateBindingsAsync()
        {
            PageTitle = _contentService.PageTitle();
            var headerData = HeaderData;
            headerData.AreActionsViewVisible = false;
            headerData.SetHeaderActionButtons(_contentService.HeaderButtons());
            if (headerData.HeaderActions.Count > 0)
            {
                headerData.AreActionsViewVisible = true;
            }
            HeaderData = headerData;
            ConfigLayout = _contentService.Layout;
            Widgets = new ObservableCollection<UIWidget>();

            if (ConfigLayout?.Tabs?.Count > 0)
            {
                foreach (var layoutTab in ConfigLayout.Tabs)
                {
                    Widgets.Add(await Utils.FormBuilderExtensions.BuildWidget("ConfigPanel", layoutTab, this, _cancellationTokenSource));
                }
                Widgets.Add(await Utils.FormBuilderExtensions.BuildWidget("UserConfigPanel", null, this, _cancellationTokenSource));
            }

            IsLoading = false;
        }

        private async Task HeaderAction(ObservableGroups.HeaderActionButton headerActionButton)
        {
            if (headerActionButton != null)
            {
                _logService.LogInfo($"SettingsDetailsPage header user action: {headerActionButton.UserAction.ActionDisplayName}");
                CustomControls.IUserActionSchuttle userActionSchuttle = Utils.AppContainer.Resolve<CustomControls.IUserActionSchuttle>();
                await userActionSchuttle.Carry(headerActionButton.UserAction, null, _cancellationTokenSource.Token);
            }
        }

        public override async Task RefreshAsync(object data)
        {
            await _contentService.RefreshData(_cancellationTokenSource.Token);
            await UpdateBindingsAsync();
        }
    }
}

