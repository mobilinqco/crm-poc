using System;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IDataSyncService
    {
        Task<bool> FullConfigSync(CancellationToken ct, bool force = false);
        Task<bool> FullDataSync(CancellationToken ct, bool force = false);
        Task<bool> IncrementalDataSync(CancellationToken ct);
        Task<bool> CatalogSync(CancellationToken ct);
        Task<bool> SyncResources(CancellationToken ct, bool force = false);
        Task<bool> SyncRecord(string recordId, IConfigurationService _configurationService, CancellationToken ct);

        Task BuildConfigurationCache(CancellationToken ct);

        Task<bool> IsFullSyncNeeded();
        void CreateConfigDatabaseBackup();
        void RestoreConfigDatabase();
        void CreateCrmDataDatabaseBackup();
        void RestoreCrmDataDatabase();
        void CreateLocalStoreBackup();
        void RestoreLocalStore();
    }
}
