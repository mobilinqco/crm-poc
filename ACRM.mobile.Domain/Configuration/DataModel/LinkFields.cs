using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<LinkFields>))]
    public class LinkFields
    {
        public int Id { get; set; }

        [JsonArrayIndex(0)]
        public int SourceFieldId { get; set; }
        [JsonArrayIndex(1)]
        public int DestFieldId { get; set; }
        [JsonArrayIndex(2)]
        public string SourceValue { get; set; }
        [JsonArrayIndex(3)]
        public string DestValue { get; set; }

        public LinkFields()
        {
        }

        public string GetDestField()
        {
            return "F" + DestFieldId;
        }

        public string GetSoruceField()
        {
            return "F" + SourceFieldId;
        }
    }
}
