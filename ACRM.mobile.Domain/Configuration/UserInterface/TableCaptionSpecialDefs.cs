using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<TableCaptionSpecialDefs>))]
    public class TableCaptionSpecialDefs
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string EmptyFields { get; set; }
        [JsonArrayIndex(1)]
        public string FormatString { get; set; }

        public TableCaptionSpecialDefs()
        {
        }
    }
}
