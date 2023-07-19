using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<DataSet>))]
    public class DataSet : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(4)]
        public string Label { get; set; }
        [JsonArrayIndex(2)]
        public List<DataSetFilter> Filters { get; set; }

        public DataSetFilter SyncDocument => Filters.FirstOrDefault(f => f.Type.Equals("Documents"));

        public DataSet()
        {
        }
    }
}
