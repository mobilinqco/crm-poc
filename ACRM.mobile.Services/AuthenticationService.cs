using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Network;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Network.Responses;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services
{
    public class AuthenticationService: IAuthenticationService
    {
        private readonly INetworkRepository _networkRepository;


        public AuthenticationService(INetworkRepository networkRepository)
        {
            _networkRepository = networkRepository;
        }

        public async Task<AuthenticationResponse> Authenticate(CrmInstance crmInstance, string userName, string password, string languageCode = null, PasswordChangeStatus passwordChangeStatus = null)
        {
            if (crmInstance.IsRevolutionCrmInstance())
            {
                return await AuthenticateRevolution(crmInstance, userName, password, languageCode);
            }

            return await AuthenticateWeb(crmInstance, userName, password, languageCode, passwordChangeStatus);
        }

        private async Task<AuthenticationResponse> AuthenticateWeb(CrmInstance crmInstance, string userName, string password, string languageCode = null, PasswordChangeStatus passwordChangeStatus = null)
        {
            UriBuilder uriBuilder = new UriBuilder(crmInstance.AuthenticationUrl());
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            _networkRepository.SetLoginAuthContext(crmInstance, userName, password);
            query["Service"] = "Authenticate";
            query["ClientVersion"] = "3.0";
            query["ServerInfo"] = "true";
            query["AppInfo"] = "crmclient";

            string authType = crmInstance.AuthenticationType.ToLower();
            if (authType.Equals("username") || authType.Equals("usernamecredentials"))
            {
                query["Username"] = userName;
                query["Password"] = password;
            }

            if (languageCode != null)
            {
                query["Language"] = languageCode;
            }

            if (passwordChangeStatus != null)
            {
                query["NewPassword"] = passwordChangeStatus.NewPassword;
                query["NewPasswordEmpty"] = passwordChangeStatus.EmptyPassword ? "true" : "";
            }
        
            uriBuilder.Query = query.ToString();

            return await _networkRepository.GetAsync<AuthenticationResponse>(uriBuilder.ToString(), 10000, userCancellationToken: null, cookies: null, WithAuthRetry: false);
        }

        private async Task<AuthenticationResponse> AuthenticateRevolution(CrmInstance crmInstance, string userName, string password, string languageCode = null)
        {
            UriBuilder uriBuilder = new UriBuilder(crmInstance.AuthenticationUrl());

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["Service"] = "Authenticate";
            query["Username"] = userName;
            query["Password"] = password;
            query["ClientVersion"] = "3.0";

            RevolutionLoginResponse networkResponse = await _networkRepository.PostLoginAsync(uriBuilder.ToString(), query.ToString(), 10000, null);

            crmInstance.RevolutionRuntimeUrl = networkResponse.RedirectionUrl;
            uriBuilder = new UriBuilder(crmInstance.UrlPath());
            query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["Service"] = "Authenticate";
            query["Method"] = "RASLogon";
            query["ClientVersion"] = "3.0";
            query["ServerInfo"] = "true";
            query["AppInfo"] = "crmclient";

            if (languageCode != null)
            {
                query["Language"] = languageCode;
            }
            uriBuilder.Query = query.ToString();

            AuthenticationResponse authenticationResponse = await _networkRepository.GetAsync<AuthenticationResponse>(uriBuilder.ToString(), 10000, null, networkResponse.Cookies, false);
            authenticationResponse.RedirectionUrl = networkResponse.RedirectionUrl;

            return authenticationResponse;
        }
    }
}
