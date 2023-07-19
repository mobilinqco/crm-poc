using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class SaveAndConfirmTemplate : ActionTemplateBase
    {

        public SaveAndConfirmTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string ConfirmFilter()
        {
            return GetValue("ConfirmFilter");
        }

        public string BaseRecordConfirmFilter()
        {
            return GetValue("BaseRecordConfirmFilter");
        }
    }
}
