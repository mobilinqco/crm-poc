using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<TableInfo>))]
    public class TableInfo
    {
        [Key]
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public string RootInfoAreaId { get; set; }
        [JsonArrayIndex(3)]
        public string Name { get; set; }

        public int HasLookup { get; set; }

        [JsonArrayIndex(4)]
        public List<FieldInfo> Fields { get; set; }
        [JsonArrayIndex(5)]
        public List<LinkInfo> Links { get; set; }


        public TableInfo()
        {
        }

        public string DbStorageName()
        {
            return "CRM_" + InfoAreaId;
        }


        public string GetRootPhysicalInfoAreaId()
        {
            //if (!string.IsNullOrEmpty(RootInfoAreaId) && this.Database.IsUpdateCrm && Equals(RootInfoAreaId, "KP"))
            //{
            //    return "CP";
            //}

            return RootInfoAreaId;
        }

        public FieldInfo GetFieldInfo(int fieldId)
        { 
            foreach (FieldInfo field in Fields)
            {
                if(field.FieldId.Equals(fieldId))
                {
                    return field;
                }
            }

            return null;
        }

        public LinkInfo GetLinkInfo(string infoAreaId, int linkId)
        {
            LinkInfo infoAreaDefaultLink = null;
            LinkInfo reverseLink = null;

            foreach (LinkInfo link in Links)
            {
                if (link.TargetInfoAreaId.Equals(infoAreaId))
                {
                    if(link.LinkId == linkId)
                    {
                        return link;
                    }

                    if(linkId > 0 && linkId == link.ReverseLinkId)
                    {
                        reverseLink = link;
                    }

                    if(linkId <= 0)
                    {
                        if(link.LinkId <= 0)
                        {
                            return link;
                        }

                        if(infoAreaDefaultLink == null || (infoAreaDefaultLink.IsGenericLink() && !link.IsGenericLink()))
                        {
                            infoAreaDefaultLink = link;
                        }
                    }
                }
            }

            return reverseLink != null ? reverseLink : infoAreaDefaultLink;
        }
    }
}
