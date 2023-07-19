using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Application.Network
{
    public class DataSaveResponse
    {
        [JsonProperty(PropertyName = "RequestControlKey")]
        public string RequestControlKey { get; set; }

        [JsonProperty(PropertyName = "Record0000")]
        public DataSaveRecord Record { get; set; }

        [JsonProperty(PropertyName = "StatusInfo")]
        public List<object> StatusInfo { get; set; }


        public bool IsError()
        {
            if (StatusInfo != null && StatusInfo.Count > 0 && StatusInfo[0] is string)
            {
                var info = StatusInfo[0] as string;
                if(!string.IsNullOrWhiteSpace(info) && "ServerError".Equals(info))
                {
                    return true;
                }
            }

            return false;
        }

        public DataSaveResponse()
        {
        }
    }

    [JsonConverter(typeof(JsonArrayToObjectConverter<DataSaveRecord>))]
    public class DataSaveRecord
    {
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public string RecordId { get; set; }
        [JsonArrayIndex(2)]
        public string SomeField { get; set; }
        [JsonArrayIndex(3)]
        public List<DataSetData> DataSets { get; set; }
        [JsonArrayIndex(4)]
        public FieldsDenied Denied { get; set; }
    }

    public class FieldsDenied
    {
        [JsonProperty(PropertyName = "fieldsDeniedWithAccessRights")]
        public List<string> Fields { get; set; }
    }
}