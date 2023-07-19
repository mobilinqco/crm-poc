using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.ActionTemplates
{
    public class RecordListViewTemplate : ActionTemplateBase
    {

        public RecordListViewTemplate(ViewReference viewReference) : base(viewReference)
        {

        }

        // If set to true, the user can scan items with a barcode scanner. Default value: false.
        public bool ScanMode()
        {
            string strVal = GetValue("ScanMode");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }
            
            return false;
        }

        public bool Sections()
        {
            string strVal = GetValue("Sections");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }

            return false;
        }

        public string MaxResults()
        {
            return GetValue("MaxResults");
        }

        public override RequestMode GetRequestMode()
        {
            //  The Request mode and probably some other parameters of the RecordListView can be configured
            //  as part of the header definition by specifing the values in SubListParams parameter.
            //  Unfortunatelly for this case the server returns the value of the SubListParameter as a view reference argument
            //  and is doing this by setting the Name parameter with the content of the SubListParameter
            //  Example: "{\"RequestMode\": \"Online\"}", "Value", "true"
            if (viewReferenceModel != null)
            {
                Dictionary<string, object> extraData = viewReferenceModel.GetSubListParams();
                if(extraData != null)
                {
                    string requestModeStringValue = string.IsNullOrWhiteSpace(GetParamStringValue(extraData, "RequestOption"))
                        ? GetParamStringValue(extraData, "RequestMode")
                        : GetParamStringValue(extraData, "RequestOption");

                    RequestMode reqMode;
                    if (Enum.TryParse(requestModeStringValue, out reqMode))
                    {
                        return reqMode;
                    }
                }
                return viewReferenceModel.GetRequestMode();
            }

            return RequestMode.Best;
        }

        private string GetParamStringValue(Dictionary<string, object> data, string key)
        {
            if (data.ContainsKey(key) && data[key] is string value && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            return string.Empty;
        }
    }
}
