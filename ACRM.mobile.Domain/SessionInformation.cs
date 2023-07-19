using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain
{
    [JsonConverter(typeof(JsonArrayOfKeyValueToObjectConverter<SessionAttributes>))]
    public class SessionAttributes
    {
        [JsonProperty(PropertyName = "repName")]
        public string RepName { get; set; }
        [JsonProperty(PropertyName = "repId")]
        public string RepId { get; set; }

        // TODO: check why the fuck this is returned twice in the json
        // authentication response
        //[JsonProperty(PropertyName = "repGroupId")]
        //public string RepGroupId { get; set; }
        [JsonProperty(PropertyName = "rights")]
        public string Rights { get; set; }
        [JsonProperty(PropertyName = "roles")]
        public string Roles { get; set; }
        [JsonProperty(PropertyName = "configurationnamenice")]
        public string ConfigurationNameNice { get; set; }
        [JsonProperty(PropertyName = "configurationname")]
        public string ConfigurationName { get; set; }
        [JsonProperty(PropertyName = "configurationid")]
        public string ConfigurationId { get; set; }
        [JsonProperty(PropertyName = "webversion")]
        public string WebVersion { get; set; }
        [JsonProperty(PropertyName = "repDeputyId")]
        public string RepDeputyId { get; set; }
        [JsonProperty(PropertyName = "repSuperiorId")]
        public string RepSuperiorId { get; set; }
        [JsonProperty(PropertyName = "tenantNo")]
        public string TenantNo { get; set; }
        [JsonProperty(PropertyName = "tenantName")]
        public string TenantName { get; set; }
        [JsonProperty(PropertyName = "tenantAdd")]
        public string TenantAdd { get; set; }
        [JsonProperty(PropertyName = "datamodelVersion")]
        public string DatamodelVersion { get; set; }
        [JsonProperty(PropertyName = "clientRequestTimeout")]
        public string ClientRequestTimeout { get; set; }
        [JsonProperty(PropertyName = "asyncRequestRetryTime")]
        public string AsyncRequestRetryTime { get; set; }
        [JsonProperty(PropertyName = "asyncRequestWaitTime")]
        public string AsyncRequestWaitTime { get; set; }
        [JsonProperty(PropertyName = "systemOptions")]
        public string SystemOptions { get; set; }

    }

    [JsonConverter(typeof(JsonArrayToObjectConverter<SessionInformation>))]
    public class SessionInformation
    {
        /*
         * {"authenticated":true,
         * "sessioninfo":["1000",
         *          [["repName","Super User"],
         *              ["repId","010000001"],
         *              ["repGroupId",""],
         *              ["rights",""],
         *              ["roles",""],
         *              ["configurationnamenice","OZ_TE_ISI_TABLET(1004)->TE_ISI_TABLET(1003)->update.tablet(15)"],
         *              ["configurationname","OZ_TE_ISI_TABLET"],
         *              ["configurationid","1004"],
         *              ["webversion","9.4.20.915"],
         *              
         *              ["repDeputyId",""],
         *              ["repSuperiorId",""],
         *              ["repGroupId",""],
         *              ["tenantNo","0"],
         *              ["tenantName",""],
         *              ["tenantAdd",""],
         *              ["datamodelVersion","1.0"],
         *              ["clientRequestTimeout",""],
         *              ["asyncRequestRetryTime",""],
         *              ["asyncRequestWaitTime",""],
         *              ["systemOptions",""]
         *           ]
         *  ]}
         */
        [JsonArrayIndex(0)]
        public string ServerUserDefaultLanguage { get; set; }
        [JsonArrayIndex(1)]
        public SessionAttributes Attributes { get; set; }


        public SessionInformation()
        {
        }

        public string RepIdStr()
        {
            if (Attributes != null && !string.IsNullOrEmpty(Attributes.RepId))
            {
                return Attributes.RepId;
            }

            return "0";
        }

        public int TenantNo()
        {
            if(Attributes != null && !string.IsNullOrEmpty(Attributes.TenantNo))
            {
                if (int.TryParse(Attributes.TenantNo, out int result))
                {
                    return result;
                }
            }

            return 0;
        }

        public List<int> AllUserTenants()
        {
            List<int> allUserTenants = new List<int>();
            if(TenantNo() > 0)
            {
                allUserTenants.Add(TenantNo());
            }
            if(Attributes != null && !string.IsNullOrEmpty(Attributes.TenantAdd))
            {
                foreach (var tenantString in Attributes.TenantAdd.Split(','))
                {
                    if (int.TryParse(tenantString, out int result))
                    {
                        allUserTenants.Add(result);
                    }
                }
            }
            return allUserTenants;
        }

        //public SessionInformation(List<object> sessionInfo)
        //{
        //    _serverUserDefaultLanguage = sessionInfo[0] as string;
        //    List<object> sessionInfoAttributes = sessionInfo[1] as List<object>;
        //    if (sessionInfoAttributes != null)
        //    {
        //        _attributes = new Dictionary<string, object>();

        //        foreach (List<object> attribute in sessionInfoAttributes)
        //        {
        //            _attributes[attribute[0] as string] = attribute[1];
        //        }
        //    }

        //}
    }
}
