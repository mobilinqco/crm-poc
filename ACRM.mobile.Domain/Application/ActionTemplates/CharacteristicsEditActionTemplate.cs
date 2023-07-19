using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class CharacteristicsEditActionTemplate : ActionTemplateBase
    {

        public CharacteristicsEditActionTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        new public string ExpandName()
        {
            return GetValue("DestinationFieldGroup");
        }

        public string GroupSearchAndList()
        {
            return GetValue("GroupSearchAndList");
        }

        public string ItemSearchAndList()
        {
            return GetValue("ItemSearchAndList");
        }
    }
}
