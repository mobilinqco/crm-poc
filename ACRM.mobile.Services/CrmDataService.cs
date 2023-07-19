using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ACRM.mobile.DataAccess;
using ACRM.mobile.DataAccess.Network;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.DataTree;
using ACRM.mobile.Domain.Application.Network;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Processors;
using ACRM.mobile.Services.SubComponents;
using ACRM.mobile.Services.Utils;

namespace ACRM.mobile.Services
{
    public class CrmDataService : ICrmDataService
    {
        private readonly ISessionContext _sessionContext;
        private readonly ICrmDataUnitOfWork _crmDataUnitOfWork;
        private readonly INetworkRepository _networkRepository;
        private readonly NetworkQueryBuilder _networkQueryBuilder;
        private readonly ILogService _logService;
        private readonly LocalQueryBuilder _localQueryBuilder;
        private readonly SubNodeProcessor _subNodeProcessor;
        protected readonly ICrmRequestBuilder _crmRequestBuilder;

        public CrmDataService(ISessionContext sessionContext,
            ICrmDataUnitOfWork crmDataUnitOfWork,
            INetworkRepository networkRepository,
            NetworkQueryBuilder networkQueryBuilder,
            LocalQueryBuilder localQueryBuilder,
            ICrmRequestBuilder crmRequestBuilder,
            SubNodeProcessor subNodeProcessor,
            ILogService logService)
        {
            _sessionContext = sessionContext;
            _networkQueryBuilder = networkQueryBuilder;

            _crmDataUnitOfWork = crmDataUnitOfWork;
            _logService = logService;
            _localQueryBuilder = localQueryBuilder;
            _networkRepository = networkRepository;
            _crmRequestBuilder = crmRequestBuilder;
            _subNodeProcessor = subNodeProcessor;
        }

        public async Task<DataResponse> GetData(CancellationToken cancellationToken, DataRequestDetails requestDetails,
            ParentLink parentLink = null,
            int maxResults = 100,
            RequestMode requestMode = RequestMode.Fastest)
        {
            DataResponse response = new DataResponse() { Result = null, IsRetrievedOnline = false };
            if (_sessionContext.IsInOfflineMode)
            {
                requestMode = RequestMode.Offline;
            }

            if (requestMode == RequestMode.Offline || requestMode == RequestMode.Fastest)
            {
                response.Result = await GetDataFromLocal(cancellationToken, requestDetails, parentLink, maxResults);

                if (response.Result != null || requestMode == RequestMode.Offline)
                {
                    return response;
                }
            }

            try
            {
                if (parentLink == null || !parentLink.IsNewRecord())
                {
                    response.IsRetrievedOnline = true;
                    response.Result = await GetDataFromNetwork(cancellationToken, requestDetails, parentLink, maxResults);
                    if (requestMode == RequestMode.Fastest && (response.Result == null || response.Result.Rows == null || response.Result.Rows.Count == 0))
                    {
                        response.IsRetrievedOnline = false;
                    }
                }
                else
                {
                    requestMode = RequestMode.Best;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"RecordData network request failed with {ex}");
            }

            if (response.Result == null && requestMode == RequestMode.Best)
            {
                response.IsRetrievedOnline = false;
                response.Result = await GetDataFromLocal(cancellationToken, requestDetails, parentLink, maxResults);
            }

            return response;
        }

        private async Task<DataTable> GetDataFromLocal(CancellationToken cancellationToken, DataRequestDetails requestDetails,
            ParentLink parentLink, int maxResults)
        {
            QueryRequest queryRequest = _localQueryBuilder.GetQueryDetails(requestDetails, parentLink, maxResults);
            return await _crmDataUnitOfWork.RetrieveData(queryRequest, cancellationToken);
        }

