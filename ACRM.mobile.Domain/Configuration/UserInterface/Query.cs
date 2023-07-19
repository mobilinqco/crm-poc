using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Query>))]
    public class Query: QueryFilterBase
    {
        [JsonArrayIndex(4)]
        public List<QueryField> QueryFields { get; set; }
        [JsonArrayIndex(5)]
        public List<QueryField> SortFields { get; set; }

        public Query()
        {
        }
    }
}
