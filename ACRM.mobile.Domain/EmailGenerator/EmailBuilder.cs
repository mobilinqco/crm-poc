using ACRM.mobile.Domain.EmailGenerator.Interfaces;
using System.Threading.Tasks;

namespace ACRM.mobile.Domain.EmailGenerator
{
    public class EmailBuilder : IMessageBuilder
    {
        private Email _email;

        public EmailBuilder()
        {
            _email = new Email();
        }

        public EmailBuilder AddBody(string body)
        {
            _email.Body = body;
            return this;
        }

        public EmailBuilder AddReceiver(string receiver)
        {
            _email.To = receiver;
            return this;
        }

        public EmailBuilder AddSubject(string subject)
        {
            _email.Subject = subject;
            return this;
        }

        public async Task BuildAndSendAsync(EmailConfiguration emailConfiguration)
        {
            if (_email.To != null && _email.Subject != null && _email.Body != null)
            {
                await EmailSender.SendMailAsync(_email, emailConfiguration);
            }
        }
    }
}
