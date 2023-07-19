using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Application.Network
{
    public class RecordDataResponse
    {
        [JsonProperty(PropertyName = "resultRows")]
        public List<RecordDataResponseRow> Rows { get; set; }
        
        public RecordDataResponse()
        {
        }
    }

    [JsonConverter(typeof(JsonArrayToObjectConverter<RecordDataResponseRow>))]
    public class RecordDataResponseRow
    {
        [JsonArrayIndex(0)]
        public List<string?> RecordIds { get; set; }
        [JsonArrayIndex(1)]
        public List<List<string?>?> Values { get; set; }

        public RecordDataResponseRow()
        {
        }
    }
}
