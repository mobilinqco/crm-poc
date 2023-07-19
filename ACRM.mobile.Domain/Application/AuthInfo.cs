using System;
using System.Collections.Generic;
using System.Net;

namespace ACRM.mobile.Domain.Application
{
    public class AuthInfo
    {
        public string URI { get; set; }
        public int Timeout { get; set; }
        public List<Cookie> Cookies { get; set; }
    }
}
