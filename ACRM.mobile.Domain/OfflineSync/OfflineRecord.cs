using Newtonsoft.Json;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.OfflineSync
{
    public class OfflineRecord
    {
        public int Id { get; set; }
        public string InfoAreaId { get; set; }
        public string RecordId { get; set; }
        public string Mode { get; set; }
        public string Options { get; set; }

        [JsonIgnore]
        public OfflineRequest OfflineRequest { get; set; }

        public List<OfflineRecordField> RecordFields { get; set; }
        public List<OfflineRecordLink> RecordLinks { get; set; }

        public OfflineRecord()
        {
        }
    }
}