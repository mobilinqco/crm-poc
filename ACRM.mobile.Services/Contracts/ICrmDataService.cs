using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Application.Network;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICrmDataService
    {
        Task<DataResponse> GetData(CancellationToken cancellationToken, DataRequestDetails requestDetails,
            ParentLink parentLink = null,
            int maxResults = 100,
            RequestMode requestMode = RequestMode.Fastest);

        Task<DataResponse> GetRecord(CancellationToken cancellationToken, DataRequestDetails requestDetails,
            RequestMode requestMode = RequestMode.Fastest, ParentLink parentLink = null);

        Task<ModifyRecordResult> ModifyRecord(CancellationToken cancellationToken, TableInfo tableInfo, OfflineRequest request,
            RequestMode requestMode = RequestMode.Fastest);
        Task<string> GetDocumentPath(string imageKey, CancellationToken cancellationToken);
        Task<string> GetDocument(DocumentObject selectedDoc, CancellationToken token);
        Task<DocumentUploadResponse> UploadDocument(CancellationToken token, DocumentObject document, ParentLink parentLink, RequestMode online);
        bool AddOfflineRecords(TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord dataSetRecord);
        bool UpdateOfflineRecords(TableInfo tableInfo, DataSetMetaInfo dataMetaInfo, DataSetRecord dataSetRecord);
        Task<DocumentUpload> UploadDocumentRequest(CancellationToken token, DocumentObject document);
        Task<bool> SaveConfigurationsOnline(Dictionary<string,string> configs, CancellationToken cancellationToken);
    }
}
