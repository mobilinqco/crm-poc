using System;
using System.Collections.Generic;

namespace ACRM.mobile.Services.Contracts
{
    public interface ICrmRequestBuilder
    {
        public string SyncConfigurationQuery(bool includeCatalogs = true,
           bool includeSyncConfiguration = true,
           bool includeSyncDataModel = true);

        public string SyncCatalogsQuery();
        public string SyncService();
        public string DataSetQuery(string dataSetName);
        public string IncrementalDataSetQuery(string dataSetName, string timeSinceLastSync);

        public string RecordDataQuery(object queryDef,
            string recordIdentification,
            string linkRecordIdentification,
            int linkId,
            int maxResults);

        string SaveRecords(List<string> recordsDef);
    }

}
