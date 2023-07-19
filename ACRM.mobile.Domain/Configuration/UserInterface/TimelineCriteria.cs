using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<TimelineCriteria>))]
    public class TimelineCriteria
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int FieldId { get; set; }
        [JsonArrayIndex(1)]
        public string CompareOperator { get; set; }
        [JsonArrayIndex(2)]
        public string CompareValue { get; set; }
        [JsonArrayIndex(3)]
        public string CompareValueTo { get; set; }
        [JsonArrayIndex(4)]
        public string Setting1 { get; set; }
        [JsonArrayIndex(5)]
        public string Setting2 { get; set; }

        public TimelineCriteria()
        {
        }
    }
}
