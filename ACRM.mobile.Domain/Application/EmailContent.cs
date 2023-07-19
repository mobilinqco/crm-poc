using System;
namespace ACRM.mobile.Domain.Application
{
    public class EmailContent
    {
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
