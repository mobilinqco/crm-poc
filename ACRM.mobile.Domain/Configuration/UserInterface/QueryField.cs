using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<QueryField>))]
    public class QueryField
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int FieldIndex { get; set; }
        [JsonArrayIndex(1)]
        public string TableAlias { get; set; }
        [JsonArrayIndex(2)]
        public int Flags { get; set; }
        [JsonArrayIndex(3)]
        public int SortFlags { get; set; }

        public QueryTable QueryTable { get; set; }

        public QueryField()
        {
        }
    }
}
