using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<DataSetRecordEnvelope>))]
    public class DataSetRecordEnvelope
    {
        [JsonArrayIndex(0)]
        public DataSetRecord DataSetRecord { get; set; }

        public DataSetRecordEnvelope()
        {
        }
    }
}