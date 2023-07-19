using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services
{
    public class CrmInstanceService : ICrmInstanceService
    {
        private ICrmInstancesContext _crmInstancesContext;
        private IConfigurationService _configurationService;
        private List<CrmInstance> _crmInstances;

        public CrmInstanceService(ICrmInstancesContext crmInstancesContext,
            IConfigurationService configurationService)
        {
            _crmInstancesContext = crmInstancesContext;
            _configurationService = configurationService;
        }

        public async Task<List<CrmInstance>> GetCrmInstancesAsync()
        {
            if(_crmInstances == null)
            {
                await LoadCrmInstances();
            }

            return _crmInstances;
        }

        private async Task LoadCrmInstances()
        {
            _crmInstances = new List<CrmInstance>();
            List<CrmInstance> crms = await _crmInstancesContext.GetCrmInstancesAsync();
            _crmInstances.AddRange(crms);

            if (crms.Count == 0)
            {
#if DEBUG
                // If we have no instance configured and we are in development mode
                // we add the preconfigured development servers
                crms = await _crmInstancesContext.GetCrmDevInstancesAsync();
                _crmInstances.AddRange(crms);
                await _crmInstancesContext.SaveCrmInstances(crms);
#else
                _crmInstances.Add(_crmInstancesContext.GetCrmDemoInstance());
#endif
            }
            else
            {
                if (_configurationService.GetBoolConfigValue("Login.ShowDemoServer"))
                {
                    _crmInstances.Add(_crmInstancesContext.GetCrmDemoInstance());
                }
            }
        }

        public async Task<bool> AddNewCrmInstance(CrmInstance crmInstance)
        {
            bool returnValue = false;
            List<CrmInstance> crms = await GetCrmInstancesAsync();
            try
            {
                crms.First(crm => crm.Identification == crmInstance.Identification);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{ e.GetType().Name + " : " + e.Message}");
                crms.Add(crmInstance);
                returnValue = true;
            }

            await SetLastUseInstanceAsync(crmInstance.Identification);

            return returnValue;
        }

        public async Task<bool> AddNewCrmInstanceAsync(string queryString)
        {
            if (queryString != null)
            {
                var query = HttpUtility.ParseQueryString(queryString);
                string id = query.Get("identification");
                string name = query.Get("name");
                string url = query.Get("url");
                string authType = query.Get("authenticationType") ?? "username";
                string username = query.Get("networkUsername") ?? "";
                string password = query.Get("networkPassword") ?? "";
                string loginMode = query.Get("loginMode") ?? "";
                string networkUsername = query.Get("networkUsername") ?? "";
                string networkPassword = query.Get("networkPassword") ?? "";

                if (id != null && (id.Count() > 0)
                    && name != null && name.Count() > 0
                    && url != null && url.Count() > 0)
                {
                    return await AddNewCrmInstance(new CrmInstance
                    {
                        Identification = id,
                        Name = name,
                        Url = url,
                        AuthenticationType = authType,
                        Username = username,
                        Password = password,
                        LoginMode = loginMode,
                        NetworkUsername = networkUsername,
                        NetworkPassword = networkPassword
                    });
                }
            }

            return false;
        }

        public async Task<bool> UpdateCrmInstanceAsync(string identification, string queryString)
        {
            if (queryString != null)
            {
                var query = HttpUtility.ParseQueryString(queryString);
                string name = query.Get("name");
                string url = query.Get("url");
                string authType = query.Get("authenticationType") ?? "username";
                string username = query.Get("networkUsername") ?? "";
                string password = query.Get("networkPassword") ?? "";
                string loginMode = query.Get("loginMode") ?? "";
                string networkUsername = query.Get("networkUsername") ?? "";
                string networkPassword = query.Get("networkPassword") ?? "";

                if (identification != null && (identification.Count() > 0)
                    && name != null && name.Count() > 0
                    && url != null && url.Count() > 0)
                {
                    List<CrmInstance> crms = await GetCrmInstancesAsync();
                    var existingCrm = crms.Find(c => c.Identification == identification);
                    if (existingCrm != null)
                    {
                        existingCrm.Name = name;
                        existingCrm.Url = url;
                        existingCrm.AuthenticationType = authType;
                        existingCrm.Username = username;
                        existingCrm.Password = password;
                        existingCrm.LoginMode = loginMode;
                        existingCrm.NetworkUsername = networkUsername;
                        existingCrm.NetworkPassword = networkPassword;
                    }
                    await _crmInstancesContext.SaveCrmInstances(crms);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> SaveCrmInstanceAsync(string identification, Dictionary<string, string> configs)
        {
            if(configs?.Keys?.Count>0)
            {
                List<CrmInstance> crms = await GetCrmInstancesAsync();
                var existingCrm = crms.Find(c => c.Identification == identification);
                if (existingCrm != null)
                {
                    existingCrm.UserSettings = configs;
                }
                await _crmInstancesContext.SaveCrmInstances(crms);
            }
            return true;
        }

        public async Task<bool> RemoveCrmInstanceAsync(string queryString)
        {
            if (queryString != null)
            {
                var query = HttpUtility.ParseQueryString(queryString);
                string name = query.Get("name");
                if (name != null && name.Count() > 0)
                {
                    List<CrmInstance> crms = await GetCrmInstancesAsync();
                    try
                    {
                        CrmInstance crmInst = crms.First(crm => crm.Name == name);
                        crms.Remove(crmInst);
                        await _crmInstancesContext.SaveCrmInstances(crms);
                        return true;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"{ e.GetType().Name + " : " + e.Message}");
                    }
                }
            }

            return false;
        }

        public async Task SetLastUseInstanceAsync(string identification)
        {
            List<CrmInstance> crms = await GetCrmInstancesAsync();
            foreach(CrmInstance crmInstance in crms)
            {
                crmInstance.IsLastUsed = false;
                if(crmInstance.Identification == identification)
                {
                    crmInstance.IsLastUsed = true;
                }
            }
            
            await _crmInstancesContext.SaveCrmInstances(crms);
        }

        public async Task<CrmInstance> GetDefaultServer()
        {
            return new CrmInstance
            {
                Identification = "AureaSAAS",
                Name = "AureaSAAS",
                Url = "https://my.update.com/login",
                AuthenticationType = "revolution"
            };
      
        }
        public async Task<CrmInstance> GetLastUsedInstance()
        {
            List<CrmInstance> crms = await GetCrmInstancesAsync();
            foreach (CrmInstance crmInstance in crms)
            {
                if (crmInstance.IsLastUsed)
                {
                    return crmInstance;
                }
            }

            // if none was used we return the first instance from the list
            if (crms.Count > 0)
            {
                return crms[0];
            }

            return null;
        }

        public async Task<CrmInstance> GetCrmInstanceAsync(string identification)
        {
            List<CrmInstance> crms = await GetCrmInstancesAsync();
            return crms.Find(m => m.Identification == identification);
        }

        public async Task<bool> CreateOrUpdateCrmInstanceAsync(string queryString)
        {
            var returnValue = false;
            var id = HttpUtility.ParseQueryString(queryString).Get("identification");
            if (id != null)
            {
                var existingCrmInstance = await GetCrmInstanceAsync(id);
                if (existingCrmInstance != null)
                {
                    returnValue = await UpdateCrmInstanceAsync(id, queryString);
                }
                else
                {
                    returnValue = await AddNewCrmInstanceAsync(queryString);
                }
            }
            return returnValue;
        }
    }
}

// If no server is registered and user inptus user and name system will automatically added 