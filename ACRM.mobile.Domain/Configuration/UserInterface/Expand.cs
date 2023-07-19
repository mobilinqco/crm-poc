using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Expand>))]
    public class Expand : BaseConfigUnit
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
        public string TableCaptionName { get; set; }
        [JsonArrayIndex(6)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string AltenrateExpands { get; set; }
        [JsonArrayIndex(7)]
        public string ColorKey { get; set; }
        [JsonArrayIndex(8)]
        public string ImageName { get; set; }

        public Expand()
        {
        }

        public string GetColorString()
        {
            return string.IsNullOrWhiteSpace(ColorKey) ? "#ffffffff" : ColorKey;
        }
    }
}
