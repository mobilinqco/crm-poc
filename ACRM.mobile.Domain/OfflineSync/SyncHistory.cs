using System;
namespace ACRM.mobile.Domain.OfflineSync
{
    public class SyncHistory
    {
        public int Id { get; set; }
        public string StartDate { get; set; }
        public string SyncType { get; set; }
        public string Details { get; set; }
        public double RunTimeServer { get; set; }
        public double RunTimeUnpacking { get; set; }
        public double RunTimeLocalStorage { get; set; }
        public int RecordCount { get; set; }
        public string ErrorText { get; set; }
        public string ErrorDetails { get; set; }

        public SyncHistory()
        {
        }
    }
}