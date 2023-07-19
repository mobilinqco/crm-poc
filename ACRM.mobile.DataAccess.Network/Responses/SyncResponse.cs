using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.Configuration.DataModel;
using Newtonsoft.Json;

namespace ACRM.mobile.Network.Responses
{
    public class ConfigurationResponse
    {
        [JsonProperty(PropertyName = "Analysis")]
        public List<Analysis> Analysis { get; set; }
        [JsonProperty(PropertyName = "AnalysisCategory")]
        public List<AnalysisCategory> AnalysisCategory { get; set; }

        [JsonProperty(PropertyName = "Button")]
        public List<Button> Buttons { get; set; }
        [JsonProperty(PropertyName = "DataSets")]
        public List<DataSet> DataSets { get; set; }
        [JsonProperty(PropertyName = "Details")]
        public List<Expand> Details { get; set; }
        [JsonProperty(PropertyName = "FieldControl")]
        public List<FieldControl> FieldControls { get; set; }

        [JsonProperty(PropertyName = "Filter")]
        public List<Filter> Filters { get; set; }
        [JsonProperty(PropertyName = "Form")]
        public List<Form> Forms { get; set; }

        [JsonProperty(PropertyName = "Header")]
        public List<Header> Headers { get; set; }
        [JsonProperty(PropertyName = "Image")]
        public List<ConfigResource> Images { get; set; }

        [JsonProperty(PropertyName = "InfoAreas")]
        public List<InfoArea> InfoAreas { get; set; }
        [JsonProperty(PropertyName = "Menu")]
        public List<Menu> Menus { get; set; }
        [JsonProperty(PropertyName = "Query")]
        public List<Query> Queries { get; set; }
        [JsonProperty(PropertyName = "QuickSearch")]
        public List<QuickSearch> QuickSearch { get; set; }
        [JsonProperty(PropertyName = "Search")]
        public List<SearchAndList> Searches { get; set; }
        [JsonProperty(PropertyName = "TableCaptions")]
        public List<TableCaption> TableCaptions { get; set; }
        [JsonProperty(PropertyName = "Textgroups")]
        public List<Textgroup> Textgroups { get; set; }
        [JsonProperty(PropertyName = "Timeline")]
        public List<Timeline> Timelines { get; set; }
        [JsonProperty(PropertyName = "TreeView")]
        public List<TreeView> TreeViews { get; set; }
        [JsonProperty(PropertyName = "WebConfigLayout")]
        public List<WebConfigLayout> WebConfigLayouts { get; set; }
        [JsonProperty(PropertyName = "WebConfigValue")]
        public List<WebConfigValue> WebConfigValues { get; set; }
    }

    public class DataModelResponse
    {
        [JsonProperty(PropertyName = "tables")]
        public List<TableInfo> TableInfo { get; set; }
    }

    public class SyncResponse
    {
        [JsonProperty(PropertyName = "configuration")]
        public ConfigurationResponse Configuration { get; set; }

        [JsonProperty(PropertyName = "dataModel")]
        public DataModelResponse DataModelResponse { get; set; }

        [JsonProperty(PropertyName = "fixedCatalogs")]
        public List<Catalog> FixCatalogs { get; set; }

        [JsonProperty(PropertyName = "variableCatalogs")]
        public List<Catalog> VariableCatalogs { get; set; }

        [JsonProperty(PropertyName = "licenseInfo")]
        public LicenseInfoResponse LicenseInfo { get; set; }

    }
}
