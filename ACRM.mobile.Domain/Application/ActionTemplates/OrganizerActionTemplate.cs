using System;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.ActionTemplates
{
    public class OrganizerActionTemplate: ActionTemplateBase
    {
        public OrganizerActionTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string Action()
        {
            return GetValue("Action");
        }

        public string ContextMenuAction()
        {
            return GetValue("ContextMenuAction");
        }

        public string TemplateFilterName()
        {
            return GetValue("TemplateFilter");
        }

        public string CopySourceFieldGroupName()
        {
            return GetValue("CopySourceFieldGroupName");
        }

        public bool IsToggleFavorite()
        {
            if (Action().ToLower().Equals("togglefavorite"))
            {
                return true;
            }

            return false;
        }
    }
}
