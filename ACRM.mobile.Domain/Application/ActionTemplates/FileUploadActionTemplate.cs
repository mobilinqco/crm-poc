using System;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class FileUploadActionTemplate : ActionTemplateBase
    {
        public FileUploadActionTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string TemplateFilterName()
        {
            var filterName = GetValue("TemplateFilterName");
            if (string.IsNullOrEmpty(filterName))
            {
                filterName = GetValue("FilterName");
            }
            return filterName;
        }

        public string UploadFieldsName()
        {      
            return GetValue("UploadFields"); ;
        }

    }
}
