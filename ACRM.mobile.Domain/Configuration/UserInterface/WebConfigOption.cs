using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<WebConfigOption>))]
    public class WebConfigOption
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Value { get; set; }
        [JsonArrayIndex(1)]
        public string Label { get; set; }

        public WebConfigOption()
        {
        }
    }
}
