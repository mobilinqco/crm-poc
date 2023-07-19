using System;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.ViewModels.ObservableGroups;
using System.Collections.Generic;

namespace ACRM.mobile.Utils
{
	public class SearchAndListCommons
	{
		public SearchAndListCommons()
		{
		}

        public static List<FilterUI> GetFilterUIList(ISearchContentService contentService, int selectedTabId)
        {
            List<FilterUI> filterUIs = new List<FilterUI>();
            var filters = contentService.GetUserFilters(selectedTabId);
            var defaultEnabled = contentService.GetUserDefaultEnabledFilters(selectedTabId);

            foreach (var item in filters)
            {
                bool isEnabled = false;
                foreach (var enabledFilter in defaultEnabled)
                {
                    if (item.UnitName.Equals(enabledFilter.UnitName))
                    {
                        isEnabled = true;
                    }
                }
                filterUIs.Add(new FilterUI(item, isEnabled));
            }

            return filterUIs;
        }
    }
}

