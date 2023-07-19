using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<WebConfigLayoutTab>))]
    public class WebConfigLayoutTab
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Label { get; set; }
        [JsonArrayIndex(1)]
        public List<WebConfigLayoutField> Fields { get; set; }

        public WebConfigLayoutTab()
        {
        }
    }
}
