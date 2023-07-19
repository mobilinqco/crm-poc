using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class RecordSelectorTemplate: ActionTemplateBase
    {
        public RecordSelector RecordSelector { get; private set; }

        public RecordSelectorTemplate(RecordSelector recordSelector,
            ViewReference viewReference) : base(viewReference)
        {
            RecordSelector = recordSelector;
        }

        public List<string> ClearFields()
        {
            List<string> clearFields = null;

            if (HasRecordSelector())
            {
                clearFields = RecordSelector.Clear;
            }

            if (clearFields == null && HasViewReference())
            {
                clearFields = viewReferenceModel.GetArrayArgumentValue("ClearValues");
            }

            return clearFields; 
        }

        public string TargetPrefix()
        {
            string targetPrefix = string.Empty;

            if (HasRecordSelector())
            {
                targetPrefix = RecordSelector.TargetPrefix;
            }

            if (string.IsNullOrEmpty(targetPrefix) && HasViewReference())
            {
                targetPrefix = viewReferenceModel.GetArgumentValue("TargetPrefix");
            }

            return targetPrefix;
        }

        public bool HasRecordSelector()
        {
            return RecordSelector != null;
        }

        public bool HasViewReference()
        {
            return viewReferenceModel != null;
        }

        public override string ParentLink()
        {
            var linkInfoArea = base.ParentLink();
            if(string.IsNullOrWhiteSpace(linkInfoArea))
            {
                linkInfoArea = RecordSelector?.LinkRecord;
            }
            return linkInfoArea;
        }

        public int TargetLinkId()
        {
            int targetLinkId = -1;

            if (HasRecordSelector())
            {
                if (int.TryParse(RecordSelector.TargetLinkId, out int linkId))
                {
                    targetLinkId = linkId;
                }
            }

            if (targetLinkId < 0 && HasViewReference())
            {
                if(int.TryParse(viewReferenceModel.GetArgumentValue("TargetLinkId"), out int linkId))
                {
                    targetLinkId = linkId;
                }
            }

            return targetLinkId;
        }

        public string TargetLinkInfoAreaId()
        {
            string targetPrefix = string.Empty;

            if (HasRecordSelector())
            {
                targetPrefix = RecordSelector.TargetLinkInfoAreaId;
            }

            if (string.IsNullOrEmpty(targetPrefix) && HasViewReference())
            {
                targetPrefix = viewReferenceModel.GetArgumentValue("TargetLinkInfoAreaId");
            }

            return targetPrefix;
        }
    }
}
