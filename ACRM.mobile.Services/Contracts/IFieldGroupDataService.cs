using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application.ActionTemplates;

namespace ACRM.mobile.Services.Contracts
{
    public interface IFieldGroupDataService
    {
        Task<Dictionary<string, string>> GetSourceFieldGroupData(string sourceRecordId, string fieldGroupName, RequestMode requestMode, CancellationToken cancellationToken);
    }
}
