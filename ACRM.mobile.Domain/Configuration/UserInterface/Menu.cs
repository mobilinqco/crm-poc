using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Menu>))]
    public class Menu : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string DisplayName { get; set; }
        [JsonArrayIndex(2)]
        public ViewReference ViewReference { get; set; }
        [JsonArrayIndex(3)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string Items { get; set; }
        [JsonArrayIndex(4)]
        public string ImageName { get; set; }
        
        public Menu()
        {
        }
    }
}
