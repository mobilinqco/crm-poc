using ACRM.mobile.Domain.EmailGenerator.Interfaces;

namespace ACRM.mobile.Domain.EmailGenerator
{
    public class Email : IMessage
    {
        public string To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
