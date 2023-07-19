using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.DataAccess.Network.NetworkHttpClient;
using ACRM.mobile.Domain.Application.Network;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using NLog;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Logging;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using ACRM.mobile.Network.Responses;
using System.Web;

namespace ACRM.mobile.DataAccess.Network
{
    public class NetworkRepository : INetworkRepository
    {
        private readonly ISessionContext _sessionContext;
        private readonly HttpNetworkRetryService _httpNetworkRetryService;
        private readonly HttpClientService _httpClientService;
        private readonly ILogService _logService;

        public NetworkRepository(ISessionContext sessionContext, HttpNetworkRetryService httpNetworkRetryService,
                                HttpClientService httpClientService, ILogService logService)
        {
            _sessionContext = sessionContext;
            _httpNetworkRetryService = httpNetworkRetryService;
            _httpClientService = httpClientService;
            _logService = logService;

        }

        public async Task<T> GetAsync<T>(string uri, int timeout, CancellationToken? userCancellationToken, List<Cookie> cookies = null, bool WithAuthRetry = true)
        {
            _logService.LogInfo($"Starting network GET: {uri}");
            var httpClient = _httpClientService.HttpClient;
            _httpClientService.SetCookieValues(_sessionContext, cookies);
            _logService.LogInfo($"Cookies: {cookies}");
            string jsonResult = string.Empty;

            HttpResponseMessage responseMessage;
            using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

            if (userCancellationToken is CancellationToken uct)
            {
                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                _httpNetworkRetryService.UserCancelationToken = uct;
                responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                .ExecuteAsync(async ct => await httpClient.GetAsync(uri, ct), linkedCts.Token);
            }
            else
            {
                responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                .ExecuteAsync(async () => await httpClient.GetAsync(uri));
            }

            _logService.LogInfo($"Response Status Code: {responseMessage.StatusCode}");

            jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (HasAuthenticationFailed(responseMessage, jsonResult))
            {
                if (WithAuthRetry)
                {
                    bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                    if (AuthSuccess)
                    {
                        return await GetAsync<T>(uri, timeout, userCancellationToken, cookies, false);
                    }
                }

                _logService.LogError($"Authentication Error");
                _logService.LogError(responseMessage.ToString());

                throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, jsonResult);

            }
            else if (responseMessage.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(AuthenticationResponse) && !_httpClientService.IsLoggedIn && !_sessionContext.IsChangePasswordEnabled)
                {
                    _httpClientService.authInfo = new AuthInfo
                    {
                        URI = uri,
                        Timeout = timeout,
                        Cookies = cookies
                    };
                }

