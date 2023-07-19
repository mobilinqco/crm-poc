using System;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    public enum FieldAttributeType
    {
        ReadOnly = 1,
        Bold = 2,
        Italic = 3,
        Hyperlink = 5,
        Email = 6,
        DontSave = 7,
        Hide = 10,
        Color = 11,
        Password = 12,
        Must = 13,
        NoLabel = 18,
        Empty = 19,
        MultiLine = 25,
        NoMultiLine = 26,
        LabelBold = 28,
        LabelItalic = 29,
        LabelColor = 31,
        SelectFunction = 32,
        Phone = 33,
        ColSpan = 40,
        FieldStyle = 47,
        PlaceHolder = 52,
        Image = 53,
        ExtendedOptions = 60,
        RenderHook = 62,
        DontCacheOffline = 72,
        RowSpan = 81,
    }

    [JsonConverter(typeof(JsonArrayToObjectConverter<FieldAttribute>))]
    public class FieldAttribute
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public int AttributeType { get; set; }
        [JsonArrayIndex(1)]
        public int EditMode { get; set; }
        [JsonArrayIndex(2)]
        public string Value { get; set; }
        

        public FieldAttribute()
        {
        }

        public bool IsImageAttribute()
        {
            return AttributeType == (int)FieldAttributeType.Image;
        }
    }
}
