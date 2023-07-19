using System;
using System.Collections.Generic;
using System.Text;

namespace ACRM.mobile.Domain.OfflineSync
{
    public class SyncInfo
    {
        public int Id { get; set; }
        public string DatasetName { get; set; }
        public int RecordCount { get; set; }
        public string FullSyncTimestamp { get; set; }
        public string SyncTimestamp { get; set; }
        public string InfoAreaId { get; set; }

        public SyncInfo()
        {
        }
    }
}
