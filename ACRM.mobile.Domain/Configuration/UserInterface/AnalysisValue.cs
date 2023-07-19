using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<AnalysisValue>))]
    public class AnalysisValue
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int ValueNumber { get; set; }
        [JsonArrayIndex(1)]
        public string Name { get; set; }
        [JsonArrayIndex(2)]
        public string FixedType { get; set; }
        [JsonArrayIndex(3)]
        public string Label { get; set; }
        [JsonArrayIndex(4)]
        public string Parameter { get; set; }
        [JsonArrayIndex(5)]
        public string OptionString { get; set; }

        public AnalysisValue()
        {
        }
    }
}
