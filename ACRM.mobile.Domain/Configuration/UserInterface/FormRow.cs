using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FormRow>))]
    public class FormRow
    {
        static int DesignerOrderId = 0;
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public List<FormItem> Items { get; set; }
        [JsonIgnore]
        public int OrderId { get; set; }

        public FormRow()
        {
            OrderId = DesignerOrderId++;
        }
    }
}
