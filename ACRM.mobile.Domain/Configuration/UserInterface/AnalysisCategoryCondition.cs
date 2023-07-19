using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<AnalysisCategoryCondition>))]
    public class AnalysisCategoryCondition
    {
        public int Id { get; set; }
        [JsonArrayIndex(1)]
        public string Type { get; set; }
        [JsonArrayIndex(2)]
        public string Value { get; set; }
        [JsonArrayIndex(3)]
        public string ValueTo { get; set; }

        public AnalysisCategoryCondition()
        {
        }
    }
}
