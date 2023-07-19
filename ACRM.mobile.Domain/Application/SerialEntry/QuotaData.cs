using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.SerialEntry
{
    public class QuotaData
    {
        public string RecordIdentification { get; set; }
        public string InfoAreaId { get; set; }
        public Dictionary<string, string> FunctionKeyPairs { get; set; }
        public string ItemNumber { get; set; }
        public int QuantityIssued { get; set; }
        public string EndOfPeriod { get; set; }
        public string Year { get; set; }
        public string StartOfPeriod { get; set; }
        public bool Allocated { get; set; } = false;
        public QuotaData()
        {
        }
    }
}