                T json;// = JsonConvert.DeserializeObject<T>(jsonResult);
                try
                {
                    if (userCancellationToken is CancellationToken uct2)
                    {
                        json = await Task.Run(() => JsonConvert.DeserializeObject<T>(jsonResult), uct2);
                    }
                    else
                    {
                        json = await Task.Run(() => JsonConvert.DeserializeObject<T>(jsonResult));
                    }
                    // TODO login hack, I should find a better method for
                    // extracting the cookie.
                    if (typeof(T).GetProperty("Cookies") != null)
                    {
                        List<Cookie> sessionCookies = _httpClientService.GetCookies(uri);// GetSessionIdCookie(responseMessage);
                        typeof(T).GetProperty("Cookies").SetValue(json, sessionCookies);
                    }
                    return json;
                }
                catch (Exception e)
                {
                    _logService.LogError($"Error decoding json {e.GetType().Name} : {e.Message}");
                    _logService.LogError(responseMessage.ToString());
                    throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);
                }
            }
            else
            {
                _logService.LogError("Http Error:");
                _logService.LogInfo(responseMessage.ToString());
                throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);
            }
        }

        private async Task<bool> ReAuthenticate(CancellationToken? userCancellationToken)
        {
            if (_httpClientService.IsLoggedIn && _httpClientService.authInfo != null)
            {
                AuthenticationResponse authenticationResponse = null;

                if (_sessionContext.CrmInstance.IsRevolutionCrmInstance())
                {
                    authenticationResponse = await AuthenticateRevolution(_sessionContext.CrmInstance);
                }
                else
                {
                    var authinfo = _httpClientService.authInfo;
                    authenticationResponse = await GetAsync<AuthenticationResponse>(authinfo.URI, authinfo.Timeout, userCancellationToken, authinfo.Cookies, false);
                }

                if (authenticationResponse.IsAuthenticated)
                {
                    _sessionContext.SessionCookies = authenticationResponse.Cookies;
                    return true;
                }
            }
            return false;
        }

        public async Task<T> PostAsync<T>(string uri, T data, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true)
        {
            try
            {
                var httpClient = _httpClientService.HttpClient;
                _httpClientService.SetCookieValues(_sessionContext);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

                if (userCancellationToken is CancellationToken uct)
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                    _httpNetworkRetryService.UserCancelationToken = uct;
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async ct => await httpClient.PostAsync(uri, content, ct), linkedCts.Token);
                }
                else
                {
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async () => await httpClient.PostAsync(uri, content, timeoutCts.Token));
                }

                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (HasAuthenticationFailed(responseMessage, jsonResult))
                {
                    if (WithAuthRetry)
                    {
                        bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                        if (AuthSuccess)
                        {
                            return await PostAsync<T>(uri, data, timeout, userCancellationToken, false);
                        }
                    }

                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, jsonResult);
                }
                else if (responseMessage.IsSuccessStatusCode)
                {
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json;
                }

                _logService.LogError("Http Error:");
                _logService.LogInfo(responseMessage.ToString());
                throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);

            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<(double, double, TR)> PostSyncReqAsync<T, TR>(string uri, T data, int timeout, CancellationToken userCancellationToken, bool WithAuthRetry = true)
        {
            try
            {
                DateTime start = DateTime.Now;
                double serverProcessing = 0;
                double unpacking = 0;

                var httpClient = _httpClientService.HttpClient;
                _httpClientService.SetCookieValues(_sessionContext);

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);
                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, userCancellationToken);

                _httpNetworkRetryService.UserCancelationToken = userCancellationToken;
                responseMessage = await _httpNetworkRetryService.PolicyWithCustomTimeout(timeout).ExecuteAsync(async ct =>
                {
                    using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri))
                    {
                        StringContent content;

                        if (data is string)
                        {
                            content = new StringContent(data as string);
                        }
                        else
                        {
                            content = new StringContent(JsonConvert.SerializeObject(data));
                        }

                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        requestMessage.Content = content;
                        return await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, ct);
                    };
                }, linkedCts.Token);

                serverProcessing = (DateTime.Now - start).TotalMilliseconds;
                start = DateTime.Now;

                string jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (HasAuthenticationFailed(responseMessage, jsonResult))
                {
                    responseMessage.Dispose();
                    if (WithAuthRetry)
                    {
                        bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                        if (AuthSuccess)
                        {
                            return await PostSyncReqAsync<T, TR>(uri, data, timeout, userCancellationToken, false);
                        }
                    }
                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, jsonResult);
                }
                else if (responseMessage.IsSuccessStatusCode)
                {
                    // Optimize the memory usage for handling large data sets
                    using (var sr = new StreamReader(await responseMessage.Content.ReadAsStreamAsync()))
                    {
                        using (var jsonTextReader = new JsonTextReader(sr))
                        {
                            var serializer = new Newtonsoft.Json.JsonSerializer();
                            var json = serializer.Deserialize<TR>(jsonTextReader);
                            responseMessage.Dispose();
                            unpacking = (DateTime.Now - start).TotalMilliseconds;
                            return (serverProcessing, unpacking, json);
                        }
                    }
                }

                _logService.LogError("Http Error:");
                _logService.LogInfo(responseMessage.ToString());
                responseMessage.Dispose();
                throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);

            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<TR> PostAsync<T, TR>(string uri, T data, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true)
        {
            string jsonResult = string.Empty;

            try
            {
                var httpClient = _httpClientService.HttpClient;
                _httpClientService.SetCookieValues(_sessionContext);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

                if (userCancellationToken is CancellationToken uct)
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                    _httpNetworkRetryService.UserCancelationToken = uct;
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async ct => await httpClient.PostAsync(uri, content, ct), linkedCts.Token);
                }
                else
                {
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async () => await httpClient.PostAsync(uri, content, timeoutCts.Token));
                }

                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (HasAuthenticationFailed(responseMessage, jsonResult))
                {
                    if (WithAuthRetry)
                    {
                        bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                        if (AuthSuccess)
                        {
                            return await PostAsync<T, TR>(uri, data, timeout, userCancellationToken, false);
                        }
                    }
                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, jsonResult);
                }
                else if (responseMessage.IsSuccessStatusCode)
                {
                    string logResult = jsonResult != null && jsonResult.Length > 200 ? jsonResult.Substring(0, 200) : jsonResult;
                    _logService.LogDebug($"Network response {logResult}");
                    var json = JsonConvert.DeserializeObject<TR>(jsonResult);
                    return json;
                }
                else
                {
                    _logService.LogError("Http Error:");
                    _logService.LogDebug(responseMessage.ToString());
                    throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);
                }
            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<bool> PostAsync(string uri, string data, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true)
        {
            try
            {
                var httpClient = _httpClientService.HttpClient;
                _httpClientService.SetCookieValues(_sessionContext);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

                if (userCancellationToken is CancellationToken uct)
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                    _httpNetworkRetryService.UserCancelationToken = uct;
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async ct => await httpClient.PostAsync(uri, content, ct), linkedCts.Token);
                }
                else
                {
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async () => await httpClient.PostAsync(uri, content, timeoutCts.Token));
                }

                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (HasAuthenticationFailed(responseMessage, jsonResult))
                {
                    if (WithAuthRetry)
                    {
                        bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                        if (AuthSuccess)
                        {
                            return await PostAsync(uri, data, timeout, userCancellationToken, false);
                        }
                    }
                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, jsonResult);
                }
                else if (responseMessage.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logService.LogError("Http Error:");
                    _logService.LogDebug(responseMessage.ToString());
                    throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);
                }
            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<T> PutAsync<T>(string uri, T data, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true)
        {
            try
            {
                var httpClient = _httpClientService.HttpClient;
                _httpClientService.SetCookieValues(_sessionContext);

                var content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string jsonResult = string.Empty;

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

                if (userCancellationToken is CancellationToken uct)
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                    _httpNetworkRetryService.UserCancelationToken = uct;
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async ct => await httpClient.PutAsync(uri, content, ct), linkedCts.Token);
                }
                else
                {
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async () => await httpClient.PutAsync(uri, content, timeoutCts.Token));
                }

                jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (HasAuthenticationFailed(responseMessage, jsonResult))
                {
                    if (WithAuthRetry)
                    {
                        bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                        if (AuthSuccess)
                        {
                            return await PutAsync<T>(uri, data, timeout, userCancellationToken, false);
                        }
                    }

                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, jsonResult);
                }
                else if (responseMessage.IsSuccessStatusCode)
                {
                    var json = JsonConvert.DeserializeObject<T>(jsonResult);
                    return json;
                }

                throw new HttpRequestExceptionEx(responseMessage.StatusCode, jsonResult);
            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }


        public async Task<RevolutionLoginResponse> PostLoginAsync(string uri, string urlEncodedData, int timeout, CancellationToken? userCancellationToken)
        {
            try
            {
                var httpClient = _httpClientService.HttpClient;

                var content = new StringContent(urlEncodedData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

                if (userCancellationToken is CancellationToken uct)
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                    _httpNetworkRetryService.UserCancelationToken = uct;
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async ct => await httpClient.PostAsync(uri, content, ct), linkedCts.Token);
                }
                else
                {
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async () => await httpClient.PostAsync(uri, content, timeoutCts.Token));
                }


                string response = String.Empty;
                if (responseMessage.IsSuccessStatusCode)
                {
                    response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    List<Cookie> sessionCookies = _httpClientService.GetCookies(uri);

                    return new RevolutionLoginResponse { Cookies = sessionCookies, RedirectionUrl = response };
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                    responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, response);
                }

                throw new HttpRequestExceptionEx(responseMessage.StatusCode, response);

            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<bool> DownloadDocuemntAsync(string uri, string filePath, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true)
        {
            try
            {
                _logService.LogInfo($"Starting document download retrieval GET: {uri} to Path: {filePath}");
                var httpClient = _httpClientService.HttpClient;
                _httpClientService.SetCookieValues(_sessionContext);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

                if (userCancellationToken is CancellationToken uct)
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                    _httpNetworkRetryService.UserCancelationToken = uct;
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async ct => await httpClient.GetAsync(uri, ct), linkedCts.Token);
                }
                else
                {
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async () => await httpClient.GetAsync(uri, timeoutCts.Token));
                }

                var jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!Directory.Exists(_sessionContext.DocumentsFolder()))
                {
                    Directory.CreateDirectory(_sessionContext.DocumentsFolder());
                }
                if (responseMessage.IsSuccessStatusCode)
                {
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await responseMessage.Content.CopyToAsync(fs);
                    }
                }

                if (HasAuthenticationFailed(responseMessage, jsonResult))
                {
                    if (WithAuthRetry)
                    {
                        bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                        if (AuthSuccess)
                        {
                            return await DownloadDocuemntAsync(uri, filePath, timeout, userCancellationToken, false);
                        }
                    }

                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, responseMessage.ToString());
                }

                return true;

            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task<DocumentUploadResponse> UploadDocument(string uri, MultipartFormDataContent multiContent, int timeout, CancellationToken userCancellationToken, bool WithAuthRetry = true)
        {
            try
            {
                _logService.LogInfo($"Starting upload download POST: {uri}");
                var httpClient = _httpClientService.HttpClient;
                _httpClientService.SetCookieValues(_sessionContext);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

                if (userCancellationToken is CancellationToken uct)
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                    _httpNetworkRetryService.UserCancelationToken = uct;
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async ct => await httpClient.PostAsync(uri, multiContent, ct), linkedCts.Token);
                }
                else
                {
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async () => await httpClient.PostAsync(uri, multiContent, timeoutCts.Token));
                }

                _logService.LogInfo($"UploadDocument server response: {responseMessage}");

                var jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (HasAuthenticationFailed(responseMessage, jsonResult))
                {
                    if (WithAuthRetry)
                    {
                        bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                        if (AuthSuccess)
                        {
                            return await UploadDocument(uri, multiContent, timeout, userCancellationToken, false);
                        }
                    }
                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, responseMessage.ToString());
                }
                else if (responseMessage.IsSuccessStatusCode)
                {
                    _logService.LogInfo($"UploadDocument server response data: {jsonResult}");
                    var json = JsonConvert.DeserializeObject<DocumentUploadResponse>(jsonResult);
                    return json;
                }

                return null;

            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task GetResourceAsync(string uri, string resourceName, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true)
        {
            try
            {
                _logService.LogInfo($"Starting network resource retrieval GET: {uri}");
                var httpClient = _httpClientService.HttpClient;
                _httpClientService.SetCookieValues(_sessionContext);

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                //httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { MustRevalidate = true };

                HttpResponseMessage responseMessage;
                using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

                if (userCancellationToken is CancellationToken uct)
                {
                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                    _httpNetworkRetryService.UserCancelationToken = uct;
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async ct => await httpClient.GetAsync(uri, ct), linkedCts.Token);
                }
                else
                {
                    responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                                   .ExecuteAsync(async () => await httpClient.GetAsync(uri, timeoutCts.Token));
                }

                var jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (HasAuthenticationFailed(responseMessage, jsonResult))
                {
                    if (WithAuthRetry)
                    {
                        bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                        if (AuthSuccess)
                        {
                            await GetResourceAsync(uri, resourceName, timeout, userCancellationToken, false);
                        }
                    }
                    throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, responseMessage.ToString());
                }
                else if (responseMessage.IsSuccessStatusCode)
                {
                    using (var fs = new FileStream(_sessionContext.ResourcePath(resourceName), FileMode.CreateNew))
                    {
                        await responseMessage.Content.CopyToAsync(fs);
                    }
                }

                throw new HttpRequestExceptionEx(responseMessage.StatusCode, responseMessage.ToString());

            }
            catch (Exception e)
            {
                _logService.LogError($"{e.GetType().Name + " : " + e.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(string uri, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true)
        {
            var httpClient = _httpClientService.HttpClient;
            _httpClientService.SetCookieValues(_sessionContext);
            HttpResponseMessage responseMessage;
            using CancellationTokenSource timeoutCts = new CancellationTokenSource(timeout);

            if (userCancellationToken is CancellationToken uct)
            {
                using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, uct);

                _httpNetworkRetryService.UserCancelationToken = uct;
                responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                               .ExecuteAsync(async ct => await httpClient.DeleteAsync(uri, ct), linkedCts.Token);
            }
            else
            {
                responseMessage = await _httpNetworkRetryService.NetworkOperationRetryPolicy
                                                               .ExecuteAsync(async () => await httpClient.DeleteAsync(uri, timeoutCts.Token));
            }

            var jsonResult = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (HasAuthenticationFailed(responseMessage, jsonResult))
            {
                if (WithAuthRetry)
                {
                    bool AuthSuccess = await ReAuthenticate(userCancellationToken);
                    if (AuthSuccess)
                    {
                        await DeleteAsync(uri, timeout, userCancellationToken, false);
                    }
                }
                throw new AuthenticationException(AuthenticationException.AuthExceptionType.Network, responseMessage.ToString());
            }
            else if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            throw new HttpRequestExceptionEx(responseMessage.StatusCode, responseMessage.ToString());
        }

        bool HasAuthenticationFailed(HttpResponseMessage responseMessage, string jsonResult)
        {
            return responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                            responseMessage.StatusCode == HttpStatusCode.Unauthorized ||
                            jsonResult.Contains("rasLogin");
        }

        void INetworkRepository.SetLoginAuthContext(CrmInstance crmInstance, string userName, string password)
        {
            _httpClientService.SetSessionHttpClientHandler(crmInstance, userName, password);
        }

        void INetworkRepository.LogoutCleanup()
        {
            _httpClientService.Clear();
        }

        async Task<AuthenticationResponse> AuthenticateRevolution(CrmInstance crmInstance, string languageCode = null)
        {
            UriBuilder uriBuilder = new UriBuilder(crmInstance.AuthenticationUrl());

            _httpClientService.CookieContainer = new CookieContainer();

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            query["Service"] = "Authenticate";
            query["Username"] = crmInstance.Username;
            query["Password"] = crmInstance.Password;
            query["ClientVersion"] = "3.0";

            RevolutionLoginResponse networkResponse = await PostLoginAsync(uriBuilder.ToString(), query.ToString(), 10000, null);

            crmInstance.RevolutionRuntimeUrl = networkResponse.RedirectionUrl;
            uriBuilder = new UriBuilder(crmInstance.UrlPath());

            query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["Service"] = "Authenticate";
            query["Method"] = "RASLogon";
            query["ClientVersion"] = "3.0";
            query["ServerInfo"] = "true";
            query["AppInfo"] = "crmclient";
            query["Language"] = _sessionContext.LanguageCode;

            uriBuilder.Query = query.ToString();

            AuthenticationResponse authenticationResponse = await GetAsync<AuthenticationResponse>(uriBuilder.ToString(), 10000, null, networkResponse.Cookies, false);
            authenticationResponse.RedirectionUrl = networkResponse.RedirectionUrl;

            return authenticationResponse;
        }
    }
}