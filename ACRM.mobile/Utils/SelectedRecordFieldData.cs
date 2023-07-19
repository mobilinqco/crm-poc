using System;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Utils
{
    public class SelectedRecordFieldData
    {
        public string StringValue { get; set; }
        public SelectableFieldValue SelectedValue { get; set; }
        public string InfoAreaId { get; set; }
        public ListDisplayRow SelectedField { get; set; }
        public int ParentCode { get; set; } = -1;

        public SelectedRecordFieldData()
        {
        }
    }
}
