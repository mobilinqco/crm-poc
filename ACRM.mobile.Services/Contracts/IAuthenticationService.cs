using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Network.Responses;

namespace ACRM.mobile.Services.Contracts
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> Authenticate(CrmInstance crmInstance, string userName, string password, string languageCode = null, PasswordChangeStatus passwordChangeStatus = null);
    }
}
