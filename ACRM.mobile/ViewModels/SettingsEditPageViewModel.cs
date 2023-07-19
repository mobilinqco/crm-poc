using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class SettingsEditPageViewModel : Base.NavigationBarBaseViewModel
    {
        readonly private ISettingsContentService _contentService;
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

        private string _cancelButtonTitle;
        public string CancelButtonTitle
        {
            get
            {
                return _cancelButtonTitle;
            }

            set
            {
                _cancelButtonTitle = value;
                RaisePropertyChanged(() => CancelButtonTitle);
            }
        }

        private bool _errorMessageEmabled;
        public bool ShowErrorMessage
        {
            get
            {
                return _errorMessageEmabled;
            }

            set
            {
                _errorMessageEmabled = value;
                RaisePropertyChanged(() => ShowErrorMessage);
            }
        }

        private string _saveButtonTitle;
        public string SaveButtonTitle
        {
            get
            {
                return _saveButtonTitle;
            }

            set
            {
                _saveButtonTitle = value;
                RaisePropertyChanged(() => SaveButtonTitle);
            }
        }

        private string _errorLabel = "Failed saving configurations.";
        public string ErrorLabel
        {
            get
            {
                return _errorLabel;
            }

            set
            {
                _errorLabel = value;
                RaisePropertyChanged(() => ErrorLabel);
            }
        }

        public ICommand OnCancelCommand => new Command(async () => await OnCancel());

        

        public ICommand OnSaveCommand => new Command(async () => await OnSave());

        public SettingsEditPageViewModel(ISettingsContentService contentService)
        {
            _contentService = contentService;
            IsBackButtonVisible = true;
            CancelButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicCancel);
            SaveButtonTitle = _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicSave);
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

            ConfigLayout = _contentService.Layout;
            Widgets = new ObservableCollection<UIWidget>();

            if (ConfigLayout?.Tabs?.Count > 0)
            {
                foreach (var layoutTab in ConfigLayout.Tabs)
                {
                    Widgets.Add(await Utils.FormBuilderExtensions.BuildWidget("ConfigEditPanel", layoutTab, this, _cancellationTokenSource));
                }
            }
            Widgets.Add(await Utils.FormBuilderExtensions.BuildWidget("UserConfigEditPanel", null, this, _cancellationTokenSource));

            IsLoading = false;
        }

        private async Task OnCancel()
        {
            await _navigationController.BackAsync();
        }

        private async Task OnSave()
        {
            IsLoading = true;
            ShowErrorMessage = false;
            (List<WebConfigData>, List<WebConfigData>) modifiedConfigData = GetModifiedConfigData();
            bool result = await _contentService.Save(modifiedConfigData.Item1, modifiedConfigData.Item2, _cancellationTokenSource.Token);

            IsLoading = false;
            if (result)
            {
                await _navigationController.BackAsync(true);
            }
            else
            {
                ShowErrorMessage = true;
            }
        }

        private (List<WebConfigData>, List<WebConfigData>) GetModifiedConfigData()
        {
            List<WebConfigData> webConfig = new List<WebConfigData>();
            List<WebConfigData> userConfig = new List<WebConfigData>();
            if (Widgets?.Count > 0)
            {
                foreach (var widget in Widgets)
                {
                    if (widget is UIModels.ConfigEditPanelModel ctrl)
                    {
                        var data = ctrl.GetConfigData();
                        if (data?.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                if (item.IsChanged)
                                {
                                    webConfig.Add(item);
                                }
                            }
                        }
                    }
                    else if (widget is UIModels.UserConfigEditPanelModel userCtrl)
                    {
                        var data = userCtrl.GetConfigData();
                        if (data?.Count > 0)
                        {
                            foreach (var item in data)
                            {
                                if (item.IsChanged)
                                {
                                    userConfig.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            return (webConfig, userConfig);
        }
    }
}

