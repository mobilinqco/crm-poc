using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace ACRM.mobile.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationUnitOfWork _configurationUnitOfWork;
        private readonly ILogService _logService;
        private readonly ICacheService _cacheService;

        public ConfigurationService(IConfigurationUnitOfWork configurationUnitOfWork,
            ICacheService cacheService,
            ILogService logService)
        {
            _configurationUnitOfWork = configurationUnitOfWork;
            _cacheService = cacheService;
            _logService = logService;
        }

        public async Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString, CancellationToken cancellationToken)
        {
            return await _configurationUnitOfWork.ExecuteRawQueryString(queryString, cancellationToken);
        }

        public async ValueTask<ViewReference> GetViewForMenu(string menuName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(menuName))
            {
                return null;
            }

            List<Menu> menus = (List<Menu>)_cacheService.GetItem(CacheItemKeys.Menus);

            if (menus == null)
            {
                menus = await _configurationUnitOfWork.GetMenus(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.Menus, menus);
            }

            Menu menu = menus.Find(m => m.UnitName == menuName);

            if (menu != null)
            {
                return menu.ViewReference;
            }
            return null;
        }

        public async ValueTask<Menu> GetMenu(string menuName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(menuName))
            {
                return null;
            }

            List<Menu> menus = (List<Menu>)_cacheService.GetItem(CacheItemKeys.Menus);

            if (menus == null)
            {
                menus = await _configurationUnitOfWork.GetMenus(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.Menus, menus);
            }

            return menus.Find(m => m.UnitName == menuName);
        }

        public async ValueTask<SearchAndList> GetSearchAndList(string unitName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(unitName))
            {
                return null;
            }

            List<SearchAndList> searchAndLists = (List<SearchAndList>)_cacheService.GetItem(CacheItemKeys.SearchAndList);

            if (searchAndLists == null)
            {
                searchAndLists = await _configurationUnitOfWork.GetSearchAndLists(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.SearchAndList, searchAndLists);
            }

            return searchAndLists.Find(sal => sal.UnitName == unitName);
        }

        public async ValueTask<Header> GetHeader(string headerName, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(headerName))
            {
                return null;
            }

            List<Header> headers = (List<Header>)_cacheService.GetItem(CacheItemKeys.Headers);

            if (headers == null)
            {
                headers = await _configurationUnitOfWork.GetHeaders(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.Headers, headers);
            }

            return headers.Find(m => m.UnitName == headerName);
        }

        public InfoArea GetInfoArea(string infoAreaName)
        {
            return _configurationUnitOfWork.GenericRepository<InfoArea>().Get(infoAreaName);
        }

        public async ValueTask<FieldControl> GetFieldControl(string fieldControlName, CancellationToken cancellationToken)
        {
            _logService.LogDebug($"Retrieving filed control: {fieldControlName}");
            if (string.IsNullOrEmpty(fieldControlName))
            {
                return null;
            }

            List<FieldControl> fieldControls = (List<FieldControl>)_cacheService.GetItem(CacheItemKeys.FieldControls);

            if (fieldControls == null)
            {
                fieldControls = await _configurationUnitOfWork.GetFieldControls(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.FieldControls, fieldControls);
            }

            FieldControl fieldControl = fieldControls.Find(m => m.UnitName == fieldControlName);

            if (fieldControl != null)
            {
                _logService.LogDebug("Field control retrieved");
            }
            else
            {
                _logService.LogDebug("No such field control defined");
            }

            return fieldControl;
        }


        public async ValueTask<Query> GetQuery(string queryName, CancellationToken cancellationToken)
        {
            _logService.LogDebug($"Retrieving query: {queryName}");
            if (string.IsNullOrEmpty(queryName))
            {
                return null;
            }

            List<Query> queries = (List<Query>)_cacheService.GetItem(CacheItemKeys.Queries);

            if (queries == null)
            {
                queries = await _configurationUnitOfWork.GetQueries(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.Queries, queries);
            }

            Query query = queries.Find(m => m.UnitName == queryName);

            if (query != null)
            {
                _logService.LogDebug("Query retrieved");
            }
            else
            {
                _logService.LogDebug("No such query defined");
            }

            return query;
        }

        public async ValueTask<QuickSearch> GetQuickSearchConfig(CancellationToken cancellationToken)
        {
            _logService.LogDebug($"Retrieving QuickSearch");


            List<QuickSearch> quickSearch = (List<QuickSearch>)_cacheService.GetItem(CacheItemKeys.QuickSearch);

            if (quickSearch == null)
            {
                quickSearch = await _configurationUnitOfWork.GetQuickSearch(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.QuickSearch, quickSearch);
            }

            QuickSearch quickSearchObject = quickSearch.FirstOrDefault();

            if (quickSearchObject != null)
            {
                _logService.LogDebug("QuickSearch retrieved");
            }
            else
            {
                _logService.LogDebug("No such query defined");
            }

            return quickSearchObject;
        }

        public async ValueTask<TableInfo> GetTableInfoAsync(string infoArea, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(infoArea))
            {
                return null;
            }

            List<TableInfo> tableInfos = (List<TableInfo>)_cacheService.GetItem(CacheItemKeys.TableInfos);

            if (tableInfos == null)
            {
                tableInfos = await _configurationUnitOfWork.GetTableInfos(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.TableInfos, tableInfos);
            }

            return tableInfos.Find(t => t.InfoAreaId == infoArea);
        }

        public async ValueTask<List<TableInfo>> GetAllTableInfoAsync(CancellationToken cancellationToken)
        {
            List<TableInfo> tableInfos = (List<TableInfo>)_cacheService.GetItem(CacheItemKeys.TableInfos);

            if (tableInfos == null)
            {
                tableInfos = await _configurationUnitOfWork.GetTableInfos(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.TableInfos, tableInfos);
            }

            return tableInfos;
        }

        public async ValueTask<CatalogValue> GetCatalogValue(int catNr, int code, bool isVariableCatalog, CancellationToken cancellationToken)
        {
            List<Catalog> catalogs = (List<Catalog>)_cacheService.GetItem(CacheItemKeys.Catalogs);

            if (catalogs == null)
            {
                catalogs = await _configurationUnitOfWork.GetCatalogs(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.Catalogs, catalogs);
            }

            Catalog catalog = catalogs.Find(c => (c.CatNr == catNr && c.IsVariableCatalog == isVariableCatalog));
            if (catalog != null)
            {
                return catalog.CatalogValues.Find(cv => cv.Code == code);
            }

            return null;
        }

        public async ValueTask<List<CatalogValue>> GetCatalogValuesForCatalogField(FieldInfo field, CancellationToken cancellationToken)
        {
            if (field.IsCatalog)
            {
                List<Catalog> catalogs = (List<Catalog>)_cacheService.GetItem(CacheItemKeys.Catalogs);

                if (catalogs == null)
                {
                    catalogs = await _configurationUnitOfWork.GetCatalogs(cancellationToken);
                    _cacheService.AddItem(CacheItemKeys.Catalogs, catalogs);
                }

                Catalog catalog = catalogs.Find(c => (c.CatNr == field.CatalogId() && c.IsVariableCatalog == field.IsVariableCatalog));
                if (catalog != null)
                {
                    return catalog.CatalogValues;
                }
            }
            return null;
        }

        public Expand GetExpand(string unitName)
        {
            if (string.IsNullOrEmpty(unitName))
            {
                return null;
            }
            List<Expand> expands = (List<Expand>)_cacheService.GetItem(CacheItemKeys.Expands);

            if (expands == null)
            {
                expands = (List<Expand>)_configurationUnitOfWork.GenericRepository<Expand>().All();
                _cacheService.AddItem(CacheItemKeys.Expands, expands);
            }

            return expands.Find(b => b.UnitName == unitName);
        }

        public async Task<WebConfigLayout> GetWebConfigLayout(string layoutName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(layoutName))
            {
                return null;
            }
            var  config = await _configurationUnitOfWork.GetWebConfigLayout(layoutName, cancellationToken);
            return config;
        }

        public List<Expand> GetInfoAreaExpands(string infoAreaId)
        {
            if (string.IsNullOrEmpty(infoAreaId))
            {
                return new List<Expand>();
            }

            List<Expand> expands = (List<Expand>)_cacheService.GetItem(CacheItemKeys.Expands);

            if (expands == null)
            {
                expands = (List<Expand>)_configurationUnitOfWork.GenericRepository<Expand>().All();
                _cacheService.AddItem(CacheItemKeys.Expands, expands);
            }

            return expands.FindAll(b => b.InfoAreaId == infoAreaId);
        }

        public async ValueTask<Button> GetButton(string unitName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(unitName))
            {
                return null;
            }

            List<Button> buttons = (List<Button>)_cacheService.GetItem(CacheItemKeys.Buttons);

            if (buttons == null)
            {
                buttons = await _configurationUnitOfWork.GetButtons(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.Buttons, buttons);
            }

            return buttons.Find(b => b.UnitName == unitName);
        }

        public ConfigResource GetConfigResource(string unitName)
        {
            return _configurationUnitOfWork.GenericRepository<ConfigResource>().Get(unitName);
        }

        public List<string> ExtractConfigFromJsonString(string value)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<string>>(value);
                }
                catch (Exception ex)
                {
                    _logService.LogError($"ExtractConfigFromJsonString: Unable to extract data from {value}: {ex.Message}");
                }
            }
            
            return result;
        }


        public FieldInfo GetFieldInfo(TableInfo tableInfo, int FieldId)
        {
            if (tableInfo.Fields != null)
            {
                try
                {
                    return tableInfo.Fields.First(fld => fld.FieldId == FieldId);
                }
                catch (Exception e)
                {
                    _logService.LogError($"{"Unable to retrieve FieldInfo for FieldId" + FieldId + ". Error: " + e.GetType().Name + " : " + e.Message}");
                    return null;
                }
            }

            return null;
        }

        public async ValueTask<FieldInfo> GetFieldInfo(TableInfo tableInfo, FieldControlField fd, CancellationToken cancellationToken)
        {
            FieldInfo fieldInfo = null;
            if (fd.InfoAreaId == tableInfo.InfoAreaId)
            {
                fieldInfo = GetFieldInfo(tableInfo, fd.FieldId);
            }
            else
            {
                TableInfo linkTableInfo = await GetTableInfoAsync(fd.InfoAreaId, cancellationToken);
                if (linkTableInfo != null)
                {
                    fieldInfo = GetFieldInfo(linkTableInfo, fd.FieldId);
                }
            }

            return fieldInfo;
        }

        public WebConfigValue GetConfigValue(string unitName)
        {
            _logService.LogDebug($"Requesing config value for {unitName}");
            WebConfigValue configValue = _configurationUnitOfWork.GenericRepository<WebConfigValue>().Get(unitName);
            if (configValue != null && configValue.Value != null)
            {
                _logService.LogDebug($"value retrieved: {configValue.Value}");
            }
            else
            {
                _logService.LogDebug($"value retrieved: null");
            }

            return configValue;
        }


        public bool UpdateConfigValue(string unitName, string value)
        {
            _logService.LogDebug($"Updating config value for {unitName}");
            WebConfigValue configValue = _configurationUnitOfWork.GenericRepository<WebConfigValue>().Get(unitName);
            if(configValue!=null)
            {
                configValue.Value = value;
                _configurationUnitOfWork.Save();
            }
            else
            {
                WebConfigValue config = new WebConfigValue()
                {
                    UnitName = unitName,
                    Value = value,
                    Inherited = 1
                };
                _configurationUnitOfWork.GenericRepository<WebConfigValue>().Add(config);
            }

            return true;
        }


        public bool GetBoolConfigValue(string unitName, bool defaultValue = false)
        {
            WebConfigValue value = GetConfigValue(unitName);

            if (value != null && !string.IsNullOrEmpty(value.Value))
            {
                return value.Value.CrmBool();
            }

            return defaultValue;
        }

        public T GetNumericConfigValue<T>(string unitName, T defaultValue)
        {
            WebConfigValue value = GetConfigValue(unitName);

            if (value != null && !string.IsNullOrWhiteSpace(value.Value))
            {
                try
                {
                    return (T)Convert.ChangeType(value.Value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        public string GetServerTimezone()
        {
            try
            {
                var timeZoneConfigs = new List<string>() { "System.iOSServerTimeZone", "System.IOSServerTimeZone", "System.UWPServerTimeZone", "System.AndroidServerTimeZone", "System.ServerTimeZone" };
                foreach (var config in timeZoneConfigs)
                {
                    var serverTimezone = GetServerTimezoneFromConfig(config);
                    if (!string.IsNullOrWhiteSpace(serverTimezone))
                    {
                        return serverTimezone;
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.LogDebug($"No Server TimeZone found value, Exception :{ex.Message}");
            }

            _logService.LogDebug($"No Server TimeZone found value");
            return null;
        }

        private string GetServerTimezoneFromConfig(string timeZoneConfig)
        {
            WebConfigValue serverTimezone = GetConfigValue(timeZoneConfig);
            var time = TimeZoneInfo.GetSystemTimeZones();

            if (serverTimezone != null && !string.IsNullOrEmpty(serverTimezone.Value))
            {
                var timezo = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(a => a.Id.Equals(serverTimezone.Value) || a.DisplayName.Equals(serverTimezone.Value));
                if (timezo != null)
                {
                    _logService.LogDebug($"Server TimeZone config {timeZoneConfig} found Id {timezo.Id}");

                    return timezo.Id;
                }

            }
            return null;
        }

        public Expand GetExpandItem(string expandName, string key)
        {
            return _cacheService.GetExpandItem(expandName, key);
        }

        public void AddExpandItem(string expandName, string key, Expand expand)
        {
            _cacheService.AddExpandItem(expandName, key, expand);
        }

        public async Task<Form> GetForm(string unitName, CancellationToken cancellationToken)
        {
            return await _configurationUnitOfWork.GetForm(unitName, cancellationToken);
        }

        public async ValueTask<Filter> GetFilter(string unitName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(unitName))
            {
                return null;
            }

            List<Filter> filters = (List<Filter>)_cacheService.GetItem(CacheItemKeys.Filters);

            if (filters == null)
            {
                filters = await _configurationUnitOfWork.GetFilters(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.Filters, filters);
            }

            return filters.Find(b => b.UnitName == unitName);
        }

        public async ValueTask<TableCaption> GetTableCaption(string unitName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(unitName))
            {
                return null;
            }

            List<TableCaption> tableCaptions = (List<TableCaption>)_cacheService.GetItem(CacheItemKeys.TableCaptions);

            if (tableCaptions == null)
            {
                tableCaptions = await _configurationUnitOfWork.GetTableCaptions(cancellationToken);
                _cacheService.AddItem(CacheItemKeys.TableCaptions, tableCaptions);
            }

            return tableCaptions.Find(b => b.UnitName == unitName);
        }
    }
}
