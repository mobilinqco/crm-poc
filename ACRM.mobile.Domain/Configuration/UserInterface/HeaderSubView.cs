using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<HeaderSubView>))]
    public class HeaderSubView
    {
        // We need this because the Entity Framework Core dose not guarantee that the
        // primary key (id) will not be generated in the order we have added the data to the
        // list. And for the FieldControls we need to have a way to preserve the order of the fields
        // from the designer configuration.
        static int DesignerOrderId = 0;

        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Label { get; set; }
        [JsonArrayIndex(1)]
        public ViewReference ViewReference { get; set; }
        [JsonArrayIndex(2)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(3)]
        public int LinkId { get; set; }
        [JsonArrayIndex(4)]
        public string Options { get; set; }

        [JsonIgnore]
        public int OrderId { get; set; }

        public HeaderSubView()
        {
            OrderId = DesignerOrderId++;
        }

        public static void ResetDesignerOrderId()
        {
            DesignerOrderId = 0;
        }
    }
}
