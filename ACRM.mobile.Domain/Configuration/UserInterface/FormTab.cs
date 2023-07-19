using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FormTab>))]
    public class FormTab
    {
        static int DesignerOrderId = 0;
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Label { get; set; }
        [JsonArrayIndex(1)]
        public List<FormRow> Rows { get; set; }
        [JsonArrayIndex(2)]
        public string Attributes { get; set; }
        [JsonIgnore]
        public int OrderId { get; set; }

        public FormTab()
        {
            OrderId = DesignerOrderId++;
        }
    }
}
