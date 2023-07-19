using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Domain.Application.ActionTemplates;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using ACRM.mobile.Services.Utils;
using System.Text.RegularExpressions;
using ACRM.mobile.Services.Extensions;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;
using Jint.Parser;

namespace ACRM.mobile.Services
{
    public class SearchContentService : ContentServiceBase, ISearchContentService
    {
        protected readonly IDocumentService _documentService;
        protected List<TabDataWithConfig> tabs = new List<TabDataWithConfig>();
        protected ExpandComponent _expandComponent;
        protected readonly ICrmDataFieldResolver _crmDataFieldResolver;

        public SearchContentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            IDocumentService documentService,
            ICrmDataFieldResolver crmDataFieldResolver,
            ExpandComponent expandComponent) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _documentService = documentService;
            _expandComponent = expandComponent;
            _crmDataFieldResolver = crmDataFieldResolver;
        }

        private string _searchAndListName = string.Empty;
        public string SearchAndListName
        {
            get
            {
                return _searchAndListName;
            }
            set
            {
                _searchAndListName = value;
            }
        }

        private Filter additionalFilter;
        public Filter AdditionalFilter
        {
            get
            {
                return additionalFilter;
            }
            set
            {
                additionalFilter = value;
            }
        }

        private bool? _forceOnline = null;
        public bool? ForceOnline
        {
            get
            {
                return _forceOnline;
            }
            set
            {
                _forceOnline = value;
            }
        }

        public async Task SetAdditionalFilterName(string filterName, CancellationToken cancellationToken)
        {
            AdditionalFilter = await _filterProcessor.RetrieveFilterDetails(filterName, cancellationToken);
        }

        public void SetAdditionalFilterParams(Dictionary<string, string> filterAdditionalParems)
        {
            _filterProcessor?.SetAdditionalFilterParams(filterAdditionalParems);
        }

        public bool IsValidConfig(int tabId)
        {
            if (tabId >= tabs.Count)
            {
                return false;
            }

            if (_fieldGroupComponent == null)
            {
                return false;
            }

            return true;
        }

        public string SearchColumns(int tabId)
        {
            string columns = "";
            if (tabId >= tabs.Count)
            {
                return columns;
            }

            FieldControl searchControl = tabs[tabId].SearchControl;
            if (searchControl != null && searchControl.Tabs != null && searchControl.Tabs.Count > 0)
            {
                List<FieldControlField> fieldDefinitions = searchControl.Tabs[0].GetQueryFields();

                foreach (FieldControlField fd in fieldDefinitions)
                {
                    if (fd.InfoAreaId == tabs[tabId].TableInfo.InfoAreaId)
                    {
                        FieldInfo fieldInfo = _configurationService.GetFieldInfo(tabs[tabId].TableInfo, fd.FieldId);
                        columns += fieldInfo.Name + " | ";
                    }
                    else
                    {
                        (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(tabs[tabId].TableInfo, fd.InfoAreaId, fd.LinkId);

                        if (relatedTable != null)
                        {
                            FieldInfo fieldInfo = _configurationService.GetFieldInfo(relatedTable, fd.FieldId);
                            columns += fieldInfo.Name + " | ";
                        }
                    }
                }
            }

            if (columns.Length > 3)
            {
                columns = columns.Remove(columns.Length - 3);
            }

            return columns;
        }

        public bool IsOnlineOfflineVisible(int tabId)
        {
            if (tabId < tabs.Count)
            {
                return !tabs[tabId].ActionTemplate.HideOnlineOfflineButton();
            }

            return true;
        }

        public bool HasUserFilters(int tabId)
        {
            if (tabId < tabs.Count)
            {
                return tabs[tabId].UserFilters?.Count > 0;
            }

            return false;
        }

        public List<Filter> GetUserFilters(int tabId)
        {
            if (tabId < tabs.Count)
            {
                return tabs[tabId].UserFilters;
            }
            return new List<Filter>();
        }

        public List<Filter> GetUserDefaultEnabledFilters(int tabId)
        {
            if (tabId < tabs.Count)
            {
                return tabs[tabId].EnabledUserFilters;
            }
            return new List<Filter>();
        }

        public void SetEnabledUserFilters(List<Filter> filters, int tabId)
        {
            if (tabId < tabs.Count)
            {
                tabs[tabId].EnabledUserFilters = filters;
            }
        }

        public RequestMode InitialRequestMode(int tabId)
        {
            if (tabId < tabs.Count)
            {
                return tabs[tabId].ActionTemplate.GetRequestMode();
            }
            return RequestMode.Offline;
        }

        public TabDataWithConfig GetTabData(int index)
        {
            return tabs != null && tabs.Count > index ? tabs[index] : null;
        }

        public async Task<List<ListDisplayRow>> RecordListViewDataAsync(int tabId, CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start RecordListViewDataAsync");
            List<ListDisplayRow> result = new List<ListDisplayRow>();

            if (tabId >= tabs.Count)
            {
                return result;
            }

            var watch = new Stopwatch();
            watch.Start();

            DataResponse rawData = tabs[tabId].RawData;
            if (rawData.Result != null)
            {
                List<FieldControlField> fieldDefinitions = tabs[tabId].FieldControl.Tabs[0].GetQueryFields();
                _fieldGroupComponent.InitializeContext(tabs[tabId].FieldControl, tabs[tabId].TableInfo);
                var tasks = rawData.Result.Rows.Cast<DataRow>().Select(async row => await GetRow(fieldDefinitions, row, tabs[tabId], rawData.IsRetrievedOnline, cancellationToken));
                result.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
            }

            watch.Stop();
            _logService.LogDebug($"End RecordListViewDataAsync. Execution Time: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public async Task<List<ListDisplayRow>> RecordListViewDataPageAsync(int tabId, int startIndex, int endIndex, CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start RecordListViewDataAsync");
            List<ListDisplayRow> result = new List<ListDisplayRow>();

            if (tabId >= tabs.Count)
            {
                return result;
            }

            var watch = new Stopwatch();
            watch.Start();

            DataResponse rawData = tabs[tabId].RawData;
            if (rawData.Result != null)
            {
                int end = endIndex > rawData.Result.Rows.Count ? rawData.Result.Rows.Count : endIndex;

                List<FieldControlField> fieldDefinitions = tabs[tabId].FieldControl.Tabs[0].GetQueryFields();
                _fieldGroupComponent.InitializeContext(tabs[tabId].FieldControl, tabs[tabId].TableInfo);
                for (int i = startIndex; i < end; i++)
                {
                    result.Add(await GetRow(fieldDefinitions, rawData.Result.Rows[i], tabs[tabId], rawData.IsRetrievedOnline, cancellationToken));
                }
            }

            watch.Stop();
            _logService.LogDebug($"End RecordListViewDataAsync. Execution Time: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public (FieldGroupComponent, List<FieldControlField>) GetFieldConfig(int tabId)
        {
            _logService.LogDebug("Start GetFieldConfigAsync");
            if (tabId >= tabs.Count)
            {
                return (null, null);
            }
            List<FieldControlField> fieldDefinitions = tabs[tabId].FieldControl.Tabs[0].GetQueryFields();
            _fieldGroupComponent.InitializeContext(tabs[tabId].FieldControl, tabs[tabId].TableInfo);
            _logService.LogDebug("End GetFieldConfigAsync");
            return (_fieldGroupComponent, fieldDefinitions);
        }

        public async Task<ObservableCollection<DocumentObject>> DocumentViewDataAsync(int tabId, CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start DocumentViewDataAsync");
            ObservableCollection<DocumentObject> result = new ObservableCollection<DocumentObject>();

            if (tabId >= tabs.Count)
            {
                return result;
            }

            DataResponse rawData = tabs[tabId].RawData;
            if (rawData.Result != null)
            {

                List<FieldControlField> fieldDefinitions = tabs[tabId].FieldControl.Tabs[0].GetQueryFields();
                _fieldGroupComponent.InitializeContext(tabs[tabId].FieldControl, tabs[tabId].TableInfo);
                result = await _documentService.ProcessDocumentItemsAsync(fieldDefinitions, _fieldGroupComponent, rawData.Result, null, cancellationToken);
            }

            _logService.LogDebug("End DocumentViewDataAsync");
            return result;
        }

        private async Task<ListDisplayRow> GetRow(List<FieldControlField> fieldDefinitions, DataRow row, TabDataWithConfig tab, bool isRetrievedOnline, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ListDisplayRow outRow = await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, _action?.AreSectionsEnabled() ?? false, cancellationToken);
            ExtractRowLeftMargin(row, tab, outRow);
            outRow.IsRetrievedOnline = isRetrievedOnline;
            return outRow;
        }

        private void ExtractRowLeftMargin(DataRow row, TabDataWithConfig tab, ListDisplayRow outRow)
        {
            outRow.RowDecorators = _expandComponent.GetRowDecorators(row, tab);
        }

        public async Task<ListDisplayRow> GetDisplayRow(List<FieldControlField> fieldDefinitions, DataRow row, TabDataWithConfig tab, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ListDisplayRow outRow = await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, _action?.AreSectionsEnabled() ?? false, cancellationToken);
            ExtractRowLeftMargin(row, tab, outRow);
            return outRow;
        }

        public async Task<UserAction> ActionForItemSelect(int tabId, ListDisplayRow selectedRecord, CancellationToken cancellationToken)
        {
            Debug.Assert(tabId < tabs.Count);
            string expandName = selectedRecord.RowDecorators.Expand != null ? selectedRecord.RowDecorators.Expand.UnitName : string.Empty;

            SearchAndList searchAndList = tabs[tabId].SearchAndList;
            if (searchAndList.DefaultAction == null
                || searchAndList.DefaultAction == String.Empty
                || searchAndList.DefaultAction.ToUpper().Equals("SETTINGS_SYNC"))
            {
                return new UserAction
                {
                    RecordId = selectedRecord.RecordId,
                    RawRecordId = selectedRecord.RawRecordId(),
                    IsRecordRetrievedOnline = AreResultsRetrievedOnline(tabId),
                    InfoAreaUnitName = InfoAreaUnitName(tabs[tabId].InfoArea, tabs[tabId].ActionTemplate, tabs[tabId].SearchAndList),
                    ActionUnitName = "SHOWRECORD",
                    ActionType = UserActionType.ShowRecord,
                    ResolvedExpandName = expandName
                };
            }

            Menu menu = await _configurationService.GetMenu(searchAndList.DefaultAction, cancellationToken);
            if (menu?.ViewReference != null)
            {
                UserAction userAction = _userActionBuilder.UserActionFromMenu(_configurationService,
                    menu, selectedRecord.RecordId,
                    InfoAreaUnitName(tabs[tabId].InfoArea, tabs[tabId].ActionTemplate, tabs[tabId].SearchAndList));
                userAction.ResolvedExpandName = expandName;
                userAction.RawRecordId = selectedRecord.RawRecordId();
                return userAction;
            }

            return new UserAction
            {
                RecordId = selectedRecord.RecordId,
                RawRecordId = selectedRecord.RawRecordId(),
                IsRecordRetrievedOnline = AreResultsRetrievedOnline(tabId),
                InfoAreaUnitName = InfoAreaUnitName(tabs[tabId].InfoArea, tabs[tabId].ActionTemplate, tabs[tabId].SearchAndList),
                ActionUnitName = searchAndList.DefaultAction,
                ActionType = UserActionType.Menu,
                ResolvedExpandName = expandName
            };
        }

        public async Task PrepareConfigurationAsync(string expand, CancellationToken cancellationToken)
        {
            _logService.LogDebug("Start PrepareConfigurationAsync");
            tabs.Clear();
            if (!string.IsNullOrEmpty(expand))
            {
                TabDataWithConfig tab = await PrepareTabData(_action, expand, cancellationToken);
                _infoArea = tab.InfoArea;
                tabs.Add(tab);
            }

            OnDataReady();
            _logService.LogDebug("End PrepareConfigurationAsync");
        }


        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            _logService.LogDebug($"Start PrepareContentAsync for action: {_action?.ActionUnitName}");
            tabs.Clear();
            if (_action != null)
            {
                if (_action.AdditionalArguments != null)
                {
                    _filterProcessor.SetAdditionalFilterParams(_action.AdditionalArguments);
                }
                TabDataWithConfig tab = await PrepareTabData(_action, cancellationToken);
                string infoAreaId = InfoAreaUnitName(tab.InfoArea, tab.ActionTemplate, tab.SearchAndList);

                if (tab.SearchAndList != null)
                {
                    if (!string.IsNullOrEmpty(infoAreaId))
                    {
                        _infoArea = tab.InfoArea;
                        string headerName = tab.SearchAndList.HeaderGroupName + ".Search";
                        Header header = await _configurationService.GetHeader(headerName, cancellationToken).ConfigureAwait(false);
                        _headerComponent.InitializeContext(header, _action);
                        _headerButtons = await _headerComponent.HeaderButtons(cancellationToken).ConfigureAwait(false);
                    }

                    // Initialize to first Tab in order to be able to get the correct PageTitle
                    _fieldGroupComponent.InitializeContext(tab.FieldControl, tab.TableInfo);
                    tabs.Add(tab);

                    if (_action.ActionType != UserActionType.RecordSelector)
                    {
                        List<UserAction> relatedInfoAreas = HeaderRelatedInfoAreas();
                        if (relatedInfoAreas.Count > 1)
                        {
                            for (int i = 1; i < relatedInfoAreas.Count; i++)
                            {
                                UserAction action = relatedInfoAreas[i];
                                tabs.Add(await PrepareTabData(action, cancellationToken));
                            }
                        }

                    }
                }
            }

            OnDataReady();
            _logService.LogDebug("End PrepareContentAsync");
        }

        protected async Task<TabDataWithConfig> PrepareTabData(UserAction action, CancellationToken cancellationToken)
        {
            TabDataWithConfig tab = new TabDataWithConfig();
            tab.ActionTemplate = await ActionTemplateUtility.ResolveActionTemplate(action, cancellationToken, _configurationService);

            tab.SearchAndList = await ResolveSearchAndListConfig(tab.ActionTemplate, cancellationToken).ConfigureAwait(false);

            string infoAreaId = InfoAreaUnitName(tab.InfoArea, tab.ActionTemplate, tab.SearchAndList);

            if (!string.IsNullOrEmpty(infoAreaId))
            {
                tab.InfoArea = _configurationService.GetInfoArea(infoAreaId);

                if (tab.SearchAndList == null)
                {
                    tab.SearchAndList = await _configurationService.GetSearchAndList(infoAreaId, cancellationToken).ConfigureAwait(false);
                }

                if (tab.SearchAndList == null)
                {
                    return tab;
                }

                tab.SearchControl = await _configurationService.GetFieldControl(tab.SearchAndList.FieldGroupName + ".Search",
                    cancellationToken).ConfigureAwait(false);

                tab.FieldControl = await _configurationService.GetFieldControl(tab.SearchAndList.FieldGroupName + ".List", cancellationToken).ConfigureAwait(false);

                tab.TableInfo = await _configurationService.GetTableInfoAsync(InfoAreaUnitName(tab.InfoArea, tab.ActionTemplate, tab.SearchAndList),
                    cancellationToken).ConfigureAwait(false);
                await _filterProcessor.ExtractTabFilters(tab, cancellationToken);
            }

            return tab;
        }

        protected async Task<TabDataWithConfig> PrepareTabData(UserAction action, string expand, CancellationToken cancellationToken)
        {
            TabDataWithConfig tab = new TabDataWithConfig();
            tab.ActionTemplate = await ActionTemplateUtility.ResolveActionTemplate(action, cancellationToken, _configurationService);
            tab.FieldControl = await _configurationService.GetFieldControl(expand, cancellationToken).ConfigureAwait(false);

            if (tab.FieldControl != null)
            {
                string infoAreaId = tab.FieldControl.InfoAreaId;
                tab.InfoArea = _configurationService.GetInfoArea(infoAreaId);
                tab.TableInfo = await _configurationService.GetTableInfoAsync(infoAreaId,
                    cancellationToken).ConfigureAwait(false);
                tab.EnabledDataFilters = new List<Filter>();
            }

            return tab;
        }

        protected async Task<SearchAndList> ResolveSearchAndListConfig(ActionTemplateBase actionTemplate, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(SearchAndListName))
            {
                return await _configurationService.GetSearchAndList(SearchAndListName, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return await _configurationService.GetSearchAndList(actionTemplate.ConfigName(), cancellationToken).ConfigureAwait(false);
            }
        }

        protected string InfoAreaUnitName(InfoArea infoArea, ActionTemplateBase actionTemplate, SearchAndList searchAndList)
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

        private List<FieldControlField> GetTabFields(TabDataWithConfig tab)
        {
            List<FieldControlField> fields = tab.FieldControl.Tabs[0].GetQueryFields();
            _expandComponent.InitializeContext(tab.InfoArea.UnitName, tab.InfoArea.UnitName, tab.TableInfo);
            List<FieldControlField> expandFields = _expandComponent.GetExpandRuleFields(tab.InfoArea.UnitName);
            foreach (var field in expandFields)
            {
                if (!fields.Exists(f => f.FieldId == field.FieldId && f.InfoAreaId == field.InfoAreaId))
                {
                    fields.Add(field);
                }
            }
            if (tab.ExternalFields != null && tab.ExternalFields.Count > 0)
            {
                foreach (var ertraField in tab.ExternalFields)
                {
                    fields.AddRange(tab.ExternalFields);
                }

            }
            return fields;
        }

        public async Task<bool> PerformSearch(int tabId, string searchValue, RequestMode requestMode, CancellationToken cancellationToken)
        {
            List<FieldControlField> searchFields = new List<FieldControlField>();
            if (!IsValidConfig(tabId))
            {
                return false;
            }

            if (tabs[tabId].SearchControl != null && tabs[tabId].SearchControl.Tabs.Count > 0)
            {
                searchFields = tabs[tabId].SearchControl.Tabs[0].GetQueryFields();
            }

            _fieldGroupComponent.InitializeContext(tabs[tabId].FieldControl, tabs[tabId].TableInfo);
            if (_fieldGroupComponent.HasTabs())
            {
                List<FieldControlField> fields = GetTabFields(tabs[tabId]);

                ParentLink parentLink = _userActionBuilder.GetParentLink(_action, tabs[tabId].ActionTemplate);

                string searchText = searchValue;
                if (_configurationService.GetBoolConfigValue("Search.ReplaceCaseSensitiveCharacters")
                    && !string.IsNullOrWhiteSpace(searchValue))
                {
                    Regex regex = new Regex(@"[^a-zA-Z0-9\s]", RegexOptions.None);
                    searchText = regex.Replace(searchValue, "?");
                }

                var filters = tabs[tabId].GetAllEnabledFilters();
                if (AdditionalFilter != null)
                {
                    filters.Add(AdditionalFilter);
                }
               
                tabs[tabId].RawData = await _crmDataService.GetData(cancellationToken,
                    new DataRequestDetails
                    {
                        TableInfo = tabs[tabId].TableInfo,
                        Fields = fields,
                        SortFields = tabs[tabId].FieldControl.SortFields,
                        SearchFields = searchFields,
                        SearchValue = searchText,
                        Filters = filters
                    },
                    parentLink, GetTemplateMaxResults(tabs[tabId].ActionTemplate), requestMode);

                return true;
            }

            return false;
        }

        public async Task<DataTable> GetRecords(int tabId, CancellationToken cancellationToken, ParentLink parentLink = null)
        {
            DataTable results = null;

            _fieldGroupComponent.InitializeContext(tabs[tabId].FieldControl, tabs[tabId].TableInfo);
            if (_fieldGroupComponent.HasTabs())
            {
                List<FieldControlField> fields = tabs[tabId].FieldControl.Tabs[0].GetQueryFields();
                if (tabs[tabId].ExternalFields != null && tabs[tabId].ExternalFields.Count > 0)
                {
                    fields.AddRange(tabs[tabId].ExternalFields);
                }

                if (parentLink == null)
                {
                    parentLink = _userActionBuilder.GetParentLink(_action, tabs[tabId].ActionTemplate);
                }

                var filters = tabs[tabId].GetAllEnabledFilters();
                if (AdditionalFilter != null)
                {
                    filters.Add(AdditionalFilter);
                }
                
                var requestMode = RequestMode.Best;

                if (ForceOnline.HasValue && ForceOnline.Value)
                {
                    requestMode = RequestMode.Online;
                }
                else if(ForceOnline.HasValue && !ForceOnline.Value)
                {
                    requestMode = RequestMode.Offline;
                }
                    
                tabs[tabId].RawData = await _crmDataService.GetData(cancellationToken,
                    new DataRequestDetails
                    {
                        TableInfo = tabs[tabId].TableInfo,
                        Fields = fields,
                        SortFields = tabs[tabId].FieldControl.SortFields,
                        Filters = filters
                    },
                    parentLink, 1000, requestMode);
                if (tabs[tabId].RawData.Result != null)
                {
                    results = tabs[tabId].RawData.Result;
                }

            }
            return results;
        }

        private int GetTemplateMaxResults(ActionTemplateBase actionTemplate)
        {
            int defaultMaxResults = 100;

            if (actionTemplate is RecordListViewTemplate recordListViewTemplate)
            {
                string actionTemplateValue = recordListViewTemplate.MaxResults();
                int maxResults;
                if (!string.IsNullOrWhiteSpace(actionTemplateValue) && int.TryParse(actionTemplateValue, out maxResults))
                {
                    if (maxResults <= 0)
                    {
                        return defaultMaxResults;
                    }

                    return maxResults;
                }

                var configurationValue = _configurationService.GetConfigValue("Search.MaxResults");
                if (configurationValue != null && !string.IsNullOrWhiteSpace(configurationValue.Value)
                    && int.TryParse(configurationValue.Value, out maxResults))
                {
                    if (maxResults <= 0)
                    {
                        return defaultMaxResults;
                    }

                    return maxResults;
                }
            }

            return defaultMaxResults;
        }

        public bool HasResults(int tabId)
        {
            if (tabId >= tabs.Count)
            {
                return false;
            }

            DataResponse rawData = tabs[tabId].RawData;
            if (rawData.Result != null && rawData.Result.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public int CountResults(int tabId)
        {
            if (tabId >= tabs.Count)
            {
                return 0;
            }

            DataResponse rawData = tabs[tabId].RawData;
            if (rawData != null && rawData.Result != null && rawData.Result.Rows.Count > 0)
            {
                return rawData.Result.Rows.Count;
            }

            return 0;
        }

        public bool AreResultsRetrievedOnline(int tabId)
        {
            if (tabId >= tabs.Count)
            {
                return false;
            }

            if (tabs[tabId].RawData != null)
            {
                return tabs[tabId].RawData.IsRetrievedOnline;
            }

            return false;
        }

        public async Task<List<ListDisplayRow>> PrepareClildRecordsAsync(PanelData inputPanelData, CancellationToken token)
        {
            List<ListDisplayRow> records = new List<ListDisplayRow>();
            try
            {
                string[] typeParts = inputPanelData.PanelTypeKey.Split('_');
                string searchAndListConfigurationName;
                int linkId = -1;

                if (typeParts.Length > 1)
                {
                    int maxResults = GetChildRecordMaxResults(typeParts);

                    searchAndListConfigurationName = typeParts[1];
                    string[] configParts = searchAndListConfigurationName.Split('#');


                    if (configParts.Length > 1)
                    {
                        searchAndListConfigurationName = configParts[0];
                        if (!int.TryParse(configParts[1], out linkId))
                        {
                            linkId = -1;
                        }
                    }
                    _logService.LogDebug($"Child Panel SearchAndList {inputPanelData.PanelTypeKey} with LinkID {linkId}");

                    TabDataWithConfig tab = new TabDataWithConfig();
                    tab.ActionTemplate = await ActionTemplateUtility.ResolveActionTemplate(inputPanelData.action, token, _configurationService);
                    tab.SearchAndList = await _configurationService.GetSearchAndList(searchAndListConfigurationName, token).ConfigureAwait(false);

                    string infoAreaId = tab.SearchAndList?.InfoAreaId;

                    if (!string.IsNullOrEmpty(infoAreaId))
                    {
                        tab.InfoArea = _configurationService.GetInfoArea(infoAreaId);
                        _infoArea = tab.InfoArea;

                        tab.FieldControl = await _configurationService.GetFieldControl(tab.SearchAndList.FieldGroupName + ".List", token).ConfigureAwait(false);

                        tab.TableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, token).ConfigureAwait(false);

                        if (tab.FieldControl != null)
                        {
                            _fieldGroupComponent.InitializeContext(tab.FieldControl, tab.TableInfo);
                            if (_fieldGroupComponent.HasTabs())
                            {
                                List<FieldControlField> fields = GetTabFields(tab);
                                ParentLink parentLink = new ParentLink
                                {
                                    LinkId = linkId,
                                    ParentInfoAreaId = inputPanelData.RecordInfoArea,
                                    RecordId = inputPanelData.RecordId,
                                };

                                await _filterProcessor.ExtractTabFilters(tab, token);

                                tab.RawData = await _crmDataService.GetData(token,
                                    new DataRequestDetails
                                    {
                                        TableInfo = tab.TableInfo,
                                        Fields = fields,
                                        SortFields = tab.FieldControl.SortFields,
                                        Filters = tab.GetAllEnabledFilters()
                                    },
                                    parentLink, maxResults, RequestMode.Best);

                                if (tab.RawData.Result != null)
                                {
                                    var tasks = tab.RawData.Result.Rows.Cast<DataRow>().Select(async row => await GetRow(fields, row, tab, tab.RawData.IsRetrievedOnline, token));
                                    records.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
                                }

                                tabs.Add(tab);
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logService.LogDebug($"Failed PrepareClildRecordsAsync with Exception:{ex.Message}");
            }
            return records;
        }

        public async Task<List<ListDisplayRow>> PrepareRecordsAsync(FormItemData formItemData, CancellationToken token)
        {
            List<ListDisplayRow> records = new List<ListDisplayRow>();
            if (formItemData != null && formItemData.FormItem !=null)
            {
                var formData = formItemData.FormItem;
                try
                {
                    string[] typeParts = formData.Func.Split(';');
                    int linkId = -1;

                    if (typeParts.Length > 2)
                    {
                        string searchAndListConfigurationName = typeParts[0];
                        string recordLink = typeParts[1];
                        string filterName = typeParts[2];
                        string param1 = string.Empty;
                        if (typeParts.Length > 3)
                        {
                            param1 = typeParts[3];
                        }

                        int maxResults = 5;

                        var keyPair = formData.OptionsDictionary();
                        if (keyPair != null && keyPair.ContainsKey("MaxResults"))
                        {
                            var objMaxResults = keyPair["MaxResults"];
                            if (!int.TryParse(objMaxResults.ToString(), out maxResults))
                            {
                                maxResults = 5;
                            }
                        }
                        ParentLink parentLink = null;

                        if (!string.IsNullOrWhiteSpace(recordLink) && (
                            recordLink.Equals("ID", StringComparison.InvariantCultureIgnoreCase)
                            || recordLink.Equals(".$", StringComparison.InvariantCultureIgnoreCase)
                            || recordLink.Equals("curRep", StringComparison.InvariantCultureIgnoreCase)
                            ))
                        {
                            parentLink = new ParentLink
                            {
                                LinkId = linkId,
                                ParentInfoAreaId = "ID",
                                RecordId = _sessionContext.User.SessionInformation.RepIdStr()
                            };
                        }

                        TabDataWithConfig tab = new TabDataWithConfig();
                        tab.ActionTemplate = await ActionTemplateUtility.ResolveActionTemplate(formItemData.Action, token, _configurationService);
                        tab.SearchAndList = await _configurationService.GetSearchAndList(searchAndListConfigurationName, token).ConfigureAwait(false);

                        string infoAreaId = tab.SearchAndList?.InfoAreaId;

                        if (!string.IsNullOrEmpty(infoAreaId))
                        {
                            tab.InfoArea = _configurationService.GetInfoArea(infoAreaId);
                            _infoArea = tab.InfoArea;

                            tab.FieldControl = await _configurationService.GetFieldControl(tab.SearchAndList.FieldGroupName + ".List", token).ConfigureAwait(false);
                            tab.TableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, token).ConfigureAwait(false);
                            await _filterProcessor.ExtractTabFilters(tab, token);

                            if (tab.FieldControl != null)
                            {
                                var filters = tab.GetAllEnabledFilters();
                                if (!string.IsNullOrWhiteSpace(filterName))
                                { 
                                    if (!string.IsNullOrWhiteSpace(param1) && formItemData?.FormParams?.Keys.Count > 0 && formItemData.FormParams.ContainsKey(param1))
                                    {
                                        var filterParamas = formItemData.FormParams[param1];
                                        _filterProcessor.SetAdditionalFilterParams(filterParamas);
                                        var filter = await _filterProcessor.RetrieveFilterDetails(filterName, token);
                                        filters.Add(filter);
                                    }

                                }

                                _fieldGroupComponent.InitializeContext(tab.FieldControl, tab.TableInfo);
                                if (_fieldGroupComponent.HasTabs())
                                {
                                    List<FieldControlField> fields = GetTabFields(tab);

                                    tab.RawData = await _crmDataService.GetData(token,
                                        new DataRequestDetails
                                        {
                                            TableInfo = tab.TableInfo,
                                            Fields = fields,
                                            SortFields = tab.FieldControl.SortFields,
                                            Filters = filters
                                        },
                                        parentLink, maxResults, RequestMode.Best);

                                    if (tab.RawData?.Result?.Rows?.Count > 0)
                                    {
                                        var tasks = tab.RawData.Result.Rows.Cast<DataRow>().Select(async row => await GetRow(fields, row, tab, tab.RawData.IsRetrievedOnline, token));
                                        records.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
                                    }
                                    tabs.Add(tab);
                                }
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    _logService.LogDebug($"Failed PrepareRecordsAsync with Exception:{ex.Message}");
                }
            }
            return records;
        }

        private int GetChildRecordMaxResults(string[] typeParts)
        {
            int maxResults = 100;
            int configurationMaxResults;

            if (typeParts.Length > 2 && !string.IsNullOrEmpty(typeParts[2]) && int.TryParse(typeParts[2], out configurationMaxResults))
            {
                if (configurationMaxResults > 0)
                {
                    return configurationMaxResults;
                }
            }

            var configurationValue = _configurationService.GetConfigValue("Search.MaxResults");
            if (configurationValue != null && int.TryParse(configurationValue.Value, out configurationMaxResults))
            {
                if (configurationMaxResults > 0)
                {
                    return configurationMaxResults;
                }
            }

            return maxResults;
        }

        public async Task<long> PrepareRecordCountAsync(UserAction action, RequestMode requestMode, CancellationToken cancellationToken)
        {
            long resultCount = 0;
            _action = action;

            if (_action == null)
            {
                return resultCount;
            }

            if (_action.AdditionalArguments != null)
            {
                SetAdditionalFilterParams(_action.AdditionalArguments);
            }

            TabDataWithConfig tab = await PrepareTabData(_action, cancellationToken);
            string infoAreaId = InfoAreaUnitName(tab.InfoArea, tab.ActionTemplate, tab.SearchAndList);

            if (string.IsNullOrEmpty(infoAreaId))
            {
                return resultCount;
            }

            tab.InfoArea = _configurationService.GetInfoArea(infoAreaId);
            _infoArea = tab.InfoArea;

            tab.FieldControl = await _configurationService.GetFieldControl(tab.SearchAndList.FieldGroupName + ".List", cancellationToken).ConfigureAwait(false);

            tab.TableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, cancellationToken).ConfigureAwait(false);

            if (tab.FieldControl == null)
            {
                return resultCount;
            }

            _fieldGroupComponent.InitializeContext(tab.FieldControl, tab.TableInfo);
            if (_fieldGroupComponent.HasTabs())
            {
                List<FieldControlField> fields = tab.FieldControl.Tabs[0].GetQueryFields();
                ParentLink parentLink = _userActionBuilder.GetParentLink(_action, tab.ActionTemplate);

                var filters = tab.GetAllEnabledFilters();

                tab.RawData = await _crmDataService.GetData(cancellationToken,
                    new DataRequestDetails
                    {
                        TableInfo = tab.TableInfo,
                        Fields = fields,
                        SortFields = tab.FieldControl.SortFields,
                        Filters = filters
                    },
                    parentLink, -1, requestMode);

                if (tab?.RawData?.Result != null
                    && tab?.RawData?.Result?.Rows != null
                    && tab?.RawData?.Result.Rows.Count > 0)
                {
                    if (tab.RawData.Result.Rows[0].Table.Columns.Contains("Count"))
                    {
                        resultCount = tab.RawData.Result.Rows[0].GetIntColumnValue("Count", 0);
                    }
                    else
                    {
                        resultCount = tab.RawData.Result.Rows.Count;
                    }
                }
                tabs.Add(tab);
            }

            return resultCount;
        }

        public double SearchDelay(bool isOfflineMode)
        {
            string configParameter = "Search.OnlineDelayTime";

            if (isOfflineMode)
            {
                configParameter = "Search.OfflineDelayTime";
            }

            return _configurationService.GetNumericConfigValue<double>(configParameter, CrmConstants.DefaultSearchDelayTime);
        }

        public bool SearchAutoSwitchToOffline()
        {
            return _configurationService.GetBoolConfigValue("Search.AutoSwitchToOffline");
        }

        public async Task<Dictionary<string, (string,string)>> GetFunctionMappingForFirstRecord(int tabId, CancellationToken cancellationToken)
        {
            Dictionary<string, (string,string)> results = new Dictionary<string, (string, string)>();
            _fieldGroupComponent.InitializeContext(tabs[tabId].FieldControl, tabs[tabId].TableInfo);

            if (_fieldGroupComponent.HasTabs())
            {
                List<FieldControlField> fields = tabs[tabId].FieldControl.Tabs[0].GetQueryFields();
                if (tabs[tabId].ExternalFields != null && tabs[tabId].ExternalFields.Count > 0)
                {
                    fields.AddRange(tabs[tabId].ExternalFields);
                }

                var parentLink = _userActionBuilder.GetParentLink(_action, tabs[tabId].ActionTemplate);

                tabs[tabId].RawData = await _crmDataService.GetData(cancellationToken,
                    new DataRequestDetails
                    {
                        TableInfo = tabs[tabId].TableInfo,
                        Fields = fields,
                        SortFields = tabs[tabId].FieldControl.SortFields,
                        Filters = tabs[tabId].GetAllEnabledFilters()
                    },
                    parentLink, 1, RequestMode.Best);

                if (tabs[tabId].RawData.Result != null && tabs[tabId].RawData.Result?.Rows.Count > 0)
                {
                    var row = tabs[tabId].RawData.Result?.Rows[0];

                    foreach (var field in fields)
                    {
                        var key = field.Function;
                        var rawValue = await _fieldGroupComponent.ExtractFieldRawValue(field, row, cancellationToken);
                        var displayValue = await _fieldGroupComponent.ExtractFieldValue(field, row, cancellationToken);
                        FieldInfo fieldInfo = await _configurationService.GetFieldInfo(tabs[tabId].TableInfo, field, cancellationToken).ConfigureAwait(false);
                        if (fieldInfo.IsCatalog && rawValue.Equals("0"))
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(key) && !results.ContainsKey(key))
                        {
                            results.Add(key, (rawValue, displayValue));
                        }
                    }
                }

            }
            return results;
        }

        public async Task<List<ListDisplayRow>> GetQuickSearchResult(string globalSearchText, QuickSearchInfoAreaData quickSearchInfoAreaData, CancellationToken token)
        {
            List<ListDisplayRow> searchResults = new List<ListDisplayRow>();
            TabDataWithConfig tab = new TabDataWithConfig
            {
                TableInfo = quickSearchInfoAreaData.TableInfo,
                InfoArea = quickSearchInfoAreaData.InfoArea,
                ActionTemplate = quickSearchInfoAreaData.ActionTemplate,
                FieldControl = quickSearchInfoAreaData.FieldControl,
                SearchControl = quickSearchInfoAreaData.SearchControl

            };
            _fieldGroupComponent.InitializeContext(quickSearchInfoAreaData.FieldControl, quickSearchInfoAreaData.TableInfo);
            if (_fieldGroupComponent.HasTabs())
            {
                _expandComponent.InitializeContext(tab.InfoArea.UnitName, tab.InfoArea.UnitName, tab.TableInfo);
                List<FieldControlField> fields = quickSearchInfoAreaData.FieldControl.Tabs[0].GetQueryFields();
                List<FieldControlField> searchFields = quickSearchInfoAreaData.SearchControl != null
                    ? quickSearchInfoAreaData.SearchControl.Tabs[0].GetQueryFields()
                    : new List<FieldControlField>();

                ParentLink parentLink = null;

                string searchText = globalSearchText;
                if (_configurationService.GetBoolConfigValue("Search.ReplaceCaseSensitiveCharacters")
                    && !string.IsNullOrWhiteSpace(globalSearchText))
                {
                    Regex regex = new Regex(@"[^a-zA-Z0-9\s]", RegexOptions.None);
                    searchText = regex.Replace(globalSearchText, "?");
                }

                var rawData = await _crmDataService.GetData(token,
                    new DataRequestDetails
                    {
                        TableInfo = quickSearchInfoAreaData.TableInfo,
                        Fields = fields,
                        SearchFields = searchFields,
                        SearchValue = searchText
                    },
                    parentLink, 1000, RequestMode.Offline);

                if (rawData.Result != null && rawData.Result?.Rows?.Count>0)
                {
                    var tasks = rawData.Result.Rows.Cast<DataRow>().Select(async row => await GetRow(fields, row, tab, rawData.IsRetrievedOnline, token));
                    searchResults.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
                }
            }

            return searchResults;
        }

        public int GetTargetInfoAreaLinkId(int tabId, string targetInfoAreaId)
        {
            (LinkInfo linkInfo, TableInfo relatedTable) = _crmDataFieldResolver.GetLinkInfo(tabs[tabId].TableInfo, targetInfoAreaId, -1);

            if (linkInfo != null)
            {
                return linkInfo.LinkId;
            }

            return -1;
        }

        public bool IsRowCountDisplayActive(int tabId)
        {
            if (_configurationService.GetConfigValue("RecordListView.DisplayRecCount") != null)
            {
                bool isActive = _configurationService.GetBoolConfigValue("RecordListView.DisplayRecCount");
                if(!isActive)
                {
                    return false;
                }
            }

            if(tabId < tabs.Count
                && tabId > -1
                && tabs[tabId].ActionTemplate != null)
            {
                return tabs[tabId].ActionTemplate.DisplayRecCount();
            }

            return true;
        }
    }
}
