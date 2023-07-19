using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.OfflineSync;
using ACRM.mobile.Services.SubComponents;

namespace ACRM.mobile.Services.Contracts
{
    public interface INewOrEditService: IContentServiceBase
    {
        FieldGroupComponent FieldComponent
        {
            get;
        }

        public Filter TemplateFilter
        {
            get;
        }

        public bool IsNewMode
        {
            get;
        }

        public bool IsInBackground
        {
            get;
        }

        public bool ExecuteTriggersInSequence
        {
            get;
        }

        Task<ModifyRecordResult> Save(List<PanelData> inputPanels, CancellationToken cancellationToken, string RecordId = null, List<OfflineRecordLink> recordLinks = null);
        Task<List<PanelData>> PanelsAsync(CancellationToken cancellationToken);
        bool IsMandatoryDataReady(List<PanelData> inputPanels);
        bool HasHeaderTitle();
        string GetInfoArea();

        Task<UserAction> SavedAction(string recordId, CancellationToken cancellationToken);
        Task FetchRecord(string recordId, CancellationToken cancellationToken);
        Task<List<EditTriggerItem>> GetTheEditTriggers(IFilterProcessor filterProcessor, CancellationToken token);
        Task<Dictionary<string, string>> ProcessTrigger(EditTriggerItem trigger, Dictionary<string, string> AllParameters, Dictionary<string, string> inputParams, CancellationToken token);
        Task<List<EditFieldConstraintViolation>> ProcessClientFilter(List<PanelData> inputPanels, CancellationToken token);
        Task<int> GetParentCode(ListDisplayField parentField, CancellationToken token);
    }
}
