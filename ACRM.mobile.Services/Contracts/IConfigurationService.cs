using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface IConfigurationService
    {
        Task<List<DynamicStringModel>> ExecuteRawQueryString(string queryString, CancellationToken cancellationToken);

        ValueTask<SearchAndList> GetSearchAndList(string unitName, CancellationToken cancellationToken);
        Task<Form> GetForm(string unitName, CancellationToken cancellationToken);        
        Expand GetExpand(string unitName);
        Task<WebConfigLayout> GetWebConfigLayout(string layoutName, CancellationToken cancellationToken);
        List<Expand> GetInfoAreaExpands(string infoAreaId);
        ValueTask<Button> GetButton(string unitName, CancellationToken cancellationToken);
        ConfigResource GetConfigResource(string unitName);
        InfoArea GetInfoArea(string infoAreaName);

        ValueTask<ViewReference> GetViewForMenu(string menuName, CancellationToken cancellationToken);
        ValueTask<Menu> GetMenu(string menuName, CancellationToken cancellationToken);
        ValueTask<Header> GetHeader(string headerName, CancellationToken cancellationToken);
        ValueTask<FieldControl> GetFieldControl(string fieldControlName, CancellationToken cancellationToken);

        ValueTask<Query> GetQuery(string queryName, CancellationToken cancellationToken);

        ValueTask<TableInfo> GetTableInfoAsync(string infoArea, CancellationToken cancellationToken);
        ValueTask<List<TableInfo>> GetAllTableInfoAsync(CancellationToken cancellationToken);
        ValueTask<CatalogValue> GetCatalogValue(int catNr, int code, bool isVariableCatalog, CancellationToken cancellationToken);
        ValueTask<List<CatalogValue>> GetCatalogValuesForCatalogField(FieldInfo field, CancellationToken cancellationToken);
        ValueTask<Filter> GetFilter(string unitName, CancellationToken cancellationToken);
        ValueTask<TableCaption> GetTableCaption(string unitName, CancellationToken cancellationToken);

        List<string> ExtractConfigFromJsonString(string value);

        FieldInfo GetFieldInfo(TableInfo tableInfo, int FieldId);
        ValueTask<FieldInfo> GetFieldInfo(TableInfo tableInfo, FieldControlField fd, CancellationToken cancellationToken);
        WebConfigValue GetConfigValue(string unitName);
        bool GetBoolConfigValue(string unitName, bool defaultValue = false);
        public T GetNumericConfigValue<T>(string unitName, T defaultValue);
        ValueTask<QuickSearch> GetQuickSearchConfig(CancellationToken cancellationToken);
        bool UpdateConfigValue(string unitName, string value);
        string GetServerTimezone();
        Expand GetExpandItem(string expandName, string key);
        void AddExpandItem(string expandName, string key, Expand expand);

    }
}
