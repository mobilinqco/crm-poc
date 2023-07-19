using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface IUserActionBuilder
    {
        UserAction UserActionFromButton(IConfigurationService configurationService, Button button, string recordId = null, string recordInfoArea = null, string rawRecordId = null, bool isRecordRetrievedOnline = false);
        UserAction UserActionFromMenu(IConfigurationService configurationService, Menu menu, string recordId = null, string infoAreaId = null);
        ActionTemplateBase ResolveActionTemplate(ViewReference viewReference);
        UserActionType ResolveActionType(ViewReference viewReference);

        ParentLink GetParentLink(UserAction userAction, ActionTemplateBase actionTemplate);
        Task<UserAction> ResolveSavedAction(IConfigurationService configurationService, string savedActionName, string recordId, string infoAreaId, CancellationToken cancellationToken);
        ParentLink GetRootRecord(UserAction userAction);
    }
}
