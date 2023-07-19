using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Application
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<TableCaptionField>))]
    public class TableCaptionField
    {
        [JsonArrayIndex(0)]
        public int Position { get; set; }
        [JsonArrayIndex(1)]
        public int FieldId { get; set; }
        [JsonArrayIndex(2)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(3)]
        public int LinkId { get; set; }

        public TableCaptionField()
        {
        }
    }
}
