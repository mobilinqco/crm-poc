using System;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class OpenURLTemplate: ActionTemplateBase
    {
        public OpenURLTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        public string FieldGroup()
        {
            return viewReferenceModel.GetArgumentValue("FieldGroup");
        }

        public string Encoding()
        {
            return viewReferenceModel.GetArgumentValue("encoding");
        }

        public string Url()
        {
            return viewReferenceModel.GetArgumentValue("url");
        }

        public bool PopToPrevious()
        {
            string strVal = GetValue("PopToPrevious");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }

            return false;
        }

        public string DotReplaceChar()
        {
            return viewReferenceModel.GetArgumentValue("DotReplaceChar");
        }

        public bool RecomputeRecordId()
        {
            string strVal = viewReferenceModel.GetArgumentValue("RecomputeRecordId");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }

            return false;
        }
    }
}
