using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class SettingsContentService : ContentServiceBase, ISettingsContentService
    {
        private string _layoutName;
        public WebConfigLayout Layout { get; set; }
        private ICrmInstanceService _crmInstanceService;
        public SettingsContentService(ISessionContext sessionContext,
          IConfigurationService configurationService,
          ICrmDataService crmDataService,
          ILogService logService,
          IUserActionBuilder userActionBuilder,
          HeaderComponent headerComponent,
          FieldGroupComponent fieldGroupComponent,
          ImageResolverComponent imageResolverComponent,
          IFilterProcessor filterProcessor,
          ICrmInstanceService crmInstanceService) : base(sessionContext, configurationService,
              crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _crmInstanceService = crmInstanceService;
        }

        public new string PageTitle()
        { 

            if (!string.IsNullOrWhiteSpace(_action?.ActionDisplayName))
            {
                return _action.ActionDisplayName;
            }

            return "Settings";
        }

        public async override Task PrepareContentAsync(CancellationToken cancellationToken)
        {

            if (_action != null)
            {
                var headerName = _action?.ViewReference.GetArgumentValue("HeaderName");
                if (string.IsNullOrWhiteSpace(headerName))
                {
                    headerName = "SYSTEMINFO.Expand";
                }

                _layoutName = _action?.ViewReference.GetArgumentValue("LayoutName");

                if (!string.IsNullOrWhiteSpace(headerName))
                {
                    Header header = await _configurationService.GetHeader(headerName, cancellationToken).ConfigureAwait(false);
                    _headerComponent.InitializeContext(header, _action);
                    _headerButtons = await _headerComponent.HeaderButtons(cancellationToken).ConfigureAwait(false);
                }

                Layout = await _configurationService.GetWebConfigLayout(_layoutName, cancellationToken);
            }

        }

        public async Task RefreshData(CancellationToken cancellationToken)
        {
            Layout = await _configurationService.GetWebConfigLayout(_layoutName, cancellationToken);
        }

        public async Task<bool> Save(List<WebConfigData> modifiedConfigData, List<WebConfigData> userConfigData, CancellationToken token)
        {
            bool result = await SaveWebConfigurations(modifiedConfigData, token);
            if(result)
            {
                result = await SaveUserConfigurations(userConfigData, token);
            }
            return result;
        }

        private async Task<bool> SaveUserConfigurations(List<WebConfigData> userConfigData, CancellationToken token)
        {
            if (userConfigData?.Count > 0)
            {
                Dictionary<string, string> configs = new Dictionary<string, string>();
                foreach(var config in userConfigData)
                {
                    configs.Add(config.Name, config.UpdatedRawValue);
                }

                bool result = await _crmInstanceService.SaveCrmInstanceAsync(_sessionContext.CrmInstance.Identification, configs);
                if(result)
                {
                    _sessionContext.CrmInstance.UserSettings = configs;
                }
                return result;
            }

            return true;
        }

        private async Task<bool> SaveWebConfigurations(List<WebConfigData> modifiedConfigData, CancellationToken token)
        {
            Dictionary<string, string> configs = new Dictionary<string, string>();
            if (modifiedConfigData?.Count > 0)
            {
                foreach (var config in modifiedConfigData)
                {
                    if (!configs.ContainsKey(config.Name))
                    {
                        configs.Add(config.Name, config.UpdatedRawValue);
                    }
                }

                var result = await _crmDataService.SaveConfigurationsOnline(configs, token);
                if (result)
                {
                    foreach (var config in modifiedConfigData)
                    {
                        _configurationService.UpdateConfigValue(config.Name, config.UpdatedRawValue);
                    }
                }
                return result;
            }

            return true;
        }

        public async Task<List<WebConfigData>> GetLocalConfigurations(string identification, CancellationToken token)
        {
            var listItems = new List<WebConfigData>();

            var fullSyncConfig = new WebConfigData()
            {
                InputLabel = "Full-Sync after Login",
                ControlType = "Checkbox",
                Name = CrmLocalSettings.FullSyncAfterLoginKey,
                StringValue = "No",
                RawValue = "0"
            };

            listItems.Add(fullSyncConfig);
           

            List<CrmInstance> crms = await _crmInstanceService.GetCrmInstancesAsync();
            var instance = crms.Find(m => m.Identification == identification);
            if (instance != null && instance?.UserSettings?.Keys?.Count > 0)
            {
                foreach (var item in listItems)
                {
                    if(instance.UserSettings.ContainsKey(item.Name))
                    {
                        item.RawValue = instance.UserSettings[item.Name];
                    }

                    item.SetStringValue();
                }

            }

            return listItems;
        }
    }
}

