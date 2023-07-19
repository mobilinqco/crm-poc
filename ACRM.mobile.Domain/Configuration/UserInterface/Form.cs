using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Form>))]
    public class Form : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public List<FormTab> Tabs { get; set; }

        public Form()
        {
        }
    }
}
