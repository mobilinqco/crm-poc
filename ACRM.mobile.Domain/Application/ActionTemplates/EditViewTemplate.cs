using System;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.ActionTemplates
{ 
    public class EditViewTemplate: ActionTemplateBase
    {
        public EditViewTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string TemplateFilterName()
        {
            return GetValue("TemplateFilterName");
        }

        public string CopySourceFieldGroupName()
        {
            return GetValue("CopySourceFieldGroupName");
        }

        public bool IsNewAction()
        {
            if (!string.IsNullOrEmpty(viewReferenceModel.Name) && viewReferenceModel.Name.ToLower().Equals("newview"))
            {
                return true;
            }

            return false;
        }
    }
}
