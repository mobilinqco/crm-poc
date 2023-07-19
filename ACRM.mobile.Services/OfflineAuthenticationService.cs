using System;
using System.Text;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess;
using ACRM.mobile.DataAccess.Network;
using ACRM.mobile.Domain;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Network.Responses;
using ACRM.mobile.Services.Contracts;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace ACRM.mobile.Services
{
    public class OfflineAuthenticationService : IOfflineAuthenticationService
    {
        private ILocalFileStorageContext _localFileStorageContext;
        private readonly string _offlineConfigFileName = "OfflineConfig.json";
        private IConfigurationService _configurationService;

        private byte[] GenerateSalt256() => new SecureRandom().GenerateSeed(32);
        private byte[] Salt { get; set; }

        public OfflineAuthenticationService(ILocalFileStorageContext localFileStorageContext)
        {
            _localFileStorageContext = localFileStorageContext;
            Salt = GenerateSalt256();
        }

        public async Task<AuthenticationResponse> Authenticate(CrmInstance crmInstance, string userName, string password)
        {
            User offlineUser = _localFileStorageContext.GetContent<User>(_offlineConfigFileName);
            if(offlineUser == null || offlineUser.SessionInformation == null)
            {
                throw new AuthenticationException(AuthenticationException.AuthExceptionType.OfflineNoData, "Nothing stored locally");
            }

            Salt = offlineUser.Salt;

            var encPass = EncodeForOffline(offlineUser.CaseInsensitive ? password.ToLower() : password);
            if(!offlineUser.Username.Equals(userName) || !encPass.Equals(offlineUser.Password))
            {
                throw new AuthenticationException(AuthenticationException.AuthExceptionType.OfflineWrongCredentials, "Wrong credentials");
            }

            return new AuthenticationResponse(offlineUser.SessionInformation);
        }

        public bool IsOfflinePossible()
        {
            User offlineUser = _localFileStorageContext.GetContent<User>(_offlineConfigFileName);
            if (offlineUser == null || offlineUser.SessionInformation == null)
            {
                return false;
            }

            return true;
        }

        public void StoreContextForOfflineAuthentication(ISessionContext sessionContext, bool caseInsensitive)
        {
            User authenticatedUser = new User(sessionContext.User.Username,
                EncodeForOffline(caseInsensitive ? sessionContext.User.Password.ToLower() : sessionContext.User.Password),
                Salt, caseInsensitive,
                sessionContext.User.SessionInformation);

            _localFileStorageContext.SaveContent<User>(authenticatedUser, _offlineConfigFileName);
        }

        public string EncodeForOffline(string plainText)
        {
            var data = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
            var kdf = new Pkcs5S2ParametersGenerator();
            kdf.Init(data, Salt, 10000);

            var hash = ((KeyParameter)kdf.GenerateDerivedMacParameters(8 * Salt.Length)).GetKey();

            return Encoding.UTF8.GetString(hash, 0, hash.Length);
        }
    }
}
