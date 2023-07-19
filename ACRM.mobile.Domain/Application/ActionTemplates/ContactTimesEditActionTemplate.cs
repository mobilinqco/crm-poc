using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class ContactTimesEditActionTemplate : ActionTemplateBase
    {
        public ContactTimesEditActionTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string SearchList()
        {
            return GetValue("SearchList");
        }
    }
}
