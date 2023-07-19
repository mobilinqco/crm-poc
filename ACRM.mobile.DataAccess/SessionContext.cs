using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using ACRM.mobile.Domain;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.DataAccess
{
    public class SessionContext: ISessionContext, INotifyPropertyChanged
    {
        private List<Cookie> _sessionCookies;
        public List<Cookie> SessionCookies
        {
            get
            {
                return _sessionCookies;
            }
            set
            {
                _sessionCookies = value;
            }
        }

        public User User { get; set; }

        public TimeZoneInfo ClientTimeZone { get; set; } = null;
        public TimeZoneInfo ServerTimeZone { get; set; } = null;

        private string _appDataPath;
        public string AppLocalsPath
        {
            get
            {
                return _appDataPath;
            }
            set
            {
                _appDataPath = value;
            }
        }

        private CrmInstance _crmInstance;
        public CrmInstance CrmInstance
        {
            get => _crmInstance;
            set
            {
                _crmInstance = value;
                _sessionCookies = null;
                User = null;
                OnPropertyChanged();
            }
        }

        private bool _isInOfflineMode;
        public bool IsInOfflineMode 
        {
            get
            {
                return _isInOfflineMode | _isOfflineModeToggled;
            }
            set
            {
                _isInOfflineMode = value;
            }
        }

        public bool HasNetworkConnectivity { get; set; }

        private bool _isOfflineModeToggled;
        public bool IsOfflineModeToggled 
        { 
            get
            {
                return _isOfflineModeToggled;
            }
            set
            {
                _isOfflineModeToggled = value;
            }
        }

        public bool IsChangePasswordEnabled { get; set; }

        private string _languageCode;
        public string LanguageCode
        {
            get
            {
                return _languageCode;
            }
            set
            {
                _languageCode = value;
            }
        }

        private Dictionary<string, string> _extraParams;
        public Dictionary<string, string> ExtraParams
        {
            get
            {
                return _extraParams;
            }
            set
            {
                _extraParams = value;
            }
        }

        private int _offlineStationNumber;
        public int OfflineStationNumber
        {
            get
            {
                return _offlineStationNumber;
            }
            set
            {
                _offlineStationNumber = value;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public SessionContext(string appDataPath)
        {
            _appDataPath = appDataPath;
            _languageCode = string.Empty;
            _sessionCookies = new List<Cookie>();
            IsInOfflineMode = false;
            IsOfflineModeToggled = false;
            _offlineStationNumber = 0;
        }

        public bool IsAuthenticated()
        {
            if (!IsInOfflineMode)
            {
                return (_sessionCookies != null) && (_sessionCookies.Count > 0);
            }

            if(User == null)
            {
                return false;
            }

            return true;
        }

        public string LocalCrmInstancePath()
        {
            if (_crmInstance != null)
            {
                return Path.Combine(AppLocalsPath, _crmInstance.InstanceFolderPath()); 
            }

            return AppLocalsPath;
        }

        public string ResourcesFolder()
        {
            if (_crmInstance != null)
            {
                return Path.Combine(AppLocalsPath, _crmInstance.InstanceFolderPath(), "resources");
            }

            return Path.Combine(AppLocalsPath, "resources");
        }

        public string DocumentsFolder()
        {
            if (_crmInstance != null)
            {
                return Path.Combine(AppLocalsPath, _crmInstance.InstanceFolderPath(), "docs");
            }

            return Path.Combine(AppLocalsPath, "docs");
        }
        public string ReportFolder()
        {
            if (_crmInstance != null)
            {
                return Path.Combine(AppLocalsPath, _crmInstance.InstanceFolderPath(), "report");
            }

            return Path.Combine(AppLocalsPath, "report");
        }

        public string DocumentsUploadFolder()
        {
            if (_crmInstance != null)
            {
                return Path.Combine(AppLocalsPath, _crmInstance.InstanceFolderPath(), "docupload");
            }

            return Path.Combine(AppLocalsPath, "docupload");
        }

        public string DocumentPath(string docName)
        {
            return Path.Combine(DocumentsFolder(), docName);
        }

        public string ReportPath(string reportName)
        {
            return Path.Combine(ReportFolder(), reportName);
        }

        public string DocumentUploadPath(string docName)
        {
            return Path.Combine(DocumentsUploadFolder(), docName);
        }
        public string ResourcePath(string resourceName)
        {
            return Path.Combine(ResourcesFolder(), resourceName);
        }

        public void LogoutCleanup()
        {
            _sessionCookies = new List<Cookie>();
            User = null;
            IsInOfflineMode = false;
            IsOfflineModeToggled = false;
            ServerTimeZone = null;
            ClientTimeZone = null;
        }

        public CookieContainer GetCookieContainer()
        {
            var cookieContainer = new CookieContainer();

            if (SessionCookies != null)
            {
                foreach (Cookie cookie in SessionCookies)
                {
                    cookieContainer.Add(cookie);
                }
            }

            return cookieContainer;
        }

    }
}
