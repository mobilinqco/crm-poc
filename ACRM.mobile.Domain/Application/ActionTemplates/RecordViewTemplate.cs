using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.ActionTemplates
{
    public class RecordViewTemplate: ActionTemplateBase
    {
        public RecordViewTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        // If set to true, a Details organizer (that is open in the background) is refreshed when it is accessed anew.
        //  Note: If you set this option, ensure that the DefaultAction is set to ShowRecord.
        public bool OrganizerRefresh()
        {
            string strVal = GetValue("OrganizerRefresh");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }

            return false;
        }

        public List<string> SyncParentInfoAreas()
        {
            return GetValue("SyncParentInfoAreaId").Split(',').ToList();
        }
    }
}
