using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class CharacteristicsGroupService: ContentServiceBase, ICharacteristicsGroupService
    {
        private CharacteristicsEditActionTemplate _actionTemplate;
        private FieldControl _fieldControl;
        private TableInfo _tableInfo;

        private string _groupFieldName = "";
        private string _groupIsSingleSelectionFieldName = "";
        private string _groupIsExpandableFieldName = "";

        private HashSet<string> _visibleCharacteristicsGroups = new HashSet<string>();
        private Dictionary<string, bool> _characteristicsGroupsSingleSelectionValues = new Dictionary<string, bool>();
        private Dictionary<string, bool> _characteristicsGroupsExpandedValues = new Dictionary<string, bool>();

        public CharacteristicsGroupService(ISessionContext sessionContext,
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

            SearchAndList searchAndList = await _configurationService.GetSearchAndList(_actionTemplate.GroupSearchAndList(), cancellationToken).ConfigureAwait(false);

            _infoArea = _configurationService.GetInfoArea(searchAndList.InfoAreaId);

            string infoAreaUnitName = searchAndList.InfoAreaId;
            if (_infoArea != null)
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

                if (field.Function == "Group")
                {
                    _groupFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "Single")
                {
                    _groupIsSingleSelectionFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
                if (field.Function == "ShowGroupExpanded")
                {
                    _groupIsExpandableFieldName = field.QueryFieldName(!field.InfoAreaId.Equals(_fieldGroupComponent.TableInfo.InfoAreaId));
                }
            }
        }
        private void InitialiseCharacteristicsGroupsAttributes()
        {
            if (_rawData?.Result != null && _rawData.Result.Rows.Count > 0)
            {
                foreach (DataRow row in _rawData.Result.Rows)
                {
                    string groupCode = row[_groupFieldName].ToString();
                    _visibleCharacteristicsGroups.Add(groupCode);
                    if (bool.TryParse(row[_groupIsSingleSelectionFieldName].ToString(), out bool isSingleSelection))
                    {
                        _characteristicsGroupsSingleSelectionValues.Add(groupCode, isSingleSelection);
                    }
                    if (bool.TryParse(row[_groupIsExpandableFieldName].ToString(), out bool isExpandable))
                    {
                        _characteristicsGroupsExpandedValues.Add(groupCode, isExpandable);
                    }
                }
            }
        }

        public HashSet<string> GetVisibleCharacteristicsGroupCodes()
        {
            return _visibleCharacteristicsGroups;
        }

        public Dictionary<string, bool> GetCharacteristicsGroupsSingleSelectionValues()
        {
            return _characteristicsGroupsSingleSelectionValues;
        }

        public Dictionary<string, bool> GetCharacteristicsGroupsExpandedValues()
        {
            return _characteristicsGroupsExpandedValues;
        }
    }
}
