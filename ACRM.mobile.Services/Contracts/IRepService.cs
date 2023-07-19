using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface IRepService
    {
        Task PrepareContentAsync(CancellationToken cancellationToken);
        Task<string> GetRepName(string repId, CancellationToken cancellationToken);
        Task<List<CrmRep>> GetAllCrmReps(CancellationToken cancellationToken);
    }
}
