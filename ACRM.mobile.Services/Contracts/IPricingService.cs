using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application.SerialEntry;

namespace ACRM.mobile.Services.Contracts
{
    public interface IPricingService : IContentServiceBase
    {
        Task EvaluatePricing(SerialEntryItem item, int quntity, CancellationToken cancellationToken);
    }
}
