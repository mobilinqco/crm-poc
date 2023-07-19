using ACRM.mobile.Domain.Configuration.UserInterface;
using System;
namespace ACRM.mobile.Services.Contracts
{
    public enum CacheItemKeys
    {
        Menus,
        Headers,
        FieldControls,
        TableInfos,
        Buttons,
        Catalogs,
        CrmReps,
        Expands,
        Filters,
        TableCaptions,
        SearchAndList,
        Queries,
        QuickSearch
    }

    public interface ICacheService
    {        
        void ResetCache();
        object GetItem(CacheItemKeys itemKey);
        void AddItem(CacheItemKeys itemKey, object itemValue);
        Expand GetExpandItem(string expandName, string key);
        void AddExpandItem(string expandName, string key, Expand expand);
    }
}
