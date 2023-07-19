using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Header>))]
    public class Header : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(2)]
        public string ImageName { get; set; }
        [JsonArrayIndex(3)]
        public string Label { get; set; }
        [JsonArrayIndex(4)]
        public int Flags;

        [JsonArrayIndex(5)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string ButtonNames { get; set; }
        [JsonArrayIndex(6)]
        public List<HeaderSubView> SubViews { get; set; }


        public Header()
        {
        }
    }
}
