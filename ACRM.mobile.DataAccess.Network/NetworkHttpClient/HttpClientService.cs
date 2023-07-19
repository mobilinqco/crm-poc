using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.FormatUtils;

namespace ACRM.mobile.DataAccess.Network.NetworkHttpClient
{
    public class HttpClientService
    {
        public HttpClient HttpClient { get; set; }
        public CookieContainer CookieContainer { get; set; }
        private string _runtimePlatform;
        internal AuthInfo authInfo { get; set; } = null;
        public bool IsLoggedIn
        {
            get
            {
                return authInfo != null;
            }
        }


        public HttpClientService(string runtimePlatform)
        {
            _runtimePlatform = runtimePlatform;
            InitHttpClient();
        }

        private void InitHttpClient()
        {
            CookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = CookieContainer };
            HttpClient = new HttpClient(handler);

            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        public void Clear()
        {
            InitHttpClient();
            authInfo = null;
        }

        public void SetCookieValues(ISessionContext sessionContext, List<Cookie> cookies = null)
        {
            string domain = sessionContext.CrmInstance.Domain();
            if (cookies != null)
            {
                foreach (Cookie cookie in cookies)
                {
                    CookieContainer.Add(cookie);
                }
            }

            if (sessionContext.SessionCookies != null)
            {
                foreach (Cookie cookie in sessionContext.SessionCookies)
                {
                    CookieContainer.Add(cookie);
                }
            }
        }

        public void SetSessionHttpClientHandler(CrmInstance crmInstance, string userName, string password)
        {
            if (crmInstance != null)
            {
                string authType = crmInstance.AuthenticationType.ToLower();
                CookieContainer = new CookieContainer();
                var handler = CreateHttpClientHandler(_runtimePlatform);
                handler.CookieContainer = CookieContainer;
                handler.UseDefaultCredentials = true;

                switch (authType)
                {
                    case "ssocredentialsnocache":
                    case "ssocredentials":
                        var domain = userName.GetDomain();
                        var login = userName.GetLogin();
                        if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(login))
                        {
                            handler.Credentials = new NetworkCredential(login, password, domain);
                        }
                        else
                        {
                            handler.Credentials = new NetworkCredential(userName, password);
                        }
                        break;
                    case "sso":
                        break;
                    case "revolution":
                        break;
                    case "username":
                        break;
                    case "usernamecredentials":
                        if (!string.IsNullOrEmpty(crmInstance.NetworkUsername))
                        {
                            var _domain = crmInstance.NetworkUsername.GetDomain();
                            var _login = crmInstance.NetworkUsername.GetLogin();
                            if (!string.IsNullOrEmpty(_domain) && !string.IsNullOrEmpty(_login))
                            {
                                handler.Credentials = new NetworkCredential(_login, crmInstance.NetworkPassword, _domain);
                            }
                            else
                            {
                                handler.Credentials = new NetworkCredential(crmInstance.NetworkUsername, crmInstance.NetworkPassword);
                            }

                        }

                        break;
                    default:
                        break;
                }

                HttpClient = new HttpClient(handler);
                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpClient.Timeout = Timeout.InfiniteTimeSpan;
            }
        }

        public static HttpClientHandler CreateHttpClientHandler(string _runtimePlatform)
        {
            if (!string.IsNullOrEmpty(_runtimePlatform) && _runtimePlatform.Equals("iOS"))
            {
                try
                {
                    // Workaround for Xmarin IOS bug : https://github.com/xamarin/xamarin-macios/issues/7770#issuecomment-610957690

                    var LinkerTipText = "IOS Workaround";
                    var monoHandlerType = Type.GetType("System.Net.Http.MonoWebRequestHandler, System.Net.Http");
                    if (monoHandlerType == null)
                        throw new InvalidOperationException("System.Net.Http.MonoWebRequestHandler was not found in System.Net.Http." + LinkerTipText);

                    var internalMonoHandlerCtors = monoHandlerType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (internalMonoHandlerCtors.Length < 1)
                        throw new InvalidOperationException("Internal parameter-less constructor for System.Net.Http.MonoWebRequestHandler was not found." + LinkerTipText);

                    var httpClientHandlerCtors = typeof(HttpClientHandler).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                    if (httpClientHandlerCtors.Length < 1)
                        throw new InvalidOperationException("internal HttpClientHandler(IMonoHttpClientHandler) constructor was not found in System.Net.Http.HttpClientHandler." + LinkerTipText);

                    var internalMonoHandler = internalMonoHandlerCtors[0].Invoke(null);
                    return (HttpClientHandler)httpClientHandlerCtors[0].Invoke(new[] { internalMonoHandler });
                }
                catch
                {
                    return new HttpClientHandler();
                }
            }
            else
            {
                return new HttpClientHandler();
            }

        }
        public List<Cookie> GetCookies(string uriString)
        {
            Uri uri = new Uri(uriString);
            List<Cookie> cookies = new List<Cookie>();
            foreach (Cookie cookie in CookieContainer.GetCookies(uri))
            {
                cookies.Add(cookie);
            }
            return cookies;
        }
    }
}
