using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICrmInstanceService
    {
        Task<List<CrmInstance>> GetCrmInstancesAsync();
        Task<bool> AddNewCrmInstance(CrmInstance crmInstance);
        Task<bool> AddNewCrmInstanceAsync(string queryString);
        Task<bool> RemoveCrmInstanceAsync(string queryString);
        Task<bool> UpdateCrmInstanceAsync(string identification, string queryString);
        Task<bool> CreateOrUpdateCrmInstanceAsync(string queryString);
        Task<CrmInstance> GetCrmInstanceAsync(string identification); 
        Task SetLastUseInstanceAsync(string identification);
        Task<CrmInstance> GetLastUsedInstance();
        Task<CrmInstance> GetDefaultServer();
        Task<bool> SaveCrmInstanceAsync(string identification, Dictionary<string, string> configs);
    }
}
