using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.Network;

namespace ACRM.mobile.DataAccess
{
    public interface INetworkRepository
    {
        Task<T> GetAsync<T>(string uri, int timeout, CancellationToken? userCancellationToken, List<Cookie> cookies = null, bool WithAuthRetry = true);
        Task<T> PostAsync<T>(string uri, T data, int timeout, CancellationToken? userCancellationTokenbool, bool WithAuthRetry = true);
        Task<(double, double, TR)> PostSyncReqAsync<T, TR>(string uri, T data, int timeout, CancellationToken userCancellationToken, bool WithAuthRetry = true);
        Task<T> PutAsync<T>(string uri, T data, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true);
        Task DeleteAsync(string uri, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true);
        Task<R> PostAsync<T, R>(string uri, T data, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true);
        Task<bool> PostAsync(string uri, string data, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true);
        Task<RevolutionLoginResponse> PostLoginAsync(string uri, string urlEncodedData, int timeout, CancellationToken? userCancellationToken);
        Task<bool> DownloadDocuemntAsync(string uri, string filePath, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true);
        Task GetResourceAsync(string uri, string resourceName, int timeout, CancellationToken? userCancellationToken, bool WithAuthRetry = true);
        void SetLoginAuthContext(CrmInstance crmInstance, string userName, string password);
        void LogoutCleanup();
        Task<DocumentUploadResponse> UploadDocument(string uri, MultipartFormDataContent multiContent, int timeout, CancellationToken userCancellationToken, bool WithAuthRetry = true);
    }
}
