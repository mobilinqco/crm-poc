using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.JsonUtils;

namespace ACRM.mobile.Domain.Application.Network
{
	public class ChangeConfigurationResponse
	{
        [JsonArrayIndex(0)]
        public ConfigSyncResponse SyncResponse { get; set; }
        [JsonProperty(PropertyName = "Success")]
        public string Status { get; set; }
        public ChangeConfigurationResponse()
		{
		}
	}

    public class ConfigSyncResponse
    {
        [JsonProperty(PropertyName = "WebConfigValue")]
        public List<WebConfigValue> WebConfigValues { get; set; }
        [JsonProperty(PropertyName = "Success")]
        public string Status { get; set; }

    }
}

