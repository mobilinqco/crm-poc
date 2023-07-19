using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.SerialEntry
{
    public class ListingOwner
    {
        public string RecordIdentification { get; set; }
        public string InfoAreaId { get; set; }
        public Dictionary<string, string> SearchKeyPairs { get; set; }
        public bool IsRelatedParent { get; set; } = false;
        public ListingOwner()
        {
        }
    }
}
