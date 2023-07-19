using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Network.Responses;
using Newtonsoft.Json;

namespace ACRM.mobile.DataAccess.Network.Responses
{
    public class SyncDataSetResponse
    {
        [JsonProperty(PropertyName = "licenseInfo")]
        public LicenseInfoResponse LicenseInfo { get; set; }
        [JsonProperty(PropertyName = "records")]
        public List<DataSetData> DataSets { get; set; }

        public SyncDataSetResponse()
        {
        }
    }
}
