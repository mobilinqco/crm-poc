using System.Threading.Tasks;

namespace ACRM.mobile.Services.Contracts
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string bodyContent);
    }
}
