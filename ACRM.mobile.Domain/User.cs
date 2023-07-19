using System;
using System.Text;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain
{
    public class User
    {
        private string _username;
        public string Username
        {
            get => _username;
        }

        private string _password;
        public string Password
        {
            get => _password;
        }

        private byte[] _salt;
        public byte[] Salt
        {
            get => _salt;
        }

        private bool _caseInsensitive;
        public bool CaseInsensitive
        {
            get => _caseInsensitive;
        }

        private SessionInformation _sessionInformation;
        public SessionInformation SessionInformation
        {
            get => _sessionInformation;
        }

        [JsonConstructor]
        public User(string username, string password,
            byte[] salt, bool caseInsensitive,
            SessionInformation sessionInformation)
        {
            _username = username;
            _password = password;
            _salt = salt;
            _caseInsensitive = caseInsensitive;
            _sessionInformation = sessionInformation;
        }

        public User(string username, string password,
            SessionInformation sessionInformation)
        {
            _username = username;
            _password = password;
            _salt = null;
            _caseInsensitive = false;
            _sessionInformation = sessionInformation;
        }
    }
}
