using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.SerialEntry;

namespace ACRM.mobile.Services.Contracts
{
    public interface IQuotaService : IContentServiceBase
    {
        bool HasQuotaConfig
        {
            get;
        }

        Task<ItemQuota> GetQuotaItem(SerialEntryItem item,CancellationToken cancellationToken);

    }
}
