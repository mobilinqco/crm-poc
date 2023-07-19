using System;
using System.Collections.Generic;
using System.Linq;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.ViewModels.ObservableGroups;

namespace ACRM.mobile.Utils
{
    public static class FilterExtensions
    {
        public static List<Filter> GetEnabledUserFilters(this List<FilterUI> uiFilter)
        {
            List<Filter> filters = new List<Filter>();
            if (uiFilter != null && uiFilter.Count > 0)
            {
                var enabledfilter = uiFilter.Where(a => a.IsEnabled).ToList();
                if (enabledfilter != null && enabledfilter.Count > 0)
                {
                    foreach (var uifilter in enabledfilter)
                    {
                        filters.Add(uifilter.OutputFilter);
                    }
                }
            }
            return filters;
        }
    }
}
