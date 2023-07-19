using System;
using System.Collections.Generic;
using System.Data;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;

namespace ACRM.mobile.Domain.Application
{
    public class ListDisplayRow
    {
        public string RecordId;
        public string InfoAreaId;
        public string RawInfoAreaId;
        public List<ListDisplayField> Fields;
        public RowDecorators RowDecorators;
        public Dictionary<string, ListDisplayRow> LinkedDisplayRows;
        public bool IsRetrievedOnline;

        public ListDisplayField SectionGroupingValue = null;

        public ListDisplayRow()
        {
            Fields = new List<ListDisplayField>();
        }

        public string RawRecordId()
        {
            if(!string.IsNullOrWhiteSpace(RawInfoAreaId) && !RawInfoAreaId.Equals(InfoAreaId))
            {
                return RecordId.UnformatedRecordId().FormatedRecordId(RawInfoAreaId);
            }

            return RecordId.FormatedRecordId(InfoAreaId);
        }
    }
}
