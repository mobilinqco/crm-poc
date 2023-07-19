using System.Threading.Tasks;

namespace ACRM.mobile.Domain.EmailGenerator.Interfaces
{
    public interface IMessageBuilder
    {
        EmailBuilder AddSubject(string subject);

        EmailBuilder AddBody(string body);

        EmailBuilder AddReceiver(string receiver);

        Task BuildAndSendAsync(EmailConfiguration emailConfiguration);
    }
}
