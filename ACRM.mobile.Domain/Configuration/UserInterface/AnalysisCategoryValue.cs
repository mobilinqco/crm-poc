using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<AnalysisCategoryValue>))]
    public class AnalysisCategoryValue
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int ValueNumber { get; set; }
        [JsonArrayIndex(1)]
        public string RefValue { get; set; }
        [JsonArrayIndex(2)]
        public string SubCategoryName { get; set; }
        [JsonArrayIndex(3)]
        public string Label { get; set; }
        [JsonArrayIndex(4)]
        public List<AnalysisCategoryCondition> Conditions { get; set; }

        public AnalysisCategoryValue()
        {
        }
    }
}
