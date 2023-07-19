using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.DataAccess.Network.Requests;
using ACRM.mobile.DataAccess.Network.Responses;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Network.Responses;
using ACRM.mobile.Services.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using ACRM.mobile.Logging;
using NLog;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Services.Extensions;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Web;

namespace ACRM.mobile.Services
{
    public class DataSyncService : IDataSyncService
    {
        private readonly INetworkRepository _networkRepository;
        private readonly IConfigurationUnitOfWork _configurationUnitOfWork;
        private readonly ICrmDataUnitOfWork _crmDataUnitOfWork;
        private readonly IOfflineRequestsUnitOfWork _offlineRequestsUnitOfWork;

        private readonly ISessionContext _sessionContext;
        private readonly ICacheService _cacheService;
        private readonly ISyncStatusService _syncStatusService;
        private readonly ILogService _logService;

        private readonly ICrmRequestBuilder _crmRequestBuilder;

        public DataSyncService(INetworkRepository networkRepository,
            IConfigurationUnitOfWork configurationUnitOfWork,
            ICrmDataUnitOfWork crmDataUnitOfWork,
            ISessionContext sessionContext,
            ICacheService cacheService,
            ISyncStatusService syncStatusService,
            IOfflineRequestsUnitOfWork offlineRequestsUnitOfWork,
            ICrmRequestBuilder crmRequestBuilder,
            ILogService logService)
        {
            _networkRepository = networkRepository;
            _configurationUnitOfWork = configurationUnitOfWork;
            _crmDataUnitOfWork = crmDataUnitOfWork;
            _sessionContext = sessionContext;
            _cacheService = cacheService;
            _syncStatusService = syncStatusService;
            _offlineRequestsUnitOfWork = offlineRequestsUnitOfWork;
            _crmRequestBuilder = crmRequestBuilder;
            _logService = logService;
        }

        // TODO: Maybe we could move this method into SyncStatusService
        public async Task<bool> IsFullSyncNeeded()
        {
            if (Directory.Exists(_sessionContext.LocalCrmInstancePath()))
            {
                if (!_configurationUnitOfWork.DatabaseExists())
                {
                    return true; // we always want to full sync if the db doesn't exist yet
                }
            }

            Directory.CreateDirectory(_sessionContext.LocalCrmInstancePath());

            var chunkSyncStatus = _syncStatusService.GetChunkSyncStatusAsync();
            return chunkSyncStatus[SyncType.UserInterfaceSync] &&
                chunkSyncStatus[SyncType.DataModelConfigurationSync] &&
                chunkSyncStatus[SyncType.CatalogSync] &&
                chunkSyncStatus[SyncType.ResourceSync];
        }

        public void CreateConfigDatabaseBackup()
        {
            _configurationUnitOfWork.CreateDatabaseBackup();
        }

        public void RestoreConfigDatabase()
        {
            _configurationUnitOfWork.RestoreDatabase();
        }

        public void CreateCrmDataDatabaseBackup()
        {
            _crmDataUnitOfWork.CreateDatabaseBackup();
            _offlineRequestsUnitOfWork.CreateDatabaseBackup();
        }

        public void RestoreCrmDataDatabase()
        {
            _crmDataUnitOfWork.RestoreDatabase();
            _offlineRequestsUnitOfWork.RestoreDatabase();
        }

        public void CreateLocalStoreBackup()
        {
            CreateConfigDatabaseBackup();
            CreateCrmDataDatabaseBackup();
        }

        public void RestoreLocalStore()
        {
            RestoreConfigDatabase();
            RestoreCrmDataDatabase();
        }

        private void CreateOfflineRequestsLocalStore()
        {
            _offlineRequestsUnitOfWork.CreateTables();
        }

