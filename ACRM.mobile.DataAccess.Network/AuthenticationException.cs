using System;
namespace ACRM.mobile.DataAccess.Network
{
    public class AuthenticationException : Exception
    {
        public enum AuthExceptionType { Network, OfflineNoData, OfflineWrongCredentials }

        public string Content { get; private set; }
        public AuthExceptionType Type { get; private set; }

        public AuthenticationException(AuthExceptionType type, string content)
        {
            Type = type;
            Content = content;
        }
    }
}
