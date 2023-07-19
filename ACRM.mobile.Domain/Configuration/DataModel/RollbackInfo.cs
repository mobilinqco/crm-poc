using System;
using System.ComponentModel.DataAnnotations;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    public class RollbackInfo
    {
        [Key]
        public int RequestNr { get; set; }
        public string InfoAreaId { get; set; }
        public string RecordId { get; set; }
        public string Info { get; set; }

        public RollbackInfo()
        {
        }
    }
}
