using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class ModifyRecordTemplate: ActionTemplateBase
    {
        public ModifyRecordTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string RecordId()
        {
            return GetValue("RecordId");
        }

        public string TemplateFilter()
        {
            return GetValue("TemplateFilter");
        }

        public string RightsFilter()
        {
            return GetValue("RightsFilter");
        }

        public string CopySourceFieldGroupName()
        {
            return GetValue("CopySourceFieldGroupName");
        }

        public string RecordIdForSavedAction()
        {
            return GetValue("RecordIdForSavedAction");
        }

        public string LinkRecordId()
        {
            return GetValue("LinkRecordId");
        }
    }
}
