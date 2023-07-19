using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<ReferenceArgument>))]
    public class ReferenceArgument
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Name { get; set; }
        [JsonArrayIndex(1)]
        public string Type { get; set; }
        [JsonArrayIndex(2)]
        public string Value { get; set; }

        public ReferenceArgument()
        {
        }
    }
}
