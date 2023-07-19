using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<WebConfigLayoutField>))]
    public class WebConfigLayoutField
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string ValueName { get; set; }
        [JsonArrayIndex(1)]
        public string Label { get; set; }
        [JsonArrayIndex(2)]
        public string FieldType { get; set; }
        [JsonArrayIndex(3)]
        public List<WebConfigOption> options { get; set; }

        public WebConfigLayoutField()
        {
        }
    }
}
