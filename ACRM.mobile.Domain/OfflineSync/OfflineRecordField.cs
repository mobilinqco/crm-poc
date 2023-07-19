using Newtonsoft.Json;

namespace ACRM.mobile.Domain.OfflineSync
{
    public class OfflineRecordField
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int Offline { get; set; }

        [JsonIgnore]
        public OfflineRecord OfflineRecord { get; set; }

        public OfflineRecordField()
        {
        }
    }
}