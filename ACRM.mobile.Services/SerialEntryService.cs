using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.SerialEntry;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services
{
    public class SerialEntryService : ContentServiceBase, ISerialEntryService
    {
        bool rowLineShowEndPrice = false;
        bool computePriceForQuantity1 = false;
        List<ListDisplayRow> SourceChildData;
        ActionTemplateBase actionTemplate;
        private Dictionary<string, (string,string)> _sourceFunctionMapping;
        private Dictionary<string, string> _additionalSearchParams = new Dictionary<string, string>();
        protected readonly FieldGroupComponent _destFieldGroupComponent;
        protected List<FieldControlField> _destFields;
        protected readonly ISerialEntryEditService _destinationEditServic;
        protected ISerialEntryEditService _destinationChildEditServic;
        protected ISerialEntryEditService _destinationRootEditServic;
        protected readonly ISearchContentService _sourceSearchService;
        protected ISearchContentService _sourceCopySearchService;
        protected ISearchContentService _sourceChildSearchService;
        protected ISearchContentService _destinationSearchService;
        protected ISearchContentService _destinationChildSearchService;
        protected IPricingService _pricingService;
        protected IQuotaService _quotaService;
        
        protected IListingService _listingService;
        private UserAction _finishUserAction;
        private string _finishActionText;
        private string _finishActionIcon;
        private string _itemNumberFunctionName;
        private bool _hierarchicalPositionFilter;
        private readonly IDocumentService _documentService;
        protected List<Filter> Filters;
        public SerialEntryService(ISessionContext sessionContext,
           IConfigurationService configurationService,
           ICrmDataService crmDataService,
           ILogService logService,
           IUserActionBuilder userActionBuilder,
           HeaderComponent headerComponent,
           ImageResolverComponent imageResolverComponent,
           FieldGroupComponent fieldGroupComponent,
           IFilterProcessor filterProcessor,
           IDocumentService documentService,
            FieldGroupComponent destFieldGroupComponent,
            ISerialEntryEditService destinationEditServic,
            ISerialEntryEditService destinationChildEditServic,
            ISerialEntryEditService destinationRootEditServic,
            ISearchContentService sourceSearchService,
            ISearchContentService sourceChildSearchService,
            ISearchContentService destinationSearchService,
            ISearchContentService destinationChildSearchService,
            ISearchContentService sourceCopySearchService,
            IPricingService pricingService,
            IListingService listingService,
            IQuotaService quotaService) : base(sessionContext, configurationService,
               crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent,
               imageResolverComponent, filterProcessor)
        {
            _destinationEditServic = destinationEditServic;
            _destinationChildEditServic = destinationChildEditServic;
            _destinationRootEditServic = destinationRootEditServic;
            _sourceSearchService = sourceSearchService;
            _sourceChildSearchService = sourceChildSearchService;
            _documentService = documentService;
            _destFieldGroupComponent = destFieldGroupComponent;
            _destinationSearchService = destinationSearchService;
            _destinationChildSearchService = destinationChildSearchService;
            _sourceCopySearchService = sourceCopySearchService;
            _listingService = listingService;
            _pricingService = pricingService;
            _quotaService = quotaService;
        }

        private bool hasItemNumberFunctionName
        {
            get => !string.IsNullOrEmpty(_itemNumberFunctionName);
        }

        public bool HierarchicalPositionFilter
        {
            get => _hierarchicalPositionFilter;
        }
        public ISearchContentService SourceSearchService
        {
            get => _sourceSearchService;
        }

        public IListingService ListingService
        {
            get => _listingService;
        }

        public UserAction FinishUserAction
        {
            get => _finishUserAction;
        }

        public string FinishActionText
        {
            get => _finishActionText;
        }

        public string FinishActionIcon
        {
            get => _finishActionIcon;
        }

        public ISerialEntryEditService DestinationRootEditService
        {
            get => _destinationRootEditServic;
        }

        public bool HasChildRecords
        {
            get => (_destinationChildEditServic != null && SourceChildData != null && SourceChildData.Count > 0)? true : false;
        }

        public (string,string) Currency
        {
            get => (_sourceFunctionMapping?.ContainsKey("Currency")).Value ? _sourceFunctionMapping["Currency"] : (string.Empty,string.Empty);
        }
        public async Task<ObservableCollection<UserAction>> GetTabItems(CancellationToken token)
        {
            var destinationConfigName = _action?.ViewReference?.GetArgumentValue("DestinationConfigName");
            if (!string.IsNullOrWhiteSpace(destinationConfigName))
            {
                var destConfig = _configurationService.GetExpand(destinationConfigName);
                if (destConfig != null)
                {
                    string headerName = destConfig.HeaderGroupName + ".Update";
                    Header header = await _configurationService.GetHeader(headerName, token).ConfigureAwait(false);
                    _headerComponent.InitializeContext(header, _action);
                }
            }
            int actionId = 1;
            var Items = new ObservableCollection<UserAction>();
            var userAction = new UserAction(_action)
            {
                ActionTaget = UserActionTarget.Tab,
                ActionType = UserActionType.SerialEntryListing,
                IsSelected = true,
                Id = actionId
            };

            if (!string.IsNullOrWhiteSpace(_headerComponent?.Header?.Label))
            {
                userAction.ActionDisplayName = _headerComponent?.Header?.Label;
            }

            Items.Add(userAction);
            var actions = _headerComponent?.HeaderRelatedAction();
            if (actions != null && actions.Count > 0)
            {
                foreach (var action in actions)
                {
                    action.Id = ++actionId;
                    Items.Add(action);
                }

            }
            return Items;
        }
        public async Task<Dictionary<string, string>> GetFilters(int tabId, CancellationToken token)
        {
            var tab = _sourceSearchService.GetTabData(tabId);
            Dictionary<string, string> results = new Dictionary<string, string>();
            List<string> filterNames = _filterProcessor.RetrieveUserFiltersNames(tab.ActionTemplate);
            Filters = await _filterProcessor.RetrieveFilterDetails(filterNames, token);
            if (filterNames != null && Filters != null && Filters.Count > 0)
            {
                foreach (var filterName in filterNames)
                {
                    var filter = Filters.FirstOrDefault(f => f.UnitName.Equals(filterName, StringComparison.InvariantCultureIgnoreCase));
                    if (!results.ContainsKey(filterName))
                    {
                        if (filter != null)
                        {
                            results.Add(filterName, filter.DisplayName);
                        }
                        else if (filterName.EndsWith(":All", StringComparison.InvariantCultureIgnoreCase))
                        {
                            results.Add(filterName, "All");
                        }
                    }
                }
            }

            return results;

        }

        public async Task<List<string>> GetPositionFilters(CancellationToken token)
        {
            var tab = _sourceSearchService.GetTabData(0);
            Dictionary<string, string> results = new Dictionary<string, string>();
            List<string> filterNames = _filterProcessor.RetrievePositionFilterNames(tab.ActionTemplate);
          
            return filterNames;
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            actionTemplate = await ActionTemplateUtility.ResolveActionTemplate(_action, cancellationToken, _configurationService);
            if (actionTemplate is SerialEntryTemplate serialEntryAction)
            {
                if (_configurationService.GetConfigValue("SerialEntry.RowLineShowEndPrice") != null)
                {
                    rowLineShowEndPrice = _configurationService.GetBoolConfigValue("SerialEntry.RowLineShowEndPrice", false);
                }

                if (_configurationService.GetConfigValue("SerialEntry.UnitPriceCheckPriceScaleFor1") != null)
                {
                    computePriceForQuantity1 = _configurationService.GetBoolConfigValue("SerialEntry.UnitPriceCheckPriceScaleFor1", false);
                }

                _hierarchicalPositionFilter = serialEntryAction.GetHierarchicalPositionFilter();

                _itemNumberFunctionName = serialEntryAction.GetItemNumberFunctionName();

                // Prepare SourceCopyFieldGroup
                var sourceCopyFieldGroup = serialEntryAction.SourceCopyFieldGroup();
                if (!string.IsNullOrWhiteSpace(sourceCopyFieldGroup))
                {
                    _sourceCopySearchService.SetSourceAction(_action);
                    await _sourceCopySearchService.PrepareConfigurationAsync(sourceCopyFieldGroup + ".List", cancellationToken);
                    _sourceFunctionMapping = await _sourceCopySearchService.GetFunctionMappingForFirstRecord(0, cancellationToken);
                    if (_sourceFunctionMapping?.Keys.Count > 0)
                    {
                        foreach (var key in _sourceFunctionMapping.Keys)
                        {
                            _additionalSearchParams.Add(key, _sourceFunctionMapping[key].Item1);
                        }
                        var currencyKey = Currency.Item1;
                    }
                }

                // PrepareContent Source
                _sourceSearchService.SearchAndListName = serialEntryAction.SourceConfigName();
                _sourceSearchService.SetSourceAction(_action);
                await _sourceSearchService.PrepareContentAsync(cancellationToken);

                // PrepareContent SourceChildConfigName & DestinationChildConfigName
                var srcChildConfig = serialEntryAction.SourceChildConfigName();
                var destChildConfig = serialEntryAction.DestinationChildConfigName();
                if (!string.IsNullOrEmpty(srcChildConfig) && !string.IsNullOrEmpty(destChildConfig))
                {
                    _sourceChildSearchService.SetSourceAction(_action);
                    await _sourceChildSearchService.PrepareConfigurationAsync(srcChildConfig + ".List", cancellationToken);
                    _destinationChildEditServic.FieldGroupName = destChildConfig;
                    _destinationChildEditServic.SetSourceAction(_action);
                    await _destinationChildEditServic.PrepareContentAsync(cancellationToken);
                    _destinationChildSearchService.SetSourceAction(_action);
                    await _destinationChildSearchService.PrepareConfigurationAsync(destChildConfig + ".Edit", cancellationToken);
                    var externalFields = new List<FieldControlField>();
                    var field = _sourceChildSearchService?.GetTabData(0)?.FieldControl?.Tabs[0]?.GetQueryFields()
                        .Where(a => a.InfoAreaId.Equals(_sourceChildSearchService?.GetTabData(0).FieldControl.InfoAreaId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (field != null)
                    {
                        externalFields.Add(field);
                    }
                    if (_destinationChildSearchService.GetTabData(0) != null)
                    {
                        _destinationChildSearchService.GetTabData(0).ExternalFields = externalFields;
                    }

                }
                else
                {
                    _sourceChildSearchService = null;
                    _destinationChildEditServic = null;
                }

                // PrepareContent DetinationRootConfiguration
                var destRootConfig = serialEntryAction.DestinationRootConfig();
                if (!string.IsNullOrEmpty(destRootConfig))
                {
                    _destinationRootEditServic.FieldGroupName = destRootConfig;
                    _destinationRootEditServic.SetSourceAction(_action);
                    await _destinationRootEditServic.PrepareContentAsync(cancellationToken);
                }
                else
                {
                    _destinationRootEditServic = null;
                }

                // Prepare FinishAction
                var acrionName = serialEntryAction.GetFinishAction();
                if(!string.IsNullOrWhiteSpace(acrionName))
                {
                    if(acrionName.StartsWith("Button:"))
                    {
                        var button = acrionName.Replace("Button:", "");
                        var ActionButton = await _configurationService.GetButton(button, cancellationToken);
                        if (ActionButton != null)
                        {
                            var _buttonUserAction = _userActionBuilder.UserActionFromButton(_configurationService, ActionButton, _action?.RecordId, _action?.SourceInfoArea);
                            if (_buttonUserAction != null)
                            {
                                _buttonUserAction.SourceInfoArea = _action?.SourceInfoArea;
                                _finishActionText = _buttonUserAction.ActionDisplayName;
                                _finishActionIcon = _buttonUserAction.DisplayGlyphImageText;
                                _finishUserAction = _buttonUserAction;
                                
                            }
                        }
                    }
                    else if (acrionName.StartsWith("Menu:"))
                    {
                        var menuName = acrionName.Replace("Menu:", "");
                        Menu menuItem = await _configurationService.GetMenu(menuName, cancellationToken);
                        if (menuItem?.ViewReference != null)
                        {
                            UserAction userAction = _userActionBuilder.UserActionFromMenu(_configurationService,
                                menuItem, _action?.RecordId, _action?.SourceInfoArea);
                            if (userAction != null)
                            {
                                userAction.SourceInfoArea = _action?.SourceInfoArea;
                                var (imageName, glyphText) = _imageResolverComponent.ExtractImage(_configurationService, menuItem.ImageName);
                                _finishActionText = menuItem.DisplayName;
                                _finishActionIcon = glyphText;
                                _finishUserAction = userAction;
                            }

                        }
                     
                    }
                    else
                    {
                        var menuName = acrionName;
                        Menu menuItem = await _configurationService.GetMenu(menuName, cancellationToken);
                        if (menuItem?.ViewReference != null)
                        {
                            UserAction userAction = _userActionBuilder.UserActionFromMenu(_configurationService,
                                menuItem, _action?.RecordId, _action?.SourceInfoArea);
                            if (userAction != null)
                            {
                                userAction.SourceInfoArea = _action?.SourceInfoArea;
                                var (imageName, glyphText) = _imageResolverComponent.ExtractImage(_configurationService, menuItem.ImageName);
                                _finishActionText = menuItem.DisplayName;
                                _finishActionIcon = glyphText;
                                _finishUserAction = userAction;
                            }

                        }
                    }

                }
                

                // PrepareContent Detination
                _destinationEditServic.SetSourceAction(_action);
                await _destinationEditServic.PrepareContentAsync(cancellationToken);

                // PrepareContent Listing
                _listingService.SetSourceAction(_action);
                await _listingService.PrepareContentAsync(cancellationToken);

                // Prepare for Pricing
                _pricingService.SetSourceAction(_action);
                _pricingService.SetAdditionalParams(_additionalSearchParams);
                await _pricingService.PrepareContentAsync(cancellationToken);

                // Prepare for Quota Configuration
                _quotaService.SetSourceAction(_action);
                _quotaService.SetAdditionalParams(_additionalSearchParams);
                await _quotaService.PrepareContentAsync(cancellationToken);
            }
        }

        public async Task<List<SerialEntryItem>> ResultDataAsync(int tabId, CancellationToken token)
        {
            var tab = _sourceSearchService.GetTabData(tabId);
            _logService.LogDebug("Start ResultDataAsync");
            List<SerialEntryItem> results = new List<SerialEntryItem>();

            if (tab == null)
            {
                return results;
            }
            var destinationData = await LoadDetinationRecordsAsync(token);
            await LoadSourceChildRecordsAsync(token);
            DataResponse sourceRawData = tab.RawData;
            if (sourceRawData.Result != null)
            {

                List<FieldControlField> fieldDefinitions = tab.FieldControl.Tabs[0].GetQueryFields();
                _fieldGroupComponent.InitializeContext(tab.FieldControl, tab.TableInfo);
                var tasks = sourceRawData.Result.Rows.Cast<DataRow>().Select(async row => await GetRow(fieldDefinitions, _fieldGroupComponent, tab.ExternalFields, row, destinationData, token));
                var items = await Task.WhenAll(tasks).ConfigureAwait(false);
                if (items != null && items.Length > 0)
                {
                    foreach (var item in items)
                    {
                        results.AddRange(item);
                    }
                }
            }

            _logService.LogDebug("End ResultDataAsync");
            return results;

        }

        public async Task LoadSourceChildRecordsAsync(CancellationToken token)
        {
            SourceChildData = null;
            if (_sourceChildSearchService != null)
            {
                ParentLink _parentLink = new ParentLink
                {
                    LinkId = -1,
                    ParentInfoAreaId = _action.SourceInfoArea,
                    RecordId = _action.RecordId,
                };

                var results = await _sourceChildSearchService?.GetRecords(0, token, _parentLink);
                if (results != null)
                {
                    SourceChildData = await _sourceChildSearchService.RecordListViewDataAsync(0, token);
                }
            }
        }

        public async Task<DataTable> LoadDetinationRecordsAsync(CancellationToken token)
        {
            DataTable results = null;
            int linkId = -1;

            if (actionTemplate is SerialEntryTemplate serialEntryAction)
            {
                var fieldGroupName = serialEntryAction.DestinationConfigName();
                var fieldControl = await _configurationService.GetFieldControl(fieldGroupName + ".Edit", token).ConfigureAwait(false);
                string infoAreaId = fieldControl.InfoAreaId;
                var tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, token).ConfigureAwait(false);

                if (fieldControl != null)
                {

                    _destFieldGroupComponent.InitializeContext(fieldControl, tableInfo);
                    if (_destFieldGroupComponent.HasTabs())
                    {
                        _destFields = fieldControl.Tabs[0].GetQueryFields();
                        ParentLink parentLink = new ParentLink
                        {
                            LinkId = linkId,
                            ParentInfoAreaId = _action.SourceInfoArea,
                            RecordId = _action.RecordId,
                        };

                        var rawData = await _crmDataService.GetData(token,
                            new DataRequestDetails
                            {
                                TableInfo = tableInfo,
                                Fields = _destFields,
                                SortFields = fieldControl.SortFields
                            },
                            parentLink, 1000, RequestMode.Best);

                        if (rawData.Result != null)
                        {
                            results = rawData.Result;
                        }

                    }
                }
            }

            return results;
        }

        private async Task<List<SerialEntryItem>> GetRow(List<FieldControlField> fieldDefinitions, FieldGroupComponent fieldGroupComponent, List<FieldControlField> externalFields, DataRow row, DataTable destinationData, CancellationToken cancellationToken)
        {
            var results = new List<SerialEntryItem>();
            var item = new SerialEntryItem();
            item.ShowRowLineEndPrice = rowLineShowEndPrice;
            var recId = row.GetColumnValue("recid", "-1");
            ListDisplayRow outRow = await _fieldGroupComponent.ExtractDisplayRow(fieldDefinitions, row, false, cancellationToken);
            item.RecordIdentification = recId;
            if (outRow?.Fields?.Count > 0)
            {
                item.Title = outRow?.Fields[0] != null ? outRow?.Fields[0].Data.StringData : string.Empty;
                item.SubTitle1 = outRow?.Fields?.Count > 1 && outRow?.Fields[1] != null ? outRow?.Fields[1].Data.StringData : string.Empty;
                item.SubTitle2 = outRow?.Fields?.Count > 2 && outRow?.Fields[2] != null ? outRow?.Fields[2].Data.StringData : string.Empty;

            }

            item.PackageCount = await row.GetColumnValue("StepSize", fieldDefinitions, fieldGroupComponent, 1, cancellationToken);
            if (item.PackageCount <= 0)
            {
                item.PackageCount = 1;
            }

            string itemNo = string.Empty;

            if (hasItemNumberFunctionName)
            {
                itemNo = await row.GetColumnValue(_itemNumberFunctionName, fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            }

            if (string.IsNullOrEmpty(itemNo))
            {
                itemNo = await row.GetColumnValue("ItemNumber", fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
                if (string.IsNullOrEmpty(itemNo))
                {
                    itemNo = await row.GetColumnValue("CopyItemNumber", fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
                }
            }
            
            item.ItemNumber = itemNo;

            var itemName = await row.GetColumnValue("ItemName", fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = await row.GetColumnValue("CopyItemName", fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            }
            item.ItemName = itemName;
            item.UnitPrice = await row.GetColumnValue<decimal>("UnitPrice", fieldDefinitions, fieldGroupComponent, 0, cancellationToken);

            item.Currency = await row.GetColumnValue("Currency", fieldDefinitions, fieldGroupComponent, Currency.Item2, cancellationToken);
            item.CurrencyCode = await row.GetColumnRawValue("Currency", fieldDefinitions, fieldGroupComponent, cancellationToken, Currency.Item1);
          
                
            var imageDocID = await row.GetColumnValue("SourceImage", fieldDefinitions, fieldGroupComponent, string.Empty, cancellationToken);
            if (!string.IsNullOrWhiteSpace(imageDocID))
            {
                item.FileImagePath = await _documentService.GetDocumentPath(imageDocID, cancellationToken);
            }

            if(externalFields?.Count >0)
            {
                item.SearchKeyPairs = new Dictionary<string, string>();
                foreach (var extraField in externalFields)
                {
                    var key = $"{extraField.InfoAreaId.ToLower()}_{extraField.FieldId}";
                    var value = await fieldGroupComponent.ExtractFieldRawValue(extraField, row, cancellationToken);
                    if(!item.SearchKeyPairs.ContainsKey(key))
                    {
                        item.SearchKeyPairs.Add(key, value);
                    }
                }
            }

            item.FunctionKeyPairs = new Dictionary<string, string>();
            foreach (var field in fieldDefinitions)
            {
                var key = field.Function;
                var value = await fieldGroupComponent.ExtractFieldRawValue(field, row, cancellationToken);
                if (!string.IsNullOrEmpty(key) && !item.FunctionKeyPairs.ContainsKey(key))
                {
                    item.FunctionKeyPairs.Add(key, value);
                }
            }
            var quntity = computePriceForQuantity1 ? 1 : item.PackageCount;

            await _pricingService.EvaluatePricing(item, quntity, cancellationToken);

            if(_quotaService.HasQuotaConfig)
            {
                item.Quota = await _quotaService.GetQuotaItem(item, cancellationToken);
            }

            var destDataRows = await getDestinationDataRow(item, destinationData, cancellationToken);

            if (destDataRows != null && destDataRows.Count > 0)
            {
                foreach (var destRow in destDataRows)
                {
                    var destItem = item.Copy();
                    destItem.DestRecordId = destRow.GetColumnValue("recid", "-1");
                    destItem.Quantity = await destRow.GetColumnValue<decimal>("Quantity", _destFields, _destFieldGroupComponent, 0, cancellationToken);
                    destItem.Discount = await destRow.GetColumnValue<decimal>("Discount", _destFields, _destFieldGroupComponent, destItem.Discount, cancellationToken);
                    destItem.FreeGoods = await destRow.GetColumnValue<decimal>("FreeGoods", _destFields, _destFieldGroupComponent, destItem.FreeGoods, cancellationToken);
                    destItem.UnitPrice = await destRow.GetColumnValue<decimal>("UnitPrice", _destFields, _destFieldGroupComponent, destItem.UnitPrice, cancellationToken);
                    destItem.Panels = new List<PanelData>();
                    destItem.Panels.Add(await _destinationEditServic.GetPanelAsync(destRow, cancellationToken));
                    results.Add(destItem);
                }
            }
            else
            {
                item.Panels = new List<PanelData>();
                item.Panels.Add(await _destinationEditServic.GetPanelAsync(null, cancellationToken));
                results.Add(item);
            }

            return results;
        }

        public async Task<SerialEntryItem> BuildDestinationChildPanels(SerialEntryItem destItem, CancellationToken cancellationToken)
        {
            if (_destinationChildEditServic != null && SourceChildData != null && SourceChildData.Count > 0)
            {
                var sourceChildInfoAreaId = _sourceChildSearchService.GetTabData(0).TableInfo.InfoAreaId.ToUpperInvariant();
                var childPanels = new List<PanelData>();
                DataTable results = null;
                if (!string.IsNullOrEmpty(destItem.DestRecordId))
                {
                    ParentLink parentLink = new ParentLink
                    {
                        ParentInfoAreaId = _destinationEditServic.InfoAreaId,
                        LinkId = -1,
                        RecordId = destItem.DestRecordId

                    };
                    results = await _destinationChildSearchService?.GetRecords(0, cancellationToken, parentLink);

                }

                for (int i = 0; i < SourceChildData.Count; i++)
                {
                    var sourceChild = SourceChildData[i];
                    var destRow = getDestinationChildDataRow(results, sourceChild);
                    var panel = await _destinationChildEditServic.GetPanelAsync(destRow, cancellationToken);
                    var copyValues = new Dictionary<string, string>();
                    List<string> ValuesToReplace = new List<string> { $"{i + 1}" };


                    panel.ParentLink = new OfflineRecordLink
                    {
                        RecordId = sourceChild.RecordId.CleanRecordId(),
                        InfoAreaId = sourceChildInfoAreaId,
                        LinkId = -1

                    };

                    foreach (var field in sourceChild?.Fields)
                    {
                        ValuesToReplace.Add(field.Data.StringData);
                        string fieldFunction = field?.Config?.FieldConfig?.Function;
                        if (!string.IsNullOrEmpty(fieldFunction) && !copyValues.ContainsKey(fieldFunction))
                        {
                            copyValues.Add(fieldFunction, field.Data.StringData);
                        }
                    }
                    panel.CopyValues = copyValues;
                    foreach (var editField in panel.Fields)
                    {
                        editField?.Config?.PresentationFieldAttributes?.SetFormatedLabel(ValuesToReplace);
                    }
                    childPanels.Add(panel);
                }

                destItem.ChildPanels = childPanels;
            }

            return destItem;
        }

        private DataRow getDestinationChildDataRow(DataTable results, ListDisplayRow sourceChild)
        {
            if (results != null && results.Rows.Count > 0)
            {
                var sourceChildInfoAreaId = _sourceChildSearchService.GetTabData(0).TableInfo.InfoAreaId.ToUpperInvariant();

                foreach (DataRow row in results.Rows)
                { 
                    var childSrcRecID = row.GetColumnValue($"{sourceChildInfoAreaId}_0_recId", "");
                    if (!string.IsNullOrWhiteSpace(childSrcRecID) && childSrcRecID.Equals(sourceChild.RecordId,StringComparison.InvariantCultureIgnoreCase))
                    {
                        return row;
                    }

                }

            }

            return null;
        }

        private async Task<List<DataRow>> getDestinationDataRow(SerialEntryItem item, DataTable destinationData, CancellationToken cancellationToken)
        {
            if (destinationData != null && destinationData.Rows.Count > 0)
            {
                var Field_CopyItemNumber = _destFields.Where(a =>
                (hasItemNumberFunctionName && a.Function.Equals(_itemNumberFunctionName, StringComparison.InvariantCultureIgnoreCase)) ||
                a.Function.Equals("CopyItemNumber", StringComparison.InvariantCultureIgnoreCase) ||
                a.Function.Equals("ItemNumber", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (Field_CopyItemNumber != null)
                {

                    string fieldName = Field_CopyItemNumber.QueryFieldName(false);
                    var results = destinationData.Rows.Cast<DataRow>().Where(x => x.Field<string>(fieldName) == item.ItemNumber).ToList();

                    return results;
                }
            }

            return null;
        }

        public async Task<bool> SaveItem(SerialEntryItem item, List<PanelData> rootPanels, CancellationToken token)
        {
            try
            {
                var RecordLinks = new List<OfflineRecordLink>();
                // Add the parent record link
                RecordLinks.Add(new OfflineRecordLink()
                {
                    InfoAreaId = _action.SourceInfoArea,
                    LinkId = -1,
                    RecordId = _action.RecordId.CleanRecordId()

                });
                // Add the source record link 
                RecordLinks.Add(new OfflineRecordLink()
                {
                    InfoAreaId = _sourceSearchService.GetTabData(0).TableInfo.InfoAreaId,
                    LinkId = -1,
                    RecordId = item.RecordIdentification.CleanRecordId()

                });

                if (_destinationRootEditServic != null && rootPanels != null && rootPanels.HasChanges())
                {
                    var RootResult = await _destinationRootEditServic.Save(rootPanels, token, _action.RecordId.CleanRecordId(), new List<OfflineRecordLink>());
                }

                var sourcePanels = item.Panels;
                if (sourcePanels.HasChanges())
                {
                    var result = await _destinationEditServic.Save(sourcePanels, token, item.DestRecordId, RecordLinks);
                    if (result != null && string.IsNullOrWhiteSpace(item.DestRecordId))
                    {
                        if (result.SavedRecord != null && !string.IsNullOrWhiteSpace(result.SavedRecord.RecordId))
                        {
                            item.DestRecordId = result.SavedRecord.RecordId;
                            await SaveChildRecords(RecordLinks, item, result.SavedRecord.InfoAreaId, token);
                        }
                        else
                        {
                            item.State = SerialEntryItemState.InErrorState;
                        }
                    }
                    else
                    {

                        await SaveChildRecords(RecordLinks, item, result.SavedRecord.InfoAreaId, token);
                        item.State = SerialEntryItemState.WithDestinationEntry;
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                item.State = SerialEntryItemState.InErrorState;
                _logService.LogDebug("Failed SaveItem, with ex:"+ ex.Message);
                return false;
            }
        }

        private async Task<bool> SaveChildRecords(List<OfflineRecordLink> recordLinks, SerialEntryItem item, string destinfoAreaId, CancellationToken token)
        {
            // Add the source record link
            recordLinks.Add(new OfflineRecordLink()
            {
                InfoAreaId = destinfoAreaId,
                LinkId = -1,
                RecordId = item.DestRecordId.CleanRecordId()
            });

            if (item.ChildPanels != null)
            {
                for (int i = 0; i < item.ChildPanels.Count; i++)
                {
                    var cpanel = item.ChildPanels[i];
                    if (cpanel.HasChanges(true))
                    {
                        var ChildLinks = new List<OfflineRecordLink>();
                        ChildLinks.AddRange(recordLinks);
                        if (cpanel.ParentLink != null)
                        {
                            ChildLinks.Add(cpanel.ParentLink);

                        }
                        var cresult = await _destinationChildEditServic.Save(new List<PanelData> { cpanel }, token, cpanel.RecordId, ChildLinks);

                        if (cresult != null && string.IsNullOrWhiteSpace(cpanel.RecordId))
                        {
                            if (cresult.SavedRecord != null && !string.IsNullOrWhiteSpace(cresult.SavedRecord.RecordId))
                            {
                                cpanel.RecordId = cresult.SavedRecord.RecordId;
                            }
                        }
                    }

                }
            }

            return true;
        }

        public async Task<bool> DeleteItem(SerialEntryItem item, CancellationToken token)
        {
            try
            {
                return await _destinationEditServic.Delete(item.DestRecordId, token);
            }

            catch (Exception ex)
            {
                item.State = SerialEntryItemState.InErrorState;
                _logService.LogDebug("Failed DeleteItem, with ex:" + ex.Message);
                return false;
            }
        }

        public async Task EvaluatePricing(SerialEntryItem serialEntryItem, CancellationToken cancellationToken)
        {
            await _pricingService?.EvaluatePricing(serialEntryItem, serialEntryItem.Quantity > 0? (int)serialEntryItem.Quantity : serialEntryItem.PackageCount, cancellationToken); ;
        }
    }
}
