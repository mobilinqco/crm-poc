using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.OfflineSync;

namespace ACRM.mobile.Domain.Application
{
    public class PanelData
    {
        public string RecordId;
        public string RecordInfoArea;
        public string Label;
        public string PanelTypeKey;
        public PanelType Type;
        public UserAction action;
        public Dictionary<string, string> CopyValues;
        public OfflineRecordLink ParentLink;
        public bool FourceUpdate = false;

        public string FormatedRecordId
        {
            get
            {
                if(string.IsNullOrWhiteSpace(RecordId))
                {
                    return null;
                }

                return RecordId.FormatedRecordId(RecordInfoArea);
            }
        }

        public List<ListDisplayField> Fields { get; set; }

        public int CountVisibleFields()
        {
            return Fields.Where(f => !f.Config.PresentationFieldAttributes.Hide).Count();
        }

        public PanelData()
        {
        }

        public bool HasData()
        {
            return Fields != null && Fields.Any(f => !string.IsNullOrWhiteSpace(f.Data.StringData) && !f.Data.StringData.Equals("0"));
        }

        public PanelData(PanelData input)
        {
            if(input != null)
            {
                this.RecordId = input.RecordId;
                this.RecordInfoArea = input.RecordInfoArea;
                this.Label = input.Label;
                this.PanelTypeKey = input.PanelTypeKey;
                this.Type = input.Type;
                this.action = input.action;
                this.CopyValues = input.CopyValues;
                this.ParentLink = input.ParentLink;
            }

        }

        public string GetValue(string function)
        {
            string result = string.Empty;
            if (Fields != null)
            {
                var fieldIndex = Fields.FindIndex(f => f.Config.FieldConfig.Function.Equals(function, StringComparison.InvariantCultureIgnoreCase));
                if (fieldIndex >= 0)
                {
                    result = Fields[fieldIndex].EditData.StringValue;
                }
            }

            return result;
        }
    }
}
