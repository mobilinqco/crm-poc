using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using Newtonsoft.Json;

namespace ACRM.mobile.Services
{
    public class CrmRequestBuilder : ICrmRequestBuilder
    {
        private readonly ILogService _logService;
        private readonly ISessionContext _sessionContext;

        public CrmRequestBuilder(ILogService logService, ISessionContext sessionContext)
        {
            _logService = logService;
            _sessionContext = sessionContext;
        }

        public string SyncConfigurationQuery(bool includeCatalogs = true,
           bool includeSyncConfiguration = true,
           bool includeSyncDataModel = true)
        {
            Dictionary<string, bool> queryParams = new Dictionary<string, bool>()
            {
                {"SyncCatalogs", includeCatalogs },
                {"SyncConfiguration",  includeSyncConfiguration},
                {"SyncDataModel", includeSyncDataModel }
            };
            string query = "Service=Synchronization&InvalidateFullCache=true&AppInfo=crmclient";
            query += string.Join("", queryParams
                .Where(a => a.Value == true)
                .Select(e => "&" + e.Key + "=" + e.Value.ToString().ToLower())
                .ToArray());
            return query;
        }

        public string SyncCatalogsQuery()
        {
            return "Service=Synchronization&InvalidateFullCache=true&SyncCatalogs=true&AppInfo=crmclient";
        }

        public string SyncService()
        {
            return "Service=Synchronization&DataSetName0=FI&DataSetNameCount=1&SyncRecordData=true&AppInfo=crmclient";
        }

        public string DataSetQuery(string dataSetName)
        {
            return "Service=Synchronization&DataSetName0=" + dataSetName
                + "&DataSetNameCount=1&SyncRecordData=true&AppInfo=crmclient";
        }

        public string IncrementalDataSetQuery(string dataSetName, string timeSinceLastSync)
        {
            return "Service=Synchronization&DataSetName0=" + dataSetName
                + "&DataSetNameCount=1&SyncRecordData=true&Since0=" + timeSinceLastSync + "&AppInfo=crmclient";
        }

        private string QueryString(Dictionary<string, string> dictionary)
        {
            if (dictionary == null || !dictionary.Keys.Any())
            {
                return ""; 
            }

            var requestParameterValues = new List<string>();
            var unformated = new List<string>();

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                var parameter = $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
                unformated.Add($"{key}={value}");
                requestParameterValues.Add(parameter);
            }

            _logService.LogDebug($"Network Request Parameters: {string.Join("&", unformated)}");

            return string.Join("&", requestParameterValues);
        }

        public string RecordDataQuery(object queryDef,
            string recordIdentification,
            string linkRecordIdentification,
            int linkId,
            int maxResults)
        {
            var dictionary = new Dictionary<string, string>() { { "Service", "RecordData" } };
            
            dictionary["QueryDef"] = JsonConvert.SerializeObject(queryDef);
            if (!string.IsNullOrWhiteSpace(linkRecordIdentification))
            {
                dictionary["LinkRecordIdentification"] = linkRecordIdentification;
                if (linkId >= 0)
                {
                    dictionary["LinkId"] = $"{linkId}";
                }
            }

            if (!string.IsNullOrWhiteSpace(recordIdentification))
            {
                dictionary["RecordIdentification"] = recordIdentification;
            }

            if (maxResults > 0)
            {
                dictionary["MaxResults"] = $"{maxResults}";
            }
            else if (maxResults == -1)
            {
                dictionary["CountOnly"] = "1";
            }

            dictionary["AppInfo"] = "crmclient";
            return QueryString(dictionary);
        }

        public string SaveRecords(List<string> recordsDef)
        {
            var dictionary = new Dictionary<string, string>() { { "Service", "SaveRecords" } };
            dictionary["Version"] = "3.0";

            int index = 0;
            recordsDef.ForEach(record =>
            {
                dictionary[$"Record{index:d4}"] = record;
                index++;
            });

            dictionary["RecordCount"] = $"{index}";
            dictionary["AppInfo"] = "crmclient";
            return QueryString(dictionary);
        }
    }
}
