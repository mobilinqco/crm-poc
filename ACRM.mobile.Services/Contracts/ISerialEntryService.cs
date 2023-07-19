using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.SerialEntry;

namespace ACRM.mobile.Services.Contracts
{
    public interface ISerialEntryService : IContentServiceBase
    {
        Task<ObservableCollection<UserAction>> GetTabItems(CancellationToken token);
        Task<List<SerialEntryItem>> ResultDataAsync(int tabId, CancellationToken token);
        Task<Dictionary<string, string>> GetFilters(int tabId, CancellationToken token);

        ISearchContentService SourceSearchService
        {
            get;
        }
        ISerialEntryEditService DestinationRootEditService
        {
            get;
        }
        IListingService ListingService
        {
            get;
        }
        UserAction FinishUserAction
        {
            get;
        }
        string FinishActionText
        {
            get;
        }
        string FinishActionIcon
        {
            get;
        }
        bool HasChildRecords
        {
            get;
        }
        bool HierarchicalPositionFilter
        {
            get;
        }

        (string, string) Currency { get; }

        Task<bool> SaveItem(SerialEntryItem item, List<PanelData> rootPanels, CancellationToken token);
        Task<bool> DeleteItem(SerialEntryItem item, CancellationToken token);
        Task<SerialEntryItem> BuildDestinationChildPanels(SerialEntryItem destItem, CancellationToken cancellationToken);
        Task<List<string>> GetPositionFilters(CancellationToken token);
        Task EvaluatePricing(SerialEntryItem serialEntryItem, CancellationToken token);
    }
}
