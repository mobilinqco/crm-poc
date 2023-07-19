using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface IDashboardContentService : IContentServiceBase
    {
        List<UserAction> InsightBoardActions();
        Task<Form> GetDashboardForm(CancellationToken cancellationToken);
        public string HeaderLabel { get; set; }
        public UserAction StartUserAction { get;}
    }
}
