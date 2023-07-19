using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<QuickSearchEntry>))]
    public class QuickSearchEntry
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public int FieldId { get; set; }
        [JsonArrayIndex(2)]
        public string MenuName { get; set; }
        
        public QuickSearchEntry()
        {
        }
    }
}
