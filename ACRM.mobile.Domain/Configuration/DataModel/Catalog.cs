using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Catalog>))]
    public class Catalog
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int CatNr { get; set; }
        [JsonArrayIndex(1)]
        public List<CatalogValue> CatalogValues { get; set; }
        [JsonArrayIndex(2)]
        public string SyncInfo { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsVariableCatalog { get; set; }

        public Catalog()
        {
        }
    }
}
