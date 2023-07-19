using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services.Contracts
{
    public interface ISearchContentService : IContentServiceBase
    {
        Task<List<ListDisplayRow>> RecordListViewDataAsync(int tabId, CancellationToken cancellationToken);
        Task<ObservableCollection<DocumentObject>> DocumentViewDataAsync(int tabId, CancellationToken cancellationToken);
        Task<UserAction> ActionForItemSelect(int tabId, ListDisplayRow selectedRecord, CancellationToken cancellationToken);
        Task<List<ListDisplayRow>> PrepareRecordsAsync(FormItemData formItemData, CancellationToken token);
        Task<bool> PerformSearch(int tabId, string searchValue, RequestMode requestMode, CancellationToken cancellationToken);
        Task<DataTable> GetRecords(int tabId, CancellationToken cancellationToken, ParentLink parentLink = null);
        string SearchColumns(int tabId);
        bool HasResults(int tabId);
        int CountResults(int tabId);
        bool AreResultsRetrievedOnline(int tabId);
        Task<List<ListDisplayRow>> PrepareClildRecordsAsync(PanelData inputPanelData, CancellationToken token);
        Task<long> PrepareRecordCountAsync(UserAction action, RequestMode requestMode, CancellationToken token);
        RequestMode InitialRequestMode(int tabId);
        bool IsOnlineOfflineVisible(int tabId);
        bool HasUserFilters(int tabId);
        double SearchDelay(bool IsOfflineMode);
        bool SearchAutoSwitchToOffline();
        Task SetAdditionalFilterName(string filterName, CancellationToken cancellationToken);
        TabDataWithConfig GetTabData(int index);
        Task PrepareConfigurationAsync(string Expand, CancellationToken cancellationToken);
        (FieldGroupComponent, List<FieldControlField>) GetFieldConfig(int tabId);
        void SetAdditionalFilterParams(Dictionary<string, string> filterAdditionalParems);
        string SearchAndListName
        {
            get;
            set;
        }
        List<Filter> GetUserFilters(int tabId);
        List<Filter> GetUserDefaultEnabledFilters(int tabId);
        void SetEnabledUserFilters(List<Filter> filters, int selectedTabId);
        Task<List<ListDisplayRow>> RecordListViewDataPageAsync(int tabId, int startIndex, int endIndex, CancellationToken cancellationToken);
        Task<Dictionary<string, (string,string)>> GetFunctionMappingForFirstRecord(int tabId, CancellationToken cancellationToken);
        Task<List<ListDisplayRow>> GetQuickSearchResult(string globalSearchText, QuickSearchInfoAreaData quickSearchInfoAreaData, CancellationToken token);
        Task<ListDisplayRow> GetDisplayRow(List<FieldControlField> fieldDefinitions, DataRow row, TabDataWithConfig tab, CancellationToken cancellationToken);
        bool? ForceOnline
        {
            get;
            set;
        }

        int GetTargetInfoAreaLinkId(int tabId, string targetInfoAreaId);

        bool IsRowCountDisplayActive(int tabId);
    }
}
