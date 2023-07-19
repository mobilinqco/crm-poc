using System;
using ACRM.mobile.Domain.FormatUtils;

namespace ACRM.mobile.Domain.Application
{
    public class ParentLink
    {
        public string FormatedRecordId
        {
            get
            {
                if(!string.IsNullOrWhiteSpace(RecordId))
                {
                    return RecordId.FormatedRecordId(ParentInfoAreaId);
                }

                return RecordId;
            }
        }

        public string ParentInfoAreaId { get; set; }
        public int LinkId { get; set; }

        private string _recordId;
        public string RecordId
        {
            get
            {
                return _recordId;
            }

            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _recordId = value.UnformatedRecordId();
                }
                else
                {
                    _recordId = value;
                }
            }
        }

        public bool IsNewRecord()
        {
            if (string.IsNullOrWhiteSpace(RecordId) || RecordId.Contains("new"))
            {
                return true;
            }

            return false;
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(ParentInfoAreaId) || string.IsNullOrWhiteSpace(RecordId))
            {
                return false;
            }

            return true;
        }
    }
}
