using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Network;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services.Contracts
{
    public interface IDocumentService : IContentServiceBase
    {
        Task<string> GetDocumentPath(string imageKey, CancellationToken cancellationToken);
        Task<ObservableCollection<DocumentObject>> PreparePanelDataAsync(PanelData inputArgs, CancellationToken token);
        Task<string> DownloadDocumentAsync(DocumentObject selectedDoc, CancellationToken token);
        Task<ObservableCollection<DocumentObject>> ProcessDocumentItemsAsync(List<FieldControlField> fieldDefinitions, FieldGroupComponent fieldGroupComponent, DataTable _result, bool? hasImageSupport, CancellationToken token);
        Task<bool> UploadDocument(DocumentObject document, List<PanelData> inputPanels, CancellationToken token);
        Task<List<PanelData>> PanelsAsync(CancellationToken cancellationToken);
        bool IsMandatoryDataReady(List<PanelData> inputPanels);
        Task<DocumentUploadResponse> UploadDocumentOnline(CancellationToken token, DocumentObject document, ParentLink parentLink);
        bool IsValidWANUpload(DocumentObject doc);
    }
}
