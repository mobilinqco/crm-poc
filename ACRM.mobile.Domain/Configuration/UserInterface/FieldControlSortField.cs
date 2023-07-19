using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FieldControlSortField>))]
    public class FieldControlSortField
    {
        // We need this because the Entity Framework Core dose not guarantee that the
        // primary key (id) will not be generated in the order we have added the data to the
        // list. And for the FieldControls we need to have a way to preserve the order of the fields
        // from the designer configuration.
        static int DesignerOrderId = 0;

        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int FieldIndex { get; set; }
        [JsonArrayIndex(1)]
        public bool Descending { get; set; }
        [JsonArrayIndex(2)]
        public string InfoAreaId { get; set; }

        [JsonIgnore]
        public int OrderId { get; set; }

        public FieldControlSortField()
        {
            OrderId = DesignerOrderId++;
        }

        public static void ResetDesignerOrderId()
        {
            DesignerOrderId = 0;
        }
    }
}
