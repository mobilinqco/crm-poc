using System.Collections.Generic;

namespace ACRM.mobile.Domain.Configuration
{
    public class ConfigSyncData
    {
        public int Hour { get; set; }
        public IList<float> Hours { get; set; } = new List<float>();
        public int Weekday { get; set; }
        public IList<int> Weekdays { get; set; } = new List<int>();
        public string Timezone { get; set; }
        public IList<ConfigSyncData> Alternates { get; set; } = new List<ConfigSyncData>();
    }
}
