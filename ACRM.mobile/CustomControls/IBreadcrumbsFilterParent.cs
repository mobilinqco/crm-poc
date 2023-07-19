using System;
using System.Threading.Tasks;
using ACRM.mobile.ViewModels.ObservableGroups;
using ACRM.mobile.ViewModels.ObservableGroups.Breadcrumbs;

namespace ACRM.mobile.CustomControls
{
    public interface IBreadcrumbsFilterParent
    {
        Task ApplyPositionFilters(BreadcrumbsFilter filter);
    }
}
