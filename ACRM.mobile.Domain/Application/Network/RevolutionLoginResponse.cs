using System;
using System.Collections.Generic;
using System.Net;

namespace ACRM.mobile.Domain.Application.Network
{
    public class RevolutionLoginResponse
    {
        public List<Cookie> Cookies { get; set; }
        public string RedirectionUrl { get; set; }
    }
}
