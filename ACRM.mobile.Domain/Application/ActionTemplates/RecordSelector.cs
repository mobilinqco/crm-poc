using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Application.ActionTemplates
{
    public class RecordSelector
    {
        [JsonProperty(PropertyName = "Type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "LinkRecord")]
        public string LinkRecord { get; set; }
        [JsonProperty(PropertyName = "LinkId")]
        public string LinkId { get; set; }
        [JsonProperty(PropertyName = "ContextMenu")]
        public string ContextMenu { get; set; }
        [JsonProperty(PropertyName = "DisableLinkOption")]
        public bool DisableLinkOption { get; set; }
        [JsonProperty(PropertyName = "HideStandardFilter")]
        public bool HideStandardFilter { get; set; }
        [JsonProperty(PropertyName = "TargetPrefix")]
        public string TargetPrefix { get; set; }
        [JsonProperty(PropertyName = "Clear")]
        public List<string> Clear { get; set; }

        [JsonProperty(PropertyName = "FixedValues")]
        public Dictionary<string, string> FixedValues { get; set; }
        [JsonProperty(PropertyName = "FormattedParameters")]
        public Dictionary<string, string> FormattedParameters { get; set; }


        [JsonProperty(PropertyName = "IgnoreFieldInfo")]
        public bool IgnoreFieldInfo { get; set; }

        [JsonProperty(PropertyName = "TargetLinkInfoAreaId")]
        public string TargetLinkInfoAreaId { get; set; }
        [JsonProperty(PropertyName = "TargetLinkId")]
        public string TargetLinkId { get; set; }

        [JsonProperty(PropertyName = "ListConfigs")]
        public List<string> ListConfigs { get; set; }
        [JsonProperty(PropertyName = "DescriptionFormat")]
        public string DescriptionFormat { get; set; }
        [JsonProperty(PropertyName = "DetailsFormat")]
        public string DetailsFormat { get; set; }
        [JsonProperty(PropertyName = "SearchAndListName")]
        public string SearchAndListName { get; set; }
        [JsonProperty(PropertyName = "TemplateFilterName")]
        public string TemplateFilterName { get; set; }

        public RecordSelector()
        {
        }

        public bool ShouldUserRecordSelectorView()
        {
            if (Type != null && Type == "Record")
            {
                return true;
            }

            return false;
        }
    }
}
