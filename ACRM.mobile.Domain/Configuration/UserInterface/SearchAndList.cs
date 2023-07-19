using System;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<SearchAndList>))]
    public class SearchAndList : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(2)]
        public string FieldGroupName { get; set; }
        [JsonArrayIndex(3)]
        public string HeaderGroupName { get; set; }
        [JsonArrayIndex(4)]
        public string MenuLabel { get; set; }
        [JsonArrayIndex(5)]
        public string DefaultAction { get; set; }
        [JsonArrayIndex(6)]
        public string FilterName { get; set; }

        public SearchAndList()
        {
        }
    }
}
