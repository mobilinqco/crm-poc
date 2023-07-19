using System;
using System.Net;
namespace ACRM.mobile.Utils
{
    public class WebContentParams
    {
        public string BaseUrl { get; set; }
        public string HTMLContent { get; set; }
        public bool IsURLSource { get; set; } = false;
        public CookieContainer CookieContainer { get; set; }
        public WebContentParams()
        {
        }
    }
}
