using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.Utils
{
    public class BackgroundSyncWorker
    {
        protected ISessionContext _sessionContext;
        protected IOfflineRequestsService _offlineRequestsService;
        protected ICrmDataService _crmDataService;
        protected IConfigurationService _configurationService;
        protected IDataSyncService _dataSyncService;
        protected ILogService _logService;
        protected IDocumentService _docService;
        protected BackgroundSyncManager _backgroundSyncManager;

        protected CancellationTokenSource _cancellationTokenSource;

        private int _intervalMinutesUMTS = 60;
        private int _intervalMinutesWLAN = 15;
        private List<DateTime> _configSyncDateTimes = new List<DateTime>();

        public BackgroundSyncWorker(ISessionContext sessionContext,
            IOfflineRequestsService offlineRequestsService,
            ICrmDataService crmDataService,
            IConfigurationService configurationService,
            IDataSyncService dataSyncService,
            ILogService logService,
            IDocumentService docService,
            BackgroundSyncManager backgroundSyncManager)
        {
            _sessionContext = sessionContext;
            _offlineRequestsService = offlineRequestsService;
            _crmDataService = crmDataService;
            _configurationService = configurationService;
            _dataSyncService = dataSyncService;
            _logService = logService;
            _docService = docService;
            _backgroundSyncManager = backgroundSyncManager;
            _cancellationTokenSource = new CancellationTokenSource();

            SetConfigValues();
        }

        private void SetConfigValues()
        {
            if(_configurationService.GetConfigValue("Sync.IntervalMinutesUMTS") != null)
            {
                int.TryParse(_configurationService.GetConfigValue("Sync.IntervalMinutesUMTS").Value, out _intervalMinutesUMTS);
            }

            if(_configurationService.GetConfigValue("Sync.IntervalMinutesWLAN") != null)
            {
                int.TryParse(_configurationService.GetConfigValue("Sync.IntervalMinutesWLAN").Value, out _intervalMinutesWLAN);
            }

            if(_configurationService.GetConfigValue("Sync.Config") != null)
            {
                SyncConfigParser syncConfigParser = new SyncConfigParser();

                // Default timezone
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("UTC");

                if (!string.IsNullOrEmpty(_configurationService.GetServerTimezone()))
                {
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(_configurationService.GetServerTimezone());
                }

                _configSyncDateTimes.AddRange(syncConfigParser.FullSyncDateTimes(
                    _configurationService.GetConfigValue("Sync.Config").Value, timeZoneInfo));
            }
        }

        public void StartBackgroundSync(IEnumerable<ConnectionProfile> internetConnectionProfiles)
        {
            Task.Factory.StartNew(
                () => ExecuteBackgroundSync(internetConnectionProfiles),
                _cancellationTokenSource.Token
            );
        }

        private async Task ExecuteBackgroundSync(IEnumerable<ConnectionProfile> internetConnectionProfiles)
        {
            await InitOfflineRequestsConflictStatus();

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    int minutesUntilNextConfigSync = 0;
                    foreach(DateTime configSyncDateTime in _configSyncDateTimes)
                    {
                        if(configSyncDateTime >= DateTime.Now)
                        {
                            minutesUntilNextConfigSync = (int)(DateTime.Now - configSyncDateTime).TotalMinutes;
                            break;
                        }
                    }

                    if ((internetConnectionProfiles.Contains(ConnectionProfile.WiFi) || internetConnectionProfiles.Contains(ConnectionProfile.Ethernet)) && (_intervalMinutesWLAN != 0 || minutesUntilNextConfigSync != 0))
                    {
                        await OfflineRequestsSync();
                        await IncrementalSync();
                        await Task.Delay(ComputeSyncMilliseconds(_intervalMinutesWLAN, minutesUntilNextConfigSync), _cancellationTokenSource.Token);
                    }
                    else if (internetConnectionProfiles.Count() > 0 && (_intervalMinutesUMTS != 0 || minutesUntilNextConfigSync != 0))
                    {
                        await Task.Delay(ComputeSyncMilliseconds(_intervalMinutesWLAN, minutesUntilNextConfigSync), _cancellationTokenSource.Token);
                        await OfflineRequestsSync(true);
                        await IncrementalSync();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logService.LogError($"BackgroundSync failed with {ex}");

                    // Add delay before trying again in case WiFi connection is available
                    if(internetConnectionProfiles.Contains(ConnectionProfile.WiFi) && _intervalMinutesWLAN != 0)
                    {
                        await Task.Delay(_intervalMinutesWLAN * 60000, _cancellationTokenSource.Token);
                    }
                }
            }
        }

        private int ComputeSyncMilliseconds(int intervalMinutes, int syncConfigMinutes)
        {
            int minutes;
            if (intervalMinutes == 0)
            {
                minutes = syncConfigMinutes;
            }
            else if (syncConfigMinutes == 0)
            {
                minutes = intervalMinutes;
            }
            else if (intervalMinutes <= syncConfigMinutes)
            {
                minutes = intervalMinutes;
            }
            else
            {
                minutes = syncConfigMinutes;
            }
            return minutes * 60000;
        }

        private async Task InitOfflineRequestsConflictStatus()
        {
            var offlineRequests = await _offlineRequestsService.GetAllRequests(_cancellationTokenSource.Token);

            foreach (OfflineRequest offlineRequest in offlineRequests)
            {
                if(!string.IsNullOrWhiteSpace(offlineRequest.ErrorMessage))
                {
                    _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(true);
                    break;
                }
            }
        }

        private async Task OfflineRequestsSync(bool IsShowConnection = false)
        {
            bool tempConflictsFlag = false;

            var offlineRequests = await _offlineRequestsService.GetAllRequests(_cancellationTokenSource.Token);

            foreach (OfflineRequest offlineRequest in offlineRequests)
            {
                if (offlineRequest.Records.Count > 0 && !_cancellationTokenSource.Token.IsCancellationRequested)
                {

                    if (offlineRequest.RequestType.Equals("DocumentUpload"))
                    {
                        if (string.IsNullOrEmpty(offlineRequest.ErrorStack))
                        {
                            try
                            {

                                if (offlineRequest.DocumentUploads.Count > 0)
                                {
                                    var docUpoloadObj = offlineRequest.DocumentUploads[0];
                                    var offlineRecord = offlineRequest.Records[0];
                                    var offlineLink = offlineRecord.RecordLinks[0];
                                    var doc = new DocumentObject(docUpoloadObj);
                                    var filePath = _sessionContext.DocumentUploadPath(docUpoloadObj.LocalFileName);
                                    var FileResult = new FileResult(filePath);
                                  
                                    if(IsShowConnection && !_docService.IsValidWANUpload(doc))
                                    {
                                        continue;
                                    }

                                    ParentLink parentLink = new ParentLink
                                    {
                                        ParentInfoAreaId = offlineLink.InfoAreaId,
                                        RecordId = offlineLink.RecordId,
                                        LinkId = offlineLink.LinkId
                                    };

                                    using (doc.FileStream = await FileResult.OpenReadAsync())
                                    {
                                        
                                        var uploadResult = await _docService.UploadDocumentOnline(_cancellationTokenSource.Token, doc, parentLink);
                                        if (uploadResult == null)
                                        {
                                            tempConflictsFlag = true;
                                            offlineRequest.ErrorMessage = $"Failed to upload Document {doc.FileName}";
                                        }
                                        else
                                        {
                                            offlineRecord.RecordId = uploadResult.D1RecordId;
                                        }
                                    }


                                    if (!tempConflictsFlag && offlineRecord.RecordFields != null && offlineRecord.RecordFields.Count > 0)
                                    {
                                        var tableInfo = await _configurationService.GetTableInfoAsync(offlineRecord.InfoAreaId, _cancellationTokenSource.Token);

                                        var result = await _crmDataService.ModifyRecord(_cancellationTokenSource.Token, tableInfo, offlineRequest, RequestMode.Online);

                                        if (result.HasSaveErrors())
                                        {
                                            tempConflictsFlag = true;
                                            offlineRequest.ErrorMessage = result.ErrorMessage();

                                        }

                                    }

                                    if (!tempConflictsFlag)
                                    {
                                        try
                                        {
                                            if (File.Exists(filePath))
                                            {
                                                File.Delete(filePath);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logService.LogError($"OfflineRequestsSync deletion of file {filePath} failed, Ex : {ex}");

                                        }
                                        await _offlineRequestsService.Delete(offlineRequest, _cancellationTokenSource.Token);
                                    }

                                }
                            }
                            catch (AuthenticationException ex)
                            {
                                _logService.LogError($"OfflineRequestsSync upload failed, Ex : {ex}");
                            }
                            catch (Exception ex)
                            {
                                _logService.LogError($"OfflineRequestsSync upload  failed, Ex : {ex}");
                                offlineRequest.ErrorMessage = ex.Message;
                                tempConflictsFlag = true;
                            }
                            finally
                            {
                                if (tempConflictsFlag)
                                {
                                    try
                                    {
                                        offlineRequest.ErrorStack = "DocumentUploadFailed";
                                        await _offlineRequestsService.Update(offlineRequest, _cancellationTokenSource.Token);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logService.LogError($"offlineRequestsService.Updat  failed, Ex : {ex}");
                                
                                    }
                                }
                            }

                        }
                    }
                    else
                    {

                        var tableInfo = await _configurationService.GetTableInfoAsync(offlineRequest.Records[0].InfoAreaId, _cancellationTokenSource.Token);

                        var result = await _crmDataService.ModifyRecord(_cancellationTokenSource.Token, tableInfo, offlineRequest, RequestMode.Online);

                        if (!result.HasSaveErrors())
                        {
                            await _offlineRequestsService.Delete(offlineRequest, _cancellationTokenSource.Token);
                        }
                        else
                        {
                            tempConflictsFlag = true;
                            await _offlineRequestsService.Update(offlineRequest, _cancellationTokenSource.Token);
                        }

                    }
                }
            }

            _backgroundSyncManager.UpdateOfflineRequestsSyncConflictsStatus(tempConflictsFlag);
        }

        private async Task IncrementalSync()
        {
            await _dataSyncService.IncrementalDataSync(_cancellationTokenSource.Token);
        }

        public void StopBackgroundSync()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
