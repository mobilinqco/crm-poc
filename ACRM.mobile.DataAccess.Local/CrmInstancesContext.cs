using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using Newtonsoft.Json;

namespace ACRM.mobile.DataAccess.Local
{
    public class CrmInstancesContext : ICrmInstancesContext
    {
        private readonly string _developerCrmInstancesFile = "DevCrmInstances.json";
        private readonly string _userCrmInstancesFile = "CrmInstances.json";
        private string _appPath;

        public CrmInstancesContext(ISessionContext sessionContext)
        {
            _appPath = sessionContext.AppLocalsPath;
        }

        public async Task<List<CrmInstance>> GetCrmDevInstancesAsync()
        {
            return await GetCrmInstancesAsync(Path.Combine(_appPath, _developerCrmInstancesFile));
        }

        public async Task<List<CrmInstance>> GetCrmInstancesAsync()
        {
            return await GetCrmInstancesAsync(Path.Combine(_appPath, _userCrmInstancesFile));
        }

        public CrmInstance GetCrmDemoInstance()
        {
            CrmInstance crmDemoInstance = new CrmInstance 
            {
                Identification = "Aurea_Demo",
                Name = "Aurea_Demo",
                Url = "https://crm-demo.aurea.com/crmclient_aurea_crm/",
                Username = "simon",
                AuthenticationType = "username"
            };

            return crmDemoInstance;
        }

        public async Task SaveCrmInstances(List<CrmInstance> crmInstances)
        {
            await Task.Run(() => File.WriteAllText(Path.Combine(_appPath, _userCrmInstancesFile),
                JsonConvert.SerializeObject(crmInstances)));
        }

        private async Task<List<CrmInstance>> GetCrmInstancesAsync(string filePath)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<List<CrmInstance>>(File.ReadAllText(filePath));
                if (result != null)
                {
                    return result;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine($"{ e.GetType().Name + " : " + e.Message}");
            }

            return new List<CrmInstance>();
        }
    }
}
