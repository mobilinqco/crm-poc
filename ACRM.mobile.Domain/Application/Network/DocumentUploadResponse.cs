using System;
using System.Collections;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Application.Network
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<DocumentUploadResponse>))]
    public class DocumentUploadResponse 
    {
        [JsonArrayIndex(0)]
        public string D1RecordId { get; set; }
        [JsonArrayIndex(1)]
        public string D3RecordId { get; set; }
        [JsonArrayIndex(2)]
        public List<DataSetData> DataSets { get; set; }
    }
}

