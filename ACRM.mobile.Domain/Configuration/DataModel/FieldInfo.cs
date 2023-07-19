using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<FieldInfo>))]
    public class FieldInfo
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int FieldId { get; set; }
        [JsonArrayIndex(1)]
        public string Name { get; set; }
        [JsonArrayIndex(2)]
        public char FieldType { get; set; }
        [JsonArrayIndex(3)]
        public int Cat { get; set; }
        [JsonArrayIndex(4)]
        public int Ucat { get; set; }
        [JsonArrayIndex(5)]
        public int FieldLen { get; set; }
        [JsonArrayIndex(6)]
        public int Attributes { get; set; }
        [JsonArrayIndex(7)]
        public string RepMode { get; set; }
        [JsonArrayIndex(8)]
        public int Rights { get; set; }
        [JsonArrayIndex(9)]
        public int Format { get; set; }
        [JsonArrayIndex(10)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string ArrayFieldIndices { get; set; }

        public string TableInfoInfoAreaId { get; set; }
        public TableInfo TableInfo { get; set; }

        public FieldInfo()
        {
        }

        public string FieldDbName() => "F" + FieldId.ToString();
      
        public int CatalogId()
        {
            return Cat > 0 ? Cat : Ucat;
        }

        public bool IsNumeric => "FNLS".IndexOf(FieldType) > -1;
        public bool IsBoolean => "B".IndexOf(FieldType) > -1;
        public bool IsReal => "F".IndexOf(FieldType) > -1;
        public bool IsCatalog => "XK".IndexOf(FieldType) > -1;
        public bool IsVariableCatalog => "K".IndexOf(FieldType) > -1;
        public bool IsDate => "D".IndexOf(FieldType) > -1;
        public bool IsTime => "T".IndexOf(FieldType) > -1;
        public bool IsHtml => Attributes == 0x40;
        public bool IsParticipant => !string.IsNullOrEmpty(RepMode) && FieldType == 'C';
        public bool IsPercent => (Format & 8) != 0;
        public bool LeadingZeros => (Format & 128) != 0;
        public bool HasGroupingSeparator => (Format & 1) != 0;
        public bool ShowZero => (Format & 32) != 0;
        public bool OneDecimalDigit => (Format & 2) != 0;
        public bool NoDecimalDigits => (Format & 4) != 0;
        public bool ThreeDecimalDigits => (Format & 64) != 0;
        public bool FourDecimalDigits => (Format & 512) != 0;
        public bool FiveDecimalDigits => (Format & 1024) != 0;
        public bool SixDecimalDigits => (Format & 1048576) != 0;
        public bool SevenDecimalDigits => (Format & 4096) != 0;
        public bool IsAmount => (Format & 0x100) != 0;
        
    }
}
