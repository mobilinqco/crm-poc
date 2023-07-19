using ACRM.mobile.Domain.Application;
using Polly;
using Polly.Retry;
using Polly.Wrap;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ACRM.mobile.DataAccess.Network
{
    public class HttpNetworkRetryService
    {
        public AsyncRetryPolicy NetworkOperationRetryPolicy { get; private set; }
        public CancellationToken UserCancelationToken { get; set; }

        public HttpNetworkRetryService()
        {
            InitPolicy();
        }

        private void InitPolicy()
        {
            NetworkOperationRetryPolicy = Policy
                .Handle<WebException>()
                .Or<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    4,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Debug.WriteLine($"Retry count {retryCount} and timeSpan {timeSpan} with error {exception.Message}");

                        if(UserCancelationToken != null && UserCancelationToken.IsCancellationRequested)
                        {
                            throw new CrmException("User Cancel", CrmExceptionType.UserAction, CrmExceptionSubType.UserActionCanceledByUser);
                        }
                    }
                );
        }

        public AsyncPolicyWrap PolicyWithCustomTimeout(int timeout)
        {
            var overallTimeout = Policy.TimeoutAsync(timeout);
            var timeoutPerTry = Policy.TimeoutAsync(timeout);

            var waitAndRetryPolicy = Policy
                .Handle<WebException>()
                .Or<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    2,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Debug.WriteLine($"Retry count {retryCount} and timeSpan {timeSpan} with error {exception.Message}");

                        if (UserCancelationToken != null && UserCancelationToken.IsCancellationRequested)
                        {
                            throw new CrmException("User Cancel", CrmExceptionType.UserAction, CrmExceptionSubType.UserActionCanceledByUser);
                        }
                    }
                );

            return Policy.WrapAsync(overallTimeout, waitAndRetryPolicy, timeoutPerTry);
        }
    }
}
