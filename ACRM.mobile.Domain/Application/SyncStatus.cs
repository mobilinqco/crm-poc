using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.Domain.Application
{
    public class SyncStatus
    {
        public string LanguageCode { get; set; }

        public InitialSyncInfo UserInterfaceConfigurationSyncInfo { get; set; }
        public InitialSyncInfo DataModelConfigurationSyncInfo { get; set; }
        public InitialSyncInfo CatalogSyncInfo { get; set; }
        public InitialSyncInfo ResourcesSyncInfo { get; set; }
        public List<InitialSyncInfo> InfoAreasSyncInfo { get; set; }

        public SyncStatus()
        {
        }
    }
}
