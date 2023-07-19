using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.SerialEntry;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services
{
    public class ListingService : ContentServiceBase, IListingService
    {
        protected List<ListingItems> listingItems;
        protected Menu listingConfigMenu;
        private string[] distinctListingFunctionNames;
        protected readonly FieldGroupComponent _listingOwnerFieldGroupComponent;
        protected readonly FieldGroupComponent _listingRelationOwnerFieldGroupComponent;

        public ListingService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            FieldGroupComponent listingOwnerFieldGroupComponent,
            FieldGroupComponent listingRelationOwnerFieldGroupComponent) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _listingOwnerFieldGroupComponent = listingOwnerFieldGroupComponent;
            _listingRelationOwnerFieldGroupComponent = listingRelationOwnerFieldGroupComponent;
        }

        public bool HasListingItems
        {
            get => listingItems?.Count > 0;
        }

        public async override Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                var listingConfigName = _action.ViewReference.GetArgumentValue("ListingConfiguration");
                listingConfigMenu = await _configurationService.GetMenu(listingConfigName, cancellationToken);
                await LoadListingConfiguration(cancellationToken);
            }
        }

        private async Task LoadListingConfiguration(CancellationToken cancellationToken)
        {
            
            if (listingConfigMenu?.ViewReference != null)
            {
                var distinctListingFunctionNamesString = listingConfigMenu.ViewReference.GetArgumentValue("DistinctListingFunctionNames");
                distinctListingFunctionNames = distinctListingFunctionNamesString?.Split(',');
                var listingOwnerFieldGroupName = listingConfigMenu.ViewReference.GetArgumentValue("ListingOwnerFieldGroupName");
                var relatedListingOwnersConfigName = listingConfigMenu.ViewReference.GetArgumentValue("RelatedListingOwnersConfigName");
                var listingOwnerObject = await getListingOwner(listingOwnerFieldGroupName, cancellationToken);
                var listingRelatedOwnerObjects = await getListingRelatedOwners(relatedListingOwnersConfigName, listingOwnerObject, cancellationToken);
                var OwnerObjects = new List<ListingOwner> { listingOwnerObject };
                if (listingRelatedOwnerObjects?.Count > 0)
                {
                    OwnerObjects.AddRange(listingRelatedOwnerObjects);
                }
                listingItems = await getListingItems(listingConfigMenu, OwnerObjects, cancellationToken);
            }
        }

        private async Task<List<ListingItems>> getListingItems(Menu menu, List<ListingOwner> ownerObjects, CancellationToken cancellationToken)
        {
            var listingControlName = menu.ViewReference.GetArgumentValue("ListingControlName");

            List<ListingItems> results = new List<ListingItems>();

            if (!string.IsNullOrWhiteSpace(listingControlName) && ownerObjects?.Count > 0 && distinctListingFunctionNames?.Length > 0)
            {

                var SearchAndList = await _configurationService.GetSearchAndList(listingControlName, cancellationToken).ConfigureAwait(false);
                if (SearchAndList == null)
                {
                    return results;
                }
                var fieldControl = await _configurationService.GetFieldControl(SearchAndList.FieldGroupName + ".List", cancellationToken).ConfigureAwait(false);
                string infoAreaId = fieldControl.InfoAreaId;
                var tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, cancellationToken).ConfigureAwait(false);
                if (fieldControl != null)
                {

                    var enabledDataFilters = new List<Filter>();

                    if (!string.IsNullOrWhiteSpace(SearchAndList.FilterName))
                    {
                        var filterObj = await _filterProcessor.RetrieveFilterDetails(SearchAndList.FilterName, cancellationToken);
                        if (filterObj != null)
                        {
                            enabledDataFilters.Add(filterObj);
                        }

                    }

                    _fieldGroupComponent.InitializeContext(fieldControl, tableInfo);
                    if (_fieldGroupComponent.HasTabs())
                    {
                        var fieldDefinitions = fieldControl.Tabs[0].GetQueryFields();

                        foreach (var owner in ownerObjects)
                        {
                            ParentLink parentLink = new ParentLink
                            {
                                LinkId = -1,
                                ParentInfoAreaId = owner.InfoAreaId,
                                RecordId = owner.RecordIdentification,
                            };

                            var rawData = await _crmDataService.GetData(cancellationToken,
                                new DataRequestDetails
                                {
                                    TableInfo = tableInfo,
                                    Fields = fieldDefinitions,
                                    SortFields = fieldControl.SortFields,
                                    Filters = enabledDataFilters
                                },
                                parentLink, 1000, RequestMode.Best);

                            if (rawData.Result != null && rawData.Result.Rows.Count > 0)
                            {
                                var tasks = rawData.Result.Rows.Cast<DataRow>().Select(async row => await GetListingItemRow(fieldDefinitions, _fieldGroupComponent, row, owner, cancellationToken));
                                var items = await Task.WhenAll(tasks).ConfigureAwait(false);
                                if (items != null && items.Length > 0)
                                {
                                    foreach (var item in items)
                                    {
                                        if (item != null)
                                        {
                                            results.Add(item);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

            }

            return results;

        }

        private async Task<ListingItems> GetListingItemRow(List<FieldControlField> fieldDefinitions, FieldGroupComponent listingControlFieldGroupComponent, DataRow dataRow, ListingOwner listingOwnerObject, CancellationToken token)
        {
            if (dataRow == null || listingOwnerObject == null)
            {
                return null;
            }

            var recordIdentification = dataRow.GetColumnValue($"recId", "-1");

            if (string.IsNullOrWhiteSpace(recordIdentification))
            {
                return null;
            }
            ListingItems listingItem = new ListingItems();
            listingItem.RecordIdentification = recordIdentification;
            listingItem.SearchKeyPairs = new Dictionary<string, string>();
            foreach (var field in fieldDefinitions)
            {
                var key = field.Function;
                var value = await listingControlFieldGroupComponent.ExtractFieldRawValue(field, dataRow, token);
                if (!string.IsNullOrEmpty(key) && !listingItem.SearchKeyPairs.ContainsKey(key))
                {
                    listingItem.SearchKeyPairs.Add(key, value);
                }
            }

            foreach(var distinctListingFunctionName in distinctListingFunctionNames)
            {
                if(listingItem.SearchKeyPairs.ContainsKey(distinctListingFunctionName))
                {
                    if(!string.IsNullOrWhiteSpace(listingItem.SearchKeyPairs[distinctListingFunctionName]))
                    {
                        return listingItem;
                    }
                }
            }

            return null;
            
        }

        private async Task<List<ListingOwner>> getListingRelatedOwners(string relatedListingOwnersConfigName, ListingOwner listingOwnerObject, CancellationToken cancellationToken)
        {
            List<ListingOwner> results = new List<ListingOwner>();

            if (!string.IsNullOrWhiteSpace(relatedListingOwnersConfigName) && listingOwnerObject != null)
            {

                var SearchAndList = await _configurationService.GetSearchAndList(relatedListingOwnersConfigName, cancellationToken).ConfigureAwait(false);
                if (SearchAndList == null)
                {
                    return null;
                }
                var fieldControl = await _configurationService.GetFieldControl(SearchAndList.FieldGroupName + ".List", cancellationToken).ConfigureAwait(false);
                string infoAreaId = fieldControl.InfoAreaId;
                var tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, cancellationToken).ConfigureAwait(false);
                if (fieldControl != null)
                {

                    var enabledDataFilters = new List<Filter>();

                    if (!string.IsNullOrWhiteSpace(SearchAndList.FilterName))
                    {
                        var filterObj = await _filterProcessor.RetrieveFilterDetails(SearchAndList.FilterName, cancellationToken);
                        if (filterObj != null)
                        {
                            enabledDataFilters.Add(filterObj);
                        }

                    }

                    _listingRelationOwnerFieldGroupComponent.InitializeContext(fieldControl, tableInfo);
                    if (_listingOwnerFieldGroupComponent.HasTabs())
                    {
                        var fieldDefinitions = fieldControl.Tabs[0].GetQueryFields();
                        ParentLink parentLink = new ParentLink
                        {
                            LinkId = -1,
                            ParentInfoAreaId = listingOwnerObject.InfoAreaId,
                            RecordId = listingOwnerObject.RecordIdentification,
                        };

                        var rawData = await _crmDataService.GetData(cancellationToken,
                            new DataRequestDetails
                            {
                                TableInfo = tableInfo,
                                Fields = fieldDefinitions,
                                SortFields = fieldControl.SortFields,
                                Filters = enabledDataFilters
                            },
                            parentLink, 100, RequestMode.Best);

                        if (rawData.Result != null && rawData.Result.Rows.Count > 0)
                        {
                            var tasks = rawData.Result.Rows.Cast<DataRow>().Select(async row => await GetRelatedOwnerItemRow(fieldDefinitions, _listingRelationOwnerFieldGroupComponent, row, listingOwnerObject, cancellationToken));
                            var items = await Task.WhenAll(tasks).ConfigureAwait(false);
                            if (items != null && items.Length > 0)
                            {
                                foreach (var item in items)
                                {
                                    if (item != null)
                                    {
                                        results.Add(item);
                                    }
                                }
                            }
                        }

                    }
                }

            }

            return results;
        }

        private async Task<ListingOwner> GetRelatedOwnerItemRow(List<FieldControlField> fieldDefinitions, FieldGroupComponent listingRelationOwnerFieldGroupComponent, DataRow dataRow, ListingOwner listingOwnerObject, CancellationToken token)
        {
            if (dataRow == null || listingOwnerObject == null)
            {
                return null;
            }

            var recordIdentification0 = dataRow.GetColumnValue($"{listingOwnerObject.InfoAreaId}_0_recId", "");
            var recordIdentification1 = dataRow.GetColumnValue($"{listingOwnerObject.InfoAreaId}_1_recId", "");
            var recordIdentification = !string.IsNullOrWhiteSpace(recordIdentification0) ? recordIdentification0 : recordIdentification1;
            if (string.IsNullOrWhiteSpace(recordIdentification))
            {
                return null;
            }
            ListingOwner listingOwner = new ListingOwner();
            listingOwner.RecordIdentification = recordIdentification;
            listingOwner.InfoAreaId = listingOwnerObject.InfoAreaId;
            listingOwner.IsRelatedParent = true;
            listingOwner.SearchKeyPairs = new Dictionary<string, string>();
            foreach (var field in fieldDefinitions)
            {
                var key = field.Function;
                var value = await listingRelationOwnerFieldGroupComponent.ExtractFieldRawValue(field, dataRow, token);
                if (!string.IsNullOrEmpty(key) && !listingOwner.SearchKeyPairs.ContainsKey(key))
                {
                    listingOwner.SearchKeyPairs.Add(key, value);
                }
            }
            return listingOwner;
        }

        private async Task<ListingOwner> getListingOwner(string listingOwnerFieldGroupName, CancellationToken token)
        {
            ListingOwner result = null;

            if (!string.IsNullOrWhiteSpace(listingOwnerFieldGroupName))
            {
                var fieldControl = await _configurationService.GetFieldControl(listingOwnerFieldGroupName + ".List", token).ConfigureAwait(false);
                string infoAreaId = fieldControl.InfoAreaId;
                var tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, token).ConfigureAwait(false);
                if (fieldControl != null)
                {

                    _listingOwnerFieldGroupComponent.InitializeContext(fieldControl, tableInfo);
                    if (_listingOwnerFieldGroupComponent.HasTabs())
                    {
                        var fieldDefinitions = fieldControl.Tabs[0].GetQueryFields();
                        ParentLink parentLink = new ParentLink
                        {
                            LinkId = -1,
                            ParentInfoAreaId = _action.SourceInfoArea,
                            RecordId = _action.RecordId,
                        };

                        var rawData = await _crmDataService.GetData(token,
                            new DataRequestDetails
                            {
                                TableInfo = tableInfo,
                                Fields = fieldDefinitions,
                                SortFields = fieldControl.SortFields
                            },
                            parentLink, 1, RequestMode.Best);

                        if (rawData.Result != null && rawData.Result.Rows.Count > 0)
                        {
                            result = await GetOwnerItemRow(fieldDefinitions, _listingOwnerFieldGroupComponent, rawData.Result.Rows[0], infoAreaId, token);
                        }

                    }
                }

            }

            return result;
        }

        private async Task<ListingOwner> GetOwnerItemRow(List<FieldControlField> fieldDefinitions, FieldGroupComponent listingOwnerFieldGroupComponent, DataRow dataRow, string infoAreaId, CancellationToken token)
        {
            if (dataRow == null)
            {
                return null;
            }
            ListingOwner listingOwner = new ListingOwner();
            listingOwner.RecordIdentification = dataRow.GetColumnValue("recid", "-1");
            listingOwner.InfoAreaId = infoAreaId;
            listingOwner.IsRelatedParent = false;
            listingOwner.SearchKeyPairs = new Dictionary<string, string>();
            foreach (var field in fieldDefinitions)
            {
                var key = field.Function;
                var value = await listingOwnerFieldGroupComponent.ExtractFieldRawValue(field, dataRow, token);
                if (!string.IsNullOrEmpty(key) && !listingOwner.SearchKeyPairs.ContainsKey(key))
                {
                    listingOwner.SearchKeyPairs.Add(key, value);
                }
            }
            return listingOwner;

        }

        public bool Match(bool listingOn, SerialEntryItem item)
        {
            if (!listingOn)
            {
                return true;
            }

            if (!HasListingItems)
            {
                return false;
            }

            foreach (var distinctListingFunctionName in distinctListingFunctionNames)
            {
                if (item.FunctionKeyPairs.ContainsKey(distinctListingFunctionName))
                {
                    var value = item.FunctionKeyPairs[distinctListingFunctionName];
                    var result = listingItems.Any(a => a.SearchKeyPairs.ContainsKey(distinctListingFunctionName) && a.SearchKeyPairs[distinctListingFunctionName].Equals(value));
                    if(result)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
