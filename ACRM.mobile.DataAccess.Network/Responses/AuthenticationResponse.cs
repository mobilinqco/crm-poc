using System;
using System.Collections.Generic;
using System.Net;
using ACRM.mobile.DataAccess.Network.Responses;
using ACRM.mobile.Domain;
using Newtonsoft.Json;

namespace ACRM.mobile.Network.Responses
{
    public class AuthenticationResponse
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

        [JsonIgnore]
        public List<Cookie> Cookies { get; set; }
        [JsonIgnore]
        public string RedirectionUrl { get; set; }

        [JsonProperty(PropertyName = "passwordChanged")]
        public bool IsPasswordChanged { get; set; }
        [JsonProperty(PropertyName = "authenticated")]
        public bool IsAuthenticated { get; set; }
        [JsonProperty(PropertyName = "sessioninfo")]
        public SessionInformation SessionInformation { get; set; }
        [JsonProperty(PropertyName = "serverinfo")]
        public ServerInformation ServerInformation { get; set; }

        public AuthenticationResponse()
        {
        }

        public AuthenticationResponse(SessionInformation session)
        {
            IsAuthenticated = true;
            SessionInformation = session;
        }
    }
}
