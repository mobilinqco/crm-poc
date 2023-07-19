using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.ViewModels.ObservableGroups;

namespace ACRM.mobile.CustomControls
{
    public interface IFilterItemSelectionHandler
    {
        Task<List<FilterUI>> GetUserFilters();
        Task ApplyUserFilters(List<FilterUI> filters);
        bool CanApplyFilter
        {
            get ;
            set ;
        }
    }
}
