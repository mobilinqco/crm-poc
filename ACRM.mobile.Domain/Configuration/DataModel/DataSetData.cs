using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    public class DataSetData
    {
        [JsonProperty(PropertyName = "datasetName")]
        public string DataSetName { get; set; }
        [JsonProperty(PropertyName = "fullSyncTimestamp")]
        public string FullSyncTimestamp { get; set; }
        [JsonProperty(PropertyName = "infoareaid")]
        public string InfoAreaId { get; set; }
        [JsonProperty(PropertyName = "maxRecords")]
        public int MaxRecords { get; set; }
        [JsonProperty(PropertyName = "metainfo")]
        public List<DataSetMetaInfo> MetaInfos { get; set; }
        [JsonProperty(PropertyName = "rowCount")]
        public int RowCount { get; set; }

        [JsonProperty(PropertyName = "rows")]
        public List<DataSetRecordEnvelope> Rows { get; set; }

        public DataSetData()
        {
        }
    }
}
