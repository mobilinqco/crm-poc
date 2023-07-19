using System;
using System.Collections.Generic;
using System.Linq;

namespace ACRM.mobile.Domain.Application
{
    public class CrmInstance
    {
        public string Identification { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AuthenticationType { get; set; }
        public string LoginMode { get; set; }
        public bool IsLastUsed { get; set; }
        public string RevolutionRuntimeUrl { get; set; }
        public string NetworkUsername { get; set; }
        public string NetworkPassword { get; set; }
        public Dictionary<string, string> UserSettings { get; set; }

        public string LocalStorageFolerName()
        {
            return Identification + "_" + Username;
        }

        public string RasApplicationId()
        {
            if (IsRevolutionCrmInstance())
            {
                if (Username != null)
                {
                    string[] components = Username.Split('\\');
                    if (components.Length > 1)
                    {
                        return components[0];
                    }
                }
            }
            return "";
        }

        public string CuratedUsername()
        {
            string username = Username;
            if (IsRevolutionCrmInstance())
            {
                if (Username != null)
                {
                    string[] components = Username.Split('\\');
                    if (components.Length > 1)
                    {
                        username = components[1];
                    }
                    else if (components.Length > 0)
                    {
                        username = components[0];
                    }
                }
            }
            username = username?.ToLower();
            return username;
        }

        public string InstanceFolderPath()
        {
            if (IsRevolutionCrmInstance())
            {
                return CuratedUsername() + "@" + RasApplicationId();
            }

            return CuratedUsername() + "@" + Identification;
        }

        public bool IsRevolutionCrmInstance()
        {
            if (AuthenticationType.ToLower() == "revolution")
            {
                return true;
            }

            return false;
        }

        public string UrlPath(string path = "/mobile.axd")
        {
            string urlString = Url;

            if (IsRevolutionCrmInstance())
            {
                urlString = RevolutionRuntimeUrl;
            }

            if (urlString.Contains(path))
            {
                return urlString;
            }
            return urlString + path;
        }

        public string AuthenticationUrl()
        {
            if (IsRevolutionCrmInstance())
            {
                string urlString = Url;

                if (urlString.ToLower().Contains("authenticate.axd"))
                {
                    return urlString;
                }
                return urlString + "/Authenticate.axd";
            }

            return UrlPath();
        }

        public string Domain()
        {
            Uri uri = new Uri(UrlPath());
            return uri.Host;
        }

        public string GetSettingValue(string key)
        {
            if(UserSettings?.Keys?.Count>0)
            {
                if(UserSettings.ContainsKey(key))
                {
                    return UserSettings[key];
                }

            }
            return null;
        }
    }
}
