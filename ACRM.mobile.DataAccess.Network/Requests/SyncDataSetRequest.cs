using System;
namespace ACRM.mobile.DataAccess.Network.Requests
{
    public class SyncDataSetRequest
    {
        public string DataSetName0 { get; set; }
        public int DataSetNameCount { get; set; }
        public string Service { get; set; }
        public bool SyncRecordData { get; set; }

        public SyncDataSetRequest()
        {
        }
    }
}
