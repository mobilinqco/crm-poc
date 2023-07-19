using System;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Network.Responses;

namespace ACRM.mobile.Services.Contracts
{
    public interface IOfflineAuthenticationService
    {
        bool IsOfflinePossible();
        void StoreContextForOfflineAuthentication(ISessionContext sessionContext, bool caseInsensitive);
        Task<AuthenticationResponse> Authenticate(CrmInstance crmInstance, string userName, string password); 
    }
}
