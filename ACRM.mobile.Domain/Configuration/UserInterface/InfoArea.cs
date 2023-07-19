using System;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<InfoArea>))]
    public class InfoArea : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        public string DefaultAction { get; set; }
        [JsonArrayIndex(2)]
        public string DefaultMenu { get; set; }
        [JsonArrayIndex(3)]
        public string ColorKey { get; set; }
        [JsonArrayIndex(4)]
        public string ImageName { get; set; }
        [JsonArrayIndex(5)]
        public string SingularName { get; set; }
        [JsonArrayIndex(6)]
        public string PluralName { get; set; }

        public InfoArea()
        {
        }

        public string ListPageTitle()
        {
            if (!string.IsNullOrWhiteSpace(PluralName))
            {
                return PluralName;
            }

            if(!string.IsNullOrWhiteSpace(SingularName))
            {
                return SingularName;
            }
            return string.Empty;
        }

        public string DetailsPageTitle()
        {
            if (!string.IsNullOrWhiteSpace(SingularName))
            {
                return SingularName;
            }
            return string.Empty;
        }

        public bool IsSameInfoArea(string infoArea)
        {
            if (UnitName.ToLower().CompareTo(infoArea.ToLower()) == 0)
            {
                return true;
            }

            return false;
        }

        public string PageAccentColor()
        {
            if (!string.IsNullOrWhiteSpace(ColorKey))
            {
                return ColorKey;
            }

            return "#E4E4E4";
        }
    }
}
