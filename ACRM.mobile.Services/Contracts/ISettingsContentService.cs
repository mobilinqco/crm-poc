using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
	public interface ISettingsContentService : IContentServiceBase
    {
        public WebConfigLayout Layout { get; }
        Task<bool> Save(List<WebConfigData> modifiedConfigData, List<WebConfigData> userConfigData, CancellationToken token);
        public Task RefreshData(CancellationToken cancellationToken);
        Task<List<WebConfigData>> GetLocalConfigurations(string identification, CancellationToken token);
    }
}

