using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;

namespace ACRM.mobile.Services.Contracts
{
    public interface IOfflineRequestsService
    {
        Task<List<OfflineRequest>> GetAllRequests(CancellationToken cancellationToken);
        Task<OfflineRequest> CreateSaveRequest(ActionTemplateBase template, FieldControl fieldControl, TableInfo tableInfo,
            List<PanelData> inputPanels, Dictionary<string, string> templateFilterValues,
            List<OfflineRecordLink> recordLinks, CancellationToken cancellationToken);
        Task<OfflineRequest> CreateUpdateRequest(ActionTemplateBase template, FieldControl fieldControl, List<PanelData> inputPanels, List<string> recordId, CancellationToken cancellationToken);
        Task<OfflineRequest> CreateUpdateRequest(ActionTemplateBase template, TableInfo tableInfo, string recordId, Dictionary<string, string> templateFilterValues, CancellationToken cancellationToken);
        Task<OfflineRequest> CreateModifyRequest(ActionTemplateBase template, TableInfo tableInfo, string recordId, Dictionary<string, string> templateFilterValues, CancellationToken cancellationToken);
        Task<OfflineRequest> CreateDeleteRequest(ActionTemplateBase template, string infoAreaId, string recordId);
        Task Update(OfflineRequest offlineRequest, CancellationToken cancellationToken);
        Task Delete(OfflineRequest offlineRequest, CancellationToken cancellationToken);
        Task<bool> CreateDocumentUploadRequest(DocumentUpload uploadRequest, ViewReference ViewReference, FieldControl fieldControl, List<PanelData> inputPanels, Dictionary<string, string> templateFilterValues, OfflineRecordLink ParenLink, CancellationToken cancellationToken);
    }
}
