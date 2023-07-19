using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Services.Utils;
using Newtonsoft.Json;

namespace ACRM.mobile.Services
{
    public class GeoSearchService : ContentServiceBase, IGeoSearchService
    {
        protected ISearchContentService _searchService;
        private List<ListUIItem> _filterItems = null;

        public GeoSearchService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            ISearchContentService searchService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _searchService = searchService;
        }

        public List<ListUIItem> FilterItems
        {
            get => _filterItems;
        }

        public async override Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                var actionTemplate = await ActionTemplateUtility.ResolveActionTemplate(_action, cancellationToken, _configurationService);
                var filters = _filterProcessor.RetrieveDistanceFilterNames(actionTemplate);

                if(filters?.Count > 0)
                {
                    _filterItems = new List<ListUIItem>();
                    int i = 1;
                    foreach(var item in filters)
                    {
                        var filterName = item.Item1;
                        var ConfigName = item.Item2;

                        var searchObject = await _configurationService.GetSearchAndList(ConfigName, cancellationToken);
                        var expand = _configurationService.GetExpand(searchObject.FieldGroupName);
                        var tableInfo = await _configurationService.GetTableInfoAsync(expand.InfoAreaId, cancellationToken);

                        var filterItem = new ListUIItem
                        {
                            DisplayValue = tableInfo?.Name,
                            ColorString = expand.GetColorString(),
                            Selected = true,
                            Id = i++,
                            ImageName = expand.ImageName,
                            ExtKey = filterName,
                            Configuration = ConfigName

                        };
                        _filterItems.Add(filterItem);
                    }
                }

            }
        }

        public async Task<List<MapEntry>> GetMapEntries(ObservableCollection<ListUIItem> filterItems, bool isOnline ,CancellationToken token)
        {

            List<MapEntry> items = new List<MapEntry>();
            _searchService.SetSourceAction(_action);
            foreach (var filter in filterItems)
            {
                if (filter.Selected)
                {
                    _searchService.SearchAndListName = filter.Configuration;
                    _searchService.SetSourceAction(_action);
                    _searchService.SetAdditionalFilterParams(_additionalParams);
                    await _searchService.PrepareContentAsync(token);
                    await _searchService.SetAdditionalFilterName(filter.ExtKey, token);
                    _searchService.ForceOnline = isOnline;
                    var dtable = await _searchService.GetRecords(0, token);
                    var (fieldGroupComponent, fieldDefinitions) = _searchService.GetFieldConfig(0);

                    if (dtable != null && dtable?.Rows.Count > 0)
                    {
                        var tasks = dtable.Rows.Cast<DataRow>().Select(async row => await GetMapEntrieRow(row, fieldGroupComponent, fieldDefinitions, filter, token));
                        items.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
                    }
                }
            }

            return items;
        }

        private async Task<MapEntry> GetMapEntrieRow(DataRow row, FieldGroupComponent fieldGroupComponent, List<FieldControlField> fieldDefinitions, ListUIItem filter, CancellationToken token)
        {
            var mapItem = new MapEntry();
            var tab = _searchService.GetTabData(0);
            mapItem.DisplayRow = await _searchService.GetDisplayRow(fieldDefinitions, row, tab, token);
            mapItem.InfoAreaID = fieldGroupComponent.TableInfo.InfoAreaId;
            mapItem.RecordId = row.GetColumnValue("recid", "-1");
            
            foreach (var field in fieldDefinitions)
            {
                var value = await fieldGroupComponent.ExtractFieldValue(field, row, token);
                if (string.IsNullOrWhiteSpace(mapItem.Label))
                {
                    mapItem.Label = value;
                }
                var function = field.ExtendedOptionForKey("GPS");
                if(!string.IsNullOrWhiteSpace(function))
                {
                    SetValueForfunction(mapItem, value, function);
                }
            }
            return mapItem;
        }

        private void SetValueForfunction(MapEntry mapItem, string value, string function)
        {
            switch (function.ToLower())
            {
                case "street":
                case "gpsstreet":
                    mapItem.Street = value;
                    break;
                case "city":
                case "gpscity":
                    mapItem.City = value;
                    break;
                case "country":
                case "gpscountry":
                    mapItem.Country = value;
                    break;
                case "x":
                case "gpsx":
                    double longitude;
                    if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out longitude))
                    {
                        mapItem.Longitude = longitude;
                    }
                    break;
                case "y":
                case "gpsy":
                    double latitude;
                    if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out latitude))
                    {
                        mapItem.Latitude = latitude;
                    }
                    
                    break;
            }
            
        }
    }
}