        public async Task<string> GetDocumentPath(string imageKey, CancellationToken cancellationToken)
        {
            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath($"/mobile.axd?Service=Documents&DocumentKey={imageKey}"));
            _logService.LogDebug($"GetDocumentPath: {imageKey}");
            try
            {
                string filePath = _sessionContext.DocumentPath(imageKey.ToLower());
                if (File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    var success = await _networkRepository.DownloadDocuemntAsync(uriBuilder.ToString(), filePath, 100000, cancellationToken).ConfigureAwait(false);
                    if (success)
                    {
                        return filePath;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }

            }
            catch (Exception ex)
            {
                _logService.LogDebug($"Download of Document : {imageKey} failed with an exception");
                _logService.LogDebug($"{ ex.GetType().Name + " : " + ex.Message}");
            }

            return string.Empty;
        }

        public async Task<DocumentUploadResponse> UploadDocument(CancellationToken token, DocumentObject document, ParentLink parentLink, RequestMode online)
        {
            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath($"/mobile.axd?Service=Documents&Mode=Upload&RecordId={parentLink.FormatedRecordId}"));
            MultipartFormDataContent multiContent = new MultipartFormDataContent();

            multiContent.Headers.ContentType.MediaType = "multipart/form-data";

            if (document.FileStream != null)
            {
                if (document.FieldId > 0)
                {
                    multiContent.Add(new StringContent(document.FieldId.ToString()), "FieldId");
                }

                HttpContent fileStreamContent = new StreamContent(document.FileStream);
                fileStreamContent.Headers.ContentDisposition = new
                System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    FileName = document.FileName,
                    Name = document.FileName

                };
                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(document.MimeType);
                multiContent.Add(fileStreamContent);

                return await _networkRepository.UploadDocument(uriBuilder.ToString(), multiContent, 100000, token);
            }