        private void SaveUserInterfaceConfigurationToLocalStore(SyncResponse config, CancellationToken ct)
        {
            try
            {
                _logService.LogDebug("Start Saving Configuration");
                _configurationUnitOfWork.CreateTables();
                StoreConfigurationUnit<Analysis>(config.Configuration.Analysis, ct);
                StoreConfigurationUnit<AnalysisCategory>(config.Configuration.AnalysisCategory, ct);
                StoreConfigurationUnit<Button>(config.Configuration.Buttons, ct);
                StoreConfigurationUnit<DataSet>(config.Configuration.DataSets, ct);
                StoreConfigurationUnit<Expand>(config.Configuration.Details, ct);
                StoreConfigurationUnit<FieldControl>(config.Configuration.FieldControls, ct);
                StoreConfigurationUnit<Filter>(config.Configuration.Filters, ct);
                StoreConfigurationUnit<Form>(config.Configuration.Forms, ct);
                StoreConfigurationUnit<Header>(config.Configuration.Headers, ct);
                StoreConfigurationUnit<InfoArea>(config.Configuration.InfoAreas, ct);
                StoreConfigurationUnit<ConfigResource>(config.Configuration.Images, ct);
                StoreConfigurationUnit<Menu>(config.Configuration.Menus, ct);
                StoreConfigurationUnit<Query>(config.Configuration.Queries, ct);
                StoreConfigurationUnit<QuickSearch>(config.Configuration.QuickSearch, ct);
                StoreConfigurationUnit<SearchAndList>(config.Configuration.Searches, ct);
                StoreConfigurationUnit<TableCaption>(config.Configuration.TableCaptions, ct);
                StoreConfigurationUnit<Textgroup>(config.Configuration.Textgroups, ct);
                StoreConfigurationUnit<Timeline>(config.Configuration.Timelines, ct);
                StoreConfigurationUnit<TreeView>(config.Configuration.TreeViews, ct);
                StoreConfigurationUnit<WebConfigLayout>(config.Configuration.WebConfigLayouts, ct);
                StoreConfigurationUnit<WebConfigValue>(config.Configuration.WebConfigValues, ct);
                _configurationUnitOfWork.Save();
                _logService.LogDebug("Configuration Saved");
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error Saving the configuration {ex.Message}");
                throw ex;
            }
        }

        private void StoreConfigurationUnit<T>(List<T> configUnits, CancellationToken ct) where T : class
        {
            CheckCancelationToken(ct);
            // need to cleanup some instances are returning duplicate data for them.
            _configurationUnitOfWork.GenericRepository<T>().AddRange(configUnits.Distinct().ToList());
        }

        private void SaveDataModelConfigurationToLocalStore(SyncResponse config)
        {
            if (config.DataModelResponse != null && config.DataModelResponse.TableInfo != null)
            {
                _configurationUnitOfWork.GenericRepository<TableInfo>().AddRange(config.DataModelResponse.TableInfo);
            }

            _configurationUnitOfWork.Save();
        }

        private void SaveCagalogsToLocalStore(SyncResponse config)
        {
            if (config.FixCatalogs != null)
            {
                _configurationUnitOfWork.GenericRepository<Catalog>().AddRange(config.FixCatalogs);
            }

            if (config.VariableCatalogs != null)
            {
                config.VariableCatalogs.ForEach(vc => vc.IsVariableCatalog = true);
                _configurationUnitOfWork.GenericRepository<Catalog>().AddRange(config.VariableCatalogs);
            }

            _configurationUnitOfWork.Save();
        }

        public void CreateDynamicCrmDataModel(List<TableInfo> tableInfos, bool dropIfExists)
        {
            _crmDataUnitOfWork.GenerateSchema(tableInfos, dropIfExists);
        }

