using System;
using System.ComponentModel.DataAnnotations.Schema;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [Serializable]
    [JsonConverter(typeof(JsonArrayToObjectConverter<QueryFilterBase>))]
    public class QueryFilterBase : BaseConfigUnit
    {
        [JsonArrayIndex(3)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string Definition { get; set; }

        [JsonIgnore]
        [NotMapped]
        public QueryTable RootTable { get; set; }

        public QueryFilterBase()
        {
        }
    }
}
