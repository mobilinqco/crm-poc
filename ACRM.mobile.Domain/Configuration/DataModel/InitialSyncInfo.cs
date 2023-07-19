using System;
using System.ComponentModel.DataAnnotations;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    public class InitialSyncInfo
    {
        [Key]
        public string DataSetName { get; set; }
        public int RecordCount { get; set; }
        public string FullSyncTimestamp { get; set; }
        public string SyncTimestamp { get; set; }
        public string InfoAreaId { get; set; }
        public string UnitName { get; set; }

        public InitialSyncInfo()
        {
        }
    }
}
