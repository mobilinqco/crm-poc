using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<TreeViewTable>))]
    public class TreeViewTable
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int Nr { get; set; }
        [JsonArrayIndex(2)]
        public string InfoAreaId { get; set; }
        [JsonArrayIndex(3)]
        public int LinkId { get; set; }
        [JsonArrayIndex(4)]
        public string RelationName { get; set; }
        [JsonArrayIndex(5)]
        public string SearchAndListName { get; set; }
        [JsonArrayIndex(6)]
        public string ExpandName { get; set; }
        [JsonArrayIndex(7)]
        public string TableCaptionName { get; set; }
        [JsonArrayIndex(8)]
        public string RootMenuLabel { get; set; }
        [JsonArrayIndex(9)]
        public string MenuLabel { get; set; }
        [JsonArrayIndex(10)]
        public int Flags { get; set; }
        [JsonArrayIndex(11)]
        public string FilterName { get; set; }
        [JsonArrayIndex(12)]
        public string RecordCustomControl { get; set; }
        [JsonArrayIndex(13)]
        public string InfoAreaCustomControl { get; set; }
        [JsonArrayIndex(14)]
        public int RecordCount { get; set; }
        [JsonArrayIndex(15)]
        public string Label { get; set; }

        public TreeViewTable()
        {
        }
    }
}
