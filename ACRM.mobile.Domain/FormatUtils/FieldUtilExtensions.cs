using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.Configuration.UserInterface;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.FormatUtils
{
    public static class FieldUtilExtensions
    {
        public static string ExtendedOptionForKey(this FieldControlField _field, string key)
        {
            var attribute = _field?.Attributes.Where(x => x.AttributeType.Equals((int)FieldAttributeType.ExtendedOptions)).FirstOrDefault();
            if(attribute!=null)
            {
                string strValue = attribute.Value;
                if (!string.IsNullOrEmpty(strValue))
                {
                    try
                    {
                        Dictionary<string, string> extendedOptionValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(strValue);
                        if (extendedOptionValues.ContainsKey(key))
                        {
                            return extendedOptionValues[key];
                        }
                    }
                    catch (Exception error)
                    {

                    }
                }
            }
            return null;
        }

        public static Dictionary<string, string> ExtendedOptionData(this FieldControlField _field)
        {
            var attribute = _field?.Attributes.Where(x => x.AttributeType.Equals(FieldAttributeType.ExtendedOptions)).FirstOrDefault();
            if (attribute != null)
            {
                string strValue = attribute.Value;
                if (!string.IsNullOrEmpty(strValue))
                {
                    try
                    {
                        Dictionary<string, string> extendedOptionValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(strValue);
                        return extendedOptionValues;
                    }
                    catch (Exception error)
                    {

                    }
                }
            }
            return null;
        }
    }
}
