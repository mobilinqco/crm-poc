using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface IOpenUrlService
    {
        Task PrepareContentAsync(UserAction userAction, string recordId, CancellationToken cancellationToken);
        string UrlString();
        bool PopToPrevious();
        bool IsCustomUrl();
    }
}
