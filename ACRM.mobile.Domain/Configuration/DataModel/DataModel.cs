using System;
using System.ComponentModel.DataAnnotations;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    public class DataModel
    {
        [Key]
        public string Version { get; set; }
        public string Timezone { get; set; }
        public int UTCTimeOffset { get; set; }

        public DataModel()
        {
        }
    }
}
