using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Timeline>))]
    public class Timeline : BaseConfigUnit
    {
        [JsonArrayIndex(4)]
        public List<TimelineInfoArea> InfoAreas { get; set; }

        public Timeline()
        {
        }
    }
}
