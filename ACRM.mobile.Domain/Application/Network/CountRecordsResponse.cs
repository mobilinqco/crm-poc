using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.Network
{
	public class CountRecordsResponse
	{
        [JsonProperty(PropertyName = "CountRows")]
        public List<int> CountRows { get; set; }

        public CountRecordsResponse()
		{
		}
	}
}

