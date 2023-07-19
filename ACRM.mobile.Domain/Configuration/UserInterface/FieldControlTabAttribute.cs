using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FieldControlTabAttribute>))]
    public class FieldControlTabAttribute
    { 
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Key;
        [JsonArrayIndex(1)]
        public string Value;
        [JsonArrayIndex(2)]
        public int EditMode;
        [JsonArrayIndex(3)]
        public string ValueType;

        public FieldControlTabAttribute()
        {
        }
    }
}
