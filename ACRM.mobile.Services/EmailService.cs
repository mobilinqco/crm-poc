using ACRM.mobile.Domain.EmailGenerator;
using ACRM.mobile.Domain.EmailGenerator.Interfaces;
using ACRM.mobile.Services.Contracts;
using System.Threading.Tasks;

namespace ACRM.mobile.Services
{
    public class EmailService : IEmailService
    {
        private readonly IMessageBuilder _messageBuilder;

        public EmailService(IMessageBuilder messageBuilder)
        {
            _messageBuilder = messageBuilder;
        }

        public async Task SendEmailAsync(string email, string bodyContent)
        {
            string emailSubject = "Logs data";
            var emailConfiguration = GetEmailConfiguration();

            await _messageBuilder.AddReceiver(email)
                      .AddSubject(emailSubject)
                      .AddBody(bodyContent)
                      .BuildAndSendAsync(emailConfiguration);
        }

        private EmailConfiguration GetEmailConfiguration()
        {
            return new EmailConfiguration(); //add settings when we'll have all the data
        }
    }
}
