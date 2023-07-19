using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FieldControlField>))]
    public class FieldControlField: ICloneable
    {
        // We need this because the Entity Framework Core dose not guarantee that the
        // primary key (id) will not be generated in the order we have added the data to the
        // list. And for the FieldControls we need to have a way to preserve the order of the fields
        // from the designer configuration.
        static int DesignerOrderId = 0;

        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public int FieldId { get; set; }
        [JsonArrayIndex(2)]
        public List<FieldAttribute> Attributes { get; set; }
        [JsonArrayIndex(3)]
        public int TargetFieldNumber { get; set; }
        [JsonArrayIndex(4)]
        public string ExplicitLabel { get; set; }
        [JsonArrayIndex(6)]
        public string Function { get; set; }
        [JsonArrayIndex(7)]
        public int LinkId { get; set; }

        [JsonIgnore]
        public int OrderId { get; set; }

        public FieldControlTab TabConfig { get; set; }

        public FieldControlField()
        {
            OrderId = DesignerOrderId++;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string QueryFieldName(bool isLinkField)
        {
            if (isLinkField)
            {
                int linkId = LinkId < 0 ? 0 : LinkId;
                return InfoAreaId + "_" + linkId + "_" + FieldId;
            }

            return "F" + FieldId;
        }

        public string QueryLinkedRecordIdName()
        {
            int linkId = LinkId < 0 ? 0 : LinkId;
            return InfoAreaId + "_" + linkId + "_recid";
        }

        public string FieldIdentification()
        {
            if(LinkId > 0)
            {
                return InfoAreaId + ":" + LinkId + "." + FieldId;
            }

            return InfoAreaId + "." + FieldId;
        }

        public static void ResetDesignerOrderId()
        {
            DesignerOrderId = 0;
        }

        public override string ToString()
        {
            return base.ToString() + ": "
                + " Id = " + Id
                + " InfoAreaId = " + InfoAreaId
                + " FieldId = " + FieldId
                + " TargetFieldNumber = " + TargetFieldNumber
                + " ExplicitLabel = " + ExplicitLabel
                + " Function = " + Function
                + " LinkId = " + LinkId
                + " OrderId = " + OrderId
                + " Attributes = " + Attributes;
        }

        public bool HasImageAttribute()
        {

            if(Attributes != null)
            {
                foreach(FieldAttribute attribute in Attributes)
                {
                    if (attribute.IsImageAttribute())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static FieldControlField GetFieldControl(FieldInfo fieldInfo)
        {
            var field =  new FieldControlField();
            field.FieldId = fieldInfo.FieldId;
            field.InfoAreaId = fieldInfo.TableInfoInfoAreaId;
            field.Id = fieldInfo.Id;
            return field;
        }   
    }
}
