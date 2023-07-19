using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Analysis>))]
    public class Analysis : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string QueryName { get; set; }
        [JsonArrayIndex(2)]
        public List<AnalysisTable> Tables { get; set; }
        [JsonArrayIndex(3)]
        public List<AnalysisResultColumn> ResultColumns { get; set; }
        [JsonArrayIndex(4)]
        public List<AnalysisValue> Values { get; set; }
        [JsonArrayIndex(5)]
        public int MaxBars { get; set; }
        [JsonArrayIndex(6)]
        public int Options { get; set; }
        [JsonArrayIndex(7)]
        public int Flags { get; set; }
        //@property(nonatomic, readonly) BOOL noSumLine

        public Analysis()
        {
        }
    }
}
