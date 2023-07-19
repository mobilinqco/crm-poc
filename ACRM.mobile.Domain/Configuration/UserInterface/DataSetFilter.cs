using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<DataSetFilter>))]
    public class DataSetFilter
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Type { get; set; }
        [JsonArrayIndex(1)]
        public string Name { get; set; }

        public DataSetFilter()
        {
            
        }
    }
}
