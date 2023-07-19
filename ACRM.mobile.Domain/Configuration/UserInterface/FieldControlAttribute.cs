using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FieldControlAttribute>))]
    public class FieldControlAttribute
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Key { get; set; }
        [JsonArrayIndex(1)]
        public string Value { get; set; }

        public FieldControlAttribute()
        {
        }
    }
}
