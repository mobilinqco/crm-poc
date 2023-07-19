using System;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class SerialEntryTemplate: ActionTemplateBase
    {
        public SerialEntryTemplate(ViewReference viewReference) : base(viewReference)
        {
        }

        // Name of the referenced Search&List configuration, fallback: field group of the same name.
        public string SourceConfigName()
        {
            string value = GetValue("SourceConfigName");
            return value;
        }
        public string SourceChildConfigName()
        {
            string value = GetValue("SourceChildConfigName");
            return value;
        }

        public string SourceCopyFieldGroup()
        {
            string value = GetValue("SourceCopyFieldGroup");
            return value;
        }

        public string DestinationChildConfigName()
        {
            string value = GetValue("DestinationChildConfigName");
            return value;
        }

        public string DestinationRootConfig()
        {
            string value = GetValue("DestinationRootConfig");
            return value;
        }

        public string EditType()
        {
            string value = GetValue("EditType");
            return value;
        }

        public string DestinationConfigName()
        {
            string value = GetValue("DestinationConfigName");
            return value;
        }

        public string DestinationTemplateFilter()
        {
            return viewReferenceModel.GetArgumentValue("DestinationTemplateFilter");
        }

        public string GetFinishAction()
        {
            string value = GetValue("FinishAction");
            return value;
        }
        
        public bool GetHierarchicalPositionFilter()
        {
            string value = GetValue("HierarchicalPositionFilter");
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            return value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ? true : false;
        }

        public string GetItemNumberFunctionName()
        {
            return GetValue("ItemNumberFunctionName");
        }
    }
}
