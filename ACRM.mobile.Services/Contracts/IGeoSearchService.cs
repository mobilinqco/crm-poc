using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface IGeoSearchService : IContentServiceBase
    {
        List<ListUIItem> FilterItems
        {
            get;
        }

        Task<List<MapEntry>> GetMapEntries(ObservableCollection<ListUIItem> filterItems, bool isOnline, CancellationToken token);
    }
}
