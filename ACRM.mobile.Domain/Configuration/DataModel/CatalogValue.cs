using System;
using System.ComponentModel;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<CatalogValue>))]
    public class CatalogValue
    {
        public int Id { get; set; }

        [JsonArrayIndex(0)]
        public int Code { get; set; }
        [JsonArrayIndex(1)]
        public string Text { get; set; }
        [JsonArrayIndex(2)]
        public string ExtKey { get; set; }
        [JsonArrayIndex(3)]
        public int Tenant { get; set; }

        [JsonArrayIndex(4)]
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int ParentCode { get; set; }

        [JsonArrayIndex(5)]
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int SortInfo { get; set; }
        [JsonArrayIndex(6)]
        [DefaultValue(1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Access { get; set; } // Access == 1 - locked

        public CatalogValue()
        {
        }
    }
}
