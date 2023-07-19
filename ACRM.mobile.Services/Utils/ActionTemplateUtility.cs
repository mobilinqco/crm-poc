using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services.Utils
{
    public static class ActionTemplateUtility
    {
        public static async Task<ActionTemplateBase> ResolveActionTemplate(UserAction userAction, CancellationToken cancellationToken, IConfigurationService configurationService)
        {
            ViewReference vr = userAction.ViewReference;

            if (vr == null)
            {
                vr = await configurationService.GetViewForMenu(userAction.ActionUnitName, cancellationToken).ConfigureAwait(false);
            }

            if (userAction?.ActionType == UserActionType.RecordSelector)
            {
                return userAction.RecordSelector;
            }
            if (userAction?.ActionType == UserActionType.SerialEntryListing)
            {
                return new SerialEntryTemplate(vr);
            }
            else
            {
                return new RecordListViewTemplate(vr);
            }
        }
    }
}