            return null;
        }

        public async Task<DocumentUpload> UploadDocumentRequest(CancellationToken token, DocumentObject document)
        {
            var uploadRequest = document.GetDocumentUploadObject();
           
            if (document.FileStream != null)
            {
                if (!Directory.Exists(_sessionContext.DocumentsUploadFolder()))
                {
                    Directory.CreateDirectory(_sessionContext.DocumentsUploadFolder());
                }
                var filePath = _sessionContext.DocumentUploadPath(uploadRequest.LocalFileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await document.FileStream.CopyToAsync(fs);
                }
            }

            uploadRequest.InfoAreaId = "D1";
            uploadRequest.RecordId = "-1";

            return uploadRequest;
        }

        public async Task<string> GetDocument(DocumentObject document, CancellationToken cancellationToken)
        {
            var parts =  document.RecordIdentification.Split('.');
            string urlStr;

            if (parts.Length < 2)
            {
                urlStr = _sessionContext.CrmInstance.UrlPath($"/mobile.axd?Service=Documents&RecordId={document.RecordIdentification}&InfoAreaId=D1&FileName={document.FileName}");
            }
            else
            {
                urlStr = _sessionContext.CrmInstance.UrlPath($"/mobile.axd?Service=Documents&RecordId={parts[1]}&InfoAreaId={parts[0]}&FileName={document.FileName}");
            }

            UriBuilder uriBuilder = new UriBuilder(urlStr);
            _logService.LogDebug($"GetDocument: {document.FileName}");
            try
            {
                string filePath = _sessionContext.DocumentPath(document.LocalFileName);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    var success = await _networkRepository.DownloadDocuemntAsync(uriBuilder.ToString(), filePath, 100000, cancellationToken).ConfigureAwait(false);
                    if (success)
                    {
                        return filePath;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }

            }
            catch (Exception ex)
            {
                _logService.LogDebug($"Download of Document : {document.FileName} failed with an exception");
                _logService.LogDebug($"{ ex.GetType().Name + " : " + ex.Message}");
            }
            return string.Empty;

        }

        private async Task<DataTable> GetDataFromNetwork(CancellationToken cancellationToken, DataRequestDetails requestDetails,
                    ParentLink parentLink, int maxResults)
        {
            SubNode netRequestData = _networkQueryBuilder.GetQueryDetails(requestDetails, requestDetails.RecordId, parentLink, maxResults);
            var queryDef = new List<object>() { new List<object>() { netRequestData.ToArrayRepresentation() } };

            if (netRequestData.SortFields.Count > 0)
            {
                queryDef.Add(netRequestData.SortFields);
            }
            else
            {
                queryDef.Add(null);
            }

            string recordId = requestDetails.RecordId;

            if (parentLink != null)
            {
                if (requestDetails.TableInfo.InfoAreaId.Equals(parentLink.ParentInfoAreaId)
                    && parentLink.LinkId < 0
                    && string.IsNullOrWhiteSpace(recordId))
                {
                    recordId = parentLink.FormatedRecordId;
                    parentLink = null;
                }
            }

            string requestData = _crmRequestBuilder.RecordDataQuery(queryDef, recordId, parentLink?.FormatedRecordId, parentLink != null ? parentLink.LinkId : -1, maxResults);

            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath())
            {
                Query = requestData
            };

            if (maxResults == -1)
            {
                return await ProcessCount(uriBuilder, cancellationToken);
            }

            return await ProcessRecords(netRequestData, uriBuilder, cancellationToken);
        }

        private async Task<DataTable> ProcessCount(UriBuilder uriBuilder, CancellationToken cancellationToken)
        {
            var responseData = await _networkRepository.PostAsync<string, CountRecordsResponse>(uriBuilder.ToString(), "", 100000, cancellationToken);

            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("Count");
            
            DataRow dr = dt.NewRow();
            dr["Count"] = responseData.CountRows[0];
            dt.Rows.Add(dr);

            return dt;
        }

        private async Task<DataTable> ProcessRecords(SubNode netRequestData, UriBuilder uriBuilder, CancellationToken cancellationToken)
        {
            var responseData = await _networkRepository.PostAsync<string, RecordDataResponse>(uriBuilder.ToString(), "", 100000, cancellationToken);

            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("recid");
            dt.Columns.Add("title");

            foreach (string subNodeRecId in netRequestData.SubNodesRecIdAliasList())
            {
                dt.Columns.Add(subNodeRecId);
            }

            foreach (string fieldAlias in netRequestData.FieldsAliasList())
            {
                dt.Columns.Add(fieldAlias);
            }

            responseData.Rows.ForEach(row =>
            {
                if (row.RecordIds != null
                    && row.RecordIds.Count > 0
                    && row.Values != null
                    && row.Values.Count > 0)
                {
                    DataRow dr = dt.NewRow();
                    _subNodeProcessor.PopulateRowRecIds(netRequestData, row.RecordIds, dr);
                    _subNodeProcessor.PopulateRowData(netRequestData, row.Values, dr);
                    dt.Rows.Add(dr);
                }

            });

            return dt;
        }

        public async Task<DataResponse> GetRecord(CancellationToken cancellationToken,
            DataRequestDetails requestDetails,
            RequestMode requestMode = RequestMode.Fastest, ParentLink parentLink = null)
        {
            DataResponse response = new DataResponse() { Result = null, IsRetrievedOnline = false };
            if (_sessionContext.IsInOfflineMode)
            {
                requestMode = RequestMode.Offline;
            }
            if (requestMode == RequestMode.Offline || requestMode == RequestMode.Fastest || (!string.IsNullOrEmpty(requestDetails.RecordId) && requestDetails.RecordId.Contains("new")))
            {
                response.Result = await GetRecordFromLocal(cancellationToken, requestDetails, parentLink);

                if (response.Result != null || requestMode == RequestMode.Offline)
                {
                    return response;
                }
            }

            try
            {
                response.IsRetrievedOnline = true;
                response.Result = await GetDataFromNetwork(cancellationToken, requestDetails, parentLink, 1);
            }
            catch (Exception ex)
            {
                _logService.LogError($"RecordData network request failed with {ex}");
            }

            if ((response.Result == null || response.Result.Rows == null || response.Result.Rows.Count == 0)
                && requestMode == RequestMode.Best)
            {
                response.IsRetrievedOnline = false;
                response.Result = await GetRecordFromLocal(cancellationToken, requestDetails, parentLink);
            }

            return response;
        }

        private async Task<DataTable> GetRecordFromLocal(CancellationToken cancellationToken, DataRequestDetails requestDetails, ParentLink parentLink = null)
        { 
            string recId = requestDetails.RecordId;
            if (recId != null && recId.StartsWith(requestDetails.TableInfo.InfoAreaId))
            {
                recId = requestDetails.RecordId.Substring(2);
            }
            
            QueryRequest queryRequest = _localQueryBuilder.GetQueryDetails(requestDetails, parentLink, 1);
            return await _crmDataUnitOfWork.RetrieveData(queryRequest, cancellationToken);
        }

        public async Task<ModifyRecordResult> ModifyRecord(CancellationToken cancellationToken, TableInfo tableInfo, OfflineRequest request,
            RequestMode requestMode = RequestMode.Fastest)
        {
            ModifyRecordResult result = new ModifyRecordResult();
            if(request.Records != null && request.Records.Count > 0)
            {
                result = ModifyRecordLocal(tableInfo, request);
                if(result.HasSaveErrors())
                {
                    return result;
                }
                
                if (requestMode != RequestMode.Offline)
                {
                    return await ModifyRecordNetwork(cancellationToken, tableInfo, request);
                    
                }
            }
            
            return result;
        }
        public bool AddOfflineRecords(TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord dataSetRecord)
        {
            _logService.LogDebug($"Saving Record: { dataSetRecord.RecordId + ", InfoArea: " + dataSetRecord.InfoAreaId }");
            _crmDataUnitOfWork.AddRecord(tableInfo, dataMetaInfo, dataSetRecord);
            return true;
        }

        public bool UpdateOfflineRecords(TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord dataSetRecord)
        {
            _logService.LogDebug($"Saving Record: { dataSetRecord.RecordId + ", InfoArea: " + dataSetRecord.InfoAreaId }");
            _crmDataUnitOfWork.UpdateRecord(tableInfo, dataMetaInfo, dataSetRecord);
            return true;
        }

        private async Task<ModifyRecordResult> ModifyRecordNetwork(CancellationToken cancellationToken, TableInfo tableInfo, OfflineRequest request)
        {
            List<string> requestDef = _networkQueryBuilder.GetModifyRequest(tableInfo, request);
            string requestData = _crmRequestBuilder.SaveRecords(requestDef);
            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath())
            {
                Query = requestData
            };

            DataSaveResponse responseData = null;
            ModifyRecordResult result = new ModifyRecordResult();

            try
            {
                responseData = await _networkRepository.PostAsync<string, DataSaveResponse>(uriBuilder.ToString(), "", 100000, cancellationToken);
            }
            catch (HttpRequestExceptionEx hrex)
            {
                ServerErrorResponse ser = new ServerErrorResponse((int)hrex.HttpCode, hrex.StackTrace);
                request.ErrorStack = hrex.StackTrace;
                request.ErrorMessage = ser.UserText;
                request.ErrorCode = ser.ErrorCode;
                result.ServerErrorResponse = ser;
                return result;
            }
            catch (Exception ex)
            {
                ServerErrorResponse ser = new ServerErrorResponse(ex);
                request.ErrorStack = ex.StackTrace;
                request.ErrorMessage = ser.UserText;
                request.ErrorCode = ser.ErrorCode;
                result.ServerErrorResponse = ser;
                return result;
            }

            result.SavedRecord = responseData?.Record;

            if (responseData != null && responseData.StatusInfo != null)
            {
                if (responseData.IsError())
                {
                    ServerErrorResponse ser = new ServerErrorResponse(responseData.StatusInfo);
                    request.ErrorStack = string.Join(",", responseData.StatusInfo);
                    request.ErrorMessage = ser.UserText;
                    request.ErrorCode = ser.ErrorCode;
                    result.ServerErrorResponse = ser;
                    return result;
                }
            }

            if (responseData.Record.DataSets != null && responseData.Record.DataSets.Count > 0)
            {
                await Task.Run(() =>
                    responseData.Record.DataSets.ForEach(ds =>
                    {
                        if (tableInfo.InfoAreaId.Equals(ds.InfoAreaId))
                        {
                            _crmDataUnitOfWork.AddRange(tableInfo, ds);
                        }
                        else
                        {
                            var ti = _networkQueryBuilder.TableInfoForInfoArea(ds.InfoAreaId);
                            if (ti != null)
                            {
                                _crmDataUnitOfWork.AddRange(ti, ds);
                            }
                        }
                    })
                ).ConfigureAwait(false);
                

                request.Records.ForEach(record =>
                {
                    if (record.Mode.ToLower().Equals("new"))
                    {
                        _crmDataUnitOfWork.DeleteRecord(tableInfo, record.RecordId);
                    }
                });
            }
            return result;            
        }

        private ModifyRecordResult ModifyRecordLocal(TableInfo tableInfo, OfflineRequest request)
        {
            ModifyRecordResult result = new ModifyRecordResult();
            try
            {
                result.OfflineDataSetRecords = new List<DataSetRecord>();
                request.Records.ForEach(record =>
                {
                    if (!string.IsNullOrEmpty(record.Mode))
                    {
                        if (record.Mode.ToLower().Equals("new"))
                        {
                            TableInfo ti = tableInfo;
                            _logService.LogDebug($"Saving Record: { record.RecordId + ", InfoArea: " + record.InfoAreaId }");
                            if (!record.InfoAreaId.Equals(tableInfo.InfoAreaId))
                            {
                                ti = _networkQueryBuilder.TableInfoForInfoArea(record.InfoAreaId);
                            }
                            (DataSetMetaInfo dataSetMetaInfo, DataSetRecord dataSetRecord) = _localQueryBuilder.PrepareInsertData(ti, record);
                            result.OfflineDataSetRecords.Add(dataSetRecord);
                            _crmDataUnitOfWork.AddRecord(ti, dataSetMetaInfo, dataSetRecord);
                        }
                        else if (record.Mode.ToLower().Equals("delete"))
                        {
                            _logService.LogDebug($"Deleting Record: { record.RecordId + ", InfoArea: " + record.InfoAreaId }");
                            _crmDataUnitOfWork.DeleteRecord(tableInfo, record.RecordId);
                        }
                        else if (record.Mode.ToLower().Equals("update"))
                        {
                            TableInfo ti = tableInfo;
                            _logService.LogDebug($"Updating Record: { record.RecordId + ", InfoArea: " + record.InfoAreaId }");
                            if(!record.InfoAreaId.Equals(tableInfo.InfoAreaId))
                            {
                                ti = _networkQueryBuilder.TableInfoForInfoArea(record.InfoAreaId);
                            }
                            (DataSetMetaInfo dataSetMetaInfo, DataSetRecord dataSetRecord) = _localQueryBuilder.PrepareInsertData(ti, record);
                            result.OfflineDataSetRecords.Add(dataSetRecord);
                            _crmDataUnitOfWork.UpdateRecord(ti, dataSetMetaInfo, dataSetRecord);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logService.LogError($"SaveRecord offline failed with {ex}");
                result.LocalSaveError = $"{ex}";
                return result;
            }

            return result;
        }

        public async Task<bool> SaveConfigurationsOnline(Dictionary<string, string> configs, CancellationToken cancellationToken)
        {
            try
            {

                string changeData = GetChangeData(configs);
                UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath($"/mobile.axd?Service=ChangeConfiguration&Changes={HttpUtility.UrlEncode(changeData)}"));
                bool result = await _networkRepository.PostAsync(uriBuilder.ToString(), "", 100000, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _logService.LogError($"SaveConfigurationsOnline failed with {ex}");
            }

            return false;
        }

        private string GetChangeData(Dictionary<string, string> configs)
        {
            StringBuilder changeData = new StringBuilder("[");
            bool firstItem = true;
            foreach (var key in configs.Keys.ToList())
            {
                if (firstItem)
                {
                    firstItem = false;
                }
                else
                {
                    changeData.Append(",");
                }
                changeData.Append($"[\"{key}\", \"{configs[key]}\"]");
            }

            changeData.Append("]");
            return changeData.ToString();
        }
    }
}

