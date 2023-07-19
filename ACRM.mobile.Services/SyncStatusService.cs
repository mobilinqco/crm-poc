using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services
{
    public class SyncStatusService: ISyncStatusService
    {
        private ILocalFileStorageContext _localFileStorageContext;
        private readonly string _syncStatusFileName = "SyncStatus.json";

        private (SyncType? Type, string Message) _activeSyncStatus;
        public (SyncType? Type, string Message) ActiveSyncStatus
        {
            get
            {
                return _activeSyncStatus;
            }
            set
            {
                _activeSyncStatus = value;
                OnActiveSyncChanged();
            }
        }
        private EventHandler _activeSyncChanged;
        public EventHandler ActiveSyncChanged
        {
            get
            {
                return _activeSyncChanged;
            }
            set
            {
                _activeSyncChanged = value;
            }
        }

        public SyncStatusService(ILocalFileStorageContext syncStatusContext)
        {
            _localFileStorageContext = syncStatusContext;
        }

        public void OnActiveSyncChanged()
        {
            _activeSyncChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<SyncStatus> GetSyncStatusAsync()
        {
            return _localFileStorageContext.GetContent<SyncStatus>(_syncStatusFileName);
        }

        public async Task<SyncStatus> CreateDefaultSyncStatusAsync()
        {
            var defaultSyncConfig = new SyncStatus
            {
                LanguageCode="M0"
            };
            return await _localFileStorageContext.SaveContent<SyncStatus>(defaultSyncConfig, _syncStatusFileName);
        }

        public async Task SetLanguageAsync(ServerLanguage language)
        {
            var syncStatus = await GetSyncStatusAsync();
            if (syncStatus == null)
            {
                syncStatus = await CreateDefaultSyncStatusAsync();
            }
            syncStatus.LanguageCode = language.Code;
            await _localFileStorageContext.SaveContent<SyncStatus>(syncStatus, _syncStatusFileName);
        }

        public async Task SetSyncInfo(SyncType syncType, DataSet dataSet = null, int recordCount = 0)
        {
            SyncStatus syncStatus = await GetSyncStatusAsync();
            InitialSyncInfo syncInfo = new InitialSyncInfo
            {
                DataSetName = syncType.ToString(),
                FullSyncTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                SyncTimestamp = DateTimeOffset.Now.ToString(),
                InfoAreaId = "",
                RecordCount = recordCount
            };

            switch (syncType)
            {
                case SyncType.UserInterfaceSync:
                    syncStatus.UserInterfaceConfigurationSyncInfo = syncInfo;
                    break;
                case SyncType.DataModelConfigurationSync:
                    syncStatus.DataModelConfigurationSyncInfo = syncInfo;
                    break;
                case SyncType.CatalogSync:
                    syncStatus.CatalogSyncInfo = syncInfo;
                    break;
                case SyncType.ResourceSync:
                    syncStatus.ResourcesSyncInfo = syncInfo;
                    break;
                case SyncType.DataSetSync:
                    if (dataSet == null)
                    {
                        break;
                    }
                    syncInfo.InfoAreaId = dataSet.InfoAreaId;
                    syncInfo.UnitName = dataSet.UnitName;
                    if (syncStatus.InfoAreasSyncInfo == null)
                    {
                        syncStatus.InfoAreasSyncInfo = new List<InitialSyncInfo>();
                    }
                    syncStatus.InfoAreasSyncInfo.Add(syncInfo);
                    break;
                default: break;
            }
            await _localFileStorageContext.SaveContent<SyncStatus>(syncStatus, _syncStatusFileName);
        }

        public bool ShouldSync(SyncType syncType, DataSet dataSet = null)
        {
            SyncStatus syncStatus = _localFileStorageContext.GetContent<SyncStatus>(_syncStatusFileName);

            switch (syncType)
            {
                case SyncType.UserInterfaceSync: return syncStatus.UserInterfaceConfigurationSyncInfo == null;
                case SyncType.CatalogSync: return syncStatus.CatalogSyncInfo == null;
                case SyncType.DataModelConfigurationSync: return syncStatus.DataModelConfigurationSyncInfo == null;
                case SyncType.ResourceSync: return syncStatus.ResourcesSyncInfo == null;
                case SyncType.DataSetSync:
                    if (syncStatus.InfoAreasSyncInfo == null)
                    {
                        return true;
                    }
                    if (dataSet == null)
                    {
                        return (syncStatus.DataModelConfigurationSyncInfo?.RecordCount ?? 0) != (syncStatus.InfoAreasSyncInfo?.Count ?? -1);
                    }
                    return syncStatus.InfoAreasSyncInfo.Find(ds => ds.InfoAreaId == dataSet.InfoAreaId && ds.UnitName == dataSet.UnitName) == null;
                default: return true;
            }
        }

        public bool ShouldSync(int syncedInfoAreas)
        {
            SyncStatus syncStatus = _localFileStorageContext.GetContent<SyncStatus>(_syncStatusFileName);

            if (syncStatus.InfoAreasSyncInfo == null)
            {
                return true;
            }

            return syncStatus.InfoAreasSyncInfo.Count != syncedInfoAreas;
        }

        public Dictionary<SyncType, bool> GetChunkSyncStatusAsync()
        {
            var shouldSyncUserInterface = ShouldSync(SyncType.UserInterfaceSync);
            var shouldSyncDataModels = ShouldSync(SyncType.DataModelConfigurationSync);
            var shouldSyncCatalogs = ShouldSync(SyncType.CatalogSync);
            var shouldSyncResources = ShouldSync(SyncType.ResourceSync);

            return new Dictionary<SyncType, bool>
            {
                {SyncType.UserInterfaceSync, shouldSyncUserInterface },
                {SyncType.DataModelConfigurationSync, shouldSyncDataModels },
                {SyncType.CatalogSync, shouldSyncCatalogs },
                {SyncType.ResourceSync, shouldSyncResources }
            };
        }

        public List<string> GetSyncedInfoAreasAsync()
        {
            SyncStatus syncStatus = _localFileStorageContext.GetContent<SyncStatus>(_syncStatusFileName);
            if (syncStatus.InfoAreasSyncInfo != null)
            {
                return syncStatus.InfoAreasSyncInfo.Select(si => si.InfoAreaId).ToList();
            } else
            {
                return new List<string>();
            }
        }

        public async Task RemoveSyncDataAsync(SyncType syncType)
        {
            SyncStatus syncStatus = await GetSyncStatusAsync();
            switch (syncType)
            {
                case SyncType.CatalogSync: syncStatus.CatalogSyncInfo = null; break;
                case SyncType.DataModelConfigurationSync: syncStatus.DataModelConfigurationSyncInfo = null; break;
                case SyncType.DataSetSync: syncStatus.InfoAreasSyncInfo = null; break;
                case SyncType.ResourceSync: syncStatus.ResourcesSyncInfo = null; break;
                case SyncType.UserInterfaceSync: syncStatus.UserInterfaceConfigurationSyncInfo = null; break;
            }
            await _localFileStorageContext.SaveContent<SyncStatus>(syncStatus, _syncStatusFileName);
        }

        public bool IsPartialSync()
        {
            var shouldSyncUserInterface = ShouldSync(SyncType.UserInterfaceSync);
            var shouldSyncDataModels = ShouldSync(SyncType.DataModelConfigurationSync);
            var shouldSyncCatalogs = ShouldSync(SyncType.CatalogSync);
            var shouldSyncResources = ShouldSync(SyncType.ResourceSync);
            var shouldSyncData = ShouldSync(SyncType.DataSetSync);

            return !(shouldSyncUserInterface && shouldSyncDataModels && shouldSyncCatalogs && shouldSyncResources && shouldSyncResources) &&
                (shouldSyncUserInterface || shouldSyncDataModels || shouldSyncCatalogs || shouldSyncResources || shouldSyncResources);
        }
    }
}