        public async Task<bool> FullConfigSync(CancellationToken ct, bool force = false)
        {
            var synced = false;
            _syncStatusService.ActiveSyncStatus = (null, "Configuration");
            var shouldSyncUserInterface =  _syncStatusService.ShouldSync(SyncType.UserInterfaceSync) || force;
            var shouldSyncCatalogs = _syncStatusService.ShouldSync(SyncType.CatalogSync) || force;
            var shouldSyncDataModelConfiguration = _syncStatusService.ShouldSync(SyncType.DataModelConfigurationSync) || force;

            // this way we won't do the next request.
            if (!shouldSyncUserInterface &&
                !shouldSyncCatalogs &&
                !shouldSyncDataModelConfiguration)
            {
                return synced;
            }

            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath())
            {
                Query = _crmRequestBuilder.SyncConfigurationQuery(shouldSyncCatalogs,
                shouldSyncUserInterface,
                shouldSyncDataModelConfiguration)
            };

            FieldControlField.ResetDesignerOrderId();
            FieldControlTab.ResetDesignerOrderId();
            HeaderSubView.ResetDesignerOrderId();

            var config = await _networkRepository.GetAsync<SyncResponse>(uriBuilder.ToString(), 100000, ct).ConfigureAwait(false);
            if (config != null)
            {
                if (config.Configuration != null)
                {
                    if (shouldSyncUserInterface)
                    {
                        CheckCancelationToken(ct);
                        _syncStatusService.ActiveSyncStatus = (SyncType.UserInterfaceSync, SyncType.UserInterfaceSync.ToString());
                        await Task.Run(() => SaveUserInterfaceConfigurationToLocalStore(config, ct), ct).ConfigureAwait(false);
                        CheckCancelationToken(ct);
                        await _syncStatusService.SetSyncInfo(SyncType.UserInterfaceSync).ConfigureAwait(false);
                        synced = true;
                    }
                }

                if (shouldSyncCatalogs)
                {
                    CheckCancelationToken(ct);
                    _syncStatusService.ActiveSyncStatus = (SyncType.CatalogSync, SyncType.CatalogSync.ToString());
                    CheckCancelationToken(ct);
                    SaveCagalogsToLocalStore(config);
                    await _syncStatusService.SetSyncInfo(SyncType.CatalogSync).ConfigureAwait(false);

                    synced = true;
                }

                if (shouldSyncDataModelConfiguration)
                {
                    CheckCancelationToken(ct);
                    _syncStatusService.ActiveSyncStatus = (SyncType.DataModelConfigurationSync, SyncType.DataModelConfigurationSync.ToString());
                    SaveDataModelConfigurationToLocalStore(config);
                    CheckCancelationToken(ct);
                    await _syncStatusService.SetSyncInfo(SyncType.DataModelConfigurationSync, null, config?.DataModelResponse?.TableInfo?.Count ?? 0).ConfigureAwait(false);
                    synced = true;
                }
            }

            return synced;
        }

        public async Task<bool> CatalogSync(CancellationToken ct)
        {
            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath())
            {
                Query = _crmRequestBuilder.SyncCatalogsQuery()
            };

            var config = await _networkRepository.GetAsync<SyncResponse>(uriBuilder.ToString(), 100000, ct);
            if (config != null)
            {
                CheckCancelationToken(ct);
                SaveCagalogsToLocalStore(config);
            }

