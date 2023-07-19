using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application
{
    public class CrmFieldConfiguration
    {
        public FieldControlField FieldConfig { get; private set; }
        public PresentationFieldAttributes PresentationFieldAttributes { get; private set; }
        public List<SelectableFieldValue> AllowedValues { get; private set; }
        public UserAction RecordSelectorAction { get; private set; }

        public CrmFieldConfiguration(FieldControlField fieldConfig,
            PresentationFieldAttributes presentationFieldAttributes,
            List<SelectableFieldValue> allowedValues = null,
            UserAction recordSelectorAction = null)
        {
            FieldConfig = fieldConfig;
            PresentationFieldAttributes = presentationFieldAttributes;
            AllowedValues = allowedValues;
            RecordSelectorAction = recordSelectorAction;
        }
    }
}
