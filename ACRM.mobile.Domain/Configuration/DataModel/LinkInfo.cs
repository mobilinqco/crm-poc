using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    enum LinkType
    {
        Unknown,
        Ident,
        Parent,
        Child,
        Generic,
        OneToOne,
        OneToMany,
        ManyToOne,
        NoLink,
        Identity
    }

    [JsonConverter(typeof(JsonArrayToObjectConverter<LinkInfo>))]
    public class LinkInfo
    {
        public int Id { get; set; }

        [JsonArrayIndex(0)]
        public string TargetInfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public int LinkId { get; set; }
        [JsonArrayIndex(2)]
        public int ReverseLinkId { get; set; }
        [JsonArrayIndex(3)]
        public string RelationType { get; set; }

        [JsonArrayIndex(4)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int SourceFieldId { get; set; }

        [JsonArrayIndex(5)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int DestFieldId { get; set; }

        [JsonArrayIndex(6)]
        public List<LinkFields> LinkFields { get; set; }

        [JsonArrayIndex(7)]
        public bool UseLinkFields { get; set; }

        public LinkInfo()
        {
            LinkId = 126;
            DestFieldId = -1;
            SourceFieldId = -1;
        }

        public bool IsGenericLink()
        {
            return (LinkId == 126) || (LinkId == 127);
        }

        public bool HasColumn()
        {
            if(UseLinkFields)
            {
                return false;
            }

            if (SourceFieldId >= 0 && DestFieldId >= 0)
            {
                return false;
            }

            if (LinkId == 126)
            {
                return true;
            }

            if (LinkId == 127)
            {
                return false;
            }

            LinkType lt;
            if (Enum.TryParse<LinkType>(RelationType, true, out lt))
            {
                return (lt == LinkType.ManyToOne)
                    || (lt == LinkType.OneToOne)
                    || (lt == LinkType.Parent);
            }

            return false;
        }

        public bool HasLinkFields()
        {
            return LinkFields != null && LinkFields.Count > 0;
        }

        public bool HasDestSrcFields()
        {
            return DestFieldId > -1 && SourceFieldId > -1;
        }

        public string GetReverseLinkColumnName(string infoAreaId)
        {
            return "LINK_" + infoAreaId + "_" + ReverseLinkId.ToString();
        }

        public string GetColumnName()
        {
            if (LinkId == 126 || LinkId == 127)
            {
                return "LINK_RECORDID";
            }

            return "LINK_" + TargetInfoAreaId + "_" + LinkId.ToString();
        }

        public string GetColumnName(string infoAreaId)
        {
            return "LINK_" + infoAreaId + "_" + LinkId.ToString();
        }

        public string GetInfoAreaColumnName()
        {
            if (LinkId == 126 || LinkId == 127)
            {
                return "LINK_INFOAREA";
            }

            return "";
        }

        public string GetDestField()
        {
            return "F" + DestFieldId;
        }

        public string GetSoruceField()
        {
            return "F" + SourceFieldId;
        }

        public LinkFields FirstField()
        {
            if(LinkFields.Count > 0)
            {
                return LinkFields.OrderBy(lf => lf.SourceFieldId).ToList()[0];
            }

            return null;
        }

        public LinkFields SecondField()
        {
            if (LinkFields.Count > 1)
            {
                return LinkFields.OrderBy(lf => lf.SourceFieldId).ToList()[1];
            }

            return null;
        }

        public LinkFields LinkFieldWithTargetFieldIndex(int targetFieldIndex)
        {
            if(LinkFields != null)
            {
                foreach(LinkFields field in LinkFields)
                {
                    if(field.DestFieldId == targetFieldIndex)
                    {
                        return field;
                    }
                }
            }

            return null;
        }
    }
}
