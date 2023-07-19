using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class NewInBackgroundActionTemplate : ActionTemplateBase
    {
        public NewInBackgroundActionTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string TemplateFilterName()
        {
            return GetValue("TemplateFilter");
        }

        public string RightsFilterName()
        {
            return GetValue("RightsFilter");
        }

        public string CreatedActionName()
        {
            return GetValue("CreatedAction");
        }

        public bool CheckExisting()
        {
            return GetValue("CheckExisting") == "true";
        }

        public string InfoAreaId()
        {
            return GetValue("InfoAreaId");
        }

        public string CopySourceFieldGroupName()
        {
            return GetValue("CopySourceFieldGroupName");
        }

        public override string SavedAction()
        {
            return GetValue("CreatedAction");
        }
    }
}
