using System;
using System.Collections.Generic;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<ViewReference>))]
    public class ViewReference
    {
        public int Id { get; set; }
        [JsonArrayIndex(0)]
        public string Name { get; set; }
        [JsonArrayIndex(1)]
        public string ViewName { get; set; }
        [JsonArrayIndex(2)]
        public List<ReferenceArgument> Arguments { get; set; }

        public ViewReference()
        {
        }

        public string ToJsonString()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["viewName"] = ViewName;

            if (Arguments != null)
            {
                Arguments.ForEach(argument =>
                {
                    data[argument.Name] = argument.Value;
                });
            }

            return JsonConvert.SerializeObject(data);
        }

        public string GetArgumentValue(string argumentName)
        {
            if (Arguments != null)
            {
                ReferenceArgument arg = Arguments.Find(argument => argument.Name == argumentName);

                if (arg != null)
                {
                    return arg.Value;
                }
            }

            return String.Empty;
        }

        public List<string> GetArrayArgumentValue(string argumentName)
        {
            if (Arguments != null)
            {
                ReferenceArgument arg = Arguments.Find(argument => argument.Name == argumentName);

                if (arg != null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<List<string>>(arg.Value);
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public Dictionary<string, object> GetDictionaryArgumentValue(string argumentName)
        {
            if (Arguments != null)
            {
                ReferenceArgument arg = Arguments.Find(argument => argument.Name == argumentName);

                if (arg != null && !string.IsNullOrWhiteSpace(arg.Value))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<Dictionary<string, object>>(arg.Value);
                    }
                    catch(Exception ex)
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public bool IsOpenUrlAction()
        {
            if (!string.IsNullOrEmpty(Name) && Name.ToLower().Equals("openurl"))
            {
                return true;
            }

            return false;
        }

        public bool IsOrganizerAction()
        {
            if (!string.IsNullOrEmpty(ViewName) && ViewName.ToLower().Equals("organizeraction"))
            {
                return true;
            }

            return false;
        }

        public bool IsDeleteAction()
        {
            if (!IsOrganizerAction())
            {
                return false;
            }

            string action = GetArgumentValue("Action");
            if(!string.IsNullOrEmpty(action) && action.ToLower().Equals("deleterecord"))
            {
                return true;
            }

            return false;
        }

        public bool IsFileUploadAction()
        {
            if (!string.IsNullOrEmpty(ViewName) && ViewName.ToLower().Equals("fileuploadaction"))
            {
                return true;
            }

            return false;
        }

        public bool IsImageUploadAction()
        {
            if (!string.IsNullOrEmpty(ViewName) && ViewName.ToLower().Equals("photouploadaction"))
            {
                return true;
            }

            return false;
        }

        public bool IsDocmentUploadAction()
        {
            if (IsFileUploadAction() || IsImageUploadAction())
            {
                return true;
            }

            return false;
        }

        public bool IsNewInBackgroundAction()
        {
            if (!string.IsNullOrEmpty(ViewName) && ViewName.ToLower().Equals("action:newinbackground"))
            {
                return true;
            }

            return false;
        }

        public bool IsCharacteristicsEditAction()
        {
            if (!string.IsNullOrEmpty(ViewName) && ViewName.ToLower().Equals("characteristicseditview"))
            {
                return true;
            }

            return false;
        }

        public bool IsContactTimesEditAction()
        {
            if (!string.IsNullOrEmpty(ViewName) && ViewName.ToLower().Equals("contacttimeseditview"))
            {
                return true;
            }

            return false;
        }

        public bool SerialEntryAction()
        {
            if (!string.IsNullOrEmpty(ViewName) && ViewName.ToLower().Equals("serialentry"))
            {
                return true;
            }

            return false;
        }

        public bool IsQuestionnaireEditAction()
        {
            if (!string.IsNullOrEmpty(Name) && Name.ToLower().Equals("questionnaireeditview"))
            {
                return true;
            }

            return false;
        }


        public bool IsModifyRecordAction()
        {
            if (!string.IsNullOrEmpty(Name) && Name.ToLower().Equals("modifyrecord"))
            {
                return true;
            }

            return false;
        }

        public bool IsSyncRecordAction()
        {
            if (!string.IsNullOrEmpty(Name) && Name.ToLower().Equals("syncrecord"))
            {
                return true;
            }

            return false;
        }

        public string IdentificationName()
        {
            string name = ViewName.ToLower();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = Name.ToLower();
            }

            if (name.Contains("://"))
            {
                return name.Substring(name.IndexOf("://") + 3);
            }

            if (name.Contains(":"))
            {
                return name.Substring(name.IndexOf(":") + 1);
            }

            return name;
        }

        public RequestMode GetRequestMode(RequestMode defaultMode = RequestMode.Fastest)
        {
            string strVal = GetArgumentValue("RequestOption");
            if (string.IsNullOrWhiteSpace(strVal))
            {
                strVal = GetArgumentValue("RequestMode");
            }
            return strVal.GetRequestMode(defaultMode);
        }

        public bool IsNoLink()
        {
            string noLinkParam = GetArgumentValue("ParentLink");
            if(!string.IsNullOrWhiteSpace(noLinkParam) && noLinkParam.ToLower().Equals("nolink"))
            {
                return true;
            }

            noLinkParam = GetArgumentValue("TargetLinkInfoAreaId");
            if (!string.IsNullOrWhiteSpace(noLinkParam) && noLinkParam.ToLower().Equals("nolink"))
            {
                return true;
            }

            return false;
        }

        public Dictionary<string, object> GetSubListParams()
        {
            foreach (var argument in Arguments)
            {
                if (!string.IsNullOrWhiteSpace(argument.Name) && argument.Name.StartsWith("{"))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<Dictionary<string, object>>(argument.Name);
                    }
                    catch
                    {

                    }
                }
            }

            return null;
        }
    }
}
