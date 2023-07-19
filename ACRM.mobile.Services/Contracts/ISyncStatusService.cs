using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface ISyncStatusService
    {
        (SyncType? Type, string Message) ActiveSyncStatus { get; set; }
        EventHandler ActiveSyncChanged { get; set; }

        Task<SyncStatus> GetSyncStatusAsync();
        Task<SyncStatus> CreateDefaultSyncStatusAsync();

        Task SetLanguageAsync(ServerLanguage language);

        Task SetSyncInfo(SyncType syncType, DataSet dataSet = null, int recordCount = 0);

        bool ShouldSync(SyncType syncType, DataSet dataSet = null);
        bool ShouldSync(int syncedInfoAreas);
        Task RemoveSyncDataAsync(SyncType syncType);

        Dictionary<SyncType, bool> GetChunkSyncStatusAsync();
        List<string> GetSyncedInfoAreasAsync();

        bool IsPartialSync();
    }
}
