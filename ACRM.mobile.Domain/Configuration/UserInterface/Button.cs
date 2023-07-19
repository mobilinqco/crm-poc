using System;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Button>))]
    public class Button : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string Label { get; set; }
        [JsonArrayIndex(2)]
        public string ImageName { get; set; }
        [JsonArrayIndex(3)]
        public ViewReference ViewReference { get; set; }
        [JsonArrayIndex(4)]
        public int Flags { get; set; }

        public Button()
        {
        }
    }
}
