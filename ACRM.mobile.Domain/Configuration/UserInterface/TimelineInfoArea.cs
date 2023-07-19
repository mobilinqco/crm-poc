using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<TimelineInfoArea>))]
    public class TimelineInfoArea
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public int LinkId { get; set; }
        [JsonArrayIndex(2)]
        public string ConfigName { get; set; }
        [JsonArrayIndex(3)]
        public string DateFieldIndex { get; set; }
        [JsonArrayIndex(4)]
        public string ColorString { get; set; }
        [JsonArrayIndex(5)]
        public string Color2String { get; set; }
        [JsonArrayIndex(8)]
        public string FilterName { get; set; }
        [JsonArrayIndex(9)]
        public string Text { get; set; }

        [JsonArrayIndex(10)]
        public List<TimelineCriteria> ColorCriteria { get; set; }
        
        

        public TimelineInfoArea()
        {
        }
    }
}