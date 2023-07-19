using System;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<WebConfigValue>))]
    public class WebConfigValue : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string Value { get; set; }
        [JsonArrayIndex(2)]
        public int Inherited { get; set; }
        
        public WebConfigValue()
        {
        }
    }
}
