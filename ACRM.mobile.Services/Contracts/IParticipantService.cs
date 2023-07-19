using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.Services.Contracts
{
    public interface IParticipantService : IContentServiceBase
    {
        List<PopupListItem> Requirements { get; set;}
        List<PopupListItem> Acceptance { get; set; }
        ISerialEntryEditService ChildEditServic { get;}
        FieldInfo AcceptanceFieldInfo { get; }
        FieldInfo RequirementFieldInfo { get; }
        Task<List<ParticipantData>> BuildParticipants(PanelData data, CancellationToken cancellationToken);
        Task<List<ParticipantData>> BuildRepParticipants(PanelData data, CancellationToken cancellationToken);
        Task<List<PopupListItem>> GetRequirementCatalogs(CancellationToken cancellationToken);
        Task<List<PopupListItem>> GetAcceptanceCatalogs(CancellationToken cancellationToken);
        Task<List<SelectableFieldValue>> GetRepList(CancellationToken cancellationToken);
        Task<ParticipantData> GetParticipant(SelectableFieldValue selectedRep, CancellationToken token);
        Task<List<ParticipantData>> BuildLinkParticipants(PanelData data, CancellationToken cancellationToken);
        Task<string> GetFieldText(ListDisplayField field, CancellationToken token);
    }
}
