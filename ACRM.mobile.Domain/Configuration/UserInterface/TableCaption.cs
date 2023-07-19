using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<TableCaption>))]
    public class TableCaption : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(2)]
        public string PrefixString { get; set; }
        [JsonArrayIndex(3)]
        public string FormatString { get; set; }
        [JsonArrayIndex(4)]
        public string ImageName { get; set; }
        
        [JsonArrayIndex(5)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string Fields { get; set; }
        [JsonArrayIndex(6)]
        public List<TableCaptionSpecialDefs> SpecialDefs { get; set; }

        public TableCaption()
        {
        }
    }
}
