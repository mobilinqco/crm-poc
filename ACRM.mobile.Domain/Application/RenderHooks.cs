using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Application
{
    public class RenderHooks
    {
        private readonly Dictionary<string, string> RenderHooksValues;

        public RenderHooks(string value)
        {
            try
            {
                RenderHooksValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
            }
            catch (Exception error)
            {
                
            }
            RenderHooksValues = new Dictionary<string, string>();
        }

        public bool PercentField()
        {
            if (RenderHooksValues != null
                && RenderHooksValues.ContainsKey("PercentField"))
            {
                if(RenderHooksValues["PercentField"].ToLower() == "true"
                    || RenderHooksValues["PercentField"].ToLower() == "1")
                {
                    return true;
                }
            }
            return false;
        }

        public int DecimalDigits()
        {
            if (RenderHooksValues != null
                && RenderHooksValues.ContainsKey("DecimalDigits"))
            {
                return int.Parse(RenderHooksValues["PercentField"]);
            }
            
            return -1;
        }

        public char FieldType()
        {
            if (RenderHooksValues != null
                && RenderHooksValues.ContainsKey("FieldType")
                && RenderHooksValues["FieldType"].Length > 0)
            {
                return RenderHooksValues["FieldType"][0];
            }

            return ' ';
        }

        public bool GroupingSeparator()
        {
            if (RenderHooksValues != null
                && RenderHooksValues.ContainsKey("GroupingSeparator"))
            {
                if (RenderHooksValues["GroupingSeparator"].ToLower() == "false"
                    || RenderHooksValues["GroupingSeparator"].ToLower() == "0")
                {
                    return false;
                }
            }
            return true;
        }
    }
}
