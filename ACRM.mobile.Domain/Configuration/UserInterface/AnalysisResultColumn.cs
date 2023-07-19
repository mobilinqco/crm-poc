using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<AnalysisResultColumn>))]
    public class AnalysisResultColumn {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int FieldTableId { get; set; }
        [JsonArrayIndex(1)]
        public int FieldId { get; set; }
        [JsonArrayIndex(2)]
        public string AggregationType { get; set; }
        [JsonArrayIndex(3)]
        public string CategoryName { get; set; }
        [JsonArrayIndex(4)]
        public string ValueName { get; set; }

        public AnalysisResultColumn()
        {
        }
    }
}
