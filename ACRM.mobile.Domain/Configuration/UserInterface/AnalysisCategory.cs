using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<AnalysisCategory>))]
    public class AnalysisCategory : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public int MultiValue { get; set; }
        [JsonArrayIndex(2)]
        public int OtherMode { get; set; }
        [JsonArrayIndex(3)]
        public string Roll { get; set; }
        [JsonArrayIndex(4)]
        public string Label { get; set; }
        [JsonArrayIndex(5)]
        public string OtherLabel { get; set; }
        [JsonArrayIndex(6)]
        public List<AnalysisCategoryValue> Values { get; set; }

        public string Name => UnitName;

        public AnalysisCategory()
        {
        }
    }
}
