using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FieldControl>))]
    public class FieldControl : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(2)]
        public string ControlName { get; set; }
        [JsonArrayIndex(3)]
        public List<FieldControlTab> Tabs { get; set; }
        [JsonArrayIndex(4)]
        public List<FieldControlSortField> SortFields { get; set; }
        [JsonArrayIndex(5)]
        public List<FieldControlAttribute> Attributes { get; set; }


        public FieldControl()
        {
        }

        public FieldControl FieldControlWithSingleTab(int parentTabIndex)
        {
            if (Tabs.Count > parentTabIndex)
            {
                var fieldControl = new FieldControl();

                fieldControl.Tabs = new List<FieldControlTab> { Tabs[parentTabIndex] };
                fieldControl.InfoAreaId = this.InfoAreaId;
                fieldControl.ControlName = this.ControlName;
                fieldControl.SortFields = this.SortFields;
                fieldControl.Attributes = this.Attributes;
                fieldControl.UnitName = this.UnitName;

                return fieldControl;
            }

            return this;
        }

        public string LabelTextForFunctionName(string functionName)
        {
            foreach(FieldControlTab tab in Tabs)
            {
                foreach(FieldControlField field in tab.Fields)
                {
                    if(!string.IsNullOrWhiteSpace(field.Function) && field.Function.Equals(functionName))
                    {
                        return field.ExplicitLabel;
                    }
                }
            }

            return string.Empty;
        }

    }
}
