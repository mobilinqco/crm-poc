using ACRM.mobile.Domain.Application;
using Newtonsoft.Json;
using System;

namespace ACRM.mobile.Domain.OfflineSync
{
    public class OfflineRecordLink
    {
        public int Id { get; set; }

        public string InfoAreaId { get; set; }
        public int FieldId { get; set; }
        public int LinkId { get; set; }
        public string RecordId { get; set; }

        [JsonIgnore]
        public OfflineRecord OfflineRecord { get; set; }

        public OfflineRecordLink()
        {
        }

        public bool HasLinkIdEqualWith(int linkId)
        {
            if(linkId <= 0)
            {
                if(LinkId <= 0)
                {
                    return true;
                }
            }

            return LinkId == linkId;
        }

        public bool IsSameInfoArea(string infoArea)
        {
            if (string.Equals(infoArea, this.InfoAreaId))
            {
                return true;
            }

            string compareVirtual = "V" + infoArea;

            if (!string.IsNullOrWhiteSpace(this.InfoAreaId) && this.InfoAreaId.StartsWith(compareVirtual))
            {
                return true;
            }

            compareVirtual = "V" + this.InfoAreaId;

            if (!string.IsNullOrWhiteSpace(infoArea) && infoArea.StartsWith(compareVirtual))
            {
                return true;
            }

            return false;
        }

        public OfflineRecordLink Clone()
        {
            return new OfflineRecordLink() { LinkId = this.LinkId, FieldId = this.FieldId, InfoAreaId = this.InfoAreaId, RecordId = this.RecordId };
        }
    }
}