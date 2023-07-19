using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.CustomControls
{
    public interface IPopupItemSelectionHandler
    {
        Task<List<PopupListItem>> GetPoupList();
        Task PopupItemSelected(PopupListItem item);
    }
}
