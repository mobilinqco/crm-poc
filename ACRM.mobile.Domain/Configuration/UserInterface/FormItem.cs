using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FormItem>))]
    public class FormItem
    {
        static int DesignerOrderId = 0;
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string ControlName { get; set; }
        [JsonArrayIndex(1)]
        public ViewReference ViewReference { get; set; }
        [JsonArrayIndex(2)]
        public string Label { get; set; }
        [JsonArrayIndex(3)]
        public string Options { get; set; }               // cell type dependent dictionary with control information
        [JsonArrayIndex(4)]
        public string CellAttributes { get; set; }        // cell type independent dictionary with display information
        [JsonArrayIndex(5)]
        public string ItemAttributes { get; set; }        // cell type dependent dictionary with display information
        [JsonArrayIndex(6)]
        public string ValueName { get; set; }
        [JsonArrayIndex(7)]
        public string Func { get; set; }
        [JsonIgnore]
        public int OrderId { get; set; }

        public FormItem()
        {
            OrderId = DesignerOrderId++;
        }

        public Dictionary<string, object> OptionsDictionary()
        {
            if(string.IsNullOrWhiteSpace(Options))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(Options);
            }
            catch
            {
            }

            return null;
        }

        public Dictionary<string, object> CellAttributesDictionary()
        {
            if (string.IsNullOrWhiteSpace(CellAttributes))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(CellAttributes);
            }
            catch
            {
            }

            return null;
        }

        public Dictionary<string, object> ItemAttributesDictionary()
        {
            if (string.IsNullOrWhiteSpace(ItemAttributes))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(ItemAttributes);
            }
            catch
            {
            }

            return null;
        }
    }
}
