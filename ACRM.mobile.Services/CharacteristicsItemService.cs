using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class CharacteristicsItemService: ContentServiceBase, ICharacteristicsItemService
    {
        private CharacteristicsEditActionTemplate _actionTemplate;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private string _itemFieldName = "";
        private string _itemShowAdditionalFieldsFieldName = "";

        private HashSet<string> _visibleCharacteristicsItemCodes = new HashSet<string>();
        private Dictionary<string, bool> _characteristicsItemsShowAdditionalFieldsValues = new Dictionary<string, bool>();

        public CharacteristicsItemService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {

        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareContentAsync");

            ViewReference vr = _action.ViewReference;
            _actionTemplate = new CharacteristicsEditActionTemplate(vr);

            SearchAndList searchAndList = await _configurationService.GetSearchAndList(_actionTemplate.ItemSearchAndList(), cancellationToken).ConfigureAwait(false);

            _infoArea = _configurationService.GetInfoArea(searchAndList.InfoAreaId);

            string infoAreaUnitName = searchAndList.InfoAreaId;
            if(_infoArea != null)
            {
                infoAreaUnitName = _infoArea.UnitName;
            }

            _fieldControl = await _configurationService.GetFieldControl(searchAndList.FieldGroupName + ".List", cancellationToken);

            _tableInfo = await _configurationService.GetTableInfoAsync(infoAreaUnitName, cancellationToken);
            _fieldGroupComponent.InitializeContext(_fieldControl, _tableInfo);

            if (_fieldControl != null && _fieldControl.Tabs.Count > 0)
            {
                List<FieldControlField> fieldDefinitions = _fieldGroupComponent.GetQueryFields(_fieldControl.Tabs);

                InitiliseFieldNames(fieldDefinitions);

                List<string> filterNames = _filterProcessor.RetrieveEnabledFiltersNames(_actionTemplate);
                if (!string.IsNullOrWhiteSpace(searchAndList.FilterName))
                {
                    filterNames.Add(searchAndList.FilterName);
                }
                List<Filter> enabledDataFilters = await _filterProcessor.RetrieveFilterDetails(filterNames, cancellationToken);

                _rawData = await _crmDataService.GetData(cancellationToken,
                    new DataRequestDetails { TableInfo = _tableInfo, Fields = fieldDefinitions, Filters = enabledDataFilters },
                    requestMode: RequestMode.Best);

                InitialiseCharacteristicsGroupsAttributes();
            }

            _logService.LogDebug("End PrepareContentAsync");
        }

        private void InitiliseFieldNames(List<FieldControlField> fieldDefinitions)
        {
            foreach (FieldControlField field in fieldDefinitions)
            {

                if (field.Function == "Item")
                {
                    _itemFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "ShowAdditionalFields")
                {
                    _itemShowAdditionalFieldsFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
            }
        }
        private void InitialiseCharacteristicsGroupsAttributes()
        {
            if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
            {
                foreach (DataRow row in _rawData.Result.Rows)
                {
                    string itemCode = row[_itemFieldName].ToString();
                    _visibleCharacteristicsItemCodes.Add(itemCode);
                    if (bool.TryParse(row[_itemShowAdditionalFieldsFieldName].ToString(), out bool isSingleSelection))
                    {
                        _characteristicsItemsShowAdditionalFieldsValues.Add(itemCode, isSingleSelection);
                    }
                }
            }
        }

        public HashSet<string> GetVisibleCharacteristicsItemCodes()
        {
            return _visibleCharacteristicsItemCodes;
        }

        public Dictionary<string, bool> GetCharacteristicsItemsShowAdditionalFieldsValues()
        {
            return _characteristicsItemsShowAdditionalFieldsValues;
        }
    }
}
