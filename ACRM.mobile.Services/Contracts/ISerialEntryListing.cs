using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.SerialEntry;

namespace ACRM.mobile.Services.Contracts
{
    public interface ISerialEntryListing
    {
        Task ItemModified(SerialEntryItem item);
        Task ItemDeleted(SerialEntryItem item);
        Task ItemSelected(SerialEntryItem item);
        Task LoadChildPanels(SerialEntryItem serialEntryItem);
        Task EvaluatePricing(SerialEntryItem serialEntryItem);
    }
}
