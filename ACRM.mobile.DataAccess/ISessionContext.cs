using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using ACRM.mobile.Domain;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.DataAccess
{
    public interface ISessionContext
    {
        List<Cookie> SessionCookies { get; set; }
        User User { get; set; }
        TimeZoneInfo ClientTimeZone { get; set; }
        TimeZoneInfo ServerTimeZone { get; set; }
        string AppLocalsPath { get; set; }
        CrmInstance CrmInstance { get; set; }
        bool IsInOfflineMode { get; set; }
        bool HasNetworkConnectivity { get; set; }
        bool IsOfflineModeToggled { get; set; }
        bool IsChangePasswordEnabled { get; set; }
        string LanguageCode { get; set; }
        Dictionary<string, string> ExtraParams { get; set; }
        int OfflineStationNumber { get; set; }

        bool IsAuthenticated();
        string LocalCrmInstancePath();
        string ResourcesFolder();
        string DocumentsFolder();
        string ReportFolder();
        string DocumentsUploadFolder();
        string DocumentUploadPath(string docName);
        string ResourcePath(string resourceName);
        string DocumentPath(string resourceName);
        string ReportPath(string reportName);

        void LogoutCleanup();

        CookieContainer GetCookieContainer();

        event PropertyChangedEventHandler PropertyChanged;
    }
}
