using System;
using System.ComponentModel.DataAnnotations;

namespace ACRM.mobile.Domain.OfflineSync
{
    public class RequestControl
    {
        [Key]
        public string RequestKey { get; set; }
        public int NextRequestNumber { get; set; }

        public RequestControl()
        {
        }
    }
}