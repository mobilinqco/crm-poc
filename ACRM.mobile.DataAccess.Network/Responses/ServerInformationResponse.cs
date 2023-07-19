using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.DataAccess.Network.Responses
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<ServerInformation>))]
    public class ServerInformation
    {

        [JsonArrayIndex(2)]
        public List<ServerLanguage> ServerLanguages { get; set; }

        public ServerInformation()
        {
            
        }
    }
}
