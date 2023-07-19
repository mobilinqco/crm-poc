using System;
using System.Collections.Generic;

namespace ACRM.mobile.Domain.Application.SerialEntry
{
    public class QuotaArticleData
    {
        public string RecordIdentification { get; set; }
        public string InfoAreaId { get; set; }
        public Dictionary<string, string> FunctionKeyPairs { get; set; }
        public string ItemNumber { get; set; }
        public int Quota { get; set; }

        public QuotaArticleData()
        {
        }
    }
}
