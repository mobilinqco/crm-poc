using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels
{
    public class MapPageViewModel : BaseViewModel
    {
        private UIWidget _content;
        public UIWidget Content
        {
            get => _content;
            set
            {
                _content = value;
                RaisePropertyChanged(() => Content);
            }
        }

        public ICommand OnCloseButtonTapped => new Command(() =>
        {
            _navigationController.PopAllPopupAsync(null);
        });

        public string CloseButtonText => _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose);

        public async override Task InitializeAsync(object data)
        {
            var panelData = (PanelData)data;
            Content = await FormBuilderExtensions.BuildWidgetAsync(panelData.Type, panelData, this, _cancellationTokenSource);
            
            await base.InitializeAsync(data);
        }
    }
}
