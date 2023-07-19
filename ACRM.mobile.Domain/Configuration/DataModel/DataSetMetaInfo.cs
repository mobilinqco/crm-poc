using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<DataSetMetaInfo>))]
    public class DataSetMetaInfo
    {
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public List<int> FieldIds { get; set; }
        [JsonArrayIndex(2)]
        public List<string> LinkIds { get; set; }

        public DataSetMetaInfo()
        {
        }
    }
}
