using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class DashBoardContentService : ContentServiceBase, IDashboardContentService
    {
        private string FormName { get; set; }
        public string HeaderLabel { get; set; }
        private UserAction _startUserAction;

        UserAction IDashboardContentService.StartUserAction =>  _startUserAction;

        private readonly IAppSearchMenuService _appSearchService;
        public DashBoardContentService(ISessionContext sessionContext,
           IConfigurationService configurationService,
           ICrmDataService crmDataService,
           ILogService logService,
           IUserActionBuilder userActionBuilder,
           HeaderComponent headerComponent,
           FieldGroupComponent fieldGroupComponent,
           ImageResolverComponent imageResolverComponent,
           IAppSearchMenuService appSearchService, IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
               crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _appSearchService = appSearchService;
        }

        public List<UserAction> InsightBoardActions()
        {
            return _appSearchService.InfoAreaRelatedActions();
        }

        public async override Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            await _appSearchService.PrepareContentAsync(cancellationToken);
            var startMenuNameConfig = _configurationService.GetConfigValue("StartPage");

            Menu startMenuItem = null;
            if (startMenuNameConfig != null && !string.IsNullOrWhiteSpace(startMenuNameConfig.Value))
            {
                startMenuItem = await _configurationService.GetMenu(startMenuNameConfig.Value, cancellationToken);
               
                if (startMenuItem.ViewReference != null)
                {
                    _startUserAction =_userActionBuilder.UserActionFromMenu(_configurationService, startMenuItem);
                    FormName = startMenuItem.ViewReference.GetArgumentValue("ConfigName");
                    var headerName = startMenuItem.ViewReference.GetArgumentValue("HeaderName");

                    if (!string.IsNullOrWhiteSpace(headerName))
                    {
                        Header header = await _configurationService.GetHeader(headerName, cancellationToken).ConfigureAwait(false);
                        if (header != null)
                        {
                            _headerComponent.InitializeContext(header, _action);
                            HeaderLabel = header.Label;
                            _headerButtons = await _headerComponent.HeaderButtons(cancellationToken).ConfigureAwait(false);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(HeaderLabel))
                    {
                        Form form = await GetDashboardForm(cancellationToken);
                        string firstTabLabel = form?.Tabs?[0]?.Label;
                        if(!string.IsNullOrWhiteSpace(firstTabLabel))
                        {
                            HeaderLabel = firstTabLabel;
                        }
                    }
                }
            }

            await SetSessionRepExtraParams(cancellationToken);
        }

        private async Task SetSessionRepExtraParams(CancellationToken cancellationToken)
        {
            WebConfigValue configurationValue = _configurationService.GetConfigValue("System.OfflineStationNumber");
            if(configurationValue != null && !string.IsNullOrWhiteSpace(configurationValue.Value))
            {
                int staNo = -1;
                if(int.TryParse(configurationValue.Value, out staNo))
                {
                    _sessionContext.OfflineStationNumber = staNo;
                }
            }
            
            configurationValue = _configurationService.GetConfigValue("RepCopySearchAndLists");
            if(configurationValue != null && !string.IsNullOrWhiteSpace(configurationValue.Value))
            {
                SearchAndList sal = await _configurationService.GetSearchAndList(configurationValue.Value, cancellationToken);
                InfoArea infoArea = _configurationService.GetInfoArea(sal.InfoAreaId);
                FieldControl fieldControl = await _configurationService.GetFieldControl(sal.FieldGroupName + ".List", cancellationToken);
                TableInfo tableInfo = await _configurationService.GetTableInfoAsync(infoArea.UnitName, cancellationToken);
                _fieldGroupComponent.InitializeContext(fieldControl, tableInfo);

                if (fieldControl != null && fieldControl.Tabs.Count > 0)
                {
                    List<FieldControlField> fieldDefinitions = _fieldGroupComponent.GetQueryFields(fieldControl.Tabs);

                    if (fieldDefinitions.Count > 0)
                    {
                        List<Filter> enabledDataFilters = new List<Filter>();
                        if (!string.IsNullOrWhiteSpace(sal.FilterName))
                        {
                            enabledDataFilters.AddRange(await _filterProcessor.RetrieveFilterDetails(new List<string> { sal.FilterName }, cancellationToken));
                        }
                        _rawData = await _crmDataService.GetData(cancellationToken,
                            new DataRequestDetails
                            {
                                TableInfo = tableInfo,
                                Fields = fieldDefinitions,
                                Filters = enabledDataFilters
                            },
                            null, 1, RequestMode.Offline);

                        if (_rawData != null && _rawData.Result != null && _rawData.Result.Rows.Count > 0)
                        {
                            _sessionContext.ExtraParams = await _fieldGroupComponent.ExtractDictionary(fieldDefinitions, _rawData.Result.Rows[0], cancellationToken);
                        }
                    }
                }
            }
        }

        public async Task<Form> GetDashboardForm(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(FormName))
            {
                return await _configurationService.GetForm(FormName, cancellationToken);
            }
            return null;
        }
    }
}
