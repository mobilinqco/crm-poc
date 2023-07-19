using System;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface IRightsProcessor
    {
        public Task<(bool,string)> EvaluateRightsFilter(string filterName, string rootRecordId, CancellationToken cancellationToken, bool InvalidDefault = true);
        public Task<(bool, bool, string)> EvaluateRightsFilter(UserAction userAction, CancellationToken cancellationToken, bool InvalidDefault = true, string filterNameKey = "");
    }
}
