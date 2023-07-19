using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<DataSetRecord>))]
    public class DataSetRecord
    {
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public string RecordId { get; set; }
        [JsonArrayIndex(2)]
        public List<string> Values { get; set; }
        [JsonArrayIndex(3)]
        public List<string> Links { get; set; }
        [JsonArrayIndex(4)]
        public int RecordExists { get; set; }

        public DataSetRecord()
        {
        }
    }
}
