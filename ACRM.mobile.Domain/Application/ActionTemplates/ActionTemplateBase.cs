using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.ActionTemplates
{
    public class ActionTemplateBase
    {
        protected ViewReference viewReferenceModel;

        public ActionTemplateBase(ViewReference viewReference)
        {
            viewReferenceModel = viewReference;
        }

        // Names of additional filters that are applied
        public List<string> AdditionalFilter()
        {
            string val = GetValue("AdditionalFilter");
            if(string.IsNullOrEmpty(val))
            {
                return new List<string>();
            }
            if (val.Contains(','))
            {
                return val.Split(',').ToList();
            }
            else
            {
                return val.Split(';').ToList();
            }
        }

        // Name of the referenced Search&List configuration, fallback: field group of the same name.
        public string ConfigName()
        {
            string value = GetValue("ConfigName");
            if(string.IsNullOrEmpty(value))
            {
                return InfoArea();
            }
            return value;
        }

        // List of filters that are enabled by default
        public List<string> EnabledFilter()
        {
            string val = GetValue("EnabledFilter");
            if(string.IsNullOrEmpty(val))
            {
                return new List<string>();
            }
            return val.Split(',').ToList();
        }

        public string Filter(int i)
        {
            return GetValue("Filter" + i);
        }

        public string SearchFilter(int i)
        {
            return GetValue("SearchFilter" + i);
        }

        public string PositionFilter(int i)
        {
            return GetValue("PositionFilter" + i);
        }

        public (string,string) DistanceFilter(int i)
        {
            var filterName = GetValue($"Config{i}Filter");
            var configName = GetValue($"Config{i}Name");

            if(!string.IsNullOrWhiteSpace(filterName) && !string.IsNullOrWhiteSpace(configName))
            {
                return (filterName, configName);
            }

            return (null,null);
        }

        // The name of an additional fixed filter that is applied.
        public string FilterName()
        {
            return GetValue("FilterName");
        }

        // If set to true, the full text search is applied. The asterisk (*) can also be
        // used as a placeholder at the beginning of a search criterion.
        public bool FullTextSearch()
        {
            string strVal = GetValue("FullTextSearch");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }

            return false;
        }

        // If set to true, the(online/offline) button allowing the user to search data online
        // is not displayed.Only the offline search is available.
        public bool HideOnlineOfflineButton()
        {
            string strVal = GetValue("hideOnlineOfflineButton");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }

            return false;
        }

        // If set to true, the action defined by the SavedAction argument is ignored
        // and after adding a new child record, its root record is displayed.
        public bool IgnoreSavedAction()
        {
            string strVal = GetValue("IgnoreSavedAction");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }

            return false;
        }

        // Defines the info area for which records are displayed.
        public string InfoArea()
        {
            return GetValue("InfoArea");
        }

        protected string GetValue(string argumentName)
        {
            if(viewReferenceModel != null)
            {
                return viewReferenceModel.GetArgumentValue(argumentName);
            }
            return string.Empty;
        }

        public int LinkId()
        {
            string linkIdStr = GetValue("LinkId");
            if (string.IsNullOrWhiteSpace(linkIdStr))
            {
                string parentLink = ParentLink();
                if (string.IsNullOrWhiteSpace(parentLink) || !parentLink.Contains(":"))
                {
                    return -1;
                }

                string[] plParts = parentLink.Split(':');
                if(plParts.Count() > 1)
                {
                    linkIdStr = plParts[1];
                }
                else
                {
                    return -1;
                }
            }

            return int.TryParse(linkIdStr, out int linkId) ? linkId : -1;
        }

        public virtual string ParentLink()
        {
            return GetValue("ParentLink");
        }

        public virtual RequestMode GetRequestMode()
        {
            if(viewReferenceModel != null)
            {
                return viewReferenceModel.GetRequestMode();
            }

            return RequestMode.Best;
        }

        public string ToJsonString()
        {
            if(viewReferenceModel != null)
            {
                return viewReferenceModel.ToJsonString();
            }
            return string.Empty;
        }

        public string ExpandName()
        {
            string value = GetValue("ExpandName");
            if (string.IsNullOrEmpty(value))
            {
                value = ConfigName();
            }
            return value;
        }

        public int SearchPageMode()
        {
            string mode = GetValue("SearchPageMode");
            if (!string.IsNullOrEmpty(mode))
            {
                try
                {
                    return int.Parse(mode);
                }
                catch
                {
                }
            }

            return -1;
        }

        public object GetOption(string optionName)
        {
            if (viewReferenceModel != null)
            {
                Dictionary<string, object> options = viewReferenceModel.GetDictionaryArgumentValue("Options");
                if (options != null && options.ContainsKey(optionName))
                {
                    return options[optionName];
                }
            }
            return null;
        }

        public bool ShouldComputeLinks()
        {
            object computeLinks = GetOption("ComputeLinks");
            if (computeLinks != null)
            {
                if (computeLinks is bool)
                {
                    return (bool)computeLinks;
                }
            }

            return false;
        }

        public virtual string SavedAction()
        {
            return GetValue("SavedAction");
        }

        public Dictionary<string, object> AdditionalParameters()
        {
            return viewReferenceModel.GetDictionaryArgumentValue("AdditionalParameters");
        }

        public bool DisplayRecCount()
        {
            string strVal = GetValue("DisplayRecCount");
            // TODO: we may need some string formating.
            if (bool.TryParse(strVal, out bool boolValue))
            {
                return boolValue;
            }

            return true;
        }
    }
}
