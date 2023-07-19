using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Application
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<ExpandRule>))]
    public class ExpandRule
    {
        [JsonArrayIndex(0)]
        public int RuleOrderId { get; set; }
        [JsonArrayIndex(1)]
        public string Operator { get; set; }
        [JsonArrayIndex(2)]
        public int FieldId { get; set; }
        [JsonArrayIndex(3)]
        public string Value { get; set; }
        [JsonArrayIndex(4)]
        public string Action { get; set; }

        public ExpandRule()
        {
        }

        public bool IsAndRule()
        {
            return !string.IsNullOrWhiteSpace(Action) && Action.Equals(".");
        }
    }
}
 