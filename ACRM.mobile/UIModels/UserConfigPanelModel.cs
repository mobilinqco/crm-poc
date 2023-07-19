using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;

namespace ACRM.mobile.UIModels
{
    public class UserConfigPanelModel : UIWidget
    {
        protected readonly ISettingsContentService _crmSettingsService;

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private List<WebConfigData> _items;
        public List<WebConfigData> Items
        {
            get => _items;
            set
            {
                _items = value;
                RaisePropertyChanged(() => Items);
            }
        }

        public UserConfigPanelModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(parentCancellationTokenSource)
        {
            _crmSettingsService = Utils.AppContainer.Resolve<ISettingsContentService>();
            Title = "Local User Settings";
        }

        public async override ValueTask<bool> InitializeControl()
        {
            Items = await _crmSettingsService.GetLocalConfigurations(_sessionContext.CrmInstance.Identification, _cancellationTokenSource.Token);
            return true;

        }
    }
}

