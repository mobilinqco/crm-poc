using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FieldControlTab>))]
    public class FieldControlTab
    {
        // We need this because the Entity Framework Core dose not guarantee that the
        // primary key (id) will not be generated in the order we have added the data to the
        // list. And for the FieldControls we need to have a way to preserve the order of the fields
        // from the designer configuration.
        static int DesignerOrderId = 0;

        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Label { get; set; }
        [JsonArrayIndex(1)]
        public string Type { get; set; }
        [JsonArrayIndex(2)]
        public List<FieldControlField> Fields { get; set; }
        [JsonArrayIndex(3)]
        public List<FieldControlTabAttribute> Attributes { get; set; }

        public FieldControl FieldControl { get; set; }

        [JsonIgnore]
        public int OrderId { get; set; }

        public FieldControlTab()
        {
            OrderId = DesignerOrderId++;
        }

        public static void ResetDesignerOrderId()
        {
            DesignerOrderId = 0;
        }

        public PanelType GetEditPanelType()
        {
            if (string.IsNullOrEmpty(Type))
            {
                return PanelType.EditPanel;
            }

            if (Type.ToLower().StartsWith("edit"))
            {
                return PanelType.EditPanel;
            }

            // CRM.pad ignores the grid type and process it as edit
            if (Type.ToLower().StartsWith("grid"))
            {
                return PanelType.EditPanel;
            }

            if (Type.ToLower().StartsWith("repitition"))
            {
                return PanelType.EditPanel;
            }

            if (Type.ToLower().StartsWith("repparticipants") || Type.ToLower().StartsWith("participants") )
            {
                return PanelType.Repparticipant;
            }

            if (Type.ToLower().StartsWith("linkparticipants"))
            {
                return PanelType.Linkparticipant;
            }

            // This is for the crazy CC configuration
            if (Type.ToLower().StartsWith("children"))
            {
                return PanelType.EditPanelChildren;
            }

            return PanelType.NotSupported;
        }

        public PanelType GetPanelType()
        {
            if (string.IsNullOrEmpty(Type))
            {
                return PanelType.List;
            }

            if (Type.ToLower().StartsWith("edit"))
            {
                return PanelType.EditPanel;
            }

            if (Type.ToLower().Equals("organizerheadersublabel"))
            {
                return PanelType.OrganizerHeaderSubLabel;
            }

            if (Type.ToLower().Equals("grid"))
            {
                return PanelType.Grid;
            }

            if (Type.ToLower().Equals("grid"))
            {
                return PanelType.Grid;
            }

            if (Type.ToLower().Equals("map"))
            {
                return PanelType.Map;
            }
            if (Type.ToLower().StartsWith("participants"))
            {
                return PanelType.Participants;
            }

            if (Type.ToLower().StartsWith("parent"))
            {
                return PanelType.Parent;
            }

            if (Type.ToLower().StartsWith("children"))
            {
                return PanelType.Children;
            }

            if (Type.ToLower().StartsWith("webcontent"))
            {
                return PanelType.WebView;
            }

            if (Type.ToLower().StartsWith("doc"))
            {
                return PanelType.Doc;
            }

            if (Type.ToLower().StartsWith("insightboard"))
            {
                return PanelType.Insightboard;
            }

            if (Type.ToLower().StartsWith("characteristicscontext"))
            {
                return PanelType.Characteristics;
            }

            if(Type.ToLower().StartsWith("contacttimes"))
            {
                return PanelType.ContactTimes;
            }

            return PanelType.NotSupported;
        }

        public bool IsSupported()
        {
            return GetPanelType() != PanelType.NotSupported;
        }

        public bool IsHeaderPanel()
        {
            return GetPanelType() == PanelType.OrganizerHeaderSubLabel;
        }

        public List<FieldControlField> GetQueryFields()
        {
            List<FieldControlField> fields = new List<FieldControlField>();
            fields.AddRange(Fields.OrderBy(f => f.OrderId));
            return fields;
        }

        public string TabFieldsInfoAreaId()
        {
            if(Fields != null && Fields.Count > 0)
            {
                return Fields[0].InfoAreaId;
            }
            return string.Empty;
        }

    }
}
