using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.SerialEntry
{
    public class ListingItems
    {
        public string RecordIdentification { get; set; }
        public Dictionary<string, string> SearchKeyPairs { get; set; }
        public ListingItems()
        {
        }
    }
}
