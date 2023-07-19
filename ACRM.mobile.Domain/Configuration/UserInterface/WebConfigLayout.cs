using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<WebConfigLayout>))]
    public class WebConfigLayout
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Label { get; set; }
        [JsonArrayIndex(1)]
        public List<WebConfigLayoutTab> Tabs { get; set; }

        public WebConfigLayout()
        {
        }
    }
}
