using System;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<ConfigResource>))]
    public class ConfigResource : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string FileName { get; set; }
        [JsonArrayIndex(2)]
        public string Label { get; set; }
        [JsonArrayIndex(3)]
        public int ConfigId { get; set; }

        public ConfigResource()
        {
        }
    }
}
