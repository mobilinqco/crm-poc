using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<QuickSearch>))]
    public class QuickSearch : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public List<QuickSearchEntry> Entries { get; set; }

        public QuickSearch()
        {
        }
    }
}
