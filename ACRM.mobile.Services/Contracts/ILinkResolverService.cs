using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.Services.Contracts
{
    public interface ILinkResolverService : IContentServiceBase
    {
        Task<string> GetLinkedRecord(ParentLink link, string infoAreaId, CancellationToken cancellationToken, RequestMode requestMode = RequestMode.Best);
        Task<string> GetLinkedRecordForAction(UserAction action, string parentLink, CancellationToken token);
        Task<UserAction> ResolveRecordSwitch(UserAction userAction, CancellationToken cancellationToken);
    }
}
