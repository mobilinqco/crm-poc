using System;
using System.ComponentModel.DataAnnotations;
using ACRM.mobile.Domain.JsonUtils;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [Serializable]
    public class BaseConfigUnit
    {
        [Key]
        [JsonArrayIndex(0)]
        public string UnitName { get; set; }
        public BaseConfigUnit()
        {
            
        }
        public override bool Equals(object obj)
        {
            return ((BaseConfigUnit)obj).UnitName == UnitName;
        }
        public override int GetHashCode()
        {
            return UnitName.GetHashCode();
        }
    }
}
