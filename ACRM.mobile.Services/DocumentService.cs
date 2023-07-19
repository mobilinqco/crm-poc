using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Network;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;
using Newtonsoft.Json.Linq;

namespace ACRM.mobile.Services
{
    public class DocumentService : ContentServiceBase, IDocumentService
    {
        protected INewOrEditService _newOrEditService;
        protected IOfflineRequestsService _offlineStoreService;

        public DocumentService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            INewOrEditService newOrEditService,
            IOfflineRequestsService offlineStoreService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent,
                fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _newOrEditService = newOrEditService;
            _offlineStoreService = offlineStoreService;
        }

        public async Task<string> GetDocumentPath(string imageKey, CancellationToken cancellationToken)
        {
            return await _crmDataService.GetDocumentPath(imageKey, cancellationToken);
        }

        public new void SetSourceAction(UserAction action)
        {
            _action = action;
            _newOrEditService.SetSourceAction(action);
        }

        public async Task<List<PanelData>> PanelsAsync(CancellationToken cancellationToken)
        {
            return await _newOrEditService.PanelsAsync(cancellationToken);
        }

        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            await _newOrEditService.PrepareContentAsync(cancellationToken);
        }

        public async Task<ObservableCollection<DocumentObject>> PreparePanelDataAsync(PanelData inputArgs, CancellationToken token)
        {
            ObservableCollection<DocumentObject> records = new ObservableCollection<DocumentObject>();
            bool? HasImageSupport = null;
            var maxResults = 100;
            string fieldGroupName = "D3DocData";
            string[] typeParts = inputArgs.PanelTypeKey.Split('.');
            if (typeParts.Count() > 1)
            {
                var DocPanelType = typeParts[1];

                if (DocPanelType.ToLower().StartsWith("img"))
                {
                    HasImageSupport = true;
                }
                else if (DocPanelType.ToLower().StartsWith("noimg"))
                {
                    HasImageSupport = false;
                }

                string[] subParts = DocPanelType.Split('_');

                if (subParts.Count() > 1)
                {
                    if (!int.TryParse(subParts[1], out maxResults))
                    {
                        maxResults = 100;
                    }
                }
            }
            else
            {
                string[] Parts = inputArgs.PanelTypeKey.Split('_');
                if (Parts.Count() > 1 && Parts[0].Equals("DOCSEARCH", StringComparison.InvariantCultureIgnoreCase))
                {
                    var search = await _configurationService.GetSearchAndList(Parts[2], token).ConfigureAwait(false);
                    if (search != null)
                    {
                        fieldGroupName = search.FieldGroupName;
                    }

                    if (Parts.Count() > 2)
                    {
                        if (!int.TryParse(Parts[2], out maxResults))
                        {
                            maxResults = 100;

                        }
                    }
                }
            }

            var FieldControl = await _configurationService.GetFieldControl(fieldGroupName + ".List", token).ConfigureAwait(false);
            if (FieldControl == null)
            {
                fieldGroupName = "D1DocData";
                FieldControl = await _configurationService.GetFieldControl(fieldGroupName + ".List", token).ConfigureAwait(false);
            }

            if (FieldControl == null)
            {
                return records;
            }

            var infoAreaId = FieldControl.InfoAreaId;
            var tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, token).ConfigureAwait(false);
            _infoArea = _configurationService.GetInfoArea(infoAreaId);

            if (FieldControl != null)
            {
                _fieldGroupComponent.InitializeContext(FieldControl, tableInfo);
                if (_fieldGroupComponent.HasTabs())
                {
                    List<FieldControlField> fields = FieldControl.Tabs[0].GetQueryFields();
                    ParentLink parentLink = new ParentLink
                    {
                        LinkId = 127,
                        ParentInfoAreaId = inputArgs.RecordInfoArea,
                        RecordId = inputArgs.RecordId,
                    };

                    var RawData = await _crmDataService.GetData(token,
                        new DataRequestDetails
                        {
                            TableInfo = tableInfo,
                            Fields = fields,
                            SortFields = FieldControl.SortFields
                        },
                        parentLink, maxResults, RequestMode.Best);

                    if (RawData.Result != null)
                    {
                        records = await ProcessDocumentItemsAsync(fields, _fieldGroupComponent, RawData.Result, HasImageSupport, token);
                    }

                }
            }

            return records;
        }

        public async Task<ObservableCollection<DocumentObject>> ProcessDocumentItemsAsync(List<FieldControlField> fieldDefinitions, FieldGroupComponent fieldGroupComponent, DataTable _result, bool? hasImageSupport, CancellationToken token)
        {
            ObservableCollection<DocumentObject> records = new ObservableCollection<DocumentObject>();
            if (_result != null)
            {
                var tasks = _result.Rows.Cast<DataRow>().Select(async row => await GetRow(fieldDefinitions, fieldGroupComponent, row, hasImageSupport, token));
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                var filteredResults = results.Where(y => y != null);
                foreach (var doc in filteredResults)
                {
                    records.Add(doc);
                }
            }
            return records;
        }

        public async Task<string> DownloadDocumentAsync(DocumentObject selectedDoc, CancellationToken token)
        {
            return await _crmDataService.GetDocument(selectedDoc, token);
        }

        public async Task<bool> UploadDocument(DocumentObject document, List<PanelData> inputPanels, CancellationToken token)
        {
            try
            {
                ParentLink parentLink = new ParentLink
                {
                    ParentInfoAreaId = _action?.SourceInfoArea,
                    RecordId = _action?.RecordId,
                };

                if (document.FieldId <= 0)
                {
                    document.FieldId = await GetDocuementFieldId(_action, token);
                }

                Dictionary<string, string> templateFilterValues = await _filterProcessor.FilterToTemplateDictionary(_newOrEditService?.TemplateFilter, token);
                if (_sessionContext.IsInOfflineMode)
                {
                    var linkRecord =  new OfflineRecordLink()
                    {
                        InfoAreaId = parentLink.ParentInfoAreaId,
                        LinkId = 126,
                        RecordId = parentLink.RecordId
                    };
                    DocumentUpload uploadRequest = await _crmDataService.UploadDocumentRequest(token, document);
                    return await _offlineStoreService.CreateDocumentUploadRequest(uploadRequest, _action.ViewReference, _newOrEditService?.FieldComponent.FieldControl, inputPanels, templateFilterValues, linkRecord, token);
                }
                else
                {

                    var result = await UploadDocumentOnline(token, document, parentLink);

                    if (result == null)
                    {
                        return false;
                    }

                    if (inputPanels != null && inputPanels.Count > 0 && inputPanels.HasChanges())
                    {
                        if (inputPanels.HasChanges())
                        {
                            var editinfoArea = _newOrEditService.GetInfoArea();
                            if (editinfoArea == "D1")
                            {
                                await _newOrEditService.Save(inputPanels, token, result.D1RecordId);
                            }
                            else if (editinfoArea == "D3")
                            {
                                await _newOrEditService.Save(inputPanels, token, result.D3RecordId);
                            }

                            return true;
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logService.LogError("Document upload failed with error :" + ex.Message);
                throw ex;
            }

        }

        private async Task<int> GetDocuementFieldId(UserAction action, CancellationToken token)
        {
            int fieldId = -1;
            string configValue = action?.ViewReference.GetArgumentValue("DocumentFieldFieldGroupName");
            if (!string.IsNullOrEmpty(configValue))
            {
                FieldControl fieldControl = await _configurationService.GetFieldControl(configValue + ".Edit", token).ConfigureAwait(false);
                if (fieldControl != null &&
                    fieldControl.Tabs != null &&
                    fieldControl.Tabs.Count > 0 &&
                    fieldControl.Tabs[0].Fields != null &&
                    fieldControl.Tabs[0].Fields.Count > 0)
                {
                    var field = fieldControl.Tabs[0].Fields[0];
                    fieldId = field.FieldId;

                }
            }

            if (fieldId == -1)
            {
                configValue = action?.ViewReference.GetArgumentValue("DocumentFieldId");
                if (!string.IsNullOrEmpty(configValue) && int.TryParse(configValue, out int result))
                {
                    fieldId = result;
                }
            }

            return fieldId;
        }

        public async Task<DocumentUploadResponse> UploadDocumentOnline(CancellationToken token, DocumentObject document, ParentLink parentLink)
        {
            var result = await _crmDataService.UploadDocument(token, document, parentLink, RequestMode.Online);

            if (result != null && result.DataSets != null && result.DataSets.Count > 0)
            {
                foreach (var DataSets in result.DataSets)
                {
                    var tableInfo = await _configurationService.GetTableInfoAsync(DataSets.InfoAreaId, token);
                    if (DataSets.MetaInfos != null
                        && DataSets.MetaInfos.Count > 0
                        && DataSets.Rows != null
                        && DataSets.Rows.Count > 0)
                    {
                        var dataSetMetaInfo = DataSets.MetaInfos[0];
                        var dataSetRecord = DataSets.Rows[0].DataSetRecord;
                        if (IsDocumentInfoAreas(dataSetMetaInfo.InfoAreaId))
                        {
                            _crmDataService.AddOfflineRecords(tableInfo, dataSetMetaInfo, dataSetRecord);
                        }
                        else
                        {
                            _crmDataService.UpdateOfflineRecords(tableInfo, dataSetMetaInfo, dataSetRecord);
                        }
                    }
                }

            }

            return result;
        }

        private bool IsDocumentInfoAreas(string infoAreaId)
        {
            if(infoAreaId.Equals("D1", StringComparison.InvariantCultureIgnoreCase)
                || infoAreaId.Equals("D2", StringComparison.InvariantCultureIgnoreCase)
                || infoAreaId.Equals("D3", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public bool IsMandatoryDataReady(List<PanelData> inputPanels)
        {
            return _newOrEditService.IsMandatoryDataReady(inputPanels);
        }

        public bool IsValidWANUpload(DocumentObject doc)
        {
            long _maxUploadSize;
            if (_configurationService.GetConfigValue("Sync.DocumentUploadMaxSizeForWan") != null)
            {
                if (long.TryParse(_configurationService.GetConfigValue("Sync.DocumentUploadMaxSizeForWan").Value, out _maxUploadSize) && doc!=null && doc.Size > 0)
                {
                    var sizekb = doc.Size / 1000;
                    if (sizekb > _maxUploadSize)
                    {
                        _logService.LogError($"Document {doc.FileName} exceed maximum allowed size of {_maxUploadSize / 1000} MB to be uploaded in WAN.");
                        return false;
                    }
                }
            }

            string supportedMimeTypes = _configurationService.GetConfigValue("Sync.DocumentUploadMimeTypesForWan")?.Value;
            if (!string.IsNullOrEmpty(supportedMimeTypes))
            {
                try
                {
                    JArray typeArray = JArray.Parse(supportedMimeTypes);
                    if (typeArray != null && typeArray.Count > 0)
                    {
                        foreach (var element in typeArray)
                        {
                            string typeValue = element.ToString();

                            if (typeValue.ToLowerInvariant().Equals(doc.MimeType.ToLowerInvariant()))
                            {
                                return true;
                            }
                        }

                        _logService.LogError($"Document {doc.FileName} MimeType {doc.MimeType} can not be uploaded in WAN.");
                        return false;

                    }
                }
                catch
                {
                    _logService.LogError($"The Parameter Sync.DocumentUploadMimeTypesForWan is not well formated.");
                    return true;
                }
                
                
            }

            return true;
        }

        private async Task<DocumentObject> GetRow(List<FieldControlField> fieldDefinitions, FieldGroupComponent fieldGroupComponent, DataRow row, bool? hasImageSupport, CancellationToken cancellationToken)
        {
            var Doc = new DocumentObject();
            var recId = row.GetColumnValue("recid", "-1");
            if(recId.ToLowerInvariant().StartsWith("d1"))
            {
                Doc.RecordIdentification = recId;
                var linkRec = row.GetColumnValue("D3_0_recId", "-1");
                if(string.IsNullOrEmpty(linkRec))
                {
                    Doc.LinkedRecordId = linkRec;
                }
            }
            else
            {
                Doc.RecordIdentification = row.GetColumnValue("D1_0_recId", "-1");
                Doc.LinkedRecordId = recId;
            }
            
            var fieldMIMETYPE = fieldDefinitions.Where(a => a.Function.Equals("mimetype", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (fieldMIMETYPE != null)
            {
                Doc.MimeType = await fieldGroupComponent.ExtractFieldValue(fieldMIMETYPE, row, cancellationToken);
            }

            var fieldTitle = fieldDefinitions.Where(a => a.Function.Equals("Title", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (fieldTitle != null)
            {
                Doc.FileName = await fieldGroupComponent.ExtractFieldValue(fieldTitle, row, cancellationToken);
                string filePath = _sessionContext.DocumentPath(Doc.LocalFileName);
                if (File.Exists(filePath))
                {
                    Doc.Status = FileDownloadStatus.offline;
                }
            }

            var fieldLength = fieldDefinitions.Where(a => a.Function.Equals("Length", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (fieldLength != null)
            {
                var size = await fieldGroupComponent.ExtractFieldValue(fieldLength, row, cancellationToken);
                long.TryParse(size, out long docsize);
                Doc.Size = docsize;
            }

            var fieldDate = fieldDefinitions.Where(a => a.Function.Equals("Date", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (fieldDate != null)
            {
                var srrData = await fieldGroupComponent.ExtractFieldValue(fieldDate, row, cancellationToken);
                Doc.ModificationDate = srrData.CrmDate();
            }

            var fieldUpdDate = fieldDefinitions.Where(a => a.Function.Equals("UpdDate", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            var fieldUpdTime = fieldDefinitions.Where(a => a.Function.Equals("UpdTime", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            var fieldClass = fieldDefinitions.Where(a => a.Function.Equals("Class", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (fieldClass != null)
            {
                Doc.Class = await fieldGroupComponent.ExtractFieldValue(fieldClass, row, cancellationToken);
            }

            var fieldDisplayDate = fieldDefinitions.Where(a => a.Function.Equals("DisplayDate", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            var fieldDisplayText = fieldDefinitions.Where(a => a.Function.Equals("DisplayText", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            var isImage = Doc.IsImage();
            if (hasImageSupport == true)
            {
                if (isImage)
                {
                    return Doc;
                }
                else
                {
                    return null;
                }
            }
            else if (hasImageSupport == false)
            {
                if (!isImage)
                {
                    return Doc;
                }
                else
                {
                    return null;
                }

            }

            return Doc;
        }

    }
}
