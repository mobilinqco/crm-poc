using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Calendar;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using AsyncAwaitBestPractices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.SubComponents
{
    public class CalendarEventComponent
    {
        private IUserActionBuilder _userActionBuilder;
        private IConfigurationService _configurationService;
        private ICrmDataService _crmDataService;
        private ISearchContentService _searchContentService;
        private IFilterProcessor _filterProcessor;
        private ISessionContext _sessionContext;
        private ILogService _logService;
        private RecordViewTemplate _recordView;
        private CalendarViewTemplate _actionTemplate;
        private SearchAndList _searchAndList;
        private FieldControl _searchControl;
        private FieldGroupComponent _fieldGroupComponent;
        private TableInfo _tableInfo;
        private Expand _expand;
        private ExpandComponent _expandComponent;
        private TableCaptionComponent _tableCaptionComponent;
        private List<Filter> _enabledDataFilters = new List<Filter>();
        private List<Filter> UserFilters;
        private List<Filter> EnabledUserFilters;
        private List<UserAction> _longPressUserActions;

        private UserAction _action;
        private InfoArea _infoArea;
        private DataResponse _rawData;


        private List<FieldControlField> _fieldDefinitions;
        private ParentLink _parentLink;
        private FieldInfo _eventDateFieldInfo;
        private int _maxResults;

        private bool _repFilterCurrentRepActive; // TODO From main UserAction or not?
        public List<UserAction> LongPressUserActions => _longPressUserActions;

        public CalendarEventComponent(IUserActionBuilder userActionBuilder,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ISearchContentService searchContentService,
            IFilterProcessor filterProcessor,
            ISessionContext sessionContext,
            ILogService logService,
            FieldGroupComponent fieldGroupComponent,
            ExpandComponent expandComponent,
            TableCaptionComponent tableCaptionComponent)
        {
            _userActionBuilder = userActionBuilder;
            _configurationService = configurationService;
            _crmDataService = crmDataService;
            _searchContentService = searchContentService;
            _filterProcessor = filterProcessor;
            _sessionContext = sessionContext;
            _logService = logService;
            _fieldGroupComponent = fieldGroupComponent;
            _expandComponent = expandComponent;
            _tableCaptionComponent = tableCaptionComponent;
        }

        public async Task PrepareComponentAsync(UserAction action, CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareComponentAsync");

            _action = action;

            ViewReference vr = _action.ViewReference;

            if (vr == null)
            {
                vr = await _configurationService.GetViewForMenu(_action.ActionUnitName, cancellationToken).ConfigureAwait(false);
            }

            _recordView = new RecordViewTemplate(vr);

            _actionTemplate = new CalendarViewTemplate(vr);
            _searchAndList = await _configurationService.GetSearchAndList(_actionTemplate.ConfigName(), cancellationToken).ConfigureAwait(false);

            string infoAreaId = InfoAreaUnitName();

            if (!string.IsNullOrEmpty(infoAreaId))
            {
                _infoArea = _configurationService.GetInfoArea(infoAreaId);

                _tableInfo = await _configurationService.GetTableInfoAsync(_infoArea.UnitName, cancellationToken);

                if (_searchAndList == null)
                {
                    _searchAndList = await _configurationService.GetSearchAndList(infoAreaId, cancellationToken).ConfigureAwait(false);
                }

                _searchControl = await _configurationService.GetFieldControl(_searchAndList.FieldGroupName + ".Search", cancellationToken).ConfigureAwait(false);

                FieldControl fieldControl = await _configurationService.GetFieldControl(_searchAndList.FieldGroupName + ".List", cancellationToken).ConfigureAwait(false);
                TableInfo tableInfo = await _configurationService.GetTableInfoAsync(InfoAreaUnitName(), cancellationToken).ConfigureAwait(false);
                
                _fieldGroupComponent.InitializeContext(fieldControl, tableInfo);

                _expand = await ResolveExpand(_action, _recordView, _tableInfo, cancellationToken);

                SetConfigurationValues();

                await ConfigureLongPressUserAction(cancellationToken);

                _expandComponent.InitializeContext(infoAreaId, infoAreaId, tableInfo);
                if (_expandComponent.IsExpandDefined(_actionTemplate.ConfigName()))
                {
                    _expandComponent.ExpandName = _actionTemplate.ConfigName();
                }

                List<string> filterNames = _filterProcessor.RetrieveEnabledFiltersNames(_actionTemplate);

                if (_repFilterCurrentRepActive && !string.IsNullOrWhiteSpace(_actionTemplate.RepFilter()))
                {
                    filterNames.Add(_actionTemplate.RepFilter());
                    _filterProcessor.SetAdditionalFilterParams(new Dictionary<string, string> { { "Rep", _sessionContext.User.SessionInformation.RepIdStr() } });
                }

                _enabledDataFilters.AddRange(await _filterProcessor.RetrieveFilterDetails(filterNames, cancellationToken));

                UserFilters = new List<Filter>();
                filterNames = _filterProcessor.RetrieveUserFiltersNames(_actionTemplate);
                UserFilters.AddRange(await _filterProcessor.RetrieveFilterDetails(filterNames, cancellationToken));


                _fieldDefinitions = GetFields(_fieldGroupComponent.FieldControl, _infoArea.UnitName);
                _parentLink = _userActionBuilder.GetParentLink(_action, _actionTemplate);
                _eventDateFieldInfo = await _fieldGroupComponent.GetCalendarEventStartDateField(_fieldDefinitions, cancellationToken);
                _maxResults = GetMaxResults();

                string tableCaptionName = "";

                if (_expand == null)
                {
                    tableCaptionName = "MA.Calendar";
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(_expand.TableCaptionName))
                    {
                        tableCaptionName = _expand.TableCaptionName;
                    }
                }

                await _tableCaptionComponent.InitializeContext(tableCaptionName, _fieldGroupComponent.TableInfo, null, cancellationToken);

                var captionFields = _tableCaptionComponent.CaptionFields;
                if (captionFields?.Count > 0 && _fieldDefinitions != null)
                {
                    foreach (var field in captionFields)
                    {
                        if (!_fieldDefinitions.Any(a => a.FieldId.Equals(field.FieldId) && a.InfoAreaId.Equals(field.InfoAreaId)))
                        {
                            _fieldDefinitions.Add(field);
                        }

                    }

                }
                
            }

            _searchContentService.SetSourceAction(_action);
            _searchContentService.PrepareContentAsync(cancellationToken).SafeFireAndForget<Exception>(onException: ex =>
            {
                _logService.LogError($"Unable to prepare content {ex.Message}");
            });

            _logService.LogDebug("End PrepareComponentAsync");
        }

        private async Task ConfigureLongPressUserAction(CancellationToken cancellationToken)
        {
            var buttonName = _actionTemplate.NewAppointmentAction();
            if (!string.IsNullOrWhiteSpace(buttonName))
            {
                
                _longPressUserActions = new List<UserAction>();
                if (buttonName.Contains(","))
                {
                    List<string> buttons = new List<string>();
                    buttons.AddRange(buttonName.Split(','));
                    foreach(var button in buttons)
                    {
                        Button buttonObj = await _configurationService.GetButton(button, cancellationToken);
                        var longPressUserAction = _userActionBuilder.UserActionFromButton(_configurationService, buttonObj, null);
                        if (longPressUserAction != null)
                        {
                            _longPressUserActions.Add(longPressUserAction);
                        }
                    }
                    
                }
                else
                {
                    Button buttonObj = await _configurationService.GetButton(buttonName, cancellationToken);
                    var longPressUserAction = _userActionBuilder.UserActionFromButton(_configurationService, buttonObj, null);
                    if (longPressUserAction != null)
                    {
                        _longPressUserActions.Add(longPressUserAction);
                    }
                }
                
            }
        }


        private string InfoAreaUnitName()
        {
            string infoAreaUnitName = string.Empty;

            if (_infoArea != null && !string.IsNullOrEmpty(_infoArea.UnitName))
            {
                infoAreaUnitName = _infoArea.UnitName;
            }

            if (string.IsNullOrEmpty(infoAreaUnitName))
            {
                infoAreaUnitName = _actionTemplate.InfoArea();
            }

            if (string.IsNullOrEmpty(infoAreaUnitName))
            {
                infoAreaUnitName = _searchAndList.InfoAreaId;
            }

            return infoAreaUnitName;
        }

        private async Task<Expand> ResolveExpand(UserAction action, RecordViewTemplate recordView, TableInfo tableInfo, CancellationToken cancellationToken)
        {
            string expandName = recordView.ConfigName();

            if (string.IsNullOrWhiteSpace(expandName))
            {
                expandName = string.IsNullOrWhiteSpace(recordView.ExpandName()) ? action.InfoAreaUnitName : recordView.ExpandName();
            }

            if (!string.IsNullOrWhiteSpace(expandName))
            {
                _expandComponent.InitializeContext(expandName, action.InfoAreaUnitName, tableInfo);
                List<FieldControlField> fields = new List<FieldControlField>();

                List<FieldControlField> expandFields = _expandComponent.GetExpandRuleFields(expandName);
                foreach (var field in expandFields)
                {
                    if (!fields.Exists(f => f.FieldId == field.FieldId && f.InfoAreaId == field.InfoAreaId))
                    {
                        fields.Add(field);
                    }
                }

                if (fields.Count > 0)
                {
                    DataResponse rawData = await _crmDataService.GetRecord(cancellationToken,
                        new DataRequestDetails { TableInfo = _tableInfo, Fields = fields, RecordId = _action.RecordId },
                        DetermineRequestMode());
                    if (rawData.Result != null && rawData.Result.Rows.Count > 0)
                    {
                        return _expandComponent.ResolveExpand(expandName, rawData.Result.Rows[0]);
                    }
                }

                return _configurationService.GetExpand(expandName);
            }
            else
            {
                expandName = action.ResolvedExpandName;
                if (string.IsNullOrWhiteSpace(expandName))
                {
                    return _configurationService.GetExpand(expandName);
                }
            }

            return null;

        }

        private RequestMode DetermineRequestMode()
        {
            if (_sessionContext.IsInOfflineMode)
            {
                return RequestMode.Offline;
            }

            if (_action.IsRecordRetrievedOnline)
            {
                return RequestMode.Best;
            }

            return _recordView.GetRequestMode();
        }

        private void SetConfigurationValues()
        {
            _repFilterCurrentRepActive = _actionTemplate.RepFilterCurrentRepActive();
        }

        public async Task<List<CRMCalendarItem>> GetCrmCalendarEventsAsync(string searchText, 
            RequestMode requestMode, 
            DateTime startDate, 
            DateTime endDate,
            CancellationToken cancellationToken)
        {
            List<CRMCalendarItem> cRMCalendarItems= new List<CRMCalendarItem>();
            List<FieldControlField> searchFields = null;
            if (!this.IsValidConfig())
            {
                return cRMCalendarItems;
            }

            if (_searchControl.Tabs.Count > 0)
            {
                searchFields = _searchControl.Tabs[0].GetQueryFields();
            }

            if (_fieldGroupComponent.HasTabs())
            {

                List<Filter> eFilters = new List<Filter>();

                eFilters.AddRange(_enabledDataFilters);
                if (_eventDateFieldInfo != null)
                {
                    eFilters.Add(CreateEventDateFilter(_eventDateFieldInfo, startDate, endDate));
                }
                if (EnabledUserFilters?.Count > 0)
                {
                    eFilters.AddRange(EnabledUserFilters);
                }

                string searchValue = searchText;
                if (_configurationService.GetBoolConfigValue("Search.ReplaceCaseSensitiveCharacters")
                    && !string.IsNullOrWhiteSpace(searchValue))
                {
                    Regex regex = new Regex(@"[^a-zA-Z0-9\s]", RegexOptions.None);
                    searchValue = regex.Replace(searchText, "?");
                }

                _rawData = await _crmDataService.GetData(cancellationToken,
                    new DataRequestDetails
                    {
                        TableInfo = _fieldGroupComponent.TableInfo,
                        Fields = _fieldDefinitions,
                        SortFields = _fieldGroupComponent.FieldControl.SortFields,
                        SearchFields = searchFields,
                        SearchValue = searchValue,
                        Filters = eFilters
                    },
                    _parentLink,
                    _maxResults,
                    requestMode);

                if (_rawData.Result != null)
                {
                    var tasks = _rawData.Result.Rows.Cast<DataRow>().Select(async row => await GetCalendarEvent(_fieldDefinitions, row, cancellationToken));
                    cRMCalendarItems.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
                }
            }

            return cRMCalendarItems;
        }

        public bool IsValidConfig()
        {
            if (_searchControl == null || _fieldGroupComponent == null)
            {
                return false;
            }

            return true;
        }

        private List<FieldControlField> GetFields(FieldControl fieldControl, string infoAreaId)
        {
            List<FieldControlField> fields = fieldControl.Tabs[0].GetQueryFields();

            List<FieldControlField> expandFields = _expandComponent.GetExpandRuleFields(infoAreaId);
            foreach (var field in expandFields)
            {
                if (!fields.Exists(f => f.FieldId == field.FieldId && f.InfoAreaId == field.InfoAreaId))
                {
                    fields.Add(field);
                }
            }

            return fields;
        }

        private Filter CreateEventDateFilter(FieldInfo fieldInfo, DateTime startDate, DateTime endDate)
        {
            QueryTable queryTable = new QueryTable { InfoAreaId = _infoArea.UnitName };
            NodeCondition gtFilterNode = new NodeCondition(_infoArea.UnitName, fieldInfo.FieldId, ">=", new List<string> { startDate.ToCrmDateString() });
            NodeCondition ltFilterNode = new NodeCondition(_infoArea.UnitName, fieldInfo.FieldId, "<=", new List<string> { endDate.ToCrmDateString() });
            queryTable.ExpandedConditions = new NodeCondition("AND", ltFilterNode, gtFilterNode);

            return new Filter
            {
                DisplayName = "Calendar Events Filter",
                InfoAreaId = _infoArea.UnitName,
                RootTable = queryTable
            };
        }

        private int GetMaxResults()
        {
            int defaultMaxResults = 100;

            var configurationValue = _configurationService.GetConfigValue("Search.MaxResults");
            if (configurationValue != null && !string.IsNullOrWhiteSpace(configurationValue.Value)
                && int.TryParse(configurationValue.Value, out int maxResults) && maxResults > 0)
            {
                return maxResults;
            }
            return defaultMaxResults;
        }
        ListDisplayRow tblrow = null;
        private async Task<CRMCalendarItem> GetCalendarEvent(List<FieldControlField> fieldDefinitions, DataRow row, CancellationToken cancellationToken)
        {
           
            DeviceCalendarEvent dce = await _fieldGroupComponent.GetCalendarEvent(fieldDefinitions, row, cancellationToken);
            dce.Color = _expandComponent.GetColorString(_expandComponent.ExpandName, row);
            dce.TableCaptionContent = await _tableCaptionComponent.CaptionText(cancellationToken, row);
            dce.UserActionUnitName = _action.ActionUnitName;

            CRMCalendarItem cRMCalendarItem = new CRMCalendarItem();
            cRMCalendarItem.CalendarEvent = dce;
            var listRow = await GetRow(fieldDefinitions, row,cancellationToken);
            cRMCalendarItem.RecordId = listRow.RecordId.FormatedRecordId(listRow.InfoAreaId);
            cRMCalendarItem.ListEvent = listRow;

            return cRMCalendarItem;
        }

        private async Task<ListDisplayRow> GetRow(List<FieldControlField> fieldDefinitions, DataRow row, CancellationToken cancellationToken)
        {
            ListDisplayRow outRow = await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, false, cancellationToken);
            outRow.RowDecorators = _expandComponent.GetRowDecorators(row, _searchContentService.GetTabData(0));
            return outRow;
        }

        public void SetEnabledUserFilters(List<Filter> filters)
        {
            EnabledUserFilters = filters;
        }

        public CalendarViewTemplate GetCalendarViewTemplate()
        {
            return _actionTemplate;
        }

        public UserAction ActionForCrmCalendarEvent(string recordId)
        {
            return new UserAction
            {
                RecordId = recordId,
                IsRecordRetrievedOnline = AreResultsRetrievedOnline(0),
                InfoAreaUnitName = InfoAreaUnitName(_infoArea, _actionTemplate, _searchAndList),
                ActionUnitName = "SHOWRECORD",
                ActionType = UserActionType.ShowRecord
            };
        }

        public bool AreResultsRetrievedOnline(int tabId)
        {
            return _searchContentService.AreResultsRetrievedOnline(tabId);
        }

        private string InfoAreaUnitName(InfoArea infoArea, ActionTemplateBase actionTemplate, SearchAndList searchAndList)
        {
            string infoAreaUnitName = string.Empty;

            if (infoArea != null && !string.IsNullOrEmpty(infoArea.UnitName))
            {
                infoAreaUnitName = infoArea.UnitName;
            }

            if (string.IsNullOrEmpty(infoAreaUnitName) && actionTemplate != null)
            {
                infoAreaUnitName = actionTemplate.InfoArea();
            }

            if (string.IsNullOrEmpty(infoAreaUnitName) && searchAndList != null)
            {
                infoAreaUnitName = searchAndList.InfoAreaId;
            }

            return infoAreaUnitName;
        }
    }
}
