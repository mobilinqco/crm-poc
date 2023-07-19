using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.DataAccess
{
    public interface ICrmInstancesContext
    {
        Task<List<CrmInstance>> GetCrmDevInstancesAsync();
        Task<List<CrmInstance>> GetCrmInstancesAsync();
        CrmInstance GetCrmDemoInstance();
        Task SaveCrmInstances(List<CrmInstance> crmInstances);
    }
}
