using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface IQuickSearchService : IContentServiceBase
    {
        Task<List<ListDisplayRow>> PerformQuickSearch(string globalSearchText, CancellationToken token);
    }
}
