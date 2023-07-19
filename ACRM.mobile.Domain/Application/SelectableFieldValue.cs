using System;
namespace ACRM.mobile.Domain.Application
{
    public class SelectableFieldValue
    {
        public string RecordId { get; set; }
        public string DisplayValue { get; set; }
        public string IconValue { get; set; }
        public int Id { get; set; } = -1;
        public int ParentCode { get; set; } = -1;
        public string ExtKey { get; set; }
    }
}