            return true;

        }

        public async Task<bool> SyncRecord(string recordId, IConfigurationService configurationService, CancellationToken ct)
        {
            bool synced = false;
            _syncStatusService.ActiveSyncStatus = (SyncType.SyncRecord, "Sync Record");
            List<DataSet> dataSets = _configurationUnitOfWork.GenericRepository<DataSet>()
                .All()
                .OrderBy(ds => ds.InfoAreaId)
                .ToList();

            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath());
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            query["Service"] = "Synchronization";
            query["DataSetNameCount"] = $"{dataSets.Count}";
            query["SyncRecordData"] = "true";
            query["RecordIdentification"] = recordId;
            query["AppInfo"] = "crmclient";

            for (int i =0; i< dataSets.Count; i++)
            {
                query[$"DataSetName{i}"] = dataSets[i].UnitName;
            }

            uriBuilder.Query = query.ToString();

            var syncStartDate = DateTime.Now;

            SyncDataSetResponse syncDataSetResponse;
            double serverProcessing = 0;
            double unpacking = 0;

            string errorText = "";
            string errorDetails = "";

            _logService.LogDebug($"Syncronizing SyncRecord: {recordId}");
            try
            {
                int timeout = configurationService.GetNumericConfigValue<int>("ClientRequestTimeout", 60);
                
                (serverProcessing, unpacking, syncDataSetResponse) = await _networkRepository.PostSyncReqAsync<string, SyncDataSetResponse>(uriBuilder.ToString(), query.ToString(), timeout, ct).ConfigureAwait(false);
                if (syncDataSetResponse != null)
                {
                    CheckCancelationToken(ct);
                    if (syncDataSetResponse.DataSets != null && syncDataSetResponse.DataSets.Count > 0)
                    {

                        foreach (var dataSet in syncDataSetResponse.DataSets)
                        {
                            if (dataSet.RowCount > 0)
                            {
                                var tableInfo = await configurationService.GetTableInfoAsync(dataSet.InfoAreaId, ct);

                                if (tableInfo != null)
                                {
                                    await Task.Run(() => _crmDataUnitOfWork.UpdateRange(tableInfo, dataSet)).ConfigureAwait(false);
                                }
                 
                            }
                        }
                    }
                    else
                    {
                        _logService.LogDebug($"Syncronization returned no datasets");
                    }
                }
                else
                {
                    errorText = $"Syncronization returned no readable response";
                    _logService.LogDebug(errorText);
                }
            }
            catch (Exception ex)
            {
                // TODO Check if authentication expired and reauthenticate. Mabey not here but in the sync viewmodel?
                //_logService.LogDebug($"Syncronization of :" + tableInfo.InfoAreaId + " failed with an exception");
                errorText = $"{ ex.GetType().Name + " : " + ex.Message}";
                errorDetails = $"{ ex.StackTrace}";
                _logService.LogDebug(errorText);
                _logService.LogDebug(errorDetails);
            }

            return synced;
        }

        public async Task<bool> FullDataSync(CancellationToken ct, bool force = false)
        {
            bool synced = false;
            _syncStatusService.ActiveSyncStatus = (null, "CRM Data");
            List<DataSet> dataSets = _configurationUnitOfWork.GenericRepository<DataSet>()
                .All()
                .OrderBy(ds => ds.InfoAreaId)
                .ToList();

            var shouldSyncDataSets = _syncStatusService.ShouldSync(dataSets.Count) || force;

            if (!shouldSyncDataSets)
            {
                return synced;
            }

            // Making sure sync won't duplicate data. 
            if (force)
            {
                await _syncStatusService.RemoveSyncDataAsync(SyncType.DataSetSync);
            }

            List<TableInfo> tables = await _configurationUnitOfWork.GetTableInfos(ct).ConfigureAwait(false); // TODO: Optimize this.

            CreateDynamicCrmDataModel(tables, force);
            CreateOfflineRequestsLocalStore();

            int syncIndex = 1;

            foreach (DataSet ds in dataSets)
            {
                TableInfo tableInfo = getInfoAreaTableInfo(ds.InfoAreaId, tables);
                CheckCancelationToken(ct);

                if (tableInfo == null)
                {
                    throw new CrmException(ds.InfoAreaId + " not defined in the configuration.",
                        CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataDataSyncInfoAreaNotDefined); // todo: going to throw, as the number of tables is slightly reduced due to syncinfo
                }

                var testBool = _syncStatusService.ShouldSync(SyncType.DataSetSync, ds);
                var shouldSyncDataSet = testBool || force;
                if (shouldSyncDataSet)
                {
                    CheckCancelationToken(ct);
                    _syncStatusService.ActiveSyncStatus = (SyncType.DataSetSync, $"\n {syncIndex}/{dataSets.Count} \n {tableInfo.Name}");// ({ds.InfoAreaId})");
                    _ = await SyncDataSet(ds, tableInfo, ct).ConfigureAwait(false);
                    CheckCancelationToken(ct);
                    await _syncStatusService.SetSyncInfo(SyncType.DataSetSync, ds).ConfigureAwait(false);
                    synced = true;
                }

                syncIndex++;
            }

            return synced;
        }

        /*
         * Sync DataSet request
         * {
         *      DataSetName0 = "AU(Lookup)";
         *      DataSetNameCount = 1;
         *      Service = Synchronization;
         *      SyncRecordData = true;
         * }
         */
        private async Task<bool> SyncDataSet(DataSet dataSet, TableInfo tableInfo, CancellationToken ct)
        {
            var syncStartDate = DateTime.Now;

            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath())
            {
                Query = _crmRequestBuilder.DataSetQuery(dataSet.UnitName)
            };

            SyncDataSetRequest requestData = new SyncDataSetRequest();
            requestData.DataSetName0 = dataSet.UnitName;
            requestData.DataSetNameCount = 1;
            requestData.Service = "Synchronization";
            requestData.SyncRecordData = true;

            SyncDataSetResponse syncDataSetResponse;
            double serverProcessing = 0;
            double unpackingSeconds = 0;
            double localSaveSeconds = 0;
            double networkRetrievalSeconds = 0;
            double totalProcessing = 0;
            int recordCount = 0;
            string errorText = "";
            string errorDetails = "";

            _logService.LogDebug($"Syncronizing : {tableInfo.InfoAreaId}");
            try
            {
                int timeout = 60;
                WebConfigValue configValue = _configurationUnitOfWork.GenericRepository<WebConfigValue>().Get("ClientRequestTimeout");
                if (configValue != null && configValue.Value != null && !string.IsNullOrWhiteSpace(configValue.Value))
                {
                    if(int.TryParse(configValue.Value, out int result))
                    {
                        timeout = result;
                    }
                }

                (serverProcessing, unpackingSeconds, syncDataSetResponse) = await _networkRepository.PostSyncReqAsync<SyncDataSetRequest, SyncDataSetResponse>(uriBuilder.ToString(),
                    requestData, 600000, ct).ConfigureAwait(false);
                networkRetrievalSeconds = (DateTime.Now - syncStartDate).TotalMilliseconds;

                if (syncDataSetResponse != null)
                {
                    CheckCancelationToken(ct);
                    if (syncDataSetResponse.DataSets != null && syncDataSetResponse.DataSets.Count > 0)
                    {
                        recordCount = syncDataSetResponse.DataSets[0].RowCount;
                        var start = DateTime.Now;
                        await Task.Run(() => _crmDataUnitOfWork.AddRangeWithTransaction(tableInfo, syncDataSetResponse.DataSets[0])).ConfigureAwait(false);
                        localSaveSeconds = (DateTime.Now - start).TotalMilliseconds;
                    }
                    else
                    {
                        _logService.LogDebug($"Syncronization of :" + tableInfo.InfoAreaId + " returned no datasets");
                    }
                }
                else
                {
                    errorText = $"Syncronization of :" + tableInfo.InfoAreaId + " returned no readable response";
                    _logService.LogDebug(errorText);
                }
            }
            catch (Exception ex)
            {
                _logService.LogDebug($"Syncronization of :" + tableInfo.InfoAreaId + " failed with an exception");
                errorText = $"{ ex.GetType().Name + " : " + ex.Message}";
                errorDetails = $"{ ex.StackTrace}";
                _logService.LogDebug(errorText);
                _logService.LogDebug(errorDetails);
            }

            totalProcessing = (DateTime.Now - syncStartDate).TotalMilliseconds;

            _logService.LogDebug($"Syncronization of: {tableInfo.InfoAreaId} table with {recordCount} records done in {totalProcessing} milliseconds (network: {networkRetrievalSeconds}, unpacking: {unpackingSeconds} local save:{localSaveSeconds})");

            SyncInfo syncInfo = new SyncInfo
            {
                DatasetName = dataSet.UnitName,
                RecordCount = recordCount,
                FullSyncTimestamp = syncStartDate.ToDbSyncTimestampFieldDateFormat(),
                InfoAreaId = tableInfo.InfoAreaId
            };

            _offlineRequestsUnitOfWork.GenericRepository<SyncInfo>().Add(syncInfo);
            _offlineRequestsUnitOfWork.Save();

            SyncHistory syncHistory = new SyncHistory
            {
                SyncType = "Record",
                Details = dataSet.UnitName,
                StartDate = syncStartDate.ToControlFieldFormatString(),
                RunTimeLocalStorage = localSaveSeconds,
                RunTimeServer = serverProcessing,
                RunTimeUnpacking = unpackingSeconds,
                RecordCount = recordCount,
                ErrorText = errorText,
                ErrorDetails = errorDetails
            };

            _offlineRequestsUnitOfWork.GenericRepository<SyncHistory>().Add(syncHistory);
            _offlineRequestsUnitOfWork.Save();

            return true;
        }

        public async Task<bool> IncrementalDataSync(CancellationToken ct)
        {
            var incrementalSyncStartDate = DateTime.Now;

            double totalServerProcessing = 0;
            double totalUnpacking = 0;
            double totalLocalSave = 0;
            int totalRecordCount = 0;

            List<DataSet> dataSets = _configurationUnitOfWork.GenericRepository<DataSet>()
                .All()
                .ToList();

            List<TableInfo> tables = await _configurationUnitOfWork.GetTableInfos(ct).ConfigureAwait(false); // TODO: Optimize this.

            int syncIndex = 1;

            foreach (DataSet ds in dataSets)
            {
                TableInfo tableInfo = getInfoAreaTableInfo(ds.InfoAreaId, tables);

                CheckCancelationToken(ct);

                if (tableInfo == null)
                {
                    throw new CrmException(ds.InfoAreaId + " not defined in the configuration.",
                        CrmExceptionType.CrmData, CrmExceptionSubType.CrmDataDataSyncInfoAreaNotDefined); // todo: going to throw, as the number of tables is slightly reduced due to syncinfo
                }

                CheckCancelationToken(ct);

                SyncInfo currentDataSetSyncInfo = _offlineRequestsUnitOfWork.GenericRepository<SyncInfo>()
                    .Find(si => si.DatasetName.Equals(ds.UnitName)).First();

                if (currentDataSetSyncInfo != null)
                {
                    var result = await IncrementalSyncDataSet(ds, tableInfo, currentDataSetSyncInfo, syncIndex, dataSets.Count, ct).ConfigureAwait(false);
                    totalServerProcessing += result.Item1;
                    totalUnpacking += result.Item2;
                    totalLocalSave += result.Item3;
                    totalRecordCount += result.Item4;
                }

                CheckCancelationToken(ct);

                syncIndex++;
            }

            SyncHistory syncHistory = new SyncHistory
            {
                SyncType = "RecordIncremental",
                StartDate = incrementalSyncStartDate.ToControlFieldFormatString(),
                RunTimeLocalStorage = totalLocalSave,
                RunTimeServer = totalServerProcessing,
                RunTimeUnpacking = totalUnpacking,
                RecordCount = totalRecordCount,
            };

            _offlineRequestsUnitOfWork.GenericRepository<SyncHistory>().Add(syncHistory);
            _offlineRequestsUnitOfWork.Save();

            return true;
        }

        private async Task<(double, double, double, int)> IncrementalSyncDataSet(DataSet dataSet, TableInfo tableInfo, 
            SyncInfo currentDataSetSyncInfo, int syncIndex, int dataSetsCount, CancellationToken ct)
        {
            var syncStartDate = DateTime.Now;

            string timeSinceLastSync = null;

            if (currentDataSetSyncInfo.SyncTimestamp != null)
            {
                timeSinceLastSync = currentDataSetSyncInfo.SyncTimestamp;
            }
            else
            {
                timeSinceLastSync = currentDataSetSyncInfo.FullSyncTimestamp;
            }

            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath())
            {
                Query = _crmRequestBuilder.IncrementalDataSetQuery(dataSet.UnitName, timeSinceLastSync)
            };

            SyncDataSetRequest requestData = new SyncDataSetRequest();
            requestData.DataSetName0 = dataSet.UnitName;
            requestData.DataSetNameCount = 1;
            requestData.Service = "Synchronization";
            requestData.SyncRecordData = true;

            SyncDataSetResponse syncDataSetResponse;
            double serverProcessing = 0;
            double unpacking = 0;
            double localSave = 0;
            int recordCount = 0;
            string errorText = "";
            string errorDetails = "";

            _logService.LogDebug($"Incremental Syncronizing : {tableInfo.InfoAreaId}");
            try
            {
                _syncStatusService.ActiveSyncStatus = (SyncType.IncrementalDataSetSync, $"\n {syncIndex}/{dataSetsCount} \n {tableInfo.Name}");

                (serverProcessing, unpacking, syncDataSetResponse) = await _networkRepository.PostSyncReqAsync<SyncDataSetRequest, SyncDataSetResponse>(uriBuilder.ToString(),
                    requestData, 600000, ct).ConfigureAwait(false);
                if (syncDataSetResponse != null)
                {
                    CheckCancelationToken(ct);
                    if (syncDataSetResponse.DataSets != null && syncDataSetResponse.DataSets.Count > 0)
                    {
                        recordCount = syncDataSetResponse.DataSets[0].RowCount;
                        var start = DateTime.Now;
                        await Task.Run(() => _crmDataUnitOfWork.AddRange(tableInfo, syncDataSetResponse.DataSets[0])).ConfigureAwait(false);
                        localSave = (DateTime.Now - start).TotalMilliseconds;
                    }
                    else
                    {
                        _logService.LogDebug($"Incremental Syncronization of :" + tableInfo.InfoAreaId + " returned no datasets");
                    }

                    currentDataSetSyncInfo.SyncTimestamp = syncStartDate.ToDbSyncTimestampFieldDateFormat();
                    _offlineRequestsUnitOfWork.Save();
                }
                else
                {
                    errorText = $"Incremental Syncronization of :" + tableInfo.InfoAreaId + " returned no readable response";
                    _logService.LogDebug(errorText);
                }
            }
            catch (Exception ex)
            {
                // TODO Check if authentication expired and reauthenticate. Mabey not here but in the sync viewmodel?
                _logService.LogDebug($"Incremental Syncronization of :" + tableInfo.InfoAreaId + " failed with an exception");
                errorText = $"{ ex.GetType().Name + " : " + ex.Message}";
                errorDetails = $"{ ex.StackTrace}";
                _logService.LogDebug(errorText);
                _logService.LogDebug(errorDetails);
            }

            return (serverProcessing, unpacking, localSave, recordCount);
        }

        private TableInfo getInfoAreaTableInfo(string infoAreaId, List<TableInfo> tables)
        {
            return tables.Find(ia => ia.InfoAreaId == infoAreaId);
        }

        public async Task<bool> SyncResources(CancellationToken ct, bool force = false)
        {
            var synced = false;
            _syncStatusService.ActiveSyncStatus = (SyncType.ResourceSync, SyncType.ResourceSync.ToString());
            bool shouldSyncResources = _syncStatusService.ShouldSync(SyncType.ResourceSync) || force;
            if (!shouldSyncResources)
            {
                return synced;
            }

            // TODO: Still to implement the ZIP: option
            WebConfigValue resourcePath = _configurationUnitOfWork.GenericRepository<WebConfigValue>().Get("ResourcePath");
            CheckCancelationToken(ct);
            if (resourcePath == null)
            {
                resourcePath = _configurationUnitOfWork.GenericRepository<WebConfigValue>().Get("Resource.Path");
            }

            CheckCancelationToken(ct);
            if (resourcePath == null || resourcePath.Value == null)
            {
                _logService.LogError($"Unable to sync resources: no resources path configured");
                return false;
            }

            if (!Directory.Exists(_sessionContext.ResourcesFolder()))
            {
                Directory.CreateDirectory(_sessionContext.ResourcesFolder());
            }

            CheckCancelationToken(ct);
            List<ConfigResource> resources = (List<ConfigResource>)_configurationUnitOfWork.GenericRepository<ConfigResource>().All();
            WebConfigValue cachePolicy = _configurationUnitOfWork.GenericRepository<WebConfigValue>().Get("Resource.CachePolicy");

            foreach (ConfigResource cfgRes in resources)
            {
                CheckCancelationToken(ct);
                await SyncResource(cfgRes, resourcePath.Value, ct).ConfigureAwait(false);
                synced = true;
            }

            await _syncStatusService.SetSyncInfo(SyncType.ResourceSync).ConfigureAwait(false);
            return synced;
        }

        private async Task SyncResource(ConfigResource resource, string resourcePath, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(resource.UnitName)
                || string.IsNullOrEmpty(resource.FileName)
                || resource.FileName.ToUpper().StartsWith("ZIP:")
                || resource.FileName.ToUpper().StartsWith("\\"))
            {
                return;
            }

            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath("/" + resourcePath + "/" + resource.FileName));
            _logService.LogDebug($"Syncronizing: {resource.UnitName} - {resource.FileName}");
            try
            {
                await _networkRepository.GetResourceAsync(uriBuilder.ToString(), resource.FileName, 100000, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logService.LogDebug($"Syncronization of : {resource.UnitName} - {resource.FileName} failed with an exception");
                _logService.LogDebug($"{ ex.GetType().Name + " : " + ex.Message}");
            }
        }

        public async Task BuildConfigurationCache(CancellationToken ct)
        {
            await Task.Run(async () =>
            {
                _logService.LogDebug($"Builing the Configuration Cache");
                try
                {
                    _cacheService.ResetCache();
                    _cacheService.AddItem(CacheItemKeys.Menus, await _configurationUnitOfWork.GetMenus(ct).ConfigureAwait(false));
                    CheckCancelationToken(ct);
                    _cacheService.AddItem(CacheItemKeys.Catalogs, await _configurationUnitOfWork.GetCatalogs(ct).ConfigureAwait(false));
                    CheckCancelationToken(ct);
                    _cacheService.AddItem(CacheItemKeys.FieldControls, await _configurationUnitOfWork.GetFieldControls(ct).ConfigureAwait(false));
                    CheckCancelationToken(ct);
                    _cacheService.AddItem(CacheItemKeys.Headers, await _configurationUnitOfWork.GetHeaders(ct).ConfigureAwait(false));
                    CheckCancelationToken(ct);
                    _cacheService.AddItem(CacheItemKeys.TableInfos, await _configurationUnitOfWork.GetTableInfos(ct).ConfigureAwait(false));
                }
                catch (TaskCanceledException)
                {
                    throw new CrmException("User Cancel", CrmExceptionType.UserAction, CrmExceptionSubType.UserActionCanceledByUser);
                }
                catch (Exception ex)
                {
                    _logService.LogDebug($"Cache building failed with an exception");
                    _logService.LogDebug($"{ ex.GetType().Name + " : " + ex.Message}");
                }
            }, ct);
        }

        private void CheckCancelationToken(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                throw new CrmException("User Cancel", CrmExceptionType.UserAction, CrmExceptionSubType.UserActionCanceledByUser);
            }
        }
    }
}
