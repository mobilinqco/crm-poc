using System;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<AnalysisField>))]
    public class AnalysisField
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int FieldId { get; set; }
        [JsonArrayIndex(1)]
        public int Flags { get; set; }
        [JsonArrayIndex(2)]
        public string DefaultValue { get; set; }
        [JsonArrayIndex(3)]
        public string DefaultEnd { get; set; }
        [JsonArrayIndex(4)]
        public string CategoryName { get; set; }
        [JsonArrayIndex(5)]
        public int ListColNr { get; set; }
        [JsonArrayIndex(6)]
        public int ListWidth { get; set; }
        [JsonArrayIndex(7)]
        public string Options { get; set; }
        [JsonArrayIndex(8)]
        public int Slices { get; set; }

        //public AnalysisTable AnalysisTable { get; set; }

        public AnalysisField()
        {
        }
    }
}
