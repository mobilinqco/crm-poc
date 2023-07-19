using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<AnalysisTable>))]
    public class AnalysisTable
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(1)]
        public int Occurrence { get; set; }
        [JsonArrayIndex(2)]
        public int TableNumber { get; set; }
        [JsonArrayIndex(3)]
        public List<AnalysisField> Fields { get; set; }

        public AnalysisTable()
        {
        }
    }
}